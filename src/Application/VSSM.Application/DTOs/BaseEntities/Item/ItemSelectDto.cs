using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Item
{
    public class ItemSelectDto
    {
        public int ItemId { get; set; }

        public int? ItemClassId { get; set; }

        public string Code { get; set; }

        public string FullName { get; set; }

        public float? Volume { get; set; }

        public bool? UserAboveZeroStore { get; set; }

        public bool? UserBelowZeroStore { get; set; }

        public bool? UseOrdinaryStore { get; set; }

        public int? DayToWarmExpire { get; set; }

        public bool? RequiresDiluent { get; set; }

        public int? DiluentId { get; set; }

        public bool? IsActive { get; set; }

        public float? UnitPrice { get; set; }

        public string UniCode { get; set; }

        public int? OrderId { get; set; }
    }
}
