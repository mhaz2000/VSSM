using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Domain.Enums;

namespace VSSM.Application.DTOs.RequestEntities
{
    public class RequestCreateDto
    {
        public int Id { get; set; }

        public int? SenderId { get; set; }

        public DateOnly? SendDate { get; set; }

        public RequestStatusEnum Status { get; set; }

        public int? ReceiverId { get; set; }

        public int? EnteredBy { get; set; }

        public string Note { get; set; }

        public DateOnly? ConfirmDate { get; set; }

        public string RequestNumber { get; set; }

        public int? FiscalPeriodId { get; set; }

        public List<RequestLineDto> Lines { get; set; } = new();
    }
}
