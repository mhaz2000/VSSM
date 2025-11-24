namespace VSSM.Application.DTOs.BaseEntities.Store
{
    public class StoreEditDto
    {
        public int StoreId { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }

        public int TypeId { get; set; }
        public float FreeVolume { get; set; }

        public float? Volume { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }

        public int? RecipientId { get; set; }
        public float? GrossVol { get; set; }
        public int? Year { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Pqsno { get; set; }
        public int? CoolingUnitNo { get; set; }
        public string RefrigerantGas { get; set; }
        public string RoomBox { get; set; }

        public int? PowerSource { get; set; }
        public int? Source { get; set; }

        public string Barcode { get; set; }
        public int? PeriodNoteUse { get; set; }
        public float? UnitCost { get; set; }
    }
}
