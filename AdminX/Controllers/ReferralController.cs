using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace AdminX.Controllers
{
    public class ReferralController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly AdminContext _adminContext;
        //private readonly DocumentContext _documentContext;
        private readonly IConfiguration _config;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IPatientDataAsync _patientData;
        private readonly IActivityTypeDataAsync _activityTypeData;
        private readonly IExternalClinicianDataAsync _externalClinicianData;
        private readonly IStaffUserDataAsync _staffUserData;
        private readonly IActivityDataAsync _activityData;
        private readonly ICRUD _CRUD;
        private readonly ReferralVM _rvm;
        private readonly IReferralDataAsync _referralData;
        private readonly IAdminStatusDataAsync _adminStatusData;
        private readonly IListDiseaseDataAsync _diseaseData;
        private readonly IClinicDataAsync _clinicData;
        private readonly IExternalFacilityDataAsync _externalFacilityData;
        private readonly IReviewDataAsync _reviewData;    
        private readonly IAuditServiceAsync _audit;
        private readonly IDiseaseDataAsync _indicationData;
        private readonly IPathwayDataAsync _pathwayData;
        private readonly IPriorityDataAsync _priorityData;
        private readonly IRefReasonDataAsync _refReasonData;
        private readonly ITriageDataAsync _triageData;
        private readonly IAreaNamesDataAsync _areaNamesData;

        public ReferralController(IConfiguration config, IPatientDataAsync patient, IActivityTypeDataAsync activityType, IExternalClinicianDataAsync extClinician, IStaffUserDataAsync staffUser,
            IActivityDataAsync activity, ICRUD crud, IReferralDataAsync referral, IAdminStatusDataAsync adminStatus, IListDiseaseDataAsync listDisease, IClinicDataAsync clinic, IExternalFacilityDataAsync extFacility,
            IReviewDataAsync review, IAuditServiceAsync audit, IDiseaseDataAsync disease, IPathwayDataAsync pathway, IPriorityDataAsync priority, IRefReasonDataAsync refReason, ITriageDataAsync triage,
            IAreaNamesDataAsync areanames, IConstantsDataAsync constants)
        {
            //_clinContext = context;
            //_adminContext = adminContext;
            //_documentContext = documentContext;            
            _config = config;
            _patientData = patient;
            _activityTypeData = activityType;
            _externalClinicianData = extClinician;
            _staffUserData = staffUser;
            _activityData = activity;
            _rvm = new ReferralVM();
            _CRUD = crud;
            _referralData = referral;
            _adminStatusData = adminStatus;
            _diseaseData = listDisease;
            _clinicData = clinic;
            _externalFacilityData = extFacility;
            _reviewData = review;
            _audit = audit;
            _indicationData = disease;
            _pathwayData = pathway;
            _priorityData = priority;
            _refReasonData = refReason;
            _triageData = triage;
            _areaNamesData = areanames;
            _constantsData = constants;
            
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ReferralDetails(int refID)
        {
            try
            {
                _rvm.staffMember = await _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rvm.staffMember.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Referral", "RefID=" + refID.ToString(), _ip.GetIPAddress());
                _rvm.pathways = new List<string> { "Cancer", "General" };
                _rvm.referral = await _referralData.GetReferralDetails(refID);
                _rvm.patient = await _patientData.GetPatientDetails(_rvm.referral.MPI);
                var clinicList = await _clinicData.GetClinicByPatientsList(_rvm.referral.MPI);
                _rvm.ClinicList = clinicList.Where(a => a.ReferralRefID == refID).Distinct().ToList();
                ICP icp = await _triageData.GetICPDetailsByRefID(refID);
                _rvm.relatedICP = await _triageData.GetTriageDetails(icp.ICPID); //because ICP and Triage are different, apparently
                string canDeleteICP = await _constantsData.GetConstant("DeleteICPBtn", 1);

                if(canDeleteICP.ToUpper().Contains(User.Identity.Name.ToUpper()))
                {
                    _rvm.canDeleteICP = true;
                }

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
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ReferralDetails" });
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateReferralDetails(int refID)
        {
            try
            {
                _rvm.staffMember = await _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rvm.staffMember.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Update Referral", "RefID=" + refID.ToString(), _ip.GetIPAddress());

                _rvm.referral = await _referralData.GetReferralDetails(refID);
                _rvm.patient = await _patientData.GetPatientDetails(_rvm.referral.MPI);
                _rvm.activities = await _activityTypeData.GetReferralTypes();
                _rvm.consultants = await _staffUserData.GetConsultantsList();
                _rvm.gcs = await _staffUserData.GetGCList();
                _rvm.admin = await _staffUserData.GetAdminList();
                _rvm.admin_status = await _adminStatusData.GetStatusAdmin();
                var referrers = await _externalClinicianData.GetClinicianList();
                _rvm.referrers = referrers.OrderBy(r => r.LAST_NAME).ToList();
                //_rvm.pathways = new List<string> { "Cancer", "General   " };
                _rvm.pathways = new List<string>();
                List<Pathway> pathways = await _pathwayData.GetPathwayList();
                foreach (var item in pathways) { _rvm.pathways.Add(item.CGU_Pathway.Trim()); }
                _rvm.diseases = await _diseaseData.GetDiseases();
                var facility = await _externalFacilityData.GetFacilityList();
                _rvm.facilities = facility.Where(f => f.IS_GP_SURGERY == 0).ToList();
                var indication = await _indicationData.GetDiseaseList();
                _rvm.indicationList = indication.Where(d => d.EXCLUDE_CLINIC == 0).ToList();
                _rvm.referralReasonsList = await _refReasonData.GetRefReasonList();

                if(_rvm.patient.PtAreaCode == null)
                {
                    return RedirectToAction("PatientDetails", "Patient", new { id=_rvm.patient.MPI, message="You need to assign an area code before processing the referral.", success=false });
                }

                _rvm.areaName = await _areaNamesData.GetAreaNameDetailsByCode(_rvm.patient.PtAreaCode);

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
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "UpdateReferralDetails" });
            }
        }

        [HttpPost]
        public IActionResult UpdateReferralDetails(int refid, string? UBRN, string RefType, string PATHWAY, string? Pathway_Subset, string? PATIENT_TYPE_CODE, string? GC_CODE, string? AdminContact,
             string? ReferrerCode, string? REASON_FOR_REFERRAL, string? PREGNANCY, string? INDICATION, string? RefClass, DateTime? ClockStartDate, DateTime? ClockStopDate, string? COMPLETE,
             string? Status_Admin, string? RefReasonCode, string? OthReason1, string? OthReason2, string? OthReason3, string? OthReason4, int? RefReasonAff, int? OthReason1Aff,
             int? OthReason2Aff, int? OthReason3Aff, int? OthReason4Aff, int? RefFHF, string? consultant, string? Clics, int? symptomatic)
        {
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "UpdateReferralDetails" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkReferralDeleted(int refid, int mpi, bool logicaldelete)
        {
            try
            {
                string login = User.Identity?.Name ?? "Unknown";

              
                int success = _CRUD.ReferralDetail(
                     sType: "Referral",
                     sOperation: "LogicalDelete", 
                     sLogin: login,
                     int1: refid,           
                     bool1: logicaldelete,     

                     string1: null, string2: null, text: null, string3: null,
                     string4: null, string5: null, string6: null, string7: null,
                     string8: null, string9: null, string10: null, string11: null,
                     string12: null, dDate1: null, dDate2: null, string13: null,
                     string14: null, int2: null, int3: null, int4: null, int5: null,
                     int6: null, int7: null, int8: null
                 );

               


                string query = "Select top 1 DeleteReason, DeleteStatus from [Clinical_Dev].[dbo].[DeletedReferrals] where mpi = " + mpi + " and RefID = " + refid + " order by DeletedRefId desc";
                string readerObjString = "";
                int deleteStatus = 0;
                SqlConnection conn = new SqlConnection(_config.GetConnectionString("ConString"));
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        readerObjString = reader.GetString(0);
                        deleteStatus = reader.GetInt16(1);
                    }
                }
                conn.Close();

                bool deleteSuccess = false;
                if (deleteStatus == 1) { deleteSuccess = true; }                    
                
                return RedirectToAction("PatientDetails", "Patient", new { id = mpi, message = readerObjString, success = deleteSuccess });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "MarkReferralDeleted" });
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddNew(int mpi)
        {
            try
            {
                _rvm.staffMember = await _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rvm.staffMember.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Referral", "", _ip.GetIPAddress());

                _rvm.patient = await _patientData.GetPatientDetails(mpi);
                _rvm.activities = await _activityTypeData.GetReferralTypes();
                _rvm.consultants = await _staffUserData.GetConsultantsList();
                _rvm.gcs = await _staffUserData.GetGCList();
                _rvm.admin = await _staffUserData.GetAdminList();
                _rvm.referrals = await _activityData.GetActiveReferralList(mpi);
                var refs = await _externalClinicianData.GetClinicianList();
                _rvm.referrers = refs.OrderBy(r => r.LAST_NAME).ToList();
                var gp = await _externalClinicianData.GetPatientGPReferrer(mpi);
                _rvm.referrers.Add(gp);
                //_rvm.pathways = new List<string> { "Cancer", "General" };
                _rvm.pathways = new List<string>();
                List<Pathway> pathways = await _pathwayData.GetPathwayList();
                foreach(var item in pathways) { _rvm.pathways.Add(item.CGU_Pathway.Trim()); }
                _rvm.admin_status = await _adminStatusData.GetStatusAdmin();
                var indication = await _indicationData.GetDiseaseList();
                _rvm.indicationList = indication.Where(d => d.EXCLUDE_CLINIC == 0).ToList();
                _rvm.subPathways = await _pathwayData.GetSubPathwayList();
                _rvm.priorityList = await _priorityData.GetPriorityList();
                _rvm.pregnancy = new List<string> { "No Pregnancy", "Pregnant" };
                _rvm.referralReasonsList = await _refReasonData.GetRefReasonList();
                _rvm.areaName = await _areaNamesData.GetAreaNameDetailsByCode(_rvm.patient.PtAreaCode);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "AddReferral" });
            }
        }


        [HttpPost]
        public IActionResult AddNew(int mpi, string refType, DateTime refDate, DateTime clockStartDate, string refPhys, string refPathway, string? subPathway, 
            string indication, string consultant, string gc, string admin, string UBRN, string clinClass, string pregnancy, string? refReason, string? refReason1, 
            string? refReason2, string? refReason3, string? refReason4, int? refReasonAff, int? OthReason1Aff, int? OthReason2Aff, int? OthReason3Aff, 
            int? OthReason4Aff, string? comments, int? symptomatic, int? RefFHF, string? Status_Admin)
        {
            try
            {
                int success = _CRUD.ReferralDetail("Referral", "Create", User.Identity.Name, mpi, RefFHF, refReasonAff, OthReason1Aff,
                    OthReason2Aff, OthReason3Aff, OthReason4Aff, symptomatic, refType, indication, comments, refPathway, UBRN, subPathway,
                    consultant, gc, admin, refPhys, pregnancy, clinClass, "Active", refDate, null, Status_Admin, refReason, refReason1,
                    refReason2, refReason3, refReason4, false, false);

                if (success != 1)
                {
                    return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-edit(SQL)" });
                }

                return RedirectToAction("PatientDetails", "Patient", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "AddReferral" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ProcessNewReferral(int refID)
        {
            try
            {
                _rvm.staffMember = await _staffUserData.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rvm.staffMember.STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Process Epic Referral", "", _ip.GetIPAddress());

                _rvm.referral = await _referralData.GetReferralDetails(refID);
                _rvm.patient = await _patientData.GetPatientDetails(_rvm.referral.MPI);
                _rvm.pathways = new List<string>();
                List<Pathway> pathwayList = await _pathwayData.GetPathwayList();
                foreach (var p in pathwayList)
                {
                    _rvm.pathways.Add(p.CGU_Pathway);
                }

                _rvm.consultants = await _staffUserData.GetConsultantsList();
                _rvm.gcs = await _staffUserData.GetGCList();
                _rvm.admin = await _staffUserData.GetAdminList();
                if (string.IsNullOrEmpty(_rvm.patient.PtAreaCode))
                {
                    return RedirectToAction("PatientDetails", "Patient", new { id = _rvm.patient.MPI, message = "You need to assign an area code before processing the referral.", success = false });
                }
                _rvm.areaName = await _areaNamesData.GetAreaNameDetailsByCode(_rvm.patient.PtAreaCode);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ProcessReferral" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessNewReferral(int refID, string pathway, string consultant, string gc, string admin)
        {
            try
            {
                _rvm.referral = await _referralData.GetReferralDetails(refID);

                int success = _CRUD.ReferralDetail("Referral", "Process", User.Identity.Name, refID, 0, 0, 0, 0, 0, 0, 0, pathway, consultant, "", gc, admin);

                if (success != 1)
                {
                    return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Referral-process(SQL)" });
                }

                return RedirectToAction("PatientDetails", "Patient", new { id = _rvm.referral.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ProcessReferral" });
            }
        }

    }
}
