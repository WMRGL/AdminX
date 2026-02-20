using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AdminX.Controllers
{
    public class TriageController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly DocumentContext _docContext;
        private readonly ICPVM _ivm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPathwayDataAsync _pathwayData;
        private readonly IReferralDataAsync _referralData;
        private readonly ITriageDataAsync _triageData;        
        private readonly IDiaryDataAsync _diaryData;
        private readonly IRelativeDataAsync _relativeData;
        private readonly IExternalClinicianDataAsync _clinicianData;
        private readonly IICPActionDataAsync _icpActionData;
        private readonly ICRUD _crud;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;
        private readonly LetterController _lc;
        private readonly IDocumentsDataAsync _docData;
        private readonly IConstantsDataAsync _constantsData;

        public TriageController(IConfiguration config, IStaffUserDataAsync staffUser, IPathwayDataAsync pathway, IReferralDataAsync referral, ITriageDataAsync triage, IDiaryDataAsync diary,
            IRelativeDataAsync relative, IExternalClinicianDataAsync extClinician, IICPActionDataAsync icpAction, ICRUD crud, IAuditServiceAsync audit, LetterController lc, IDocumentsDataAsync doc,
            IConstantsDataAsync constantsData)
        {
            //_clinContext = clinContext;
            //_docContext = docContext;
            _config = config;
            _ivm = new ICPVM();
            _staffUser = staffUser;
            _pathwayData = pathway;
            _referralData = referral;
            _triageData = triage;
            _diaryData = diary;
            _relativeData = relative;
            _clinicianData = extClinician;
            _icpActionData = icpAction;
            _crud = crud;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
            _lc = lc;
            _docData = doc;
            _constantsData = constantsData;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {       
                _ivm.staffCode = await _staffUser.GetStaffCode(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(_ivm.staffCode, "AdminX - Triage","",_ip.GetIPAddress());

                _ivm.triages = await _triageData.GetTriageListFull();
                //_ivm.icpCancerList = _triageData.GetCancerICPList(User.Identity.Name).ToList();

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                new BreadcrumbItem { Text = "Triage" }
            };

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage" });
            }
        }

        [Authorize]
        public async Task<IActionResult> ICPDetails(int id)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - ICP Details", "ID=" + id.ToString(), _ip.GetIPAddress());

                _ivm.triage = await _triageData.GetTriageDetails(id);

                int cicp = await _triageData.GetCancerICPCountByICPID(id);
                int gicp = await _triageData.GetGeneralICPCountByICPID(id);

                if (cicp > 0 || gicp > 0) { _ivm.isICPTriageStarted = true; }

                if (_ivm.triage == null) { return RedirectToAction("NotFound", "WIP"); }

                _ivm.referralDetails = await _referralData.GetReferralDetails(_ivm.triage.RefID);
                
                _ivm.icpGeneral = await _triageData.GetGeneralICPDetails(id);
                _ivm.icpCancer = await _triageData.GetCancerICPDetails(id);
                AgeCalculator calc = new AgeCalculator();
                _ivm.referralAgeDays = calc.DateDifferenceDay(_ivm.referralDetails.RefDate.GetValueOrDefault(), DateTime.Today);
                _ivm.referralAgeWeeks = calc.DateDifferenceWeek(_ivm.referralDetails.RefDate.GetValueOrDefault(), DateTime.Today);
                _ivm.cancerActionsList = await _icpActionData.GetICPCancerActionsList();
                _ivm.generalActionsList = await _icpActionData.GetICPGeneralActionsList();
                _ivm.generalActionsList2 = await _icpActionData.GetICPGeneralActionsList2();

                _ivm.clinicians = await _clinicianData.GetClinicianList(); 
                _ivm.clinicians = _ivm.clinicians.Where(c => c.SPECIALITY != null).ToList();
                _ivm.clinicians = _ivm.clinicians.Where(c => c.SPECIALITY.Contains("Genetics")).ToList();

                string canDeleteICP = await _constantsData.GetConstant("DeleteICPBtn", 1);

                if (canDeleteICP.ToUpper().Contains(User.Identity.Name.ToUpper()))
                {
                    _ivm.canDeleteICP = true;
                }

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Triage",
                    Controller = "Triage",
                    Action = "Index",

                },
                new BreadcrumbItem { Text = "Complete" }
            };

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-ICP" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DoGeneralTriage(int icpID, string? clinician, string? facility, int? duration, string? comment, bool isSPR, bool isChild, int? tp1, int? tp2c, 
            int? tp2nc, int? wlPriority, string? clinician2, string? facility2, int? duration2, string? comment2, bool isChild2)
        {
            try
            {
                var icp = await _triageData.GetICPDetails(icpID);
                int refID = icp.REFID;
                var referral = await _referralData.GetReferralDetails(refID);
                var staffmember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);

                int mpi = icp.MPI;
                
                int tp2;
                string referrer = referral.ReferrerCode;
                string sApptIntent = "";
                string sStaffType = staffmember.CLINIC_SCHEDULER_GROUPS;
                string sType = "ICP General";

                if (comment == null) { comment = ""; }

                if (tp2c != null) { tp2 = tp2c.GetValueOrDefault(); }
                else { tp2 = tp2nc.GetValueOrDefault(); }

                if (tp2 == 3) { sApptIntent = "CLICS"; }

                if(referral.Pathway_Subset == "Haemoglobinopathy")
                {
                    sType = "Haemoglobinopathy";
                }
                
                int success = _crud.TriageDetail(sType, "Triage", icpID, tp1.GetValueOrDefault(), tp2, clinician, facility, comment, sApptIntent, User.Identity.Name, clinician2, facility2, comment2,
                    duration, duration2, isSPR, isChild, isChild2);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage" }); }
                                
                                
                //add to cons waiting list
                if (facility != null && facility != "")
                {
                    int successwl = _crud.AddToWaitingList(mpi, clinician, facility, wlPriority.GetValueOrDefault(), referral.refid, User.Identity.Name);

                    if (successwl == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genAddWL(SQL)" }); }
                }

                if (facility2 != null && facility2 != "")
                {
                    int successwl = _crud.AddToWaitingList(mpi, clinician2, facility2, wlPriority.GetValueOrDefault(), referral.refid, User.Identity.Name);

                    if (successwl == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genAddWL(SQL)" }); }
                }


                if (tp2 == 2) //CTB letter
                {                    
                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", "CTBAck", "", "", User.Identity.Name);
                    if (successDiary == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genDiaryUpdate(SQL)" }); }
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, "CTBAck");
                    int diaryID = diary.DiaryID;
                    _lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID);
                }

                if (tp2 == 6) //Dictate letter
                { 
                    int successDOT = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "", "", "", User.Identity.Name);

                    if (successDOT == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-DOT(SQL)" }); }
                }

                if (tp2 == 7) //Reject letter
                {                    
                    _lc.DoPDF(208, mpi, referral.refid, User.Identity.Name, referrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-genTriage" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoCancerTriage(int icpID, int action, string? clinician = "")
        {
            try
            {
                var icp = await _triageData.GetICPDetails(icpID);
                int mpi = icp.MPI;
                int refID = icp.REFID;
                var referral = await _referralData.GetReferralDetails(refID);
                string referrer = referral.ReferrerCode;

                CRUD _crud = new CRUD(_config);
                int success = _crud.TriageDetail("ICP Cancer", "Triage", icpID, action, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canTriage(SQL)" }); }
                                
                int docID = 0;
                var icpactionList = await _icpActionData.GetICPCancerActionsList();
                var icpAction = icpactionList.FirstOrDefault(a => a.ID == action);
                bool needsLetter = icpAction.LetterRequired;

                docID = icpAction.RelatedLetterID.GetValueOrDefault();
                int diaryID = 0;
                string docCode = "";

                if (docID != 0)
                {
                    var doc = await _docData.GetDocumentDetails(docID);
                    docCode = doc.DocCode;
                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", "", User.Identity.Name);
                    var diary = await _diaryData.GetLatestDiaryByRefID(refID, docCode);
                    diaryID = diary.DiaryID;
                }                

                switch (action)
                {
                    case 1:
                        //do nothing
                        break;
                    case 2:                        
                        //_lc.DoPDF(5, mpi, refID, User.Identity.Name, referrer, "", "", 0,"", false, false, diaryID); //send Ack
                        break;
                    case 3:
                        //do nothing //not currently used
                        break;
                    case 4:
                        //_lc.DoPDF(227, mpi, refID, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID); //send RejFHAW
                        break;
                    case 5:
                        //_lc.DoPDF(156, mpi, refID, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID); // send KC
                        break;
                    case 6:
                        //do nothing //no letter, patient sent FHF                        
                        _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "B", "", "", "No referral letter, patient referred by FHF", User.Identity.Name, referral.RefDate, null, false, false);
                        break;
                    case 7:
                        //do nothing //no letter, self referred
                        _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "B", "", "", "No referral letter, patient self referred at clinic", User.Identity.Name, referral.RefDate, null, false, false);
                        _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "A", "", "", "", User.Identity.Name, DateTime.Now, null, false, false);
                        break;
                    case 8:
                        //_lc.DoPDF(182, mpi, refID, User.Identity.Name, referrer,"","",0,"",false,false,diaryID,"","",0,clinician);//send OOR1 and OOR2 //(out of area)
                        //_lc.DoPDF(183, mpi, refID, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID);
                        break;
                    case 9:
                        //send DNMRC //not meet criteria                        
                        //_lc.DoPDF(202, mpi, refID, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID, "", "");
                        break;
                    case 10:
                        //_lc.DoPDF(218, mpi, refID, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID); //send RejFH
                        break;
                }

                if(needsLetter)
                {
                    return RedirectToAction("Index", "LetterMenu", new { id = mpi, isRelative = false, letterGroup = "Standard", docCode = docCode });
                }
                

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canTriage" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoHaemoglobinopathyTriage (int icpID)
        {
            try
            {
                int success = _crud.TriageDetail("Haemoglobinopathy", "", icpID, 0,0,"","","","",User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-hbeTriage(SQL)" }); }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-hbeTriage" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetICP(int icpid, int mpi)
        {
            try
            {
                string login = User.Identity?.Name ?? "Unknown";


                int success = _crud.TriageDetail(
                     sType: "Triage",
                     sOperation: "Reset ICP",
                     sLogin: login,
                     int1: icpid,
                     int2: 0, int3: 0,
                     string1: "", string2: "", string3: "", string4: "", string5: "", string6: ""
                 );


                return RedirectToAction("PatientDetails", "Patient", new
                {
                    id = mpi,
                    message = "ICP has been reset",
                    success = true
                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "MarkReferralDeleted" });
            }
        }
    }
}
