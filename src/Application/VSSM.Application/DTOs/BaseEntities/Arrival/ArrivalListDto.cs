namespace VSSM.Application.DTOs.Arrivals
{
    public class ArrivalListDto
    {
        public int ArrivalId { get; set; }
        public string DateString { get; set; }          // برای نمایش
        public string ReportNumber { get; set; }
        public string BiddingNumber { get; set; }
        public float? TotalVolume { get; set; }

        public bool? IsConfirmed { get; set; }
        public bool? IsDrafted { get; set; }
        public bool? IsCancelled { get; set; }
    }
}
