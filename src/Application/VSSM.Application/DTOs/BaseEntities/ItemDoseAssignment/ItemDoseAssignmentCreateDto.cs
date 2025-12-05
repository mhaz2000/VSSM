using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.ItemDoseAssignment
{
    public class ItemDoseAssignmentCreateDto
    {
        public int ItemDoseAssignmentId { get; set; }

        public int? ItemId { get; set; }

        public int? DoseId { get; set; }

        public float? VialPerDoseCm3 { get; set; }
    }
}
