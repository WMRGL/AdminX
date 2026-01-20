//using AdminX.Data;
using AdminX.Meta;
using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class RelativeDiaryController : Controller
    {
        //private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly IRelativeDiaryDataAsync _relDiaryData;
        private readonly IRelativeDataAsync _relData;
        private readonly IPatientDataAsync _patientData;
        private readonly ICRUD _crud;
        private readonly RelativeDiaryVM _rdvm;
        //private readonly AdminContext _adminContext;
        //private readonly DocumentContext _docContext;        
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IDocumentsDataAsync _docsData;
        private readonly IDiaryActionDataAsync _diaryActionData;        
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;
        public RelativeDiaryController(IConfiguration config, IRelativeDataAsync relative, IRelativeDiaryDataAsync relativeDiary, IPatientDataAsync patient, ICRUD crud, IStaffUserDataAsync staffUser,
            IDocumentsDataAsync documents, IDiaryActionDataAsync diaryAction, IAuditServiceAsync audit)
        {
            //_context = context;
            _config = config;
            _relDiaryData = relativeDiary;
            _relData = relative;
            _patientData = patient;
            _crud = crud;
            _rdvm = new RelativeDiaryVM();
            //_adminContext = adminContext;
            //_docContext = documentContext;
            _config = config;                        
            _staffUser = staffUser;
            _docsData = documents;
            _diaryActionData = diaryAction;            
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
        }
        public async Task<IActionResult> Index(int relID)
        {
            try
            {
                _rdvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rdvm.staffMember.STAFF_CODE;
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Relative Diary", "RelID=" + relID.ToString(), _ip.GetIPAddress());

                _rdvm.relative = await _relData.GetRelativeDetails(relID);
                _rdvm.relativeDiaryList = await _relDiaryData.GetRelativeDiaryList(relID);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relative.WMFACSID);

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDiary" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> AddNew(int relID)
        {
            try
            {
                _rdvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rdvm.staffMember.STAFF_CODE;
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Relative Diary", "RelID=" + relID.ToString(), _ip.GetIPAddress());

                _rdvm.relative = await _relData.GetRelativeDetails(relID);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relative.WMFACSID);
                _rdvm.documentsList = await _docsData.GetDocumentsList();
                _rdvm.diaryActionsList = await _diaryActionData.GetDiaryActions();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "AddNewRelativeDiary" });
            }
        }

        [HttpPost]
        public IActionResult AddNew(int relID, DateTime diaryDate, string actionCode, string docCode, string? diaryText, string? letterText)
        {
            try
            {
                int success = _crud.CallStoredProcedure("RelativeDiary", "Create", relID, 0, 0, actionCode, docCode, "", "", User.Identity.Name, diaryDate,
                    null, false, false, 0, 0, 0, diaryText, letterText);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiary-new(SQL)" }); }

                return RedirectToAction("Index", "RelativeDiary", new { relID = relID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "AddNewRelativeDiary" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int diaryID)
        {
            try
            {
                _rdvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rdvm.staffMember.STAFF_CODE;
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Update Relative Diary", "DiaryID=" + diaryID.ToString(), _ip.GetIPAddress());

                _rdvm.relativeDiary = await _relDiaryData.GetRelativeDiaryDetails(diaryID);
                _rdvm.relative = await _relData.GetRelativeDetails(_rdvm.relativeDiary.RelsID);
                _rdvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rdvm.relative.WMFACSID);
                _rdvm.documentsList = await _docsData.GetDocumentsList();
                _rdvm.diaryActionsList = await _diaryActionData.GetDiaryActions();

                return View(_rdvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditRelativeDiary" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int diaryID, DateTime diaryDate, string actionCode, string docCode, string? diaryText, string? letterText, bool? isNotReturnExpected=false)
        {
            try
            {
                _rdvm.relativeDiary = await _relDiaryData.GetRelativeDiaryDetails(diaryID);
                int relID = _rdvm.relativeDiary.RelsID;

                int success = _crud.CallStoredProcedure("RelativeDiary", "Edit", diaryID, 0, 0, actionCode, docCode, "", "", User.Identity.Name, diaryDate,
                    null, isNotReturnExpected, false, 0, 0, 0, diaryText, letterText);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiary-new(SQL)" }); }
                TempData["SuccessMessage"] = "Relative Diary entry updated successfully.";
                return RedirectToAction("Index", "RelativeDiary", new { relID = relID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditRelativeDiary" });
            }
        }
    }
}
