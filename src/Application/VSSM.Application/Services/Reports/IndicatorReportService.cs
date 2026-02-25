using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.Reports;
using VSSM.Infrastructure.Data.Dapper;

namespace VSSM.Application.Services.Reports
{
    public class IndicatorReportService
    {
        private readonly DapperContext _dapperContext;
        public IndicatorReportService(DapperContext _dapperContext)
        {
            this._dapperContext = _dapperContext;
        }

        public async Task<IEnumerable<IndicatorReportDto>> GetIndicatorReport(DateTime from, DateTime to)
        {
            using var connection = _dapperContext.CreateConnection();
            var result = await connection.QueryAsync<IndicatorReportDto>(
                "sp_GetIndicatorReport",
                new { From = from, To = to },
                commandType: CommandType.StoredProcedure);

            return result;
        }

    }
}
