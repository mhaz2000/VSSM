using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.ItemClass
{
    public class ItemClassCreateDto
    {
        public int ItemClassId { get; set; }
        public string? FullName { get; set; }

        public string? Code { get; set; }

        public bool ReqManufacturer { get; set; }

        public bool ReqBatchNo { get; set; }

        public bool ReqDosePerVial { get; set; }

        public bool ReqVVMType { get; set; }

        public bool ReqProdDate { get; set; }

        public bool? ReqExpireDate { get; set; }
        public string? UnitName { get; set; }
        public int OrderInList { get; set; }
        public bool IsActive { get; set; }
    }
}
