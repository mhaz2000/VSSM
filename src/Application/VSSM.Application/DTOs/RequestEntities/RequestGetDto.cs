using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.Arrival;

namespace VSSM.Application.DTOs.RequestEntities
{
    public class RequestGetDto
    {
        public int Id { get; set; }
        public int? EnteredBy { get; set; } = null;
        public DateOnly? SendDate { get; set; }
        public string RequestNumber { get; set; }
        public DateOnly? ConfirmDate { get; set; }
        public int? RequestStatus { get; set; }

        public List<RequestLineDto> RequestLines { get; set; } = new();
    }
}
