using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Recipient
{
    public class RecipientCreateDto
    {
        public string Code { get; set; }
        public string FullName { get; set; }

        public bool IsActive { get; set; }

        public int? TransportModeId { get; set; }
        public long? Distance { get; set; }
        public long? Population { get; set; }
        public int? Year { get; set; }
        public string Note { get; set; }

        public int? ParentId { get; set; }      // RecipientTypeEnum as int
        public DateTime? StartUp { get; set; }
        public int? ChainLevel { get; set; }
        public int? ReceivingModeId { get; set; }

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
