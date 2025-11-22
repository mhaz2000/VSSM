using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Pack
{
    public class PackCreateDto
    {
        public string Code { get; set; }
        public string FullName { get; set; }
        public float? NetVolume { get; set; }
        public float? ExternalSize { get; set; }
        public float? PackWeight { get; set; }
        public bool IsActive { get; set; }
        public bool UseForVacDL { get; set; }
        public bool UseForOS { get; set; }
    }

}
