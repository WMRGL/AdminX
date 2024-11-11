using Microsoft.AspNetCore.Mvc;
using AdminX.Data;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using AdminX.Meta;
using RestSharp;
using Newtonsoft.Json.Linq;
using AdminX.Models;

namespace AdminX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly PatientVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relativeData;
        private readonly IPathwayData _pathwayData;
        private readonly IAlertData _alertData;
        private readonly IReferralData _referralData;
        private readonly IDiaryData _diaryData;
        private readonly IHPOCodeData _hpoData;
        private readonly IExternalClinicianData _gpData;
        private readonly IExternalFacilityData _gpPracticeData;
        private readonly IAuditService _audit;
        private readonly IConstantsData _constants;

        public PatientController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _pvm = new PatientVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _pathwayData = new PathwayData(_clinContext);
            _alertData = new AlertData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _hpoData = new HPOCodeData(_clinContext);
            _gpData = new ExternalClinicianData(_clinContext);
            _gpPracticeData = new ExternalFacilityData(_clinContext);
            _audit = new AuditService(_config);
            _constants = new ConstantsData(_clinContext);
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
                _pvm.hpoTermDetails = _hpoData.GetHPOTermsAddedList(id);
                _pvm.referrals = _referralData.GetReferralsList(id);
                _pvm.patientPathway = _pathwayData.GetPathwayDetails(id);
                _pvm.alerts = _alertData.GetAlertsList(id);
                _pvm.diary = _diaryData.GetDiaryList(id);
                                

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New");

                _pvm.titles = _patientData.GetTitlesList();
                _pvm.ethnicities = _patientData.GetEthnicitiesList();
                _pvm.GPList = _gpData.GetGPList();
                _pvm.GPPracticeList = _gpPracticeData.GetGPPracticeList();

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
