using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.Consumer;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Infrastructure.Services.BaseEntities;

public sealed class ConsumerService : IConsumerService
{
    private readonly VSSMDbContext _db;

    public ConsumerService(VSSMDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<ConsumeUidResponse> ConsumeUidAsync(ConsumeUidRequest request, CancellationToken token)
    {
        var response = new ConsumeUidResponse();

        if (string.IsNullOrWhiteSpace(request.Uid))
        {
            response.StatusCode = "101";
            response.StatusDesc = "Uid موجود نمی‌باشد";
            return response;
        }

        if (request.Uid.Length != 20)
        {
            response.StatusCode = "102";
            response.StatusDesc = "Uid معتبر نمی‌باشد";
            return response;
        }

        var uidInfo = await _db.TblUidInfo
            .Include(u => u.OrderInfo)
                .ThenInclude(o => o.DosePerVial)
            .Include(u => u.OrderInfo)
                .ThenInclude(o => o.Item)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.SecLevelUid == request.Uid || u.ParentUid == request.Uid || u.FirstLevelUid == request.Uid, token);


        if (uidInfo == null)
        {
            response.StatusCode = "101";
            response.StatusDesc = "Uid موجود نمی‌باشد";
            return response;
        }

        var store =  uidInfo.CurrentStore.HasValue ? await _db.TblStores.FindAsync(uidInfo.CurrentStore) : null;

        if (uidInfo.Used)
        {
            response.StatusCode = "103";
            response.StatusDesc = "Uid مصرف شده است";
            return response;
        }

        var recipient = await _db.TblRecipients
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code == request.SiyamCode && r.IsActive == true, token);

        if (recipient == null)
        {
            response.StatusCode = "105";
            response.StatusDesc = "ساختار شبکه مورد نظر یافت نشد";
            return response;
        }

        if (uidInfo.CurrentStore.HasValue && store is not null &&  store.RecipientId != recipient.RecipientId)
        {
            response.StatusCode = "104";
            response.StatusDesc = "Uid متعلق به مرکز شما نمی‌باشد";
            return response;
        }


        await using var transaction = await _db.Database.BeginTransactionAsync(token);

        try
        {
            var newConsumedCount = (uidInfo.ConsumedCount ?? 0) + 1;
            var dosePerVial = int.Parse(uidInfo.OrderInfo?.DosePerVial?.Title ?? "0");
            var isFullyConsumed = newConsumedCount >= dosePerVial && dosePerVial > 0;

            await _db.TblUidInfo
                .Where(u => u.PkUidInfo == uidInfo.PkUidInfo)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.ConsumedCount, newConsumedCount)
                    .SetProperty(u => u.Used, isFullyConsumed), token);

            var consumerPerson = new TblConsumerPerson
            {
                FkUid = uidInfo.PkUidInfo,
                FkConsumerRecipientId = recipient.RecipientId,
                ConsumerPerson = request.ConsumerPerson,
                Vaccinator = request.Vaccinator,
                VaccinationRank = newConsumedCount,
                EntryDate = DateTime.Now
            };

            _db.TblConsumerPeople.Add(consumerPerson);
            await _db.SaveChangesAsync(token);

            await transaction.CommitAsync(token);

            response.StatusCode = "0";
            response.StatusDesc = "موفق";
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }
}