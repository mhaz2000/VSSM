using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Domain.Entities
{
    public class ItemClass
    {
        public int ItemClassId { get; set; }

        public string Code { get; set; }

        public string FullName { get; set; }

        public DateTime? EntryDate { get; set; }

        public int? Orders { get; set; }

        public bool? IsActive { get; set; }

        public string VolumeType { get; set; }

        public bool? ReqManufacturer { get; set; }

        public bool? ReqBatchNo { get; set; }

        public bool? ReqExpiryDate { get; set; }

        public bool? ReqDosePerVial { get; set; }

        public bool? IsUserEditable { get; set; }

        public bool? ReqVvmtype { get; set; }
    }
}
