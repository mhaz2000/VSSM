using System.ComponentModel.DataAnnotations;
using VSSM.Domain.Enums;

namespace VSSM.Presentation.Web.Models.Recipient
{
    public class RecipientViewModel
    {
        public int? RecipientId { get; set; }

        [Required(ErrorMessage = "کد نمی‌تواند خالی باشد.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "نام کامل نمی‌تواند خالی باشد.")]
        public string FullName { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "طریقه حمل و نقل")]
        public int? TransportModeId { get; set; }

        public long? Distance { get; set; }
        public long? Population { get; set; }
        public int? Year { get; set; }
        public string Note { get; set; }

        [Required(ErrorMessage = "سطح در زنجیره نمی‌تواند خالی باشد.")]
        public int? ChainLevel { get; set; }

        [Display(Name = "روش دریافت")]
        public int? ReceivingModeId { get; set; }

        public RecipientTypeEnum? ParentType { get; set; } // mapped to ParentId

        public DateTime? StartUp { get; set; } // simple date input

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public int? LeadTime { get; set; }
        public int? ClimateId { get; set; }
        public int? OwnershipId { get; set; }
        public int? PowerSourceId { get; set; }
        public int? TypeOfRoadId { get; set; }

        public bool HasLandLine { get; set; }
        public bool HasInternet { get; set; }
        public long? DistanceTime { get; set; }
    }
}
