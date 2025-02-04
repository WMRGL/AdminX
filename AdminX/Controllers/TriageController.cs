using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using System.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using AdminX.Models;

namespace AdminX.Controllers
{
    public class TriageController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly ICPVM _ivm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPathwayData _pathwayData;
        private readonly IReferralData _referralData;
        private readonly ITriageData _triageData;        
        private readonly IDiaryData _diaryData;
        private readonly IRelativeData _relativeData;
        private readonly IExternalClinicianData _clinicianData;
        private readonly IICPActionData _icpActionData;
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
            _referralData = new ReferralData(_clinContext);
            _triageData = new TriageData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _clinicianData = new ExternalClinicianData(_clinContext);
            _icpActionData = new ICPActionData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {       
                _ivm.staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(_ivm.staffCode, "AdminX - Triage","",_ip.GetIPAddress());

                _ivm.triages = _triageData.GetTriageListFull();
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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - ICP Details", "ID=" + id.ToString(), _ip.GetIPAddress());

                _ivm.triage = _triageData.GetTriageDetails(id);

                if (_triageData.GetCancerICPCountByICPID(id) > 0 || _triageData.GetGeneralICPCountByICPID(id) > 0) { _ivm.isICPTriageStarted = true; }

                if (_ivm.triage == null) { return RedirectToAction("NotFound", "WIP"); }

                _ivm.referralDetails = _referralData.GetReferralDetails(_ivm.triage.RefID);
                
                _ivm.icpGeneral = _triageData.GetGeneralICPDetails(id);
                _ivm.icpCancer = _triageData.GetCancerICPDetails(id);
                AgeCalculator calc = new AgeCalculator();
                _ivm.referralAgeDays = calc.DateDifferenceDay(_ivm.referralDetails.RefDate.GetValueOrDefault(), DateTime.Today);
                _ivm.referralAgeWeeks = calc.DateDifferenceWeek(_ivm.referralDetails.RefDate.GetValueOrDefault(), DateTime.Today);
                _ivm.cancerActionsList = _icpActionData.GetICPCancerActionsList();
                _ivm.generalActionsList = _icpActionData.GetICPGeneralActionsList();
                _ivm.generalActionsList2 = _icpActionData.GetICPGeneralActionsList2();

                _ivm.clinicians = _clinicianData.GetClinicianList(); 
                _ivm.clinicians = _ivm.clinicians.Where(c => c.SPECIALITY != null).ToList();
                _ivm.clinicians = _ivm.clinicians.Where(c => c.SPECIALITY.Contains("Genetics")).ToList();

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Triage",
                    Controller = "Triage",
                    Action = "Index",

                },
                new BreadcrumbItem { Text = "Add" }
            };

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
        public async Task<IActionResult> DoCancerTriage(int icpID, int action, string? clinician = "")
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

                LetterController _lc = new LetterController(_clinContext, _docContext);

                switch (action)
                {
                    case 1:
                        //do nothing
                        break;
                    case 2:
                        _lc.DoPDF(5, mpi, refID, User.Identity.Name, referrer); //send Ack
                        break;
                    case 3:
                        //do nothing //not currently used
                        break;
                    case 4:
                        _lc.DoPDF(227, mpi, refID, User.Identity.Name, referrer); //send RejFHAW
                        break;
                    case 5:
                        _lc.DoPDF(156, mpi, refID, User.Identity.Name, referrer); // send KC
                        break;
                    case 6:
                        //do nothing //no letter, patient sent FHF
                        break;
                    case 7:
                        //do nothing //no letter, self referred
                        break;
                    case 8:
                        _lc.DoPDF(182, mpi, refID, User.Identity.Name, referrer,"","",0,"",false,false,0,"","",0,clinician);//send OOR1 and OOR2 (//)out of area)
                        _lc.DoPDF(183, mpi, refID, User.Identity.Name, referrer);
                        break;
                    case 9:
                        //send DNMRC //not meet criteria
                        break;
                    case 10:
                        _lc.DoPDF(218, mpi, refID, User.Identity.Name, referrer); //send RejFH
                        break;
                }
                

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Triage-canTriage" });
            }
        }        
    }
}
