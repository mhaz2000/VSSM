using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.Uses;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class UseService : IUseService
    {
        private readonly VSSMDbContext _dbContext;

        public UseService(VSSMDbContext db)
        {
            _dbContext = db;
        }
        public async Task<List<UsesListDto>> GetActives(CancellationToken cancellationToken)
        {
            var items = await _dbContext.TblUses
                                    .AsNoTracking()
                                    .Where(c => c.IsActive == true)
                                    .ToListAsync(cancellationToken);

            var result = items.Select(x => new UsesListDto
            {
                Id = x.UseId,
                Code = x.Code,
                FullName = x.FullName
            }).ToList();

            return result;
        }


    }
}
