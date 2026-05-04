using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.Store;
using VSSM.Application.DTOs.Reports;

namespace VSSM.Application.Interfaces.Reports
{
    public interface IPeripheralReportService
    {
        Task<(List<ExpiredItemsReportDto> data, int count)> ExpiredItemsReport(
            int currentRecipientId,
            int pageNumber,
            int pageSize,
            string sort = "Item",
            string direction = "asc",
            CancellationToken cancellationToken = default);
    }
}
