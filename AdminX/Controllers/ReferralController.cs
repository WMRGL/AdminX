using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Controllers
{
    public class ReferralController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IActivityTypeData _activityTypeData;
        private readonly IExternalClinicianData _externalClinicianData;
        private readonly IStaffUserData _staffUserData;
        private readonly IActivityData _activityData;
        private readonly ReferralVM _rvm;

        public ReferralController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _patientData = new PatientData(context);
            _activityTypeData = new ActivityTypeData(context);
            _externalClinicianData = new ExternalClinicianData(context);
            _staffUserData = new StaffUserData(context);
            _activityData = new ActivityData(context);
            _rvm = new ReferralVM();
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
            _rvm.pathways = new List<string>{ "Cancer", "General" };


            return View(_rvm);
        }

        [HttpPost]
        public IActionResult AddNew(int mpi, string refType, DateTime refDate, string refPhys, string refPathway, string indication, string consultant,
            string gc, string admin)
        {
            

            return View(_rvm);
        }
    }
}
