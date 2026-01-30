using System.ComponentModel.DataAnnotations;

namespace VSSM.Presentation.Web.Models.Arrival
{
    public class ArrivalLineViewModel
    {
        public int? ArrivalLineId { get; set; }
        public int? ItemId { get; set; }
        public int? ItemDetailId { get; set; }
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public int? CommertialNameId { get; set; }
        public string? CommertialName { get; set; }
        public int? ArrivalId { get; set; }
        public int? DosePerVialId { get; set; }
        public int? ManufacturerId { get; set; }
        public int? PackingTypeId { get; set; }
        public int? UseId { get; set; }
        public int? VvmTypeId { get; set; }
        public int? VvmStageId { get; set; }
        public string? BatchNum { get; set; }
        public DateTime? ExpiryDate { get; set; }      // string (از datepicker)
        public DateTime? ProductionDate { get; set; }  // string
        public int? Quantity { get; set; }
        public int? NumVials { get; set; }
        public float? UnitVolume { get; set; }
        public float? TotalVolume { get; set; }
        public float? PackageSize { get; set; }
        public string? Note { get; set; }
    }
}
