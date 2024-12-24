﻿using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using AdminX.Data;
using Microsoft.AspNetCore.Http;
using ClinicalXPDataConnections.Models;
using System.Globalization;
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
            _rvm.referral = _referralData.GetReferralDetails(refID);
            _rvm.patient = _patientData.GetPatientDetails(_rvm.referral.MPI);
            _rvm.activities = _activityTypeData.GetReferralTypes();
            _rvm.consultants = _staffUserData.GetConsultantsList();
            _rvm.gcs = _staffUserData.GetGCList();
            _rvm.admin = _staffUserData.GetAdminList();
            _rvm.admin_status = _adminStatusData.GetStatusAdmin();
            _rvm.referrers = _externalClinicianData.GetClinicianList();
            _rvm.pathways = new List<string> { "Cancer", "General" };
            _rvm.diseases =_diseaseData.GetDiseases();
            _rvm.facilities = _externalFacilityData.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();

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
             string? Status_Admin, string? RefReasonCode, string? OthReason1, string? OthReason2, string? OthReason3, string? OthReason4, bool? RefReasonAff, bool? OthReason1Aff,
             bool? OthReason2Aff, bool? OthReason3Aff, bool? OthReason4Aff, int? RefFHF,  string? consultant)
        {
            string login = User.Identity?.Name ?? "Unknown";

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
                int2: RefFHF,
                string44: RefReasonCode,
                int3: RefReasonAff.HasValue && RefReasonAff.Value ? 1 : 0,
                int4: OthReason1Aff.HasValue && OthReason1Aff.Value ? 1 : 0,
                int5: OthReason2Aff.HasValue && OthReason2Aff.Value ? 1 : 0,
                int6: OthReason3Aff.HasValue && OthReason3Aff.Value ? 1 : 0,
                int7: OthReason4Aff.HasValue && OthReason4Aff.Value ? 1 : 0
            );

            if (success != 1)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-edit(SQL)" });
            }

            return RedirectToAction("ReferralDetails", new { refID = refid });
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
