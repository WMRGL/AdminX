using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AdminX.Meta
{
    interface IAuditService
    {
        public void CreateUsageAuditEntry(string staffCode, string formName, string? searchTerm = "");
    }
    public class AuditService : IAuditService
    { 
        private readonly IConfiguration _config;

        public AuditService(IConfiguration config)
        {
            _config = config;
        }       

        public void CreateUsageAuditEntry(string staffCode, string formName, string? searchTerm = "")
        {   
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_CreateAudit", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            
            cmd.Parameters.Add("@staffCode", SqlDbType.VarChar).Value = staffCode;
            cmd.Parameters.Add("@form", SqlDbType.VarChar).Value = formName;
            cmd.Parameters.Add("@searchTerm", SqlDbType.VarChar).Value = searchTerm;
            cmd.Parameters.Add("@database", SqlDbType.VarChar).Value = "AdminX";
            cmd.Parameters.Add("@machine", SqlDbType.VarChar).Value = System.Environment.MachineName;
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
