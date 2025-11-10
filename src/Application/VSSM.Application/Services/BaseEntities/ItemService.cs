using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.ItemClass;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class ItemService : IItemService
    {
        private readonly VSSMDbContext _dbContext;
        public ItemService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
