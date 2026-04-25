namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record ItemForRecipientInYearDto 
    {
        public string Reciever { get; set; }
        public int Jan { get; set; }
        public int Feb { get; set; }
        public int Mar { get; set; }
        public int Apr { get; set; }
        public int May { get; set; }
        public int Jun { get; set; }
        public int Jul { get; set; }
        public int Agu { get; set; }
        public int Sep { get; set; }
        public int Oct { get; set; }
        public int Nov { get; set; }
        public int Dec { get; set; }
        public int TotalQuota { get; internal set; }
        public int YearlyNeed { get; internal set; }
    };
}
