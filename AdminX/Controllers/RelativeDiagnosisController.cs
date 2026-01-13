using AdminX.Meta;
using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace ClinicX.Controllers
{
    public class RelativeDiagnosisController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        private readonly RelativeDiagnosisVM _rdvm;
        private readonly IConfiguration _config;
        private readonly IRelativeDataAsync _relativeData;
        private readonly IRelativeDiagnosisDataAsync _relativeDiagnosisData;
        private readonly IPatientDataAsync _patientData;
        private readonly IStaffUserDataAsync _staffUser;        
        private readonly ICRUD _crud;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;
        
        public RelativeDiagnosisController(IConfiguration config, IRelativeDataAsync relative, IRelativeDiagnosisDataAsync relativeDiagnosis, IPatientDataAsync patient, IStaffUserDataAsync staffUser,
            ICRUD crud, IAuditServiceAsync audit) 
        {
            //_clinContext = context;
            _config = config;                        
            _relativeData = relative;
            _relativeDiagnosisData = relativeDiagnosis;
            _patientData = patient;
            _staffUser = staffUser;
            _crud = crud;
            _rdvm = new RelativeDiagnosisVM();
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
        }
        public async Task<IActionResult> Index(int relID)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);

                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Relative Diagnoses", "ID=" + relID.ToString(), _ip.GetIPAddress());

                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(relID);
                _rdvm.relativesDiagnosisList = await _relativeDiagnosisData.GetRelativeDiagnosisList(relID);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID);
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
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add Relative Diagnosis", "ID=" + id.ToString(), _ip.GetIPAddress());
                                
                _rdvm.relativeDetails = await _relativeData.GetRelativeDetails(id);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relativeDetails.WMFACSID);
                _rdvm.cancerRegList = await _relativeDiagnosisData.GetCancerRegList();
                _rdvm.requestStatusList = await _relativeDiagnosisData.GetRequestStatusList();     
                _rdvm.consultantList = await _staffUser.GetConsultantsList();
                _rdvm.clinicianList = await _staffUser.GetClinicalStaffList();

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
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "ClinicX - Edit Relative Diagnosis", "ID=" + id.ToString());

                _rdvm.relativesDiagnosis = await _relativeDiagnosisData.GetRelativeDiagnosisDetails(id);               
                _rdvm.tumourSiteList = await _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = await _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = await _relativeDiagnosisData.GetTumourMorphList();

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
                _rdvm.relativesDiagnosis = await _relativeDiagnosisData.GetRelativeDiagnosisDetails(tumourID);
                _rdvm.tumourSiteList = await _relativeDiagnosisData.GetTumourSiteList();
                _rdvm.tumourLatList = await _relativeDiagnosisData.GetTumourLatList();
                _rdvm.tumourMorphList = await _relativeDiagnosisData.GetTumourMorphList();

                string data = "ConfDiagAge:" + confDiagAge + ",Grade:" + grade + ",Dukes:" + dukes + ",HistologyNumber:" + histologyNumber;
                           

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
