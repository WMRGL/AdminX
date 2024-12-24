﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AdminX.Meta
{
    interface ICRUD
    {
        int CallStoredProcedure(string sType, string sOperation, int int1, int int2, int int3,
            string string1, string string2, string string3, string text, string sLogin,
            DateTime? dDate1 = null, DateTime? dDate2 = null, bool? bool1 = false, bool? bool2 = false,
            int? int4 = 0, int? int5 = 0, int? int6 = 0, string? string4 = "", string? string5 = "", string? string6 = "",
            float? f1 = 0, float? f2 = 0, float? f3 = 0, float? f4 = 0, float? f5 = 0, string? string7 = "");
        
        int ReferralDetail(string sType, string sOperation, string sLogin, int int1, string string1, string string2, string text, string? string3 = "",
       string? string4 = "", string? string5 = "", string? string6 = "", string? string7 = "", string? string8 = "", string? string9 = "",
       string? string10 = "", string? string11 = "", string? string12 = "", DateTime? dDate1 = null, DateTime? dDate2 = null, string? string13 = "", int? int2 = 0, int? int3 = 0, int? int4 = 0,
       int? int5 = 0, int? int6 = 0, int? int7 = 0,
             bool? bool1 = false, bool? bool2 = false, string? string44 = "");


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
            float? f1 = 0, float? f2 = 0, float? f3 = 0, float? f4 = 0, float? f5 = 0, string? string7 = "")
        {
            if (dDate1 == null) { dDate1 = DateTime.Parse("1900-01-01"); }
            if (dDate2 == null) { dDate2 = DateTime.Parse("1900-01-01"); }
            if (text == null) { text = ""; }
            if (string3 == null) { string3 = ""; }
            if (string4 == null) { string4 = ""; }
            if (string5 == null) { string5 = ""; }
            if (string6 == null) { string6 = ""; }



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
            if (string7 != "")
            {
                cmd.Parameters.Add("@string7", SqlDbType.VarChar).Value = string7;
            }
            cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int); //return success or not
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;
        }

        public int ReferralDetail(string sType, string sOperation, string sLogin, int int1, string string1, string string2, string text, string? string3 = "",
       string? string4 = "", string? string5 = "", string? string6 = "", string? string7 = "", string? string8 = "", string? string9 = "",
       string? string10 = "", string? string11 = "", string? string12 = "", DateTime? dDate1 = null, DateTime? dDate2 = null, string? string13 = "", int? int2 = 0, int? int3 = 0, int? int4 = 0,
       int? int5 = 0, int? int6 = 0, int? int7 = 0,
             bool? bool1 = false, bool? bool2 = false, string? string44 = "")
        {
            if (dDate1 == null) { dDate1 = DateTime.Parse("1900-01-01"); }
            if (dDate2 == null) { dDate2 = DateTime.Parse("1900-01-01"); }
            if (text == null) { text = ""; }
            if (string3 == null) { string3 = ""; }
            if (string4 == null) { string4 = ""; }
            if (string5 == null) { string5 = ""; }
            if (string6 == null) { string6 = ""; }

            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("dbo.sp_AxReferralCRUD", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ItemType", SqlDbType.VarChar).Value = sType;
            cmd.Parameters.Add("@Operation", SqlDbType.VarChar).Value = sOperation;
            cmd.Parameters.Add("@int1", SqlDbType.Int).Value = int1;
            cmd.Parameters.Add("@int2", SqlDbType.Int).Value = int2;
            cmd.Parameters.Add("@int3", SqlDbType.Int).Value = int3;
            cmd.Parameters.Add("@string1", SqlDbType.VarChar).Value = string1;
            cmd.Parameters.Add("@string2", SqlDbType.VarChar).Value = string2;
            cmd.Parameters.Add("@string3", SqlDbType.VarChar).Value = string3;
            cmd.Parameters.Add("@text", SqlDbType.VarChar).Value = text;
            cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = sLogin;
            cmd.Parameters.Add("@date1", SqlDbType.DateTime).Value = dDate1;
            cmd.Parameters.Add("@date2", SqlDbType.DateTime).Value = dDate2;
            cmd.Parameters.Add("@bool1", SqlDbType.Bit).Value = bool1;
            cmd.Parameters.Add("@bool2", SqlDbType.Bit).Value = bool2;
            cmd.Parameters.Add("@int4", SqlDbType.Int).Value = int4;
            cmd.Parameters.Add("@int5", SqlDbType.Int).Value = int5;
            cmd.Parameters.Add("@int6", SqlDbType.Int).Value = int6;
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
            cmd.Parameters.Add("@string44", SqlDbType.VarChar).Value = string44;

           // cmd.Parameters.Add("@machinename", SqlDbType.VarChar).Value = System.Environment.MachineName;
            var returnValue = cmd.Parameters.Add("@ReturnValue", SqlDbType.Int); 
            returnValue.Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();
            var iReturnValue = (int)returnValue.Value;
            conn.Close();

            return iReturnValue;


        }


    }
}
