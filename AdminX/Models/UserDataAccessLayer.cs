using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace AdminX.Models
{
    public class UserDataAccessLayer : Controller //used for user login, has no function beyond this.
    {
        public static IConfiguration Configuration { get; set; }
        private static string GetConnectionString()
        {
            var loginBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddJsonFile("appsettings.json")
                .AddJsonFile("secrets.json");

            Configuration = loginBuilder.Build();
            string connectionString = Configuration["ConnectionStrings:ConString"];
           
            return connectionString;
        }

        string connectionString = GetConnectionString();


        public string ValidateLogin(UserDetails user)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_ValidateUserLogin", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@LoginID", user.EMPLOYEE_NUMBER);
                cmd.Parameters.AddWithValue("@LoginPassword", user.PASSWORD);
                
                con.Open();
                string result = cmd.ExecuteScalar().ToString();
                con.Close();
                
                return result;
            }
        }
    }
}
