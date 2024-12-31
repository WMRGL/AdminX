using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;

namespace AdminX.Controllers
{
    public class DiaryController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly DiaryVM _dvm;
        private readonly IConfiguration _config;
        private readonly IPatientData _patientData;
        private readonly IDiaryData _diaryData;
        private readonly IReferralData _referralData;
        private readonly IStaffUserData _staffUser;
        private readonly IDocumentsData _docsData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public DiaryController(ClinicalContext context, DocumentContext documentContext, IConfiguration config)
        {
            _clinContext = context;
            _docContext = documentContext;
            _config = config;
            _dvm = new DiaryVM();
            _patientData = new PatientData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _referralData = new ReferralData(_clinContext);            
            _staffUser = new StaffUserData(_clinContext);    
            _docsData = new DocumentsData(_docContext);
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
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Diary Details", "DiaryID=" + id.ToString());

                _dvm.diary = _diaryData.GetDiaryEntry(id);
                _dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);


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
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add New Diary Entry", "MPI=" + mpi.ToString());

                //_dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                _dvm.patient = _patientData.GetPatientDetails(mpi);
                _dvm.referralsList = _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                _dvm.documents = _docsData.GetDocumentsList();


                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DiaryDetails" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddNew(int refID, DateTime diaryDate, string diaryCons, string diaryAction, string diaryText, string docCode)
        {
            try
            {
                if (refID == null)
                {
                    return NotFound();
                }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Diary Details", "DiaryID=" + refID.ToString());

                _dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                _dvm.referralsList = _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                _dvm.documents = _docsData.GetDocumentsList();

                return View(_dvm);
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

                _dvm.diary = _diaryData.GetDiaryEntry(id);
                _dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                //_dvm.referralsList = _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                _dvm.documents = _docsData.GetDocumentsList();

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit DiaryEntry", "DiaryID=" + id.ToString());

                
                return View(_dvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formname = "Diary-edit" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int diaryID, int refID, DateTime diaryDate, string diaryCons, string diaryAction, string diaryText, string docCode)
        {
            try
            {
                if (diaryID == null)
                {
                    return NotFound();
                }

                //_dvm.diary = _diaryData.
                _dvm.patient = _patientData.GetPatientDetailsByWMFACSID(_dvm.diary.WMFACSID);
                //_dvm.referralsList = _referralData.GetActiveReferralsListForPatient(_dvm.patient.MPI);
                _dvm.documents = _docsData.GetDocumentsList();


                return RedirectToAction("DiaryDetails", new { id = diaryID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Diary-edit" });
            }
        }
    }
}
