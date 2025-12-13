using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Arrival
{
    public class ArrivalCopyDto
    {
        public int ArrivalId { get; set; }
        public string DisplayText { get; set; } // FiscalPeriod + ReportNumber
    }

}
