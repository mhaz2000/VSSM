using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs.BaseEntities.Dispatches;
public class DispatchEditLineDto
{
    public int DispatchLineId { get; set; }
    public int ItemDetailStoreId { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string BatchNumber { get; set; }
    public string ExpiryDateShamsi { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int StoreId { get; set; }
    public string StoreName { get; set; }
    public string ManufacturerName { get; set; }
    public int DoseCount { get; set; }
    public int NumVial { get; set; }
    public int VvmStageId { get; set; }
    public string VvmStageName { get; set; }
    public string Note { get; set; }
    public long CurrentStock { get; set; }

    public List<string> Uids { get; set; } = new();
}
 