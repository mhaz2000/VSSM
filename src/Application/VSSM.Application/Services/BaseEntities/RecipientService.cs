using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Recipient;
using VSSM.Application.DTOs.BaseEntities.TransportMode;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class RecipientService : IRecipientService
    {
        private readonly VSSMDbContext _db;

        public RecipientService(VSSMDbContext db)
        {
            _db = db;
        }

        public async Task<PagingSearchOrderDto<RecipientGetDto>> GetPagedList(
            string? search,
            int page,
            int pageSize,
            string sortField,
            string sortOrder,
            CancellationToken token)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblRecipient> query = _db.TblRecipients;

            // SEARCH by Code or FullName
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    x.Code.Contains(search) ||
                    x.FullName.Contains(search));
            }

            // SORT
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
                    RecipientId = x.RecipientId,
                    Code = x.Code,
                    FullName = x.FullName,
                    IsActive = x.IsActive ?? false,
                    Year = x.Year,
                    Population = x.Population,
                    ChainLevel = x.ChainLevel,
                    StartUp = x.StartUp,
                    ParentId = x.ParentId,

                    TransportModeName = _db.TblTransportModes
                        .Where(t => t.TransportModeId == x.TransportModeId)
                        .Select(t => t.FullName)
                        .FirstOrDefault(),

                    ReceivingModeName = _db.TblReceivingModes
                        .Where(r => r.ReceivingModeId == x.ReceivingModeId)
                        .Select(r => r.FullName)
                        .FirstOrDefault()
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

        public async Task<RecipientEditDto> GetById(int id, CancellationToken token)
        {
            var x = await _db.TblRecipients
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RecipientId == id, token);

            if (x == null)
                throw new Exception("Recipient not found.");

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
                IsActive = dto.IsActive,
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

            _db.TblRecipients.Add(entity);
            await _db.SaveChangesAsync(token);
            return entity.RecipientId;
        }

        public async Task<bool> Update(RecipientEditDto dto, CancellationToken token)
        {
            var entity = await _db.TblRecipients
                .FirstOrDefaultAsync(x => x.RecipientId == dto.RecipientId, token);

            if (entity == null)
                throw new Exception("Recipient not found.");

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

            await _db.SaveChangesAsync(token);
            return true;
        }

        public async Task<List<RecipientGetDto>> GetAll(CancellationToken cancellationToken)
        {

            var query = _db.TblRecipients;
            return await query.Select(x =>
                   new RecipientGetDto
                   {
                       RecipientId = x.RecipientId,
                       Code = x.Code,
                       FullName = x.FullName,
                       IsActive = x.IsActive ?? false,

                   }
                ).ToListAsync(cancellationToken);
        }
    }
}
