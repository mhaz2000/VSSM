using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace VSSM.Infrastructure.Data.Dapper
{

    public class DapperContext
    {
        private readonly IConfiguration _config;

        public DapperContext(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_config.GetConnectionString("VSSMDbConnectionStrings"));
    }
}
