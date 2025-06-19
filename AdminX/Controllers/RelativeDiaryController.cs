using AdminX.Data;
using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class RelativeDiaryController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly IRelativeDiaryData _relDiaryData;
        private readonly IRelativeData _relData;
        private readonly IPatientData _patientData;
        private readonly ICRUD _crud;
        private readonly RelativeDiaryVM _rdvm;
        private readonly ClinicalContext _clinContext;
        private readonly AdminContext _adminContext;
        private readonly DocumentContext _docContext;        
        private readonly IStaffUserData _staffUser;
        private readonly IDocumentsData _docsData;
        private readonly IDiaryActionData _diaryActionData;        
        private readonly IAuditService _audit;
        public RelativeDiaryController(ClinicalContext context, AdminContext adminContext, DocumentContext documentContext, IConfiguration config)
        {
            _context = context;
            _config = config;
            _relDiaryData = new RelativeDiaryData(_context);
            _relData = new RelativeData(_context);
            _patientData = new PatientData(_context);
            _crud = new CRUD(_config);
            _rdvm = new RelativeDiaryVM();
            _clinContext = context;
            _adminContext = adminContext;
            _docContext = documentContext;
            _config = config;                        
            _staffUser = new StaffUserData(_context);
            _docsData = new DocumentsData(_docContext);
            _diaryActionData = new DiaryActionData(_adminContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }
        public IActionResult Index(int relID)
        {
            _rdvm.relative = _relData.GetRelativeDetails(relID);
            _rdvm.relativeDiaryList = _relDiaryData.GetRelativeDiaryList(relID);
            _rdvm.patient = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relative.WMFACSID);

            return View(_rdvm);
        }


        [HttpGet]
        public IActionResult AddNew(int relID)
        {
            _rdvm.relative = _relData.GetRelativeDetails(relID);
            _rdvm.patient = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relative.WMFACSID);
            _rdvm.documentsList = _docsData.GetDocumentsList();
            _rdvm.diaryActionsList = _diaryActionData.GetDiaryActions();

            

            return View(_rdvm);
        }

        [HttpPost]
        public IActionResult AddNew(int relID, DateTime diaryDate, string actionCode, string docCode, string? diaryText, string? letterText)
        {
            int success = _crud.CallStoredProcedure("RelativeDiary", "Create", relID, 0, 0, actionCode, docCode, "", "", User.Identity.Name, diaryDate,
                null, false, false, 0,0,0,diaryText,letterText);

            if(success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiary-new(SQL)" }); }

            return RedirectToAction("Index", "RelativeDiary", new { relID = relID });
        }

        [HttpGet]
        public IActionResult Edit(int diaryID)
        {
            _rdvm.relativeDiary = _relDiaryData.GetRelativeDiaryDetails(diaryID);
            _rdvm.patient = _patientData.GetPatientDetailsByWMFACSID(_rdvm.relative.WMFACSID);

            return View(_rdvm);
        }

        [HttpPost]
        public IActionResult Edit(int diaryID, DateTime diaryDate, string actionCode, string docCode, string? diaryText, string? letterText, bool? isNotReturnExpected=false)
        {
            _rdvm.relativeDiary = _relDiaryData.GetRelativeDiaryDetails(diaryID);
            int relID = _rdvm.relativeDiary.RelsID;

            int success = _crud.CallStoredProcedure("RelativeDiary", "Edit", diaryID, 0, 0, actionCode, docCode, "", "", User.Identity.Name, diaryDate,
                null, isNotReturnExpected, false, 0, 0, 0, diaryText, letterText);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "RelativeDiary-new(SQL)" }); }

            return RedirectToAction("Index", "RelativeDiary", new { relID = relID });
        }
    }
}
