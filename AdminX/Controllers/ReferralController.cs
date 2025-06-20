using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using AdminX.Data;
using AdminX.Models;


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
        private readonly IListDiseaseData _diseaseData;
        private readonly IClinicData _clinicData;
        private readonly IExternalFacilityData _externalFacilityData;
        private readonly IReviewData _reviewData;    
        private readonly IAuditService _audit;
        private readonly IDiseaseData _indicationData;
        private readonly IPathwayData _pathwayData;
        private readonly IPriorityData _priorityData;
        private readonly IRefReasonData _refReasonData;


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
            _diseaseData = new ListDiseaseData(_adminContext);
            _clinicData = new ClinicData(_clinContext);
            _externalFacilityData = new ExternalFacilityData(_clinContext);
            _reviewData = new ReviewData(_clinContext);
            _audit = new AuditService(_config);
            _indicationData = new DiseaseData(_clinContext);
            _pathwayData = new PathwayData(_clinContext);
            _priorityData = new PriorityData(_clinContext);
            _refReasonData = new RefReasonData(_clinContext);
        }

        [HttpGet]
        public IActionResult ReferralDetails(int refID)
        {
            _rvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _rvm.staffMember.STAFF_CODE;

            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Referral", "RefID=" + refID.ToString(), _ip.GetIPAddress());
            _rvm.pathways = new List<string> { "Cancer", "General" };
            _rvm.referral = _referralData.GetReferralDetails(refID);
            _rvm.patient = _patientData.GetPatientDetails(_rvm.referral.MPI);            
            _rvm.ClinicList = _clinicData.GetClinicByPatientsList(_rvm.referral.MPI).Where(a => a.ReferralRefID == refID).Distinct().ToList();

            if (_rvm.referral.ClockStartDate != null)
            {
                if (_rvm.referral.ClockStopDate != null)
                {
                    _rvm.clockAgeDays = (_rvm.referral.ClockStopDate.GetValueOrDefault() - _rvm.referral.ClockStartDate.GetValueOrDefault()).Days;
                }
                else
                {
                    _rvm.clockAgeDays = (DateTime.Now - _rvm.referral.ClockStartDate.GetValueOrDefault()).Days;
                }
                _rvm.clockAgeWeeks = (int)Math.Floor((double)_rvm.clockAgeDays / 7);
            }


            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                new BreadcrumbItem { Text = "Referral" }
            };

            return View(_rvm);
        }


        [HttpGet]
        public IActionResult UpdateReferralDetails(int refID)
        {
            _rvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _rvm.staffMember.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Update Referral", "RefID=" + refID.ToString(), _ip.GetIPAddress());

            _rvm.referral = _referralData.GetReferralDetails(refID);
            _rvm.patient = _patientData.GetPatientDetails(_rvm.referral.MPI);
            _rvm.activities = _activityTypeData.GetReferralTypes();
            _rvm.consultants = _staffUserData.GetConsultantsList();
            _rvm.gcs = _staffUserData.GetGCList();
            _rvm.admin = _staffUserData.GetAdminList();
            _rvm.admin_status = _adminStatusData.GetStatusAdmin();
            _rvm.referrers = _externalClinicianData.GetClinicianList();
            _rvm.pathways = new List<string> { "Cancer", "General   " }; //because the stupid fucking thing is a text field with trailing spaces for some reason!!!!! And there's no way to remove them.
            _rvm.diseases = _diseaseData.GetDiseases();
            _rvm.facilities = _externalFacilityData.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();
            _rvm.indicationList = _indicationData.GetDiseaseList().Where(d => d.EXCLUDE_CLINIC == 0).ToList();
            _rvm.referralReasonsList = _refReasonData.GetRefReasonList();
            if (_rvm.referral.ClockStartDate != null)
            {
                if (_rvm.referral.ClockStopDate != null)
                {
                    _rvm.clockAgeDays = (_rvm.referral.ClockStopDate.GetValueOrDefault() - _rvm.referral.ClockStartDate.GetValueOrDefault()).Days;
                }
                else
                {
                    _rvm.clockAgeDays = (DateTime.Now - _rvm.referral.ClockStartDate.GetValueOrDefault()).Days;
                }
                _rvm.clockAgeWeeks = (int)Math.Floor((double)_rvm.clockAgeDays / 7);
            }


            ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Referrals",
                    Controller = "Referral",
                    Action = "ReferralDetails",
                    RouteValues = new Dictionary<string, string>
                    {
                        { "refID", refID.ToString() }
                    }
                },
                new BreadcrumbItem { Text = "Update" }
            };

            return View(_rvm);
        }

        [HttpPost]
        public IActionResult UpdateReferralDetails(int refid, string? UBRN, string RefType, string PATHWAY, string? Pathway_Subset, string? PATIENT_TYPE_CODE, string? GC_CODE, string? AdminContact,
             string? ReferrerCode, string? REASON_FOR_REFERRAL, string? PREGNANCY, string? INDICATION, string? RefClass, DateTime? ClockStartDate, DateTime? ClockStopDate, string? COMPLETE,
             string? Status_Admin, string? RefReasonCode, string? OthReason1, string? OthReason2, string? OthReason3, string? OthReason4, int? RefReasonAff, int? OthReason1Aff,
             int? OthReason2Aff, int? OthReason3Aff, int? OthReason4Aff, int? RefFHF, string? consultant, string? Clics, int? symptomatic)
        {
            
            string login = User.Identity?.Name ?? "Unknown";

            if (PATHWAY.Trim() == "Cancer")
            {
                int success = _CRUD.ReferralDetail(
                     sType: "Referral",
                     sOperation: "Update",
                     sLogin: login,
                     int1: refid,
                     string1: RefType,
                     string2: INDICATION,
                     text: REASON_FOR_REFERRAL,
                     string3: PATHWAY,
                     string4: UBRN,
                     string5: Pathway_Subset,
                     string6: PATIENT_TYPE_CODE,
                     string7: GC_CODE,
                     string8: AdminContact,
                     string9: ReferrerCode,
                     string10: PREGNANCY,
                     string11: RefClass,
                     string12: COMPLETE,
                     dDate1: ClockStartDate,
                     dDate2: ClockStopDate,
                     string13: Status_Admin,
                     string14: RefReasonCode,
                     int2: RefFHF,
                     int3: RefReasonAff,
                     int4: OthReason1Aff,
                     int5: OthReason2Aff,
                     int6: OthReason3Aff,
                     int7: OthReason4Aff,
                     int8: symptomatic,
                     string15: OthReason1,
                     string16: OthReason2,
                     string17: OthReason3,
                     string18: OthReason4
                     

                 );
                if (success != 1)
                {
                    return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-edit(SQL)" });
                }
            }
            else
            {
                int success = _CRUD.ReferralDetail(
                    sType: "Referral",
                    sOperation: "Update",
                    sLogin: login,
                    int1: refid,
                    int2: null,
                    int3: null,
                    int4: null,
                    int5: null,
                    int6: null,
                    int7: null,
                    int8: null,
                    string1: RefType,
                    string2: INDICATION,
                    text: REASON_FOR_REFERRAL,
                    string3: PATHWAY,
                    string4: UBRN,
                    string5: Pathway_Subset,
                    string6: PATIENT_TYPE_CODE,
                    string7: GC_CODE,
                    string8: AdminContact,
                    string9: ReferrerCode,
                    string10: PREGNANCY,
                    string11: RefClass,
                    string12: COMPLETE,
                    dDate1: ClockStartDate,
                    dDate2: ClockStopDate,
                    string13: Status_Admin

                );
                if (success != 1)
                {
                    return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-edit(SQL)" });
                }
            }
            return RedirectToAction("ReferralDetails", new { refID = refid });
        }


        [HttpGet]
        public IActionResult AddNew(int mpi)
        {
            _rvm.staffMember = _staffUserData.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _rvm.staffMember.STAFF_CODE;
            IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Referral", "", _ip.GetIPAddress());

            _rvm.patient = _patientData.GetPatientDetails(mpi);
            _rvm.activities = _activityTypeData.GetReferralTypes();
            _rvm.consultants = _staffUserData.GetConsultantsList();
            _rvm.gcs = _staffUserData.GetGCList();
            _rvm.admin = _staffUserData.GetAdminList();
            _rvm.referrals = _activityData.GetActiveReferralList(mpi);
            _rvm.referrers = _externalClinicianData.GetClinicianList();
            _rvm.referrers.Add(_externalClinicianData.GetPatientGPReferrer(mpi));
            _rvm.pathways = new List<string> { "Cancer", "General" };
            _rvm.admin_status = _adminStatusData.GetStatusAdmin();
            _rvm.indicationList = _indicationData.GetDiseaseList().Where(d => d.EXCLUDE_CLINIC == 0).ToList();
            _rvm.subPathways = _pathwayData.GetSubPathwayList();
            _rvm.priorityList = _priorityData.GetPriorityList();
            _rvm.pregnancy = new List<string> { "No Pregnancy", "Pregnant" };
            _rvm.referralReasonsList = _refReasonData.GetRefReasonList();            

            return View(_rvm);
        }


        [HttpPost]
        public IActionResult AddNew(int mpi, string refType, DateTime refDate, DateTime clockStartDate, string refPhys, string refPathway, string? subPathway, 
            string indication, string consultant, string gc, string admin, string UBRN, string clinClass, string pregnancy, string? refReason, string? refReason1, 
            string? refReason2, string? refReason3, string? refReason4, int? refReasonAff, int? OthReason1Aff, int? OthReason2Aff, int? OthReason3Aff, 
            int? OthReason4Aff, string? comments, int? symptomatic, int? RefFHF, string? Status_Admin)
        {            
            int success = _CRUD.ReferralDetail("Referral", "Create", User.Identity.Name, mpi, RefFHF,refReasonAff, OthReason1Aff, 
                OthReason2Aff, OthReason3Aff, OthReason4Aff, symptomatic, refType, indication, comments, refPathway, UBRN, subPathway, 
                consultant, gc, admin, refPhys, pregnancy, clinClass, "Active", refDate, null, Status_Admin, refReason, refReason1, 
                refReason2, refReason3, refReason4, false, false);

            if (success != 1)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-edit(SQL)" });
            }

            return RedirectToAction("PatientDetails", "Patient", new { id = mpi });
        }
    }
}
