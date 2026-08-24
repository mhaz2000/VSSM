using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using VSSM.Application.Integrations.Behdasht;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Presentation.Web.Controllers;

[Route("SiyamRecipients")]
[Authorize]
public class SiyamRecipientsController(
    IServiceScopeFactory scopeFactory,
    SiyamSyncStatusService syncStatus,
    ILogger<SiyamRecipientsController> logger) : Controller
{
    // Maximum number of organization detail requests running simultaneously.
    // Start with 20 and adjust based on Beh�dasht API/server capacity.
    private const int MaxDegreeOfParallelism = 20;

    [HttpPost("Sync")]
    [ValidateAntiForgeryToken]
    public IActionResult Sync()
    {
        if (syncStatus.IsSyncing())
            return Ok(syncStatus.Snapshot());

        syncStatus.SetSyncing();

        _ = Task.Run(
            () => RunSyncAsync(CancellationToken.None),
            CancellationToken.None);

        return Ok(syncStatus.Snapshot());
    }

    [HttpGet("Status")]
    public IActionResult Status()
    {
        return Ok(syncStatus.Snapshot());
    }

    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider.GetRequiredService<VSSMDbContext>();

            var behdashtClient =
                scope.ServiceProvider.GetRequiredService<BehdashtApiClient>();

            // ---------------------------------------------------------
            // 1. Get all organization IDs
            // ---------------------------------------------------------

            var ids = new List<string>();

            const int pageSize = 200;
            var page = 1;

            do
            {
                var search =
                    await behdashtClient.SearchOrganizationsAsync(
                        page,
                        pageSize,
                        cancellationToken);

                var inner = search?.Result?.Data?.Data;

                if (inner == null)
                    break;

                if (inner.Items != null)
                {
                    foreach (var item in inner.Items)
                    {
                        if (!string.IsNullOrWhiteSpace(item.Id))
                        {
                            ids.Add(item.Id);
                        }
                    }
                }

                page++;

                if (!inner.HasNextPage)
                    break;

            } while (true);

            logger.LogInformation(
                "SiyamRecipients sync: collected {Count} organization ids",
                ids.Count);

            syncStatus.UpdateProgress(0, ids.Count);

            if (ids.Count == 0)
            {
                syncStatus.SetCompleted(DateTime.Now);
                return;
            }

            // ---------------------------------------------------------
            // 2. Get organization details in parallel
            // ---------------------------------------------------------

            var dict =
                new ConcurrentDictionary<string, TblSiyamRecipient>(
                    StringComparer.OrdinalIgnoreCase);

            var processed = 0;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = cts.Token
            };

            await Parallel.ForEachAsync(
                ids,
                parallelOptions,
                async (id, ct) =>
                {
                    try
                    {
                        var response =
                            await behdashtClient.GetOrganizationAsync(
                                id,
                                ct);

                        var detail =
                            response?.Result?.Data?.Data;

                        if (detail != null &&
                            !string.IsNullOrWhiteSpace(
                                detail.OrganizationId?.Id))
                        {
                            var entity = ToEntity(detail);

                            dict[entity.Id] = entity;
                        }
                        else
                        {
                            logger.LogWarning(
                                "No organization detail returned for {OrganizationId}",
                                id);
                        }
                    }
                    catch (OperationCanceledException)
                        when (ct.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Failed to get organization {OrganizationId}",
                            id);
                    }
                    finally
                    {
                        var current =
                            Interlocked.Increment(ref processed);


                        if (current % 50 == 0 ||
                            current == ids.Count)
                        {
                            syncStatus.UpdateProgress(
                                current,
                                ids.Count);
                        }
                    }
                });

            logger.LogInformation(
                "SiyamRecipients sync: fetched {Count} organizations successfully",
                dict.Count);

            // ---------------------------------------------------------
            // 3. Resolve parent (hierarchical) relationships in memory.
            //    Keep a link ONLY when the parent is also in our dataset;
            //    otherwise the FK would point at a non-existent row and
            //    violate FK_tblSiyamRecipients_Hierarchical on insert.
            // ---------------------------------------------------------

            var parentLinks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entity in dict.Values)
            {
                var hId = entity.HierarchicalOrganizationId;
                if (!string.IsNullOrWhiteSpace(hId) && dict.TryGetValue(hId, out _))
                {
                    parentLinks[entity.Id] = hId;
                }

                // Clear the scalar FK for now; it is re-applied in phase B
                // once every row physically exists (handles insert order and cycles).
                entity.HierarchicalOrganizationId = null;
                entity.HierarchicalOrganization = null;
            }

            // ---------------------------------------------------------
            // 4. Save to database in two phases:
            //    A) insert every row with a NULL hierarchical FK,
            //    B) update the hierarchical FK only where the parent exists.
            //    This guarantees the self-referencing FK is always satisfiable
            //    regardless of insertion order or hierarchical cycles.
            // ---------------------------------------------------------

            const int batchSize = 1000;
            var entities = dict.Values.ToList();

            for (var i = 0; i < entities.Count; i += batchSize)
            {
                var batch = entities.Skip(i).Take(batchSize);
                context.TblSiyamRecipients.AddRange(batch);
                await context.SaveChangesAsync(cancellationToken);
            }

            foreach (var entity in entities)
            {
                if (parentLinks.TryGetValue(entity.Id, out var parentId))
                {
                    entity.HierarchicalOrganizationId = parentId;
                }
            }

            for (var i = 0; i < entities.Count; i += batchSize)
            {
                var batch = entities.Skip(i).Take(batchSize).ToList();
                context.TblSiyamRecipients.UpdateRange(batch);
                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation(
                "SiyamRecipients sync completed successfully. " +
                "Saved {Count} organizations",
                dict.Count);

            syncStatus.SetCompleted(DateTime.Now);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                "SiyamRecipients sync was cancelled");

            syncStatus.SetFailed("Sync was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "SiyamRecipients sync failed");

            syncStatus.SetFailed(ex.Message);
        }
    }

    private static TblSiyamRecipient ToEntity(OrganizationDetail d)
    {
        return new TblSiyamRecipient
        {
            Id = d.OrganizationId?.Id ?? string.Empty,

            OrganizationId = d.OrganizationId,

            HierarchicalOrganizationId =
                d.HierarchicalOrganizationId?.Id,

            Name = d.Name,

            Type = d.Type,

            DependencyType = d.DependencyType,

            AllegianceType = d.AllegianceType,

            OwnershipStatus = d.OwnershipStatus,

            Specialty = d.Specialty,

            ActivityStatus = d.ActivityStatus,

            OwnerOrganization = d.OwnerOrganizationId,

            ObserverOrganization = d.ObserverOrganizationId,

            NationalAreaCode =
                d.Area?.NationalAreaCode,

            Country =
                d.Area?.Country,

            Province =
                d.Area?.Province,

            Town =
                d.Area?.Town,

            District =
                d.Area?.District,

            RuralArea =
                d.Area?.RuralArea,

            City =
                d.Area?.City,

            Village =
                d.Area?.Village,

            SpecialtyTypeJson =
                JsonSerializer.Serialize(d.SpecialtyType),

            OtherIdentityInfoJson =
                JsonSerializer.Serialize(d.OtherIdentityInfo),

            ExtendedPropertyJson =
                JsonSerializer.Serialize(d.ExtendedProperty),

            PostalCode =
                d.PostalCode,

            Longitude =
                d.Longitude,

            Latitude =
                d.Latitude,

            EstablishmentDate =
                d.EstablishmentDate,

            ActivationDate =
                d.ActivationDate
        };
    }
}