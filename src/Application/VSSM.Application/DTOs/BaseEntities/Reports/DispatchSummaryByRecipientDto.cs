using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public class DispatchSummaryByRecipientDto
    {
        public string Recipient { get; set; }
        public string ItemClass { get; set; }
        public string Item { get; set; }
        public int TotalQuantity { get; set; }
        public int TotalNumVials { get; set; }
    }
}
