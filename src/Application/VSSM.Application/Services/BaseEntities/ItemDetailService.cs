using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class ItemDetailService : IItemDetailsService
    {
        private readonly VSSMDbContext _db;

        public ItemDetailService(VSSMDbContext db)
        {
            _db = db;
        }
        public async Task<List<ItemDetailsDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _db.TblItemDetails
                                  .AsNoTracking()
                                  .ToListAsync(cancellationToken);

            var result = items.Select(x => new ItemDetailsDto
            {
                Id = x.ItemDetailId,
                Name = x.UnitVolume,
                itemid = x.ItemId,


            }).ToList();

            return result;
        }

        public async Task<List<ItemFullDetailsDto>> GetItemDetailsAsync(int itemId, CancellationToken cancellationToken)
        {
            var itemDetails = await _db.TblItemDetails
                .AsNoTracking()
                .Where(x => x.ItemId == itemId)
                .Select(x => new ItemFullDetailsDto
                {
                    ItemDetailId = x.ItemDetailId,
                    ItemId = x.ItemId,
                    BatchNumber = x.BatchNumber,
                    ExpiryDate = x.ExpiryDate,
                    UnitPrice = x.UnitPrice,
                    ProductionDate = x.ProductionDate,
                    UnitVolume = x.UnitVolume,
                    EntryDate = x.EntryDate,
                    IsConfirmed = x.IsConfirmed
                })
                .ToListAsync(cancellationToken);

            return itemDetails;
        }
    }
}
