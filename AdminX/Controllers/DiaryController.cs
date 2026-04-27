using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AdminX.Controllers
{
    public class DiaryController : Controller
    {
        
        private readonly DiaryVM _dvm;
        private readonly IConfiguration _config;
        private readonly IPatientDataAsync _patientData;
        private readonly IDiaryDataAsync _diaryData;
        private readonly IReferralDataAsync _referralData;
        private readonly IActivityDataAsync _activityData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IDocumentsDataAsync _docsData;
        private readonly IDiaryActionDataAsync _diaryActionData;
        private readonly ICRUD _crud;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;

        public DiaryController(IConfiguration config, IPatientDataAsync patient, IDiaryDataAsync diary, IReferralDataAsync referral, IActivityDataAsync activity, IStaffUserDataAsync staffUser,
            IDocumentsDataAsync documents, IDiaryActionDataAsync diaryAction, ICRUD crud, IAuditServiceAsync audit)
        {
            //_clinContext = context;
            //_adminContext = adminContext;
            //_docContext = documentContext;
            _config = config;
            _dvm = new DiaryVM();
            _patientData = patient;
            _diaryData = diary;
            _referralData = referral;
            _activityData = activity;
            _staffUser = staffUser;
            _docsData = documents;
            _diaryActionData = diaryAction;
            _crud = new CRUD(_config);
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
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

                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Diary Details", "DiaryID=" + id.ToString(), _ip.GetIPAddress());

                _dvm.diary = await _diaryData.GetDiaryEntry(id);
                _dvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                _dvm.linkedRef = await _activityData.GetActivityDetails(_dvm.diary.RefID.GetValueOrDefault());
                _dvm.defaultAction = await _diaryActionData.GetDiaryActionDetails(_dvm.diary.DiaryAction);

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

                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add New Diary Entry", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                _dvm.patient = await _patientData.GetPatientDetails(mpi);
                _dvm.referralsList = await _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                if (_dvm.referralsList.Count > 0)
                {
                    _dvm.defaultRef = _dvm.referralsList.OrderByDescending(r => r.refid).First();
                }
                _dvm.documents = await _docsData.GetDocumentsList();                
                _dvm.diaryActionsList = await _diaryActionData.GetDiaryActions();
                _dvm.documentsList = await _docsData.GetDocumentsList();

          

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

                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                var referral = await _referralData.GetReferralDetails(refID);

                _dvm.patient = await _patientData.GetPatientDetails(referral.MPI);   
                
                if(diaryAction == null) { diaryAction = ""; }
                if (docCode == null) { docCode = ""; }
                if (diaryText == null) { diaryText = ""; }

                int success = _crud.CallStoredProcedure("Diary", "Create", refID, _dvm.patient.MPI, 0, diaryAction, docCode, "", diaryText, User.Identity.Name, diaryDate, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Diary-create(SQL)" }); }
                                
                _dvm.success = true;
                _dvm.message = "New diary added.";
                TempData["SuccessMessage"] = "New diary added";

                return RedirectToAction("PatientDetails", "Patient", new { id = _dvm.patient.MPI });
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

                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit DiaryEntry", "DiaryID=" + id.ToString(), _ip.GetIPAddress());

                _dvm.diary = await _diaryData.GetDiaryEntry(id);
                _dvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                _dvm.referralsList = await _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                _dvm.documents = await _docsData.GetDocumentsList();
                _dvm.diaryActionsList = await _diaryActionData.GetDiaryActions();
                _dvm.documentsList = await _docsData.GetDocumentsList();

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
        public IActionResult Edit(int diaryID, int refID, DateTime diaryDate, string diaryAction, string diaryText, string? docCode = "")
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