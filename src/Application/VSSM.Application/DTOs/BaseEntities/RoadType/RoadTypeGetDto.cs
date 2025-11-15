using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.RoadType
{
    public record RoadTypeGetDto
    {
        public string Code { get; set; }

        public string FullName { get; set; }

        public bool IsActive { get; set; }
        public int RoadTypeId { get; set; }

    }
}
