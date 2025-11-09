using VSSM.Domain.Enums;

namespace VSSM.Presentation.Web.Models.ItemClass
{
    public class ItemClassViewModel
    {
        public int? ItemClassId { get; set; }
        public string FullName { get; set; }

        public string Code { get; set; }

        public bool ReqManufacturer { get; set; }

        public bool ReqBatchNo { get; set; }

        public bool ReqDosePerVial { get; set; }

        public bool ReqVVMType { get; set; }

        public bool ReqProdDate { get; set; }

        public bool ReqExpireDate { get; set; }
        //public Guid? UnitId { get; set; }
        //public string UnitName { get; set; }
        public UnitNameEnum UnitName { get; set; }
        public int OrderInList { get; set; }
        public bool IsActive { get; set; }
    }
}
