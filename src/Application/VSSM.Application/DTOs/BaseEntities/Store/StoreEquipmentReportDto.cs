using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Store
{
    public class StoreEquipmentReportDto
    {
        public string StoreName { get; set; }
        public string StoreType { get; set; }
        public float? Capacity { get; set; }
        public float? TotalCapacity { get; set; }
        public int? Year { get; set; }
        public bool? IsActive { get; set; }
    }
}
