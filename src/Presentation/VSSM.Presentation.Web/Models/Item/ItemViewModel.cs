namespace VSSM.Presentation.Web.Models.Item
{
    public class ItemViewModel
    {
        public int? ItemId { get; set; }

        public int ItemClassId { get; set; }

        public string Code { get; set; }

        public string FullName { get; set; }

        public float? Volume { get; set; }

        public bool UserAboveZeroStore { get; set; }

        public bool UserBelowZeroStore { get; set; }

        public bool UseOrdinaryStore { get; set; }

        public int? DayToWarmExpire { get; set; }

        public bool RequiresDiluent { get; set; }

        public int DiluentId { get; set; }

        public bool IsActive { get; set; }

        //public DateTime? EntryDate { get; set; }

        public float Osvol { get; set; }

        public float? UnitPrice { get; set; }

        public string? UniCode { get; set; }

        public int? OrderInList { get; set; }

        //public int? PresentationId { get; set; }
    }
}
