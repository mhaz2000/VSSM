namespace VSSM.Presentation.Web.Models.Request
{
    public class RequestLinesViewModel
    {
        public int Id { get; set; }

        public int? RequestId { get; set; }

        public int? ItemId { get; set; }
        public string ItemName { get; set; }

        public int? ItemClassId { get; set; }

        public int? Anncount { get; set; }

        public int? Arivcount { get; set; }

        public int? Usedcount { get; set; }

        public int? Currentcount { get; set; }

        public int? Forperiod { get; set; }

        public int? Offercount { get; set; }

        public int? Reqcount { get; set; }

        public string Note { get; set; }

        public int? Confirmcount { get; set; }

        public int? DosePerVialId { get; set; }
        public int? InventoryHealth { get; set; }
        public int? TimeBetweenRequestAndArrival { get; set; }
    }
}
