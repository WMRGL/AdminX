using AdminX.ViewModels;
using APIControllers.Controllers;
using APIControllers.Data;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;


namespace AdminX.Controllers
{
    public class PhenotipsController : Controller
    {
        private readonly ClinicalContext _clinContext;
        //private readonly DocumentContext _docContext;
        private readonly APIContext _apiContext;
        private readonly IConfiguration _config;
        private readonly IApiController _api;
        private readonly IPatientDataAsync _patientData;
        private readonly PhenotipsVM _pvm;
        private readonly LetterController _lc;

        public PhenotipsController(IConfiguration config, IApiController aPIController, IPatientDataAsync patientData, LetterController lc, APIContext aPIContext)
        {
            //_clinContext = clinicalContext;
            //_docContext = docContext;
            _apiContext = aPIContext;
            _config = config;
            _api = aPIController;
            _patientData = patientData;
            _pvm = new PhenotipsVM();
            _lc = lc;
        }

        [Authorize]
        public async Task<IActionResult> PushPatientToPt(int mpi)
        {
            string sMessage = "";
            bool isSuccess = false;

            //APIControllerLOCAL api = new APIControllerLOCAL(_apiContext, _config);

            Int16 result = await _api.PushPtToPhenotips(mpi); //initiates the push, returns 1 (success), 0 (already exists), or -1 (failed)

            if (result == 1)
            {
                string ptID = await _api.GetPhenotipsPatientID(mpi);
                Patient patient = await _patientData.GetPatientDetails(mpi);
                await AddPatientToPhenotipsMirrorTable(ptID, mpi, patient.CGU_No, patient.FIRSTNAME, patient.LASTNAME, patient.DOB.GetValueOrDefault(), patient.POSTCODE, patient.SOCIAL_SECURITY);
                isSuccess = true;
                sMessage = "Push to Phenotips successful";
            }
            else if (result == 0)
            {
                sMessage = "Patient already exists in Phenotips!";
            }
            else
            {
                sMessage = "Push to Phenotips failed :(";
            }

            return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = isSuccess, message = sMessage });
        }

        async Task AddPatientToPhenotipsMirrorTable(string ptID, int mpi, string cguno, string firstname, string lastname, DateTime DOB, string postCode, string nhsNo)
        {
            SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Insert into dbo.PhenotipsPatients (PhenotipsID, MPI, CGUNumber, FirstName, Lastname, DOB, PostCode, NHSNo) values('"
                + ptID + "', " + mpi + ", '" + cguno + "', '" + firstname + "', '" + lastname + "', '" + DOB.ToString("yyyy-MM-dd") + "', '" + postCode +
                "', '" + nhsNo + "')", conn);
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }

        [Authorize]
        public async Task<IActionResult> CreatePPQ(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;

            Int16 result = await _api.SchedulePPQ(mpi, pathway); //creates the PPQ, returns 1 (success), 0 (already exists), or -1 (failed)

            if (result == 1)
            {
                isSuccess = true;
                sMessage = $"{pathway} PPQ created successfully";
            }
            else if (result == 0)
            {
                sMessage = $"{pathway} PPQ is already scheduled";
            }
            else
            {
                sMessage = "PPQ creation failed :(";
            }

            return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = isSuccess, message = sMessage });
        }

        [Authorize]
        public String GetPPQURL(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;

            string result = _api.GetPPQUrl(mpi, pathway).Result; //fetches the URL

            return result.ToString();
        }

        [Authorize]
        public async Task<IActionResult> PhenotipsQRLink(int mpi, string? pathway)
        {
            string sMessage = "";
            bool isSuccess = false;

            string result = _api.GetPPQQRCode(mpi, pathway).Result;

            _pvm.ppqQR = result.ToString(); // fetches the QR code string

            return View(_pvm);
        }        

        [Authorize]
        public async Task<IActionResult> CreatePhenotipsEmail(int mpi, string pathway, bool isEmail)
        {
            Patient patient = await _patientData.GetPatientDetails(mpi);

            string ppqURL = _api.GetPPQUrl(mpi, pathway).Result; //fetches the URL

            if (isEmail)
            {
                if (patient.EmailAddress == null)
                {
                    return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI, message = "No email address recorded.", success = false });
                }
                if (!patient.EmailAddress.Contains("@"))
                {
                    return RedirectToAction("PatientDetails", "Patient", new { id = patient.MPI, message = "Not a valid email address.", success = false });
                }

                return Redirect("mailto:" + patient.EmailAddress + "?subject=Phenotips PPQ&body=Here is your Phenotips PPQ link:%0D%0A%0D%0A" + ppqURL + "%0D%0A%0D%0APlease complete this asap.");
            }
            else
            {
                return RedirectToAction("PhenotipsPPQLink", "Phenotips", new { mpi = patient.MPI, ppqLink = ppqURL });
            }
        }

        [Authorize]
        public async Task<IActionResult> PhenotipsPPQLink(int mpi, string ppqLink)
        {
            _pvm.mpi = mpi;
            _pvm.ppqURL = ppqLink;

            return View(_pvm); //returns the URL in a web page to copy/paste
        }
    }
}
