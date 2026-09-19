using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.Reports
{
    public class ExpiredItemsReportDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemDetailId { get; set; }
        public string BatchNumber { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }

        public long? TotalStock { get; set; }
        public long? DosePerVialCount { get; set; }
        public long? VialCount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CommercialName { get; set; }
    }
}
