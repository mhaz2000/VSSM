namespace VSSM.Application.DTOs.BaseEntities.Arrival
{
    public class ArrivalLineDto
    {
        public int? ArrivalLineId { get; set; }
        public int? ArrivalId { get; set; }

        public string BatchNum { get; set; }
        public int? CommertialNameId { get; set; }
        public int? DosePerVialId { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public DateTime? ProductionDate { get; set; }

        public int? ItemDetailId { get; set; }
        public int? ItemId { get; set; }
        public int? ManufacturerId { get; set; }

        public string Note { get; set; }

        public int? NumVials { get; set; }
        public int? OrderLineId { get; set; }

        public float? PackageSize { get; set; }
        public int? PackingTypeId { get; set; }

        public int? Quantity { get; set; }
        public int? StoreId { get; set; }

        public float? TotalVolume { get; set; }
        public float? UnitVolume { get; set; }

        public int? UseId { get; set; }
        public int? VvmStageId { get; set; }
        public int? VvmTypeId { get; set; }
        public string DropDownText { get; set; } = "";
    }
}
