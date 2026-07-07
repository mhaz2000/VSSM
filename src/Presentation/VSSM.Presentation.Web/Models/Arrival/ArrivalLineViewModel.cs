using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Arrival;

public class ArrivalLineViewModel
{
    public int? ItemClassId { get; set; }
    public int? ArrivalLineId { get; set; }
    public int? ItemId { get; set; }
    public string? ItemName { get; set; }
    public int? ItemDetailId { get; set; }

    [Display(Name = "انبار")]
    [Required(ErrorMessage = "فیلد انبار اجباری است")]
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }

    public int? CommercialNameId { get; set; }
    public string? CommercialName { get; set; }
    public int? ArrivalId { get; set; }

    [Display(Name = "دوز بر ویال")]
    [Required(ErrorMessage = "فیلد دوز بر ویال اجباری است")]
    public int? DosePerVialId { get; set; }

    public int? ManufacturerId { get; set; }
    public int? UseId { get; set; }
    public int? VvmTypeId { get; set; }
    public int? VvmStageId { get; set; }

    [Display(Name = "سری ساخت")]
    [Required(ErrorMessage = "فیلد سری ساخت اجباری است")]
    public string? BatchNum { get; set; }

    // disabled — از tblorderinfo پر می‌شود
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ProductionDate { get; set; }

    // disabled — از تعداد uid های خالی پر می‌شود
    [Display(Name = "تعداد")]
    public int? Quantity { get; set; }

    public int? NumVials { get; set; }
    public string? Note { get; set; }

    public string? DoseName { get; set; }

    public string? ProductionDateShamsi { get; set; }
    public string? ExpiryDateShamsi { get; set; }


    // فیلدهای مخصوص حلال
    public int? DosePerVial { get; set; }

    public string ProductionDateMode { get; set; } = "jalali"; 
    public string ExpiryDateMode { get; set; } = "jalali";
}