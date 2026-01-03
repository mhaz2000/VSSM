namespace VSSM.Presentation.Web.Models.Request
{
    public class RequestViewModel
    {
        public int Id { get; set; }
        public int? EnteredBy { get; set; } = null;
        public DateOnly? SendDate { get; set; }
        public string RequestNumber { get; set; }
        public DateOnly? ConfirmDate { get; set; }
        public int? RequestStatus { get; set; }

        public List<RequestLinesViewModel> RequestLines { get; set; }
        public RequestLinesViewModel NewLine { get; set; }
    }
}
