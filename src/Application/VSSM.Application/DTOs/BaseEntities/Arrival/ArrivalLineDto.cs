namespace VSSM.Application.DTOs.Arrivals
{
    public class ArrivalLineDto
    {
        public int ArrivalLineId { get; set; }

        public int? ItemId { get; set; }
        public string ItemName { get; set; }

        public int? ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }

        public int? StoreId { get; set; }
        public string StoreName { get; set; }

        public int? DosePerVialId { get; set; }
        public string DosePerVialName { get; set; }

        public int? NumVials { get; set; }
        public int? Quantity { get; set; }

        public float? UnitVolume { get; set; }
        public float? TotalVolume { get; set; }

        public string BatchNum { get; set; }
        public string ExpiryDateString { get; set; }
        public string ProductionDateString { get; set; }
        public string Note { get; set; }
    }
}
