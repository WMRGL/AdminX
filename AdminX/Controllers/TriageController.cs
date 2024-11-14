using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using System.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Azure.Core;

namespace AdminX.Controllers
{
    public class TriageController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly ICPVM _ivm;
        //private readonly LetterController _lc;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPathwayData _pathwayData;
        //private readonly IPriorityData _priorityData;
        private readonly IReferralData _referralData;
        private readonly ITriageData _triageData;
        //private readonly IICPActionData _icpActionData;
        //private readonly IRiskData _riskData;
        //private readonly ISurveillanceData _survData;
        //private readonly ITestEligibilityData _testEligibilityData;
        private readonly IDiaryData _diaryData;
        private readonly IRelativeData _relativeData;
        //private readonly ICancerRequestData _cancerRequestData;
        private readonly IExternalClinicianData _clinicianData;
        //private readonly IDocumentsData _documentsData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;

        public TriageController(ClinicalContext clinContext, DocumentContext docContext, IConfiguration config)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _config = config;
            _ivm = new ICPVM();
            _staffUser = new StaffUserData(_clinContext);
            _pathwayData = new PathwayData(_clinContext);
            //_priorityData = new PriorityData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _triageData = new TriageData(_clinContext);
            //_icpActionData = new ICPActionData(_clinContext);
            //_riskData = new RiskData(_clinContext);
            //_survData = new SurveillanceData(_clinContext);
            //_testEligibilityData = new TestEligibilityData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            //_cancerRequestData = new CancerRequestData(_clinContext);
            _clinicianData = new ExternalClinicianData(_clinContext);
            //_documentsData = new DocumentsData(_docContext);
            _crud = new CRUD(_config);
            //_lc = new LetterController(_clinContext, _docContext);
            _audit = new AuditService(_config);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {       
                _ivm.staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(_ivm.staffCode, "AdminX - Triage");

                _ivm.triages = _triageData.GetTriageListFull();
                _ivm.icpCancerListOwn = _triageData.GetCancerICPList(User.Identity.Name).Where(r => r.GC_CODE == _ivm.staffCode).ToList();
                _ivm.icpCancerListOther = _triageData.GetCancerICPList(User.Identity.Name).Where(r => r.ToBeReviewedby == User.Identity.Name.ToUpper()).ToList();
                int bleep = _ivm.icpCancerListOwn.Count();                
                int bloop = _ivm.icpCancerListOther.Count();
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
                //var triages = await _clinContext.Triages.FirstOrDefaultAsync(t => t.ICPID == id);
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - ICP Details", "ID=" + id.ToString());

                _ivm.triage = _triageData.GetTriageDetails(id);

                if (_triageData.GetCancerICPCountByICPID(id) > 0 || _triageData.GetGeneralICPCountByICPID(id) > 0) { _ivm.isICPTriageStarted = true; }

                if (_ivm.triage == null) { return RedirectToAction("NotFound", "WIP"); }

                //_ivm.triage = _vm.GetTriageDetails(id);
                _ivm.referralDetails = _referralData.GetReferralDetails(_ivm.triage.RefID);
                
                _ivm.icpGeneral = _triageData.GetGeneralICPDetails(id);
                _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
                //_ivm.cancerActionsList = _icpActionData.GetICPCancerActionsList();
                //_ivm.generalActionsList = _icpActionData.GetICPGeneralActionsList();
                //_ivm.generalActionsList2 = _icpActionData.GetICPGeneralActionsList2();
                _ivm.loggedOnUserType = _staffUser.GetStaffMemberDetails(User.Identity.Name).CLINIC_SCHEDULER_GROUPS;
                //_ivm.priorityList = _priorityData.GetPriorityList();
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-ICP" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> DoGeneralTriage(int icpID, string? facility, int? duration, string? comment, bool isSPR, bool isChild, int? tp, int? tp2c, 
            int? tp2nc, int? wlPriority)
        {
            try
            {
                var icp = await _clinContext.Triages.FirstOrDefaultAsync(i => i.ICPID == icpID);
                var referral = await _clinContext.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
                var staffmember = await _clinContext.StaffMembers.FirstOrDefaultAsync(s => s.EMPLOYEE_NUMBER == User.Identity.Name);
                int mpi = icp.MPI;
                int refID = icp.RefID;
                int tp2;
                string referrer = referral.ReferrerCode;
                string sApptIntent = "";
                string sStaffType = staffmember.CLINIC_SCHEDULER_GROUPS;

                if (comment == null) { comment = ""; }

                if (tp2c != null) { tp2 = tp2c.GetValueOrDefault(); }
                else { tp2 = tp2nc.GetValueOrDefault(); }

                if (tp2 == 3) { sApptIntent = "CLICS"; }

                if (sStaffType == "Consultant")
                {
                    if (facility != null && facility != "") // && clinician != null && clinician != "")
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), 0,
                        facility, sApptIntent, "", comment, User.Identity.Name, null, null, isSPR, isChild, duration);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage" }); }
                        //_crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, facility, "General", "", comment, User.Identity.Name);

                        //_lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer);
                    }
                    else
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, tp.GetValueOrDefault(), 0,
                        "", sApptIntent, "", comment, User.Identity.Name);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage(SQL)" }); }
                    }
                }
                else
                {
                    if (facility != null && facility != "") // && clinician != null && clinician != "")
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, 0, tp2,
                        facility, sApptIntent, "", comment, User.Identity.Name, null, null, isSPR, isChild, duration);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage(SQL)" }); }
                        //_crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, facility, "General", "", comment, User.Identity.Name);

                        //_lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer);
                    }
                    else
                    {
                        int success = _crud.CallStoredProcedure("ICP General", "Triage", icpID, 0, tp2,
                        "", sApptIntent, "", comment, User.Identity.Name);

                        if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genTriage(SQL)" }); }
                    }
                }
                //add to waiting list
                if (facility != null && facility != "")
                {
                    int success = _crud.CallStoredProcedure("Waiting List", "Create", mpi, wlPriority.GetValueOrDefault(), referral.refid, facility, "General", "",
                        comment, User.Identity.Name);

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genAddWL(SQL)" }); }
                }

                if (tp2 == 2) //CTB letter
                {
                    //LetterController _lc = new LetterController(_docContext);
                    int success = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", "CTBAck", "", "", User.Identity.Name);
                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-genDiaryUpdate(SQL)" }); }
                    int diaryID = _diaryData.GetLatestDiaryByRefID(refID, "CTBAck").DiaryID;
                    //_lc.DoPDF(184, mpi, referral.refid, User.Identity.Name, referrer, "", "", 0, "", false, false, diaryID);
                }

                if (tp2 == 6) //Dictate letter
                { 
                    int success2 = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "", "", "", User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }
                }

                if (tp2 == 7) //Reject letter
                {
                    //LetterController _lc = new LetterController(_docContext);
                    //_lc.DoPDF(208, mpi, referral.refid, User.Identity.Name, referrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-genTriage" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DoCancerTriage(int icpID, int action)
        {
            try
            {
                var icp = await _clinContext.Triages.FirstOrDefaultAsync(i => i.ICPID == icpID);
                int mpi = icp.MPI;
                int refID = icp.RefID;
                var referral = await _clinContext.Referrals.FirstOrDefaultAsync(r => r.refid == icp.RefID);
                string referrer = referral.ReferrerCode;

                CRUD _crud = new CRUD(_config);
                int success = _crud.CallStoredProcedure("ICP Cancer", "Triage", icpID, action, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canTriage(SQL)" }); }

                if (action == 5)
                {
                    //LetterController _lc = new LetterController(_clinContext, _docContext);
                    //_lc.DoPDF(156, mpi, refID, User.Identity.Name, referrer);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canTriage" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancerReview(int id)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP");}

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Cancer Review", "ID=" + id.ToString());

                
                _ivm.staffMembers = _staffUser.GetClinicalStaffList();
                _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
                //_ivm.riskList = _riskData.GetRiskList(id);
                //_ivm.surveillanceList = _survData.GetSurveillanceList(_ivm.icpCancer.MPI);
                //_ivm.eligibilityList = _testEligibilityData.GetTestingEligibilityList(_ivm.icpCancer.MPI);
                //_ivm.documentList = _documentsData.GetDocumentsList().Where(d => (d.DocCode.StartsWith("O") && d.DocGroup == "Outcome") || d.DocCode.Contains("PrC")).ToList();
                //_ivm.cancerReviewActionsLists = _icpActionData.GetICPCancerReviewActionsList();
                
                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canReview" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancerReview(int id, string finalReview, string? clinician = "", string? clinic = "", string? comments = "", 
            string? addNotes = "", bool? isNotForCrossBooking = false, int? letter = 0, string? toBeReviewedBy = "", string? freeText1="") //, 
            //int? request = 0, string? freeText2 = "", int? relID = 0, string? clinicianCode = "", string? siteText = "")
        {
            string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
            //bool isfinalReview = false;
            _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
            var icpDetails = _triageData.GetICPDetails(_ivm.icpCancer.ICPID);
            string reviewText = "";
            //string finalReviewText = "";
            string reviewBy = "";
            //_ivm.cancerReviewActionsLists = _icpActionData.GetICPCancerReviewActionsList();
            //DateTime finalReviewDate = DateTime.Parse("1900-01-01");
            int mpi = icpDetails.MPI;
            int refID = icpDetails.REFID;

            if (letter != null && letter != 0)
            {                
                //_ivm.cancerAction = _icpActionData.GetICPCancerAction(letter.GetValueOrDefault());
                string docCode = _ivm.cancerAction.DocCode;

                if (letter != 1 && letter != 11)
                {
                    reviewText = docCode;

                    if (reviewText != null) { reviewText = reviewText + " letter on " + DateTime.Now.ToString("dd/MM/yyyy") + " by " + _staffUser.GetStaffMemberDetails(User.Identity.Name).NAME; }

                    string diaryText = "";
                    //int letterID = _documentsData.GetDocumentDetailsByDocCode(docCode).DocContentID;

                    int successDiary = _crud.CallStoredProcedure("Diary", "Create", refID, mpi, 0, "L", docCode, "", diaryText, User.Identity.Name, null, null, false, false);
                    int diaryID = _diaryData.GetLatestDiaryByRefID(refID, docCode).DiaryID;

                    if (letter == 3)
                    {
                        int successDOT = _crud.CallStoredProcedure("Letter", "Create", 0, refID, 0, "", "", staffCode, "", User.Identity.Name);

                        if (successDOT == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }
                    }
                    else
                    {
                        //_lc.DoPDF(letterID, mpi, refID, User.Identity.Name, _referralData.GetReferralDetails(refID).ReferrerCode, "", "", 0, "", false, false, 0, freeText1, "", 0);
                    }

                    if (successDiary == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canDiaryUpdate(SQL)" }); }
                }
            }
                        
            
            if (toBeReviewedBy == null) { toBeReviewedBy = ""; }//because the default value isn't being assigned for some reason!            

            if(clinician != null && clinician != "")
            {                
                int successWL = _crud.CallStoredProcedure("Waiting List", "Create", mpi, 0, 0, clinic, "Cancer", clinician, comments,
                    User.Identity.Name, null, null, false, false); //where is "not for cross booking" stored?

                if (successWL == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canAddWL(SQL)" }); }
            }

            //if(finalReview == "Yes")
            //{
                //finalReviewText = reviewText;
            reviewBy = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                //finalReviewDate = DateTime.Today;
            //}

            int success = _crud.CallStoredProcedure("ICP Cancer", "ICP Review", id, letter.GetValueOrDefault(), 0, reviewBy, finalReview, toBeReviewedBy, addNotes,
                    User.Identity.Name, null, null, false, false);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Triage-canReview" }); }

            return RedirectToAction("CancerReview", new { id = id });
        }
        [HttpGet]
        public async Task<IActionResult> FurtherRequest(int id)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Cancer Post Clinic Letter", "ID=" + id.ToString());
                _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
                //_ivm.cancerRequestsList = _cancerRequestData.GetCancerRequestsList();
                _ivm.relatives = _relativeData.GetRelativesList(_ivm.icpCancer.MPI);
                _ivm.clinicians = _clinicianData.GetClinicianList();
                _ivm.specialities = _clinicianData.GetClinicianTypeList();

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-postclinicletter" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FurtherRequest(int id, int request, string? freeText = "", int? relID = 0, string? clinicianCode = "", string? siteText = "")
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                //_ivm.cancerRequest = _cancerRequestData.GetCancerRequestDetail(request);
                _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
                var icpDetails = _triageData.GetICPDetails(_ivm.icpCancer.ICPID);               
                //DateTime finalReviewDate = DateTime.Parse("1900-01-01");
                int mpi = icpDetails.MPI;
                int refID = icpDetails.REFID;                

                

                
                return RedirectToAction("CancerReview", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-postclinicletter" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VHRPro(int id)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Cancer Post Clinic Letter", "ID=" + id.ToString());

                _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
                var icpDetails = _triageData.GetICPDetails(_ivm.icpCancer.ICPID);
                _ivm.screeningCoordinators = _clinicianData.GetClinicianList().Where(c => (c.LAST_NAME ?? "").ToLower().Contains("breast")
                    || (c.POSITION ?? "").ToLower().Contains("breast"))
                    .OrderBy(c => c.FACILITY)
                    .ToList();
                
                int mpi = icpDetails.MPI;
                int refID = icpDetails.REFID;

                return View(_ivm);

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-vhrpro" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> VHRPro(int id, string? freeText, string? clinician)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Cancer Post Clinic Letter", "ID=" + id.ToString());

                _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
                var icpDetails = _triageData.GetICPDetails(_ivm.icpCancer.ICPID);
                //DateTime finalReviewDate = DateTime.Parse("1900-01-01");
                int mpi = icpDetails.MPI;
                int refID = icpDetails.REFID;

               // VHRController _vhrc = new VHRController(_clinContext, _docContext);

                //_vhrc.DoPDF(213, mpi, id, User.Identity.Name, _referralData.GetReferralDetails(refID).ReferrerCode, freeText);
                //_lc.DoPDF(214, mpi, refID, User.Identity.Name, _referralData.GetReferralDetails(refID).ReferrerCode, freeText, "", 0
                  //  , "", false, false, 0, "", "", 0, clinician);

                return RedirectToAction("CancerReview", new { id = id });

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-vhrpro" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> RiskAndSurveillance(int id)
        {
            try
            {
                if (id == null) { return RedirectToAction("NotFound", "WIP"); }

                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Risk and Surveillance", "ID=" + id.ToString());

        //        _ivm.riskDetails = _riskData.GetRiskDetails(id);
                int mpi = _referralData.GetReferralDetails(_ivm.riskDetails.RefID).MPI;               
          //      _ivm.surveillanceList = _survData.GetSurveillanceList(mpi).Where(s => s.RiskID == id).ToList();

                return View(_ivm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canRiskSurv" });
            }
        }

        

        
    }
}
