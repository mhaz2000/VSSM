namespace VSSM.Application.DTOs.BaseEntities.RecipientConfiguration
{
    public class RecipientConfigurationDto
    {
        public int RecipientConfigurationId { get; set; }
        public bool? UseBundles { get; set; }
        public bool? FreezeQtyDispatchedDl { get; set; }
        public string CurrentLang { get; set; }
        public DateTime? EntryDate { get; set; }
        public int? CurrentVoucherNumber { get; set; }
        public int? RecipientId { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
