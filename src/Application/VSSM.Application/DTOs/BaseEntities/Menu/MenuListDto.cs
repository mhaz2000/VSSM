using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Menu
{
    public class MenuListDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public string Icon { get; set; }

        public int? ParentId { get; set; }

        public int? MenuOrder { get; set; }
    }
}
