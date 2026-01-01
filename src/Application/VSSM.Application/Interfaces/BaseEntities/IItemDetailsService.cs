using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.DosePerVial;
using VSSM.Application.DTOs.BaseEntities.ItemDetail;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IItemDetailsService
    {
        public Task<List<ItemDetailsDto>> GetActives(CancellationToken cancellationToken);
        Task<List<ItemFullDetailsDto>> GetItemDetailsAsync(int itemId, CancellationToken cancellationToken);
    }
}
