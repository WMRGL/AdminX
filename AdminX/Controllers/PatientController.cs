﻿using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using AdminX.Models;
using ClinicalXPDataConnections.Meta;
using RestSharp;
using Newtonsoft.Json.Linq;
using ClinicalXPDataConnections.Models;
using AdminX.Meta;
using System;
using AdminX.Data;

namespace AdminX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _documentContext;
        private readonly PatientVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly ITitleData _titleData;
        private readonly IEthnicityData _ethnicityData;
        private readonly IRelativeData _relativeData;
        private readonly IPathwayData _pathwayData;
        private readonly IAlertData _alertData;
        private readonly IReferralData _referralData;
        private readonly IDiaryData _diaryData;
        private readonly IExternalClinicianData _gpData;
        private readonly IExternalFacilityData _gpPracticeData;
        private readonly IAuditService _audit;
        private readonly IConstantsData _constants;
        private readonly ICRUD _crud;
        private readonly AdminContext _adminContext;
        private readonly ILanguageData _languageData;
        private readonly IPatientAlertData _patientAlertData;

        public PatientController(ClinicalContext context, DocumentContext documentContext, IConfiguration config, AdminContext adminContext)
        {
            _clinContext = context;
            _documentContext = documentContext;
            _adminContext = adminContext;
            _config = config;
            _crud = new CRUD(_config);
            _pvm = new PatientVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _pathwayData = new PathwayData(_clinContext);
            _alertData = new AlertData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _gpData = new ExternalClinicianData(_clinContext);
            _gpPracticeData = new ExternalFacilityData(_clinContext);
            _audit = new AuditService(_config);
            _constants = new ConstantsData(_documentContext);
            _languageData = new LanguageData(_adminContext);
            _patientAlertData = new PateintAlertData(_clinContext);
                }


        [Authorize]
        public async Task<IActionResult> PatientDetails(int id)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "MPI=" + id.ToString());

                _pvm.patient = _patientData.GetPatientDetails(id);
                if (_pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                _pvm.relatives = _relativeData.GetRelativesList(id).Distinct().ToList();
                _pvm.referrals = _referralData.GetReferralsList(id);
                _pvm.patientPathway = _pathwayData.GetPathwayDetails(id);
                _pvm.alerts = _alertData.GetAlertsList(id);
                _pvm.diary = _diaryData.GetDiaryList(id);
                _pvm.languages  = _languageData.GetLanguages();
                _pvm.protectedAddress = _patientAlertData.GetProtectedAdress(id);
                
               
                if (_pvm.patient.DECEASED == -1)
                {
                    _pvm.diedage = CalculateDiedAge(_pvm.patient.DOB.Value, _pvm.patient.DECEASED_DATE.Value);
                }
                if (_pvm.patient.DOB != null)
                {
                    _pvm.currentage = CalculateAge(_pvm.patient.DOB.Value);
                }
                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientDetails(bool? DECEASED, DateTime? DECEASED_DATE, int mpi)
        {
            string deceased = Request.Form["DECEASED"];
            string interpreterRequired = Request.Form["IsInterpreterReqd"];
            string language = Request.Form["LanguageName"];
            bool deseasedStatus = false;
            bool interpreter = false;
            if (deceased == "on")
            {
                deseasedStatus = true;
            } 
            
            if (interpreterRequired == "on")
            {
                interpreter = true;
            }

            if (language == null)
            {
                _pvm.patient = _patientData.GetPatientDetails(mpi);
                language = _pvm.patient.PrimaryLanguage;
            }

            _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Update");

            int success = _crud.CallStoredProcedure("Patient", "Update", mpi, 0, 0, "", language, "", "", User.Identity.Name, DECEASED_DATE, null, deseasedStatus, interpreter);


            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }

            return RedirectToAction("PatientDetails", new { id = mpi });

        }
        public static string CalculateAge(DateTime dob)
        {
            int years = DateTime.Now.Year - dob.Year;
            int months = DateTime.Now.Month - dob.Month;
            return $"{years} years, {months} months";
        }

        public static string CalculateDiedAge(DateTime dob, DateTime deceasedDate)
        {
            if (deceasedDate < dob)
            {
                return "Invalid Date";
            }

            int years = deceasedDate.Year - dob.Year;
            int months = deceasedDate.Month - dob.Month;
            int days = deceasedDate.Day - dob.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(deceasedDate.Year, deceasedDate.Month == 1 ? 12 : deceasedDate.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }
            return $"{years} years {months} months";


        }



        [HttpGet]
        public async Task<IActionResult> AddNew(string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New");


                //_pvm.titles = _titleData.GetTitlesList();
                //_pvm.ethnicities = _ethnicityData.GetEthnicitiesList();
                //_pvm.GPList = _gpData.GetGPList();
                //_pvm.GPPracticeList = _gpPracticeData.GetGPPracticeList();

                if (success.HasValue)
                {
                    _pvm.success = success.GetValueOrDefault();

                    if (message != null)
                    {
                        _pvm.message = message;
                    }
                }

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(string title, string firstname, string lastname, string nhsno, DateTime dob,
            string language, bool isInterpreterReqd)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New");

                int day = dob.Day;
                int month = dob.Month;

                var patient = _patientData.GetPatientDetailsByDemographicData(firstname, lastname, nhsno, dob);

                if (patient != null)
                {
                    _pvm.success = false;
                    _pvm.message = "Patient already exist, yo!";

                }
                else
                {
                    //do stuff
                    _pvm.success = true;
                    _pvm.message = "Patient saved.";
                }

                return RedirectToAction("AddNew", "Patient", new { success = _pvm.success, message = _pvm.message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }


    }
}
