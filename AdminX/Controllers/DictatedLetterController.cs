using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminX.Controllers
{
    public class DictatedLetterController : Controller
    {
        //private readonly ClinicalContext _clinContext; //these are necessary because we can't debug the letter controller otherwise!!!
        //private readonly DocumentContext _docContext;
        //private readonly AdminContext _adminContext;
        private readonly LetterController _lc;
        private readonly DictatedLetterVM _lvm;
        private readonly IConfiguration _config;
        private readonly ICRUD _crud;
        private readonly IPatientDataAsync _patientData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IActivityDataAsync _activityData;
        private readonly IDictatedLetterDataAsync _dictatedLetterData;
        private readonly IExternalClinicianDataAsync _externalClinicianData;
        private readonly IExternalFacilityDataAsync _externalFacilityData;
        private readonly IDictatedLettersReportDataAsync _dotReportData;
        private readonly IAuditServiceAsync _audit;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IPAddressFinder _ip;

        public DictatedLetterController(IConfiguration config, IStaffUserDataAsync staffUser, IPatientDataAsync patient, IActivityDataAsync activity, IDictatedLetterDataAsync dictatedLetter, 
            IExternalClinicianDataAsync externalClinician, IExternalFacilityDataAsync externalFacility, IDictatedLettersReportDataAsync dictatedLettersReport, IAuditServiceAsync audit, 
            IConstantsDataAsync constants, LetterController letterController) //, ClinicalContext clinicalContext, DocumentContext documentContext)
        {            
            _config = config;
            _crud = new CRUD(_config);
            _lvm = new DictatedLetterVM();
            _staffUser = staffUser;
            _patientData = patient;
            _activityData = activity;
            _dictatedLetterData = dictatedLetter;
            _externalClinicianData = externalClinician;
            _externalFacilityData = externalFacility;
            _dotReportData = dictatedLettersReport;
            _lc = letterController;
            _audit = audit;
            _constantsData = constants;
            _ip = new IPAddressFinder(HttpContext);
            //_clinContext = clinicalContext;
            //_docContext = documentContext;
        }

        [Authorize]
        public async Task<IActionResult> Index(string? staffCode)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Letters", "", _ip.GetIPAddress());

                var letters = await _dictatedLetterData.GetDictatedLettersListFull();

                if(staffCode != null)
                {
                    letters = letters.Where(l => l.LetterFromCode == staffCode).ToList();
                }
                _lvm.clinicalStaff = await _staffUser.GetClinicalStaffList();
                _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing").ToList();
                _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                    new BreadcrumbItem { Text = "Letters" }
                };

                _lvm.dictatedlettersReportClinicians = await _dotReportData.GetReportClinicians();
                _lvm.dictatedLettersSecTeamReports = await _dotReportData.GetDictatedLettersReport();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName="DictatedLetter" });
            }
        }

        public async Task<IActionResult> DictatedLettersForPatient(string cguNo)
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return NotFound();
                }

                var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Letters", "", _ip.GetIPAddress());

                _lvm.patientDetails = await _patientData.GetPatientDetailsByCGUNo(cguNo);

                if (_lvm.patientDetails != null)
                {
                    var letters = await _dictatedLetterData.GetDictatedLettersForPatient(_lvm.patientDetails.MPI);

                    _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing" && l.Status != "Printed").ToList();
                    _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();
                }

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {            
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Letter", "ID=" + id.ToString(), _ip.GetIPAddress());

                _lvm.dictatedLetters = await _dictatedLetterData.GetDictatedLetterDetails(id);
                _lvm.dictatedLettersPatients = await _dictatedLetterData.GetDictatedLettersPatientsList(id);
                _lvm.dictatedLettersCopies = await _dictatedLetterData.GetDictatedLettersCopiesList(id);
                _lvm.patients = await _dictatedLetterData.GetDictatedLetterPatientsList(id);
                _lvm.staffMemberList = await _staffUser.GetClinicalStaffList();
                _lvm.secteams = await _staffUser.GetSecTeamsList();
                _lvm.consultants = await _staffUser.GetConsultantsList();
                _lvm.gcs = await _staffUser.GetGCList();                
                int? mpi = _lvm.dictatedLetters.MPI;
                int? refID = _lvm.dictatedLetters.RefID;
                _lvm.patientDetails = await _patientData.GetPatientDetails(mpi.GetValueOrDefault());
                _lvm.activityDetails = await _activityData.GetActivityDetails(refID.GetValueOrDefault());
                string sGPCode = _lvm.patientDetails.GP_Facility_Code;
                if (sGPCode == null ) { sGPCode = "Unknown1"; } //because obviously there are nulls.
                string sRefFacCode = _lvm.activityDetails.REF_FAC;
                if (sRefFacCode == null) { sRefFacCode = "Unknown"; } 
                string sRefPhysCode = _lvm.activityDetails.REF_PHYS;
                if (sRefPhysCode == null) { sRefPhysCode = "Unknown"; }
                _lvm.referrerFacility = await _externalFacilityData.GetFacilityDetails(sRefFacCode);                
                _lvm.referrer = await _externalClinicianData.GetClinicianDetails(sRefPhysCode);                
                _lvm.GPFacility = await _externalFacilityData.GetFacilityDetails(sGPCode);
                var facility = await _externalFacilityData.GetFacilityList();
                _lvm.facilities = facility.Where(f => f.IS_GP_SURGERY == 0).ToList();
                var clinicians = await _externalClinicianData.GetClinicianList();
                _lvm.clinicians = clinicians.Where(c => c.Is_GP == 0 && c.LAST_NAME != null && c.FACILITY != null).ToList();                
                List<ExternalCliniciansAndFacilities> extClins = _lvm.clinicians.Where(c => c.POSITION != null).ToList();                
                _lvm.specialities = await _externalClinicianData.GetClinicianTypeList();
                _lvm.edmsLink = await _constantsData.GetConstant("GEMRLink", 1);

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem { Text = "Letters", Controller = "DictatedLetter", Action = "Index"  },

                    new BreadcrumbItem { Text = "Update" }
                };

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int dID, string status, string letterTo, string letterFromCode, string letterContent, string letterContentBold, 
            bool isAddresseeChanged, string secTeam, string consultant, string gc, string dateDictated, string letterToCode, string enclosures, string comments, string letterFrom)
        {
            try
            {
                DateTime dDateDictated = new DateTime();
                dDateDictated = DateTime.Parse(dateDictated);
                //two updates required - one to update the addressee (if addressee has changed)
                if (isAddresseeChanged)
                {
                    int success2 = _crud.CallStoredProcedure("Letter", "UpdateAddresses", dID, 0, 0, "", letterToCode, letterFromCode, letterTo, User.Identity.Name);

                    if (success2 == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-edit(SQL)" }); }
                }

                int success = _crud.CallStoredProcedure("Letter", "Update", dID, 0, 0, status, enclosures, letterContentBold, letterContent, User.Identity.Name, dDateDictated, null, false, false, 0, 0, 0, secTeam, consultant, gc, 0,0,0,0,0, comments, letterFrom );

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-edit(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unapprove(int dID)
        {
            try
            {
               
                var currentLetter = await _dictatedLetterData.GetDictatedLetterDetails(dID);

                if (currentLetter == null)
                {
                    return RedirectToAction("ErrorHome", "Error", new { error = "Letter not found.", formName = "DictatedLetter-Unapprove" });
                }

                int success = _crud.CallStoredProcedure(
                    "Letter",
                    "Update",
                    dID,
                    0,
                    0,
                    "Draft", 
                    currentLetter.Enclosures ?? "",
                    currentLetter.LetterContentBold ?? "",
                    currentLetter.LetterContent,
                    User.Identity.Name,
                    currentLetter.DateDictated, 
                    null,
                    false,
                    false,
                    0,
                    0,
                    0,
                    currentLetter.SecTeam,
                    currentLetter.Consultant,
                    currentLetter.GeneticCounsellor,
                    0, 0, 0, 0, 0,
                    currentLetter.Comments,
                    currentLetter.LetterFrom
                );

                if (success == 0)
                {
                    return RedirectToAction("ErrorHome", "Error", new { error = "Database update failed.", formName = "DictatedLetter-Unapprove" });
                }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter" });
            }
        }

        public async Task<IActionResult> Create(int id)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);                
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Create Letter", "New Letter", _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "Create", 0, id, 0, "", "", staffCode, "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-create(SQL)" }); }

                //var dot = await _clinContext.DictatedLetters.OrderByDescending(l => l.CreatedDate).FirstOrDefaultAsync(l => l.RefID == id);
                var dot = await _dictatedLetterData.GetDictatedLettersList(User.Identity.Name);
                int dID = dot.OrderByDescending(d => d.DoTID).First().DoTID;

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-create" });
            }
        }

        public async Task<IActionResult> Delete(int dID)
        {
            try 
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Delete Letter", "ID=" + dID.ToString(), _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "Delete", dID, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-create(SQL)" }); }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-create" });
            }
        }        

        [HttpPost]
        public async Task<IActionResult> AddPatientToDOT(int pID, int dID)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add Patient to DOT", "ID=" + dID.ToString(), _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "AddFamilyMember", dID, pID, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addPt(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-addPt" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCCToDOT(int dID, string cc)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add CC to DOT", "ID=" + dID.ToString(), _ip.GetIPAddress());

                int success = _crud.CallStoredProcedure("Letter", "AddCC", dID, 0, 0, cc, "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-addCC(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-addCC" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCCFromDOT(int id)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Delete CC from DOT", "ID=" + id.ToString(), _ip.GetIPAddress());

                var letter = await _dictatedLetterData.GetDictatedLetterCopyDetails(id);
                
                int dID = letter.DotID;

                int success = _crud.CallStoredProcedure("Letter", "DeleteCC", id, 0, 0, "", "", "", "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-deleteCC(SQL)" }); }

                return RedirectToAction("Edit", new { id = dID });
                
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-deleteCC" });
            }
        }

        public async Task<IActionResult> PreviewDOT(int dID)
        {
            try
            {
                //LetterControllerLOCAL lc = new LetterControllerLOCAL(_clinContext, _docContext);

                //lc.PrintDOTPDF(dID, User.Identity.Name, false);

                _lc.PrintDOTPDF(dID, User.Identity.Name, true);
                //return RedirectToAction("Edit", new { id = dID });
                return File($"~/DOTLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-preview" });
            }

            
        }

       
        public async Task<IActionResult> PrintDOT(int dID)
        {
            try
            {                
                _lc.PrintDOTPDF(dID, User.Identity.Name, false);
                //return RedirectToAction("Edit", new { id = dID });
                //string user = User.Identity.Name;

                _crud.CallStoredProcedure("DictatedLetter", "Print", dID,0,0,"","","","",User.Identity.Name,null,null); //updates everything to say the letter was printed
                return File($"~/DOTLetterPreviews/preview-{User.Identity.Name}.pdf", "Application/PDF");
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-preview" });
            }
        }

     

        public async Task<IActionResult> ActivityItems(int id)
        {
            try
            {
                _lvm.patientDetails = await _patientData.GetPatientDetails(id);
                _lvm.activities = await _activityData.GetActivityList(id);
                
                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-activityitems" });
            }

        }
    }
}
