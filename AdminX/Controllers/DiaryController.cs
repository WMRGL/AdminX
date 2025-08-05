using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AdminX.Controllers
{
    public class DiaryController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly AdminContext _adminContext;
        private readonly DocumentContext _docContext;
        private readonly DiaryVM _dvm;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IDiaryData _diaryData;
        private readonly IReferralData _referralData;
        private readonly IActivityData _activityData;
        private readonly IStaffUserData _staffUser;
        private readonly IDocumentsData _docsData;
        private readonly IDiaryActionData _diaryActionData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public DiaryController(ClinicalContext context, AdminContext adminContext, DocumentContext documentContext, IConfiguration config)
        {
            _clinContext = context;
            _adminContext = adminContext;
            _docContext = documentContext;
            _config = config;
            _dvm = new DiaryVM();
            _patientData = new PatientData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _staffUser = new StaffUserData(_clinContext);
            _docsData = new DocumentsData(_docContext);
            _diaryActionData = new DiaryActionData(_adminContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DiaryDetails(int id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Diary Details", "DiaryID=" + id.ToString(), _ip.GetIPAddress());

                _dvm.diary = _diaryData.GetDiaryEntry(id);
                _dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                _dvm.linkedRef = _activityData.GetActivityDetails(_dvm.diary.RefID.GetValueOrDefault());
                _dvm.defaultAction = _diaryActionData.GetDiaryActionDetails(_dvm.diary.DiaryAction);

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem
                    {
                        Text = "Patient Details",
                        Controller = "Patient",
                        Action = "PatientDetails",
                        RouteValues = new Dictionary<string, string> { { "id", _dvm.patient.MPI.ToString() } }
                    },
                    new BreadcrumbItem { Text = "Diary Details" }
                };  
                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DiaryDetails" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddNew(int mpi)
        {
            try
            {
                if (mpi == null)
                {
                    return NotFound();
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add New Diary Entry", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                //_dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                _dvm.patient = _patientData.GetPatientDetails(mpi);
                _dvm.referralsList = _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                if (_dvm.referralsList.Count > 0)
                {
                    _dvm.defaultRef = _dvm.referralsList.OrderByDescending(r => r.refid).First();
                }
                //_dvm.staffList = _staffUser.GetStaffMemberListAll();
                _dvm.documents = _docsData.GetDocumentsList();
                //_dvm.clinicians = _staffUser.GetConsultantsList();
                //_dvm.currentStaffUser = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                _dvm.diaryActionsList = _diaryActionData.GetDiaryActions();
                _dvm.documentsList = _docsData.GetDocumentsList();

          

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem
                    {
                        Text = "Patient Details",
                        Controller = "Patient",
                        Action = "PatientDetails",
                        RouteValues = new Dictionary<string, string> { { "id", _dvm.patient.MPI.ToString() } }
                    },
                    new BreadcrumbItem { Text = "New Diary" }
                };
                return View(_dvm);
                //return RedirectToAction("Index", "AddNew");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DiaryDetails" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddNew(int refID, DateTime diaryDate, string diaryWith, string diaryCons, string diaryAction, string diaryText, string? docCode = "")
        {
            try
            {
                if (refID == null)
                {
                    return NotFound();
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                _dvm.patient = _patientData.GetPatientDetails(_referralData.GetReferralDetails(refID).MPI);
                //_dvm.referralsList = _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                //_dvm.documents = _docsData.GetDocumentsList();

                int success = _crud.CallStoredProcedure("Diary", "Create", refID, _dvm.patient.MPI, 0, diaryAction, docCode, "", diaryText, User.Identity.Name, diaryDate, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Diary-create(SQL)" }); }

                int diaryid = _diaryData.GetLatestDiaryByRefID(refID).DiaryID;

                return RedirectToAction("DiaryDetails", "Diary", new { id = diaryid });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DiaryDetails" });
            }
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit DiaryEntry", "DiaryID=" + id.ToString(), _ip.GetIPAddress());

                _dvm.diary = _diaryData.GetDiaryEntry(id);
                _dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                _dvm.referralsList = _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                _dvm.documents = _docsData.GetDocumentsList();
                _dvm.diaryActionsList = _diaryActionData.GetDiaryActions();
                _dvm.documentsList = _docsData.GetDocumentsList();

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem
                    {
                        Text = "Patient Details",
                        Controller = "Patient",
                        Action = "PatientDetails",
                        RouteValues = new Dictionary<string, string> { { "id", _dvm.patient.MPI.ToString() } }
                    },
                    new BreadcrumbItem { Text = "Edit Diary " }
                };

                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formname = "Diary-edit" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int diaryID, int refID, DateTime diaryDate, string diaryAction, string diaryText, string? docCode = "")
        {
            try
            {
                if (diaryID == null)
                {
                    return NotFound();
                }


                int success = _crud.CallStoredProcedure("Diary", "Update", diaryID, refID, 0, diaryAction, docCode, "", diaryText, User.Identity.Name, diaryDate, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Diary-create(SQL)" }); }

                return RedirectToAction("DiaryDetails", new { id = diaryID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Diary-edit" });
            }
        }
    }
}