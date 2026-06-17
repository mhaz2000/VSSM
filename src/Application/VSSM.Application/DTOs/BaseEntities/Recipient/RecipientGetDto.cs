namespace VSSM.Application.DTOs.BaseEntities.Recipient
{
    public class RecipientGetDto
    {
        public int RecipientId { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string? StartUp { get; set; }
        public string? EntryDate { get; set; }
        public bool IsActive { get; set; }
        public string IsActiveText => IsActive ? "بله" : "خیر";
        public int? Year { get; set; }
        public long? Population { get; set; }
        public int? ChainLevel { get; set; }
 
        public string TransportModeName { get; set; }
        public string ReceivingModeName { get; set; }

 
        public int? ParentId { get; set; } // numeric enum value
    }



  
}
