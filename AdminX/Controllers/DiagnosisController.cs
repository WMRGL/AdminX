using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminX.Data;
using AdminX.ViewModels;
using AdminX.Meta;

namespace AdminX.Controllers
{
    public class DiagnosisController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly TestDiseaseVM _dvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly IDiseaseData _diseaseData;
        private readonly IAuditService _audit; 

        public DiagnosisController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _staffUser = new StaffUserData(_clinContext);
            _dvm = new TestDiseaseVM();            
            _patientData = new PatientData(_clinContext);
            _diseaseData = new DiseaseData(_clinContext);
            _audit = new AuditService(_config);
        }


        [Authorize]
        public async Task <IActionResult> Index(int id)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Diagnosis");
                _dvm.diagnosisList = _diseaseData.GetDiseaseListByPatient(id);
                _dvm.patient = _patientData.GetPatientDetails(id);

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="Diagnosis" });
            }
        }        
    }
}

