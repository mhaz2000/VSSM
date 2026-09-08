using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Recipient;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class RecipientService(VSSMDbContext db) : IRecipientService
{
    public async Task<List<RecipientSearchDto>> SearchActiveRecipientsAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = db.TblRecipients.Where(r => r.IsActive == true);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var s = searchTerm.Trim();
            query = query.Where(r =>
                r.FullName.Contains(s) ||
                r.Code.Contains(s) ||
                r.Province.Contains(s) ||
                r.City.Contains(s)
            );
        }

        return await query
            .OrderBy(r => r.FullName)
            .Take(20)
            .Select(r => new RecipientSearchDto
            {
                RecipientId = r.RecipientId,
                FullName = r.FullName,
                Code = r.Code,
                Province = r.Province,
                City = r.City
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagingSearchOrderDto<RecipientGetDto>> GetPagedList(
        string? search,
        int? levelId,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken token)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblRecipient> query = db.TblRecipients
            .Include(t => t.TransportMode)
            .Include(t => t.ReceivingMode);

        if (levelId.HasValue && levelId.Value > 0)
            query = query.Where(x => x.ChainLevel == levelId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.FullName.Contains(search));
        }

        query = sortField switch
        {
            "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            "FullName" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            _ => query.OrderBy(x => x.RecipientId) 
        };

        paging.TotalItems = await query.CountAsync(token);

        var list = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new RecipientGetDto
            {
                EntryDate = x.EntryDate.HasValue ? Helpers.DateTimeHelper.ToShamsi(x.EntryDate.Value) : string.Empty,
                RecipientId = x.RecipientId,
                Code = x.Code,
                FullName = x.FullName,
                IsActive = x.IsActive ?? false,
                Year = x.Year,
                Population = x.Population,
                ChainLevel = x.ChainLevel,
                LevelName = db.TblLevels
                    .Where(l => l.LevelId == x.ChainLevel)
                    .Select(l => l.ShipCount.ToString())
                    .FirstOrDefault() ?? string.Empty,
                StartUp = x.StartUp.HasValue ? Helpers.DateTimeHelper.ToShamsi(x.StartUp.Value) : string.Empty,
                ParentId = x.ParentId,
                TransportModeName = x.TransportMode != null ? x.TransportMode.Title : string.Empty,
                ReceivingModeName = x.ReceivingMode != null ? x.ReceivingMode.Title : string.Empty
            })
            .ToListAsync(token);

        return new PagingSearchOrderDto<RecipientGetDto>
        {
            Items = list,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<RecipientEditDto?> GetById(int id, CancellationToken token)
    {
        var x = await db.TblRecipients
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecipientId == id, token);

        if (x == null)
            return null;

        return new RecipientEditDto
        {
            RecipientId = x.RecipientId,
            Code = x.Code,
            FullName = x.FullName,
            IsActive = x.IsActive ?? false,
            TransportModeId = x.TransportModeId,
            Distance = x.Distance,
            Population = x.Population,
            Year = x.Year,
            Note = x.Note,
            ParentId = x.ParentId,
            StartUp = x.StartUp,
            ChainLevel = x.ChainLevel,
            ReceivingModeId = x.ReceivingModeId,
            Address1 = x.Address1,
            Address2 = x.Address2,
            PostalCode = x.PostalCode,
            City = x.City,
            Province = x.Province,
            Phone = x.Phone,
            Email = x.Email,
            LeadTime = x.LeadTime,
            ClimateId = x.ClimateId,
            OwnershipId = x.OwnershipId,
            PowerSourceId = x.PowerSourceId,
            TypeOfRoadId = x.TypeOfRoadId,
            HasLandLine = x.HasLandLine ?? false,
            HasInternet = x.HasInternet ?? false,
            DistanceTime = x.DistanceTime,
            EntryDate = x.EntryDate
        };
    }

    public async Task<int> Create(RecipientCreateDto dto, CancellationToken token)
    {
        var entity = new TblRecipient
        {
            Code = dto.Code,
            FullName = dto.FullName,
            EntryDate = DateTime.Now,
            IsActive = true,
            TransportModeId = dto.TransportModeId,
            Distance = dto.Distance,
            Population = dto.Population,
            Year = dto.Year,
            Note = dto.Note,
            ParentId = dto.ParentId,
            StartUp = dto.StartUp,
            ChainLevel = dto.ChainLevel,
            ReceivingModeId = dto.ReceivingModeId,
            Address1 = dto.Address1,
            Address2 = dto.Address2,
            PostalCode = dto.PostalCode,
            City = dto.City,
            Province = dto.Province,
            Phone = dto.Phone,
            Email = dto.Email,
            LeadTime = dto.LeadTime,
            ClimateId = dto.ClimateId,
            OwnershipId = dto.OwnershipId,
            PowerSourceId = dto.PowerSourceId,
            TypeOfRoadId = dto.TypeOfRoadId,
            HasLandLine = dto.HasLandLine,
            HasInternet = dto.HasInternet,
            DistanceTime = dto.DistanceTime
        };

        await db.TblRecipients.AddAsync(entity, token);
        await db.SaveChangesAsync(token);
        return entity.RecipientId;
    }

    public async Task<bool> Update(RecipientEditDto dto, CancellationToken token)
    {
        var entity = await db.TblRecipients
            .FirstOrDefaultAsync(x => x.RecipientId == dto.RecipientId, token);

        if (entity == null)
            return false;

        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.IsActive = dto.IsActive;
        entity.TransportModeId = dto.TransportModeId;
        entity.Distance = dto.Distance;
        entity.Population = dto.Population;
        entity.Year = dto.Year;
        entity.Note = dto.Note;
        entity.ParentId = dto.ParentId;
        entity.StartUp = dto.StartUp;
        entity.ChainLevel = dto.ChainLevel;
        entity.ReceivingModeId = dto.ReceivingModeId;
        entity.Address1 = dto.Address1;
        entity.Address2 = dto.Address2;
        entity.PostalCode = dto.PostalCode;
        entity.City = dto.City;
        entity.Province = dto.Province;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.LeadTime = dto.LeadTime;
        entity.ClimateId = dto.ClimateId;
        entity.OwnershipId = dto.OwnershipId;
        entity.PowerSourceId = dto.PowerSourceId;
        entity.TypeOfRoadId = dto.TypeOfRoadId;
        entity.HasLandLine = dto.HasLandLine;
        entity.HasInternet = dto.HasInternet;
        entity.DistanceTime = dto.DistanceTime;

        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<bool> ToggleActive(int id, CancellationToken token)
    {
        var entity = await db.TblRecipients
            .FirstOrDefaultAsync(x => x.RecipientId == id, token);

        if (entity == null)
            throw new Exception("Record not found.");

        entity.IsActive = !entity.IsActive;
        await db.SaveChangesAsync(token);

        return entity.IsActive ?? false;
    }

    public async Task<bool> ExistsByCode(string? code, int? excludeId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var query = db.TblRecipients
            .Where(x => x.Code == code.Trim());

        if (excludeId.HasValue && excludeId.Value > 0)
            query = query.Where(x => x.RecipientId != excludeId.Value);

        return await query.AnyAsync(token);
    }

    public async Task<List<RecipientGetDto>> GetAll(CancellationToken cancellationToken)
    {
        var query = db.TblRecipients;
        return await query.Select(x => new RecipientGetDto
        {
            RecipientId = x.RecipientId,
            Code = x.Code,
            FullName = x.FullName,
            IsActive = x.IsActive ?? false,
        }).ToListAsync(cancellationToken);
    }

    public async Task<int?> GetChainLevelById(int id, CancellationToken token)
    {
        var reci = await db.TblRecipients
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RecipientId == id, token);

        if (reci == null)
            throw new Exception("Recipient not found.");

        return reci.ChainLevel;
    }

    public async Task<List<RecipientGetDto>> GetActiveRecipients(CancellationToken token)
    {
        var recipients = await db.TblRecipients
            .AsNoTracking()
            .Where(recipient => recipient.IsActive == true)
            .Select(recipient => new RecipientGetDto
            {
                RecipientId = recipient.RecipientId,
                Code = recipient.Code,
                FullName = recipient.FullName,
                IsActive = recipient.IsActive ?? false,
            }).ToListAsync(token);

        return recipients;
    }

    public async Task<List<RecipientGetDto>> GetActiveRecipientChildren(int currentRecipientId, CancellationToken token)
    {
        var recipients = await db.TblRecipients
            .AsNoTracking()
            .Where(recipient => currentRecipientId == recipient.ParentId && recipient.IsActive == true)
            .Select(recipient => new RecipientGetDto
            {
                ChainLevel = recipient.ChainLevel,
                RecipientId = recipient.RecipientId,
                Code = recipient.Code,
                FullName = recipient.FullName,
                IsActive = recipient.IsActive ?? false,
            }).ToListAsync(token);

        return recipients;
    }
}