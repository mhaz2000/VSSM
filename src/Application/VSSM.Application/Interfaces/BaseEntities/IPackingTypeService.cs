using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.PackType;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IPackingTypeService
    {
        public Task<List<PackTypeDto>> GetActives(CancellationToken cancellationToken);
    }
}
