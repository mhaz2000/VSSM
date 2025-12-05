using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Recipient
{
    public class RecipientEditDto : RecipientCreateDto
    {
        public int RecipientId { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
