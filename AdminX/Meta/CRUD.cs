using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;

namespace AdminX.Meta
{
    public interface ICRUD
    {
        public int CallStoredProcedure(string sType, string sOperation, int int1, int int2, int int3,
            string string1, string string2, string string3, string text, string sLogin,
            DateTime? dDate1 = null, DateTime? dDate2 = null, bool? bool1 = false, bool? bool2 = false,
            int? int4 = 0, int? int5 = 0, int? int6 = 0, string? string4 = "", string? string5 = "", string? string6 = "",
            float? f1 = 0, float? f2 = 0, float? f3 = 0, float? f4 = 0, float? f5 = 0, string? string7 = "", string? string8 = "",
            string? string9 = "", string? string10 = "", string? string11 = "", string? string12 = "", string? string13 = "", string? string14 = "",
            string? string15 = "", string? string16 = "", string? string17 = "", string? string18 = "", string? string19 = "", string? string20 = "",
            string? string21 = "");

        public int PatientDetail(string sType, string sOperation, string sLogin, int int1, string string1, string string2, string text, string? string3 = "",
       string? string4 = "", string? string5 = "", string? string6 = "", string? string7 = "", string? string8 = "", string? string9 = "",
       string? string10 = "", string? string11 = "", string? string12 = "", DateTime? dDate1 = null, DateTime? dDate2 = null, string? string13 = "",
        bool? bool1 = false, bool? bool2 = false, string? string14 = "", string? string15 = "", string? string16 = "", string string17 = "", string string18 = "", 
        string? string19 = "", string? string20 = "", string? string21 = "", string? string22 = "", string? string23 = "", string? string24 = "", string? string25 = ""
            );

        public int ReferralDetail(string sType, string sOperation, string sLogin, int int1, int? int2, int? int3, int? int4, int? int5, int? int6, int? int7,
            int? int8, string string1, string string2, string text, string? string3 = "", string? string4 = "", string? string5 = "", string? string6 = "",
            string? string7 = "", string? string8 = "", string? string9 = "", string? string10 = "", string? string11 = "", string? string12 = "",
            DateTime? dDate1 = null, DateTime? dDate2 = null, string? string13 = "", string? string14 = "", string? string15 = "", string? string16 = "",
            string? string17 = "", string? string18 = "", bool? bool1 = false, bool? bool2 = false);

        public int TriageDetail(string sType, string sOperation, int int1, int int2, int int3, string? string1, string? string2, string? string3, string? string4, string sLogin,
       string? string5 = "", string? string6 = "", string? string7 = "", int? int4 = 0, int? int5 = 0, bool? bool1 = false, bool? bool2 = false, bool? bool3 = false);

        public int SysAdminCRUD(string sType, string sOperation, int int1, int int2, int int3, string? string1, string? string2, string? string3, string? text, string sLogin,
        DateTime? dDate1 = null, DateTime? dDate2 = null, bool? bool1 = false, bool? bool2 = false, bool? bool3 = false, int? int4 = 0, int? int5 = 0, int? int6 = 0, string? string4 = "", string?
            string5 = "", string? string6 = "", string? string7 = "", string? string8 = "", string? string9 = "", string? string10 = "", string? string11 = "",
        string? string12 = "", string? string13 = "", string? string14 = "", string? string15 = "", string? string16 = "", string? string17 = "", string? string18 = "",
        string? string19 = "", string? string20 = ""); //the Clinic Setup needs a ridiculous number of strings!!!

        public int AddToWaitingList(int mpi, string clinicianID, string clinicID, int priorityLevel, int refID, string username);

        public void NewPatientSearch(string firstName, string lastName, DateTime dob, string postCode, string nhsNo, string staffCode);

        public int PatientReview(string sType, string sOperation, string sLogin, int int1, string string1, string string2, string? string3 = "",
       string? string4 = "", string? string6 = "", string? string7 = "", string? string8 = "", DateTime? dDate1 = null,  int? int2 = 0);
        public int MergePatient(int mpiFrom, int mpiTo, string staffCode);

        public void AddPatientToPhenotipsMirrorTable(string ptID, int mpi, string cguno, string firstname, string lastname, DateTime DOB, string postCode, string nhsNo);

        public Task<int> PatientAssignCGUNumber(int mpi, string cguno, string sLogin);

        public int EpicReferralStaging(int id, string epicPatID, int epicRefID, DateTime referralDate, string? refBy, string? refTo, string? speciality, string? pathway,
            string? refStatus, DateTime createdDate);

        public int EpicAcceptChanges(int mpi, string epicID, string sLogin, string itemType, int? refID = 0);
    }


    public class CRUD : ICRUD //CRUD stands for "create-update-delete", and contains the call to the SQL stored procedure that handles all
                              //data modifications - creation, updates, and deletions. It does not retrieve any data, but uses a generic
                              //list of integers, strings, dates, and booleans that are passed to it.
    {
        private readonly IConfiguration _config;

        public CRUD(IConfiguration config)
        {
            _config = config;
        }

        public int CallStoredProcedure(string sType, string sOperation, int int1, int int2, int int3,
            string string1, string string2, string string3, string text, string sLogin,
            DateTime? dDate1 = null, DateTime? dDate2 = null, bool? bool1 = false, bool? bool2 = false,
            int? int4 = 0, int? int5 = 0, int? int6 = 0, string? string4 = "", string? string5 = "", string? string6 = "",
            float? f1 = 0, float? f2 = 0, float? f3 = 0, float? f4 = 0, float? f5 = 0, string? string7 = "", string? string8 = "",
            string? string9 = "", string? string10 = "", string? string11 = "", string? string12 = "", string? string13 = "", string? string14 = "",
            string? string15 = "", string? string16 = "", string? string17 = "", string? string18 = "", string? string19 = "", string? string20 = "",
            string? string21 = "")
        {
            if (dDate1 == null) { dDate1 = DateTime.Parse("1900-01-01"); }
            if (dDate2 == null || dDate2 == DateTime.Parse("0001-01-01")) { dDate2 = DateTime.Parse("1900-01-01"); }
            if (text == null) { text = ""; }
            if (string3 == null) { string3 = ""; }
            

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AXCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType; //thing you want to create/update (Clinic, review, letter etc)
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation; //task you want to perform (create, update, etc)
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@int2", SqlDbType.Int).Value = int2;
            cmd.Parameters.Add("@int3", SqlDbType.Int).Value = int3;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1; //varchar 30 - for small text fields
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2; //varchar 750 - for stuff that has more text
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3; //varchar max - for those who need to write War and Peace
            cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = text; //varchar max - to be used for long text
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin; //login name of user
            //Optional parameters - will pass default value to SQL sp if not supplied
            cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = dDate1;
            cmd.Parameters.Add("@date2", SqlDbType.DateTime).Value = dDate2;
            cmd.Parameters.Add("@bool1", SqlDbType.VarChar).Value = bool1;
            cmd.Parameters.Add("@bool2", SqlDbType.VarChar).Value = bool2;
            cmd.Parameters.Add("@int4", SqlDbType.Int).Value = int4;
            cmd.Parameters.Add("@int5", SqlDbType.Int).Value = int5;
            cmd.Parameters.Add("@int6", SqlDbType.Int).Value = int6;
            cmd.Parameters.Add("@string4", SqlDbType.VarChar).Value = string4;
            cmd.Parameters.Add("@string5", SqlDbType.VarChar).Value = string5;
            cmd.Parameters.Add("@string6", SqlDbType.VarChar).Value = string6;
            cmd.Parameters.Add("@float1", SqlDbType.Float).Value = f1;
            cmd.Parameters.Add("@float2", SqlDbType.Float).Value = f2;
            cmd.Parameters.Add("@float3", SqlDbType.Float).Value = f3;
            cmd.Parameters.Add("@float4", SqlDbType.Float).Value = f4;
            cmd.Parameters.Add("@float5", SqlDbType.Float).Value = f5;
            cmd.Parameters.Add("@string7", SqlDbType.VarChar).Value = string7;
            cmd.Parameters.Add("@string8", SqlDbType.VarChar).Value = string8;
            cmd.Parameters.Add("@string9", SqlDbType.VarChar).Value = string9;
            cmd.Parameters.Add("@string10", SqlDbType.VarChar).Value = string10;
            cmd.Parameters.Add("@string11", SqlDbType.VarChar).Value = string11;
            cmd.Parameters.Add("@string12", SqlDbType.VarChar).Value = string12;
            cmd.Parameters.Add("@string13", SqlDbType.VarChar).Value = string13;
            cmd.Parameters.Add("@string14", SqlDbType.VarChar).Value = string14;
            cmd.Parameters.Add("@string15", SqlDbType.VarChar).Value = string15;
            cmd.Parameters.Add("@string16", SqlDbType.VarChar).Value = string16;
            cmd.Parameters.Add("@string17", SqlDbType.VarChar).Value = string17;
            cmd.Parameters.Add("@string18", SqlDbType.VarChar).Value = string18;
            cmd.Parameters.Add("@string19", SqlDbType.VarChar).Value = string19;
            cmd.Parameters.Add("@string20", SqlDbType.VarChar).Value = string20;
            cmd.Parameters.Add("@string21", SqlDbType.VarChar).Value = string21;

            cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int); //return success or not
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue; 
        }

        public int PatientDetail(string sType, string sOperation, string sLogin, int int1, string string1, string string2, string text, string? string3 = "",
       string? string4 = "", string? string5 = "", string? string6 = "", string? string7 = "", string? string8 = "", string? string9 = "",
       string? string10 = "", string? string11 = "", string? string12 = "", DateTime? dDate1 = null, DateTime? dDate2 = null, string? string13 = "",
        bool? bool1 = false, bool? bool2 = false, string? string14 = "", string? string15 = "", string? string16 = "", string string17 = "", string string18 = "",
        string? string19 = "", string? string20 = "", string? string21 = "", string? string22 = "", string? string23 = "", string? string24 = "", string? string25 = ""
        )
        {
            if (dDate1 == null) { dDate1 = DateTime.Parse("1900-01-01"); }
            if (dDate2 == null) { dDate2 = DateTime.Parse("1900-01-01"); }

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AxPatientCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType;
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation;
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1;
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2;
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3;
            cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = text;
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = dDate1;
            cmd.Parameters.Add("@date2", SqlDbType.DateTime).Value = dDate2;
            cmd.Parameters.Add("@bool1", SqlDbType.Bit).Value = bool1;
            cmd.Parameters.Add("@bool2", SqlDbType.Bit).Value = bool2;
            cmd.Parameters.Add("@string4", SqlDbType.VarChar).Value = string4;
            cmd.Parameters.Add("@string5", SqlDbType.VarChar).Value = string5;
            cmd.Parameters.Add("@string6", SqlDbType.VarChar).Value = string6;
            cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = Dns.GetHostName();
            cmd.Parameters.Add("@string7", SqlDbType.VarChar).Value = string7;
            cmd.Parameters.Add("@string8", SqlDbType.VarChar).Value = string8;
            cmd.Parameters.Add("@string9", SqlDbType.VarChar).Value = string9;
            cmd.Parameters.Add("@string10", SqlDbType.VarChar).Value = string10;
            cmd.Parameters.Add("@string11", SqlDbType.VarChar).Value = string11;
            cmd.Parameters.Add("@string12", SqlDbType.VarChar).Value = string12;
            cmd.Parameters.Add("@string13", SqlDbType.VarChar).Value = string13;
            cmd.Parameters.Add("@string14", SqlDbType.VarChar).Value = string14;
            cmd.Parameters.Add("@string15", SqlDbType.VarChar).Value = string15;
            cmd.Parameters.Add("@string16", SqlDbType.VarChar).Value = string16;
            cmd.Parameters.Add("@string17", SqlDbType.VarChar).Value = string17;
            cmd.Parameters.Add("@string18", SqlDbType.VarChar).Value = string18;
            cmd.Parameters.Add("@string19", SqlDbType.VarChar).Value = string19;
            cmd.Parameters.Add("@string20", SqlDbType.VarChar).Value = string20;
            cmd.Parameters.Add("@string21", SqlDbType.VarChar).Value = string21;
            cmd.Parameters.Add("@string22", SqlDbType.VarChar).Value = string22;
            cmd.Parameters.Add("@string23", SqlDbType.VarChar).Value = string23;
            cmd.Parameters.Add("@string24", SqlDbType.VarChar).Value = string24;
            cmd.Parameters.Add("@string25", SqlDbType.VarChar).Value = string25;

            // cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;
        }

        public int ReferralDetail(string sType, string sOperation, string sLogin, int int1, int? int2, int? int3, int? int4, int? int5, int? int6, int? int7,
            int? int8, string string1, string string2, string text, string? string3 = "", string? string4 = "", string? string5 = "", string? string6 = "",
            string? string7 = "", string? string8 = "", string? string9 = "", string? string10 = "", string? string11 = "", string? string12 = "",
            DateTime? dDate1 = null, DateTime? dDate2 = null, string? string13 = "", string? string14 = "", string? string15 = "", string? string16 = "",
            string? string17 = "", string? string18 = "", bool? bool1 = false, bool? bool2 = false)
        {
            if (dDate1 == null) { dDate1 = DateTime.Parse("1900-01-01"); }
            if (dDate2 == null) { dDate2 = DateTime.Parse("1900-01-01"); }
            if (text == null) { text = ""; }


            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AxReferralCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType;
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation;
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@int2", SqlDbType.Int).Value = int2;
            cmd.Parameters.Add("@int3", SqlDbType.Int).Value = int3;
            cmd.Parameters.Add("@int4", SqlDbType.Int).Value = int4;
            cmd.Parameters.Add("@int5", SqlDbType.Int).Value = int5;
            cmd.Parameters.Add("@int6", SqlDbType.Int).Value = int6;
            cmd.Parameters.Add("@int7", SqlDbType.Int).Value = int7;
            cmd.Parameters.Add("@int8", SqlDbType.Int).Value = int8;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1;
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2;
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3;
            cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = text;
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = dDate1;
            cmd.Parameters.Add("@date2", SqlDbType.DateTime).Value = dDate2;
            cmd.Parameters.Add("@bool1", SqlDbType.Bit).Value = bool1;
            cmd.Parameters.Add("@bool2", SqlDbType.Bit).Value = bool2;
            cmd.Parameters.Add("@string4", SqlDbType.VarChar).Value = string4;
            cmd.Parameters.Add("@string5", SqlDbType.VarChar).Value = string5;
            cmd.Parameters.Add("@string6", SqlDbType.VarChar).Value = string6;
            cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            cmd.Parameters.Add("@string7", SqlDbType.VarChar).Value = string7;
            cmd.Parameters.Add("@string8", SqlDbType.VarChar).Value = string8;
            cmd.Parameters.Add("@string9", SqlDbType.VarChar).Value = string9;
            cmd.Parameters.Add("@string10", SqlDbType.VarChar).Value = string10;
            cmd.Parameters.Add("@string11", SqlDbType.VarChar).Value = string11;
            cmd.Parameters.Add("@string12", SqlDbType.VarChar).Value = string12;
            cmd.Parameters.Add("@string13", SqlDbType.VarChar).Value = string13;
            cmd.Parameters.Add("@string14", SqlDbType.VarChar).Value = string14;
            cmd.Parameters.Add("@string15", SqlDbType.VarChar).Value = string15;
            cmd.Parameters.Add("@string16", SqlDbType.VarChar).Value = string16;
            cmd.Parameters.Add("@string17", SqlDbType.VarChar).Value = string17;
            cmd.Parameters.Add("@string18", SqlDbType.VarChar).Value = string18;

            // cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;
        }

        public int PatientReview(string sType, string sOperation, string sLogin, int int1, string string1, string string2, string? string3 = "",
       string? string4 = "", string? string6 = "", string? string7 = "", string? string8 = "", DateTime? dDate1 = null, int? int2 = 0)
        {

            if (dDate1 == null) { dDate1 = DateTime.Parse("1900-01-01"); }


            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AxReviewCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType;
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation;
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@int2", SqlDbType.Int).Value = int2;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1;
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2;
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3;
            cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = dDate1;
            cmd.Parameters.Add("@string4", SqlDbType.VarChar).Value = string4;
            cmd.Parameters.Add("@string6", SqlDbType.VarChar).Value = string6;
            //cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            cmd.Parameters.Add("@string7", SqlDbType.VarChar).Value = string7;
            cmd.Parameters.Add("@string8", SqlDbType.VarChar).Value = string8;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;
        }


        public int TriageDetail(string sType, string sOperation, int int1, int int2, int int3, string? string1, string? string2, string? string3, string? string4, string sLogin,
       string? string5 = "", string? string6 = "", string? string7 = "", int? int4 = 0, int? int5 = 0, bool? bool1 = false, bool? bool2 = false, bool? bool3 = false)
        {

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AxTriageCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType;
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation;
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@int2", SqlDbType.Int).Value = int2;
            cmd.Parameters.Add("@int3", SqlDbType.Int).Value = int3;
            cmd.Parameters.Add("@int4", SqlDbType.Int).Value = int4;
            cmd.Parameters.Add("@int5", SqlDbType.Int).Value = int5;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1;
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2;
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3;
            cmd.Parameters.Add("@string4", SqlDbType.VarChar).Value = string4;
            cmd.Parameters.Add("@string5", SqlDbType.VarChar).Value = string5;
            cmd.Parameters.Add("@string6", SqlDbType.VarChar).Value = string6;
            cmd.Parameters.Add("@string7", SqlDbType.VarChar).Value = string7;
            cmd.Parameters.Add("@bool1", SqlDbType.Bit).Value = bool1;
            cmd.Parameters.Add("@bool2", SqlDbType.Bit).Value = bool2;
            cmd.Parameters.Add("@bool3", SqlDbType.Bit).Value = bool3;
            cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;
        }


        public int SysAdminCRUD(string sType, string sOperation, int int1, int int2, int int3, string? string1, string? string2, string? string3, string? text, string sLogin,
        DateTime? dDate1 = null, DateTime? dDate2 = null, bool? bool1 = false, bool? bool2 = false, bool? bool3 = false, int? int4 = 0, int? int5 = 0, int? int6 = 0, string? string4 = "", string?
            string5 = "", string? string6 = "", string? string7 = "", string? string8 = "", string? string9 = "", string? string10 = "", string? string11 = "",
        string? string12 = "", string? string13 = "", string? string14 = "", string? string15 = "", string? string16 = "", string? string17 = "", string? string18 = "",
        string? string19 = "", string? string20 = "")
        {
            if (dDate1 == null) { dDate1 = DateTime.Parse("1900-01-01"); }
            if (dDate2 == null || dDate2 == DateTime.Parse("0001-01-01")) { dDate2 = DateTime.Parse("1900-01-01"); }

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AXSysAdminCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType;
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation;
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@int2", SqlDbType.Int).Value = int2;
            cmd.Parameters.Add("@int3", SqlDbType.Int).Value = int3;
            cmd.Parameters.Add("@int4", SqlDbType.Int).Value = int4;
            cmd.Parameters.Add("@int5", SqlDbType.Int).Value = int5;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1;
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2;
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3;
            cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = text;
            cmd.Parameters.Add("@string4", SqlDbType.VarChar).Value = string4;
            cmd.Parameters.Add("@string5", SqlDbType.VarChar).Value = string5;
            cmd.Parameters.Add("@string6", SqlDbType.VarChar).Value = string6;
            cmd.Parameters.Add("@string7", SqlDbType.VarChar).Value = string7;
            cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = dDate1;
            cmd.Parameters.Add("@date2", SqlDbType.DateTime).Value = dDate2;
            cmd.Parameters.Add("@bool1", SqlDbType.Bit).Value = bool1;
            cmd.Parameters.Add("@bool2", SqlDbType.Bit).Value = bool2;
            cmd.Parameters.Add("@bool3", SqlDbType.Bit).Value = bool3;
            cmd.Parameters.Add("@string8", SqlDbType.VarChar).Value = string8;
            cmd.Parameters.Add("@string9", SqlDbType.VarChar).Value = string9;
            cmd.Parameters.Add("@string10", SqlDbType.VarChar).Value = string10;
            cmd.Parameters.Add("@string11", SqlDbType.VarChar).Value = string11;
            cmd.Parameters.Add("@string12", SqlDbType.VarChar).Value = string12;
            cmd.Parameters.Add("@string13", SqlDbType.VarChar).Value = string13;
            cmd.Parameters.Add("@string14", SqlDbType.VarChar).Value = string14;
            cmd.Parameters.Add("@string15", SqlDbType.VarChar).Value = string15;
            cmd.Parameters.Add("@string16", SqlDbType.VarChar).Value = string16;
            cmd.Parameters.Add("@string17", SqlDbType.VarChar).Value = string17;
            cmd.Parameters.Add("@string18", SqlDbType.VarChar).Value = string18;
            cmd.Parameters.Add("@string19", SqlDbType.VarChar).Value = string19;
            cmd.Parameters.Add("@string20", SqlDbType.VarChar).Value = string20;
            cmd.Parameters.Add("@int6", SqlDbType.Int).Value = int6;
            cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;
        }


        public int AddToWaitingList(int mpi, string clinicianID, string clinicID, int priorityLevel, int refID, string username)
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_ClinicPlannerCreateWaitingListEntry", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@MPI", SqlDbType.Int).Value = mpi;
            cmd.Parameters.Add("@ClinicianID", SqlDbType.VarChar).Value = clinicianID;
            cmd.Parameters.Add("@ClinicID", SqlDbType.VarChar).Value = clinicID;
            cmd.Parameters.Add("@priorityLevel", SqlDbType.Int).Value = priorityLevel;
            cmd.Parameters.Add("@refID", SqlDbType.Int).Value = refID;
            cmd.Parameters.Add("@Username", SqlDbType.VarChar).Value = username;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;
        }

        public void NewPatientSearch(string firstName, string lastName, DateTime dob, string postCode, string nhsNo, string staffCode)
        {
            string safeFirst = (firstName ?? "").Replace("'", "''");
            string safeLast = (lastName ?? "").Replace("'", "''");

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();

            SqlCommand cmd = new SqlCommand("Insert into dbo.PatientSearches (Firstname, Lastname, DOB, PostCode, NHSNo, SearchBy, SearchDate, SearchType) values ('"
                + safeFirst + "', '" + safeLast + "', '" + dob.ToString("yyyy-MM-dd") + "', '" + postCode + "', '" + nhsNo + "', '" + staffCode + "', '" +
                DateTime.Today.ToString("yyyy-MM-dd") + "', 'NewPatientSearch')", conn);

            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public int MergePatient(int mpiFrom, int mpiTo, string staffCode)
        {
            int success = 0;

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Insert into dbo.PatientMerges (mpi_from, mpi_to, DateStamp, Status, LoggedOnUser) values ("
                + mpiFrom + ", " + mpiTo + ", '" + DateTime.Today.ToString("yyyy-MM-dd") + "', 0, '" + staffCode + "')", conn);
            Console.WriteLine(cmd.CommandText);
            cmd.ExecuteNonQuery();
            conn.Close();
            success = 1;

            return success;
        }

        public void AddPatientToPhenotipsMirrorTable(string ptID, int mpi, string cguno, string firstname, string lastname, DateTime DOB, string postCode, string nhsNo)
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Insert into dbo.PhenotipsPatients (PhenotipsID, MPI, CGUNumber, FirstName, Lastname, DOB, PostCode, NHSNo) values('"
                + ptID + "', " + mpi + ", '" + cguno + "', '" + firstname + "', '" + lastname + "', '" + DOB.ToString("yyyy-MM-dd") + "', '" + postCode +
                "', '" + nhsNo + "')", conn);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public async Task<int> PatientAssignCGUNumber(int mpi, string cguno, string sLogin)
        {
            int success = 0;

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AxPatientCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = "Patient";
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = "AssignCGUNumber";
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = mpi;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = cguno;
            cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;            
            conn.Close();
            success = iReturnValue;
            return success;
        }

        public int EpicReferralStaging(int id, string epicPatID, int epicRefID, DateTime referralDate, string? refBy, string? refTo, string? speciality, string? pathway, 
            string? refStatus, DateTime createdDate)
        {
            int success = 0;
            if(refBy == null) { refBy = ""; }
            if (refTo == null) { refTo = ""; }
            if (speciality == null) { speciality = ""; }
            if (pathway == null) { pathway = ""; }
            if (createdDate == null || createdDate == DateTime.Parse("0001-01-01")) { createdDate = DateTime.Parse("1900-01-01"); }
            if (referralDate == null || referralDate == DateTime.Parse("0001-01-01")) { referralDate = DateTime.Parse("1900-01-01"); }

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_DownstreamRefStagingTableInsert", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@PatientID", SqlDbType.VarChar).Value = epicPatID;
            cmd.Parameters.Add("@ReferralID", SqlDbType.Int).Value = epicRefID;
            cmd.Parameters.Add("@ReferralDate", SqlDbType.DateTime).Value = referralDate;
            cmd.Parameters.Add("@ReferredBy", SqlDbType.VarChar).Value = refBy;
            cmd.Parameters.Add("@ReferredTo", SqlDbType.VarChar).Value = refTo;
            cmd.Parameters.Add("@CreatedDate", SqlDbType.DateTime).Value = createdDate;
            cmd.Parameters.Add("@Speciality", SqlDbType.VarChar).Value = speciality;
            cmd.Parameters.Add("@Pathway", SqlDbType.VarChar).Value = pathway;            
            cmd.Parameters.Add("@RefStatus", SqlDbType.VarChar).Value = refStatus;

            cmd.ExecuteNonQuery();            
            conn.Close();

            success = 1;

            return success;
        }

        public int EpicAcceptChanges(int mpi, string epicID, string sLogin, string itemType, int? refID = 0)
        {
            int success = 0;

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AXAcceptEpicChange", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@mpi", SqlDbType.Int).Value = mpi;
            cmd.Parameters.Add("@epicID", SqlDbType.VarChar).Value = epicID;
            cmd.Parameters.Add("@RefID", SqlDbType.Int).Value = refID;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = itemType;
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int);
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();
            success = iReturnValue;
            
            return success;           
        }
    }
}
