using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.PackType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class PackingTypeService : IPackingTypeService
    {
        private VSSMDbContext _dbContext;

        public PackingTypeService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PackTypeDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _dbContext.TblPackingTypes
                                   .AsNoTracking()
                                   .Where(c => c.IsActive == true)
                                   .ToListAsync(cancellationToken);

            var result = items.Select(x => new PackTypeDto
            {
                PackingTypeId = x.PackingTypeId,
                FullName = x.FullName
            }).ToList();

            return result;
        }


    }
}
