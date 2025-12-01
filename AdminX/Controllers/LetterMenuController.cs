using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using AdminX.Data;
using APIControllers.Data;
using APIControllers.Controllers;

namespace AdminX.Controllers
{
    public class LetterMenuController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ClinicalContext _context;
        private readonly DocumentContext _documentContext;
        private readonly AdminContext _adminContext;
        private readonly APIContext _apiContext;
        private readonly IDocumentsData _documentsData;
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relData;
        private readonly IReferralData _referralData;
        private readonly LettersMenuVM _lvm;
        private readonly LetterController _lc;
        private readonly ICRUD _crud;
        private readonly IDiaryData _diaryData;
        private readonly ILeafletData _leafletData;
        private readonly IExternalClinicianData _clinicianData;
        private readonly IStaffUserData _staffData;
        private readonly IAuditService _audit;
        private readonly IPAddressFinder _ip;
        private readonly APIController _api;
        private readonly PhenotipsMirrorData _mirrorData;

        public LetterMenuController(ClinicalContext context, DocumentContext documentContext, AdminContext adminContext, APIContext apiContext, IConfiguration config)
        {
            _config = config;   
            _context = context;
            _documentContext = documentContext;
            _adminContext = adminContext;
            _apiContext = apiContext;
            _documentsData = new DocumentsData(_documentContext);
            _patientData = new PatientData(_context);
            _relData = new RelativeData(_context);
            _referralData = new ReferralData(_context);
            _lvm = new LettersMenuVM();
            _lc = new LetterController(_context, _documentContext);
            _crud = new CRUD(_config);
            _diaryData = new DiaryData(_context);
            _leafletData = new LeafletData(_documentContext);
            _clinicianData = new ExternalClinicianData(_context);
            _staffData = new StaffUserData(_context);
            _audit = new AuditService(_config);
            _ip = new IPAddressFinder(HttpContext);
            _api = new APIController(_apiContext, _config);
            _mirrorData = new PhenotipsMirrorData(_context);
        }

        public IActionResult Index(int id, bool? isRelative = false)
        {
            try
            {
                string staffCode = _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Letter Menu", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _lvm.isRelative = isRelative.GetValueOrDefault();

                if (!isRelative.GetValueOrDefault())
                {
                    _lvm.patient = _patientData.GetPatientDetails(id);

                }
                else
                {
                    _lvm.relative = _relData.GetRelativeDetails(id);
                    _lvm.patient = _patientData.GetPatientDetailsByWMFACSID(_lvm.relative.WMFACSID);
                }
                int mpi = _lvm.patient.MPI;
                _lvm.docsListStandard = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "Standard").ToList();
                _lvm.docsListMedRec = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "MEDREC").ToList();
                _lvm.docsListDNA = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "DNATS").ToList();
                _lvm.docsListOutcome = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "Outcome").ToList();
                _lvm.docsListReport = _documentsData.GetDocumentsList().Where(d => d.DocGroup == "REPORTS").ToList();
                _lvm.leaflets = new List<Leaflet>();
                _lvm.referralList = _referralData.GetActiveReferralsListForPatient(mpi);
                _lvm.clinicianList = _clinicianData.GetClinicianList().Where(c => c.POSITION.Contains("Histo") || c.SPECIALITY.Contains("Histo")).ToList();
                _lvm.staffMemberList = _staffData.GetStaffMemberListAll();

                string salutation = "";

                if (_lvm.patient.DOB > DateTime.Now.AddYears(-16))
                {
                    salutation = $"Parent / Guardian of {_lvm.patient.Title} {_lvm.patient.FIRSTNAME} {_lvm.patient.LASTNAME}";
                }
                else
                {
                    salutation = $"{_lvm.patient.Title} {_lvm.patient.FIRSTNAME} {_lvm.patient.LASTNAME}";
                }

                _lvm.patientAddress = salutation + Environment.NewLine;


                _lvm.patientAddress += $"{_lvm.patient.ADDRESS1}" + Environment.NewLine;
                if (_lvm.patient.ADDRESS2 != null) { _lvm.patientAddress += $"{_lvm.patient.ADDRESS2}" + Environment.NewLine; }
                if (_lvm.patient.ADDRESS3 != null) { _lvm.patientAddress += $"{_lvm.patient.ADDRESS3}" + Environment.NewLine; }
                if (_lvm.patient.ADDRESS4 != null) { _lvm.patientAddress += $"{_lvm.patient.ADDRESS4}" + Environment.NewLine; }
                _lvm.patientAddress += $"{_lvm.patient.POSTCODE}";

                if (_lvm.referral != null)
                {
                    if (_lvm.referral.PATHWAY == "Cancer")
                    {
                        _lvm.leaflets = _leafletData.GetCancerLeafletsList();
                    }
                    else if (_lvm.referral.PATHWAY == "General")
                    {
                        _lvm.leaflets = _leafletData.GetGeneralLeafletsList();
                    }
                    else
                    {
                        _lvm.leaflets = _leafletData.GetAllLeafletsList(); //in case there is no referral pathway listed
                    }
                }
                else
                {
                    _lvm.leaflets = _leafletData.GetAllLeafletsList(); //in case there is no referral at all
                }

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "LetterMenu" });
            }
        }

        [HttpPost]
        public IActionResult DoLetter(int refID, string docCode, bool isPreview, string? additionalText, string? enclosures, string? clinicianCode, string? letterFrom, string? letterTo,
            int? relID = 0, int? leafletID = 0, bool? isRelative = false)
        {
            try
            {
                int docID = 0;

                if (docCode != "HS")
                {
                    docID = _documentsData.GetDocumentDetailsByDocCode(docCode).DocContentID;
                }
                _lvm.referral = _referralData.GetReferralDetails(refID);
                _lvm.patient = _patientData.GetPatientDetails(_lvm.referral.MPI);

                int mpi = _lvm.patient.MPI;
                int diaryID = 0;

                if (isRelative.GetValueOrDefault())
                {
                    _lvm.relative = _relData.GetRelativeDetails(relID.GetValueOrDefault());
                }

                if (!isPreview) //don't create a diary entry for every preview!!
                {
                    int success = 0;

                    if (!isRelative.GetValueOrDefault())
                    {
                        success = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name, null, null, false, false);
                    }
                    else
                    {
                        success = _crud.CallStoredProcedure("RelativeDiary", "Create", refID, relID.GetValueOrDefault(), 0, "L", docCode, letterFrom, letterTo, User.Identity.Name, null, null, false, false);
                    }

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DoLetter-diary insert(SQL)" }); }

                    diaryID = _diaryData.GetLatestDiaryByRefID(refID, docCode).DiaryID; //get the diary ID of the entry just created to add to the letter's filename
                }

                if (docCode == "HS")
                {
                    HSController hs = new HSController(_context, _documentContext, _adminContext);
                    hs.PrintHSForm(refID, diaryID, User.Identity.Name, isPreview);
                }
                else
                {                    
                    string qrCode = "";

                    if(_mirrorData.GetPhenotipsPatientByID(mpi) != null)
                    {
                        //APIControllerLOCAL api = new APIControllerLOCAL(_apiContext, _config);
                        string ppqType = "";
                        
                        if (_api.CheckPPQExists(mpi, _lvm.referral.PATHWAY).Result)
                        {
                            qrCode = _api.GetPPQQRCode(mpi, _lvm.referral.PATHWAY).Result;
                        }
                    }

                    //LetterControllerLOCAL lc = new LetterControllerLOCAL(_context, _documentContext); //for testing
                    _lc.DoPDF(docID, mpi, refID, User.Identity.Name, _lvm.referral.ReferrerCode, additionalText, enclosures, 0, "", false, false, diaryID, "", "", relID, clinicianCode,
                           "", null, isPreview, qrCode, leafletID);
                }



                if (isPreview)
                {
                    return File($"~/StandardLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
                }

                TempData["SuccessMessage"] = "Letter has been sent to EDMS ";
                return RedirectToAction("Index", new { id = mpi, isRelative = isRelative });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DoLetter" });
            }
        }
    }
}
