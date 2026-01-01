using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.RequestEntities
{
    public class RequestListDto
    {
        public int Id { get; set; }
        public int? EnteredBy { get; set; } = null;
        public DateOnly? SendDate { get; set; }
        public string RequestNumber { get; set; }
        public DateOnly? ConfirmDate { get; set; }
        public int? RequestStatus { get; set; }
    }
}
