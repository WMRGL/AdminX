using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Data;
using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class RelativeDiagnosisController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly RelativeDiagnosisVM _rdvm;
        private readonly IConfiguration _config;
        private readonly IRelativeData _relativeData;
        private readonly IRelativeDiagnosisData _relativeDiagnosisData;
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;        
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        
        public RelativeDiagnosisController(ClinicalContext context, IConfiguration config) 
        {
            _clinContext = context;
            _config = config;                        
            _relativeData = new RelativeData(_clinContext);
            _relativeDiagnosisData = new RelativeDiagnosisData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _crud = new CRUD(_config);
            _rdvm = new RelativeDiagnosisVM();
            _audit = new AuditService(_config);
        }
        public IActionResult Index(int relID)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Relative Diagnoses", "ID=" + relID.ToString(), _ip.GetIPAddress());

                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(relID);
                _rdvm.relativesDiagnosisList = _relativeDiagnosisData.GetRelativeDiagnosisList(relID);
                _rdvm.patient = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID);
                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis" });
            }            
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add Relative Diagnosis", "ID=" + id.ToString(), _ip.GetIPAddress());

                _rdvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rdvm.cancerRegList = _relativeDiagnosisData.GetCancerRegList();
                _rdvm.requestStatusList = _relativeDiagnosisData.GetRequestStatusList();     
                _rdvm.staffList = _staffUser.GetStaffMemberList();
                _rdvm.clinicianList = _staffUser.GetClinicalStaffList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-add" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(int id, string diagnosis, string? age, string? hospital, string? cRegCode, DateTime? dateRequested,
            string? consultant, string? status, string? consent, DateTime? dateReceived)
        {
            try
            {
                if (cRegCode == null) { cRegCode = ""; }
                int success = _crud.CallStoredProcedure("RelativeDiagnosis", "Create", id, 0, 0, diagnosis, age, cRegCode, hospital, User.Identity.Name,
                    dateRequested, DateTime.Parse("1900-01-01"), false, false, 0, 0, 0, status, consent, consultant);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiagnosis-add(SQL)" }); }

                return RedirectToAction("Index", "RelativeDiagnosis", new { relID = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-add" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Relative Diagnosis", "ID=" + id.ToString());

                _rdvm.relativesDiagnosis = _relativeDiagnosisData.GetRelativeDiagnosisDetails(id);               
                _rdvm.tumourSiteList = _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = _relativeDiagnosisData.GetTumourMorphList();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int tumourID, string? consent="", DateTime? dateReceived=null, string? confirmed="",
            DateTime? confDiagDate=null, string? confDiagAge="", string? siteCode="", string? latCode="", string? grade="", 
            string? dukes="", string? morphCode="", string? histologyNumber="", string? notes="")
        {
            try
            {
                _rdvm.relativesDiagnosis = _relativeDiagnosisData.GetRelativeDiagnosisDetails(tumourID);
                _rdvm.tumourSiteList = _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = _relativeDiagnosisData.GetTumourMorphList();

                string data = "ConfDiagAge:" + confDiagAge + ",Grade:" + grade + ",Dukes:" + dukes + ",HistologyNumber:" + histologyNumber;
               
                //there are too many strings, so I need to concatenate them all to send them to the SP
                //(it's either that or add another 4 optional string variables!!!)

                int success = _crud.CallStoredProcedure("RelativeDiagnosis", "Edit", tumourID, 0, 0, consent, confirmed, data, notes, User.Identity.Name, dateReceived, confDiagDate,
                    false, false, 0, 0, 0, siteCode, latCode, morphCode);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiagnosis-edit(SQL)" }); }

                //return View(_rdvm);
                return RedirectToAction("Index", "RelativeDiagnosis", new { relID = _rdvm.relativesDiagnosis.RelsID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiagnosis-edit" });
            }
        }
    }
}
