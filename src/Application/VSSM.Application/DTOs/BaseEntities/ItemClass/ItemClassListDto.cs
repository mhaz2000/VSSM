using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.ItemClass
{
    public class ItemClassListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string UnitName { get; set; }
        public int OrderInList { get; set; }
        public bool IsActive { get; set; }
    }
}
