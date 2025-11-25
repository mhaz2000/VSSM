// Application/DTOs/BaseEntities/MinMaxItem/MinMaxItemGetDto.cs
namespace VSSM.Application.DTOs.BaseEntities.MinMaxItem
{
    public class MinMaxItemGetDto
    {
        public int MinMaxItemId { get; set; }

        public int? ItemId { get; set; }
        public int? RecipientId { get; set; }

        public long? StockMin { get; set; }
        public long? StockMax { get; set; }

        public long? Min { get; set; }
        public long? Max { get; set; }

        public long? AnnualNeeds { get; set; }
 

        public long? NumberShipment { get; set; }
        public int? PercentSafety { get; set; }
        public long? LeadConsumption { get; set; }
        public long? TotalPop { get; set; }
        public int? PercentUnder { get; set; }
        public long? NumDose { get; set; }
        public long? Wastage { get; set; }
        public long? PercentCoverage { get; set; }

        public DateTime? EntryDate { get; set; }
        public long? OrderLevel { get; set; }
    }
}
