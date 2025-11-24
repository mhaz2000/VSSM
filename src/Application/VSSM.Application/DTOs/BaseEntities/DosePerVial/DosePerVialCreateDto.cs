using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.DosePerVial
{
    public class DosePerVialCreateDto
    {
        public int DosePerVialId { get; set; }

        public int NumDosePerVial { get; set; }

        public bool? IsActive { get; set; }
    }
}
