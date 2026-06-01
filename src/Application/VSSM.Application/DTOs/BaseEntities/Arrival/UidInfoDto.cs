using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Arrival;

public class UidInfoDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; }
    public int? ArrivalFkId { get; set; }
    public int? ItemFkId { get; set; }
    public int? StoreCurrent { get; set; }
    public int? DoseCount { get; set; }
}
