using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Reports
{
    public record InventoryQuantityReportDto
    {
        public string ItemClass { get; internal set; }
        public string Item { get; internal set; }
        public string Manufacturer { get; internal set; }
        public string BatchNumber { get; internal set; }
        public string Store { get; internal set; }
        public string ExpiryDate { get; internal set; }
        public int Quantity { get; internal set; }
    }
}
