using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.Uses;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IUseService
    {
        public Task<List<UsesListDto>> GetActives(CancellationToken cancellationToken);
    }
}
