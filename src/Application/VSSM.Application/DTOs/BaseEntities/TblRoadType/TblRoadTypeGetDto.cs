using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.TblRoadType
{
    public record TblRoadTypeGetDto
    {
        public string Code { get; set; }

        public string FullName { get; set; }

        public bool? IsActive { get; set; }
    }
}
