using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs.BaseEntities.VoucherConfig;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class VoucherConfigService : IVoucherConfigService
{
    private readonly VSSMDbContext _context;

    public VoucherConfigService(VSSMDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherConfigDto?> GetAsync()
    {
        var entity = await _context.TblVoucherConfigs.FirstOrDefaultAsync();
        if (entity == null) return null;

        return new VoucherConfigDto
        {
            ShowExpiryDate = entity.ShowExpiryDate ??false,
            ShowNumberOfVials = entity.ShowNumberOfVials ??false,
            ShowDoseVial = entity.ShowDoseVial ??false,
            ShowVvmtype = entity.ShowVvmtype ??false,
            ShowVvmstage = entity.ShowVvmstage ??false,
            ShowStore = entity.ShowStore ??false,
            ShowUnitPrice = entity.ShowUnitPrice ??false,
            ShowTotalPriceLine = entity.ShowTotalPriceLine ??false,
            ShowNotesLine = entity.ShowNotesLine ??false,
            ShowNotes = entity.ShowNotes ??false,
            ShowTotalWeight = entity.ShowTotalWeight ??false,
            ShowPacksNumber = entity.ShowPacksNumber ??false,
            ShowTotalPrice = entity.ShowTotalPrice ??false,
            ShowTotalExternalSize = entity.ShowTotalExternalSize ?? false,
            ShowTotalVolume = entity.ShowTotalVolume ?? false,
            FooterNotes = entity.FooterNotes,
            SignPlaceHolder1 = entity.SignPlaceHolder1,
            SignPlaceHolder2 = entity.SignPlaceHolder2,
            SignPlaceHolder3 = entity.SignPlaceHolder3,
            SignPlaceHolder4 = entity.SignPlaceHolder4,
            ShowItem = entity.ShowItem  ?? false,
            ShowBatchNumber = entity.ShowBatchNumber  ?? false,
            ShowRequestNumber = entity.ShowRequestNumber  ?? false,
            ShowTransportMode = entity.ShowTransportMode  ?? false,
            ShowManufacturer = entity.ShowManufacturer  ?? false,
            ShowQuantity = entity.ShowQuantity  ?? false,
            ShowItemCode = entity.ShowItemCode  ?? false,
            ShowPresentation = entity.ShowPresentation  ?? false,
            ShowPackingType = entity.ShowPackingType  ?? false,
            ShowPackageSize = entity.ShowPackageSize  ?? false,
            ShowStageDelivery = entity.ShowStageDelivery  ?? false,
            ShowIndicator = entity.ShowIndicator  ?? false,
            ShowSource = entity.ShowSource  ?? false
        };
    }

    public async Task<VoucherConfigDto> AddOrUpdateAsync(VoucherConfigDto dto)
    {
        var entity = await _context.TblVoucherConfigs.FirstOrDefaultAsync();

        if (entity == null)
        {
            entity = new TblVoucherConfig();
            _context.TblVoucherConfigs.Add(entity);
        }

        // Map DTO to entity
        entity.ShowExpiryDate = dto.ShowExpiryDate;
        entity.ShowNumberOfVials = dto.ShowNumberOfVials;
        entity.ShowDoseVial = dto.ShowDoseVial;
        entity.ShowVvmtype = dto.ShowVvmtype;
        entity.ShowVvmstage = dto.ShowVvmstage;
        entity.ShowStore = dto.ShowStore;
        entity.ShowUnitPrice = dto.ShowUnitPrice;
        entity.ShowTotalPriceLine = dto.ShowTotalPriceLine;
        entity.ShowNotesLine = dto.ShowNotesLine;
        entity.ShowNotes = dto.ShowNotes;
        entity.ShowTotalWeight = dto.ShowTotalWeight;
        entity.ShowPacksNumber = dto.ShowPacksNumber;
        entity.ShowTotalPrice = dto.ShowTotalPrice;
        entity.ShowTotalExternalSize = dto.ShowTotalExternalSize;
        entity.ShowTotalVolume = dto.ShowTotalVolume;
        entity.FooterNotes = dto.FooterNotes;
        entity.SignPlaceHolder1 = dto.SignPlaceHolder1;
        entity.SignPlaceHolder2 = dto.SignPlaceHolder2;
        entity.SignPlaceHolder3 = dto.SignPlaceHolder3;
        entity.SignPlaceHolder4 = dto.SignPlaceHolder4;
        entity.ShowItem = dto.ShowItem;
        entity.ShowBatchNumber = dto.ShowBatchNumber;
        entity.ShowRequestNumber = dto.ShowRequestNumber;
        entity.ShowTransportMode = dto.ShowTransportMode;
        entity.ShowManufacturer = dto.ShowManufacturer;
        entity.ShowQuantity = dto.ShowQuantity;
        entity.ShowItemCode = dto.ShowItemCode;
        entity.ShowPresentation = dto.ShowPresentation;
        entity.ShowPackingType = dto.ShowPackingType;
        entity.ShowPackageSize = dto.ShowPackageSize;
        entity.ShowStageDelivery = dto.ShowStageDelivery;
        entity.ShowIndicator = dto.ShowIndicator;
        entity.ShowSource = dto.ShowSource;

        await _context.SaveChangesAsync();

        return dto;
    }
}
