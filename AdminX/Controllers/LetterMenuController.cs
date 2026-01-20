using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using APIControllers.Data;
using APIControllers.Controllers;

namespace AdminX.Controllers
{
    public class LetterMenuController : Controller
    {
        private readonly IConfiguration _config;
        //private readonly ClinicalContext _context;
        //private readonly DocumentContext _documentContext;
        //private readonly AdminContext _adminContext;
        //private readonly APIContext _apiContext;
        private readonly IDocumentsDataAsync _documentsData;        
        private readonly IPatientDataAsync _patientData;
        private readonly IRelativeDataAsync _relData;
        private readonly IReferralDataAsync _referralData;
        private readonly LettersMenuVM _lvm;
        private readonly LetterController _lc;
        private readonly HSController _hs;
        private readonly ICRUD _crud;
        private readonly IDiaryDataAsync _diaryData;
        private readonly ILeafletDataAsync _leafletData;
        private readonly IExternalClinicianDataAsync _clinicianData;
        private readonly IStaffUserDataAsync _staffData;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;
        private readonly APIController _api;
        private readonly IPhenotipsMirrorDataAsync _mirrorData;

        public LetterMenuController(IConfiguration config, IDocumentsDataAsync documents, IPatientDataAsync patient, IRelativeDataAsync relative, IReferralDataAsync referral, LetterController letter, 
            ICRUD crud, IDiaryDataAsync diary, ILeafletDataAsync leaflet, IExternalClinicianDataAsync clinician, IStaffUserDataAsync staffUser, IPhenotipsMirrorDataAsync ptmirror, 
            IAuditServiceAsync audit, HSController hs, APIController api)
        {
            _config = config;   
            //_context = context;
            //_documentContext = documentContext;
            //_adminContext = adminContext;
            //_apiContext = apiContext;
            _documentsData = documents;
            _patientData = patient;
            _relData = relative;
            _referralData = referral;
            _lvm = new LettersMenuVM();
            _lc = letter;
            _crud = crud;
            _diaryData = diary;
            _leafletData = leaflet;
            _clinicianData = clinician;
            _staffData = staffUser;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
            _api = api;
            _mirrorData = ptmirror;
            _hs = hs;
        }

        public async Task<IActionResult> Index(int id, bool? isRelative = false)
        {
            try
            {
                string staffCode = await _staffData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Letter Menu", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _lvm.isRelative = isRelative.GetValueOrDefault();

                if (!isRelative.GetValueOrDefault())
                {
                    _lvm.patient = await _patientData.GetPatientDetails(id);

                }
                else
                {
                    _lvm.relative = await _relData.GetRelativeDetails(id);
                    _lvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_lvm.relative.WMFACSID);
                }
                int mpi = _lvm.patient.MPI;
                var docList = await _documentsData.GetDocumentsList();
                _lvm.docsListStandard = docList.Where(d => d.DocGroup == "Standard").ToList();
                _lvm.docsListMedRec = docList.Where(d => d.DocGroup == "MEDREC").ToList();
                _lvm.docsListDNA = docList.Where(d => d.DocGroup == "DNATS").ToList();
                _lvm.docsListOutcome = docList.Where(d => d.DocGroup == "Outcome").ToList();
                _lvm.docsListReport = docList.Where(d => d.DocGroup == "REPORTS").ToList();
                _lvm.docContentList = await _documentsData.GetDocumentsContentList();
                _lvm.leaflets = new List<Leaflet>();
                _lvm.referralList = await _referralData.GetActiveReferralsListForPatient(mpi);
                var clinList = await _clinicianData.GetClinicianList();
                var clins = new List<ExternalCliniciansAndFacilities>();
                _lvm.patGP = await _clinicianData.GetPatientGPReferrer(mpi);
                clins.Add(_lvm.patGP);
                _lvm.clinicianList = clins;
                _lvm.histoList = clinList.Where(c => c.POSITION.Contains("Histo") || c.SPECIALITY.Contains("Histo")).ToList();
                _lvm.breastList = clinList.Where(c => c.POSITION.Contains("Breast") || c.SPECIALITY.Contains("Breast")).ToList();
                _lvm.geneticsList = clinList.Where(c => c.POSITION.Contains("Genetics") || c.SPECIALITY.Contains("Genetics")).ToList();
                

                _lvm.staffMemberList = await _staffData.GetStaffMemberListAll();

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
                        _lvm.leaflets = await _leafletData.GetCancerLeafletsList();
                    }
                    else if (_lvm.referral.PATHWAY == "General")
                    {
                        _lvm.leaflets = await _leafletData.GetGeneralLeafletsList();
                    }
                    else
                    {
                        _lvm.leaflets = await _leafletData.GetAllLeafletsList(); //in case there is no referral pathway listed
                    }
                }
                else
                {
                    _lvm.leaflets = await _leafletData.GetAllLeafletsList(); //in case there is no referral at all
                }

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "LetterMenu" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoLetter(int refID, string docCode, bool isPreview, string? additionalText, string? enclosures, string? clinicianCode, string? letterFrom, string? letterTo,
            int? relID = 0, int? leafletID = 0, bool? isRelative = false)
        {
            try
            {
                int docID = 0;

                if (docCode != "HS")
                {
                    var doc = await _documentsData.GetDocumentDetailsByDocCode(docCode);
                    docID = doc.DocContentID;
                }
                _lvm.referral = await _referralData.GetReferralDetails(refID);
                _lvm.patient = await _patientData.GetPatientDetails(_lvm.referral.MPI);

                int mpi = _lvm.patient.MPI;
                int diaryID = 0;

                if (isRelative.GetValueOrDefault())
                {
                    _lvm.relative = await _relData.GetRelativeDetails(relID.GetValueOrDefault());
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

                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    diaryID = diary.DiaryID; //get the diary ID of the entry just created to add to the letter's filename
                }

                if (docCode == "HS")
                {
                    //HSController hs = new HSController(_context, _documentContext, _adminContext);
                    _hs.PrintHSForm(refID, diaryID, User.Identity.Name, isPreview);
                }
                else
                {                    
                    string qrCode = "";

                    if(await _mirrorData.GetPhenotipsPatientByID(mpi) != null)
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
