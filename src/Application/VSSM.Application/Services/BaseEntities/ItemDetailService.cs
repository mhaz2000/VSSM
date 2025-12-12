using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
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
        public async Task<List<ItemDetailListDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _db.TblItemDetails
                                  .AsNoTracking()
                                  .ToListAsync(cancellationToken);

            var result = items.Select(x => new ItemDetailListDto
            {
                Id = x.ItemDetailId,
                Name = x.PackageSize,


            }).ToList();

            return result;
        }


    }
}
