using VSSM.Application.DTOs.BaseEntities.ItemDetail;

namespace VSSM.Application.Interfaces.BaseEntities
{
    public interface IItemDetailsService
    {
        public Task<List<ItemDetailsDto>> GetActives(CancellationToken cancellationToken);
        Task<List<ItemBatchNumbersDto>> GetItemBatchNumbers(int itemId, int recipientId, CancellationToken none);
        Task<List<ToDispatchItemDetailDto>> GetItemDetailsAsync(int itemId, int recipientId, CancellationToken cancellationToken);
    }
}
