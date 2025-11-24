using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.RecipientConfiguration;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services
{
    public class RecipientConfigurationService : IRecipientConfigurationService
    {
        private readonly VSSMDbContext _context;

        public RecipientConfigurationService(VSSMDbContext context)
        {
            _context = context;
        }

     


        // --------------------------------------------------------------------
        // GET THE ONLY RECORD (THERE MUST BE ONLY 1)
        // --------------------------------------------------------------------
        public async Task<RecipientConfigurationDto?> GetSingle(CancellationToken token)
        {
            var entity = await _context.TblRecipientConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(token);

            return entity == null ? null : MapToDto(entity);
        }

        // --------------------------------------------------------------------
        // GET BY ID
        // --------------------------------------------------------------------
        public async Task<RecipientConfigurationDto?> GetById(int id, CancellationToken token)
        {
            var entity = await _context.TblRecipientConfigurations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RecipientConfigurationId == id, token);

            return entity == null ? null : MapToDto(entity);
        }

        // --------------------------------------------------------------------
        // CREATE
        // --------------------------------------------------------------------
        public async Task<int> Create(RecipientConfigurationCreateDto dto, CancellationToken token)
        {
            var entity = new TblRecipientConfiguration
            {
                UseBundles = dto.UseBundles,
                FreezeQtyDispatchedDl = dto.FreezeQtyDispatchedDl,
                CurrentLang = dto.CurrentLang,
                EntryDate = DateTime.Now,
                CurrentVoucherNumber = dto.CurrentVoucherNumber,
                RecipientId = dto.RecipientId,
                LastLogin = null
            };

            _context.TblRecipientConfigurations.Add(entity);
            await _context.SaveChangesAsync(token);

            return entity.RecipientConfigurationId;
        }

        // --------------------------------------------------------------------
        // UPDATE
        // --------------------------------------------------------------------
        public async Task<bool> Update(RecipientConfigurationEditDto dto, CancellationToken token)
        {
            var entity = await _context.TblRecipientConfigurations
                .FirstOrDefaultAsync(x => x.RecipientConfigurationId == dto.RecipientConfigurationId, token);

            if (entity == null)
                return false;

            entity.UseBundles = dto.UseBundles;
            entity.FreezeQtyDispatchedDl = dto.FreezeQtyDispatchedDl;
            entity.CurrentLang = dto.CurrentLang;
            entity.CurrentVoucherNumber = dto.CurrentVoucherNumber;
            entity.RecipientId = dto.RecipientId;

            await _context.SaveChangesAsync(token);
            return true;
        }

        // --------------------------------------------------------------------
        // MAPPER
        // --------------------------------------------------------------------
        private RecipientConfigurationDto MapToDto(TblRecipientConfiguration e)
        {
            return new RecipientConfigurationDto
            {
                RecipientConfigurationId = e.RecipientConfigurationId,
                UseBundles = e.UseBundles,
                FreezeQtyDispatchedDl = e.FreezeQtyDispatchedDl,
                CurrentLang = e.CurrentLang,
                EntryDate = e.EntryDate,
                CurrentVoucherNumber = e.CurrentVoucherNumber,
                RecipientId = e.RecipientId,
                LastLogin = e.LastLogin
            };
        }
    }
}
