using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using AdminX.Data;

namespace AdminX.Controllers
{
    public class ReferralController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly AdminContext _adminContext;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IActivityTypeData _activityTypeData;
        private readonly IExternalClinicianData _externalClinicianData;
        private readonly IStaffUserData _staffUserData;
        private readonly IActivityData _activityData;
        private readonly ICRUD _CRUD;
        private readonly ReferralVM _rvm;
        private readonly IReferralData _referralData;
        private readonly IAdminStatusData _adminStatusData;
        private readonly IDiseaseData _diseaseData;
        private readonly IClinicData _clinicData;


        public ReferralController(ClinicalContext context, AdminContext adminContext, IConfiguration config)
        {
            _clinContext = context;
            _adminContext = adminContext;
            _config = config;
            _patientData = new PatientData(context);
            _activityTypeData = new ActivityTypeData(context, adminContext);
            _externalClinicianData = new ExternalClinicianData(context);
            _staffUserData = new StaffUserData(context);
            _activityData = new ActivityData(context);
            _rvm = new ReferralVM();
            _CRUD = new CRUD(_config);
            _referralData = new ReferralData(_clinContext);
            _adminStatusData = new AdminStatusData(_adminContext);
            _diseaseData = new DiseaseData(_clinContext);
            _clinicData = new ClinicData(_clinContext);


        }

        [HttpGet]
        public IActionResult ReferralDetails(int refID)
        {
            _rvm.referral = _referralData.GetReferralDetails(refID);
            _rvm.patient = _patientData.GetPatientDetails(_rvm.referral.MPI);
            _rvm.Clinic = _clinicData.GetClinicDetails(refID);

            //_rvm.consultants = _staffUserData.GetConsultantsList();
            //_rvm.admin = _staffUserData.GetAdminList();
            //_rvm.referrals = _activityData.GetActiveReferralList(_rvm.referral.MPI);
            //_rvm.referrers = _externalClinicianData.GetClinicianList();

            return View(_rvm);
        }

        
        [HttpGet]
        public IActionResult UpdateReferralDetails(int refID)
        {
            _rvm.referral = _referralData.GetReferralDetails(refID);
            _rvm.patient = _patientData.GetPatientDetails(_rvm.referral.MPI);
            _rvm.activities = _activityTypeData.GetReferralTypes();
            _rvm.consultants = _staffUserData.GetConsultantsList();
            _rvm.gcs = _staffUserData.GetGCList();
            _rvm.admin = _staffUserData.GetAdminList();
            _rvm.admin_status = _adminStatusData.GetStatusAdmin();
            _rvm.referrers = _externalClinicianData.GetClinicianList();
            _rvm.pathways = new List<string> { "Cancer", "General" };
            _rvm.diseases =_diseaseData.GetDiseaseList();


            return View(_rvm);
        }

        [HttpPost]
        public IActionResult UpdateReferralDetails(int refID, string refType, DateTime refDate, DateTime clockStartDate, string refPhys, string refPathway, string indication, string consultant,
            string gc, string admin)
        {
            int success = _CRUD.CallStoredProcedure("Referral", "Update", refID, 0, 0, refType, indication, refPathway, refPhys, User.Identity.Name,
                refDate, clockStartDate, false, false, 0, 0, 0, consultant, gc, admin);

            return View(_rvm);
        }


        [HttpGet]
        public IActionResult AddNew(int mpi)
        {
            _rvm.patient = _patientData.GetPatientDetails(mpi);
            _rvm.activities = _activityTypeData.GetReferralTypes();
            _rvm.consultants = _staffUserData.GetConsultantsList();
            _rvm.gcs = _staffUserData.GetGCList();
            _rvm.admin = _staffUserData.GetAdminList();
            _rvm.referrals = _activityData.GetActiveReferralList(mpi);
            _rvm.referrers = _externalClinicianData.GetClinicianList();
            _rvm.pathways = new List<string> { "Cancer", "General" };
            _rvm.admin_status = _adminStatusData.GetStatusAdmin();

            return View(_rvm);
        }


        [HttpPost]
        public IActionResult AddNew(int mpi, string refType, DateTime refDate, DateTime clockStartDate, string refPhys, string refPathway, string indication, string consultant,
            string gc, string admin, string UBRN)
        {
            int success = _CRUD.CallStoredProcedure("Referral", "Create", mpi, 0, 0, refType, indication, refPathway, refPhys, User.Identity.Name,
                refDate, clockStartDate, false, false, 0, 0, 0, consultant, gc, admin, 0,0,0,0,0,UBRN);
            
            return View(_rvm);
        }
    }
}
