using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;

namespace AdminX.Controllers
{
    public class DiagnosisController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        private readonly TestDiseaseVM _dvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientDataAsync _patientData;
        private readonly IDiseaseDataAsync _diseaseData;
        private readonly IAuditServiceAsync _audit; 
        private readonly IPAddressFinder _ip;

        public DiagnosisController(IConfiguration config, IStaffUserDataAsync staffUser, IPatientDataAsync patient, IDiseaseDataAsync disease, IAuditServiceAsync audit)
        {
            //_clinContext = context;
            _config = config;
            _staffUser = staffUser;
            _dvm = new TestDiseaseVM();            
            _patientData = patient;
            _diseaseData = disease;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
        }


        [Authorize]
        public async Task <IActionResult> Index(int id)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Diagnosis", "", _ip.GetIPAddress());
                _dvm.diagnosisList = await _diseaseData.GetDiseaseListByPatient(id);
                _dvm.patient = await _patientData.GetPatientDetails(id);

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="Diagnosis" });
            }
        }        
    }
}