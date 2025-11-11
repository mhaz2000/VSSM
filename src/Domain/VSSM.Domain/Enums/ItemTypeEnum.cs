using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Domain.Enums
{
    public enum ItemTypeEnum
    {
        [Description("واکسن")]
        Vaccine,
        [Description("حلال")]
        Solvent
    }
}
