using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Meta;
using AdminX.Models;
using AdminX.Data;

namespace AdminX.Controllers
{
    public class DictatedLetterController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly AdminContext _adminContext;
        private readonly LetterController _lc;
        private readonly DictatedLetterVM _lvm;
        private readonly IConfiguration _config;
        private readonly ICRUD _crud;
        private readonly IPatientData _patientData;
        private readonly IStaffUserData _staffUser;
        private readonly IActivityData _activityData;
        private readonly IDictatedLetterData _dictatedLetterData;
        private readonly IExternalClinicianData _externalClinicianData;
        private readonly IExternalFacilityData _externalFacilityData;
        private readonly IDictatedLettersReportData _dotReportData;
        private readonly IAuditService _audit;

        public DictatedLetterController(IConfiguration config, ClinicalContext clinContext, DocumentContext docContext, AdminContext adminContext)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _adminContext = adminContext;
            _config = config;
            _crud = new CRUD(_config);
            _lvm = new DictatedLetterVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _activityData = new ActivityData(_clinContext);
            _dictatedLetterData = new DictatedLetterData(_clinContext);
            _externalClinicianData = new ExternalClinicianData(_clinContext);
            _externalFacilityData = new ExternalFacilityData(_clinContext);
            _dotReportData = new DictatedLettersReportData(_adminContext);
            _lc = new LetterController(_clinContext, _docContext);
            _audit = new AuditService(_config);
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

                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Letters", "", _ip.GetIPAddress());

                var letters = _dictatedLetterData.GetDictatedLettersListFull();

                if(staffCode != null)
                {
                    letters = letters.Where(l => l.LetterFromCode == staffCode).ToList();
                }
                _lvm.clinicalStaff = _staffUser.GetClinicalStaffList();
                _lvm.dictatedLettersForApproval = letters.Where(l => l.Status != "For Printing").ToList();
                _lvm.dictatedLettersForPrinting = letters.Where(l => l.Status == "For Printing").ToList();

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                    new BreadcrumbItem { Text = "Letters" }
                };

                _lvm.dictatedlettersReportClinicians = _dotReportData.GetReportClinicians();
                _lvm.dictatedLettersSecTeamReports = _dotReportData.GetDictatedLettersReport();

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

                var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Letters", "", _ip.GetIPAddress());

                _lvm.patientDetails = _patientData.GetPatientDetailsByCGUNo(cguNo);

                if (_lvm.patientDetails != null)
                {
                    var letters = _dictatedLetterData.GetDictatedLettersForPatient(_lvm.patientDetails.MPI);

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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Letter", "ID=" + id.ToString(), _ip.GetIPAddress());

                _lvm.dictatedLetters = _dictatedLetterData.GetDictatedLetterDetails(id);
                _lvm.dictatedLettersPatients = _dictatedLetterData.GetDictatedLettersPatientsList(id);
                _lvm.dictatedLettersCopies = _dictatedLetterData.GetDictatedLettersCopiesList(id);
                _lvm.patients = _dictatedLetterData.GetDictatedLetterPatientsList(id);
                _lvm.staffMemberList = _staffUser.GetClinicalStaffList();
                _lvm.secteams = _staffUser.GetSecTeamsList();
                _lvm.consultants = _staffUser.GetConsultantsList();
                _lvm.gcs = _staffUser.GetGCList();                
                int? mpi = _lvm.dictatedLetters.MPI;
                int? refID = _lvm.dictatedLetters.RefID;
                _lvm.patientDetails = _patientData.GetPatientDetails(mpi.GetValueOrDefault());
                _lvm.activityDetails = _activityData.GetActivityDetails(refID.GetValueOrDefault());
                string sGPCode = _lvm.patientDetails.GP_Facility_Code;
                if (sGPCode == null ) { sGPCode = "Unknown1"; } //because obviously there are nulls.
                string sRefFacCode = _lvm.activityDetails.REF_FAC;
                if (sRefFacCode == null) { sRefFacCode = "Unknown"; } 
                string sRefPhysCode = _lvm.activityDetails.REF_PHYS;
                if (sRefPhysCode == null) { sRefPhysCode = "Unknown"; }
                _lvm.referrerFacility = _externalFacilityData.GetFacilityDetails(sRefFacCode);                
                _lvm.referrer = _externalClinicianData.GetClinicianDetails(sRefPhysCode);                
                _lvm.GPFacility = _externalFacilityData.GetFacilityDetails(sGPCode);
                _lvm.facilities = _externalFacilityData.GetFacilityList().Where(f => f.IS_GP_SURGERY == 0).ToList();
                _lvm.clinicians = _externalClinicianData.GetClinicianList().Where(c => c.Is_GP == 0 && c.LAST_NAME != null && c.FACILITY != null).ToList();                
                List<ExternalCliniciansAndFacilities> extClins = _lvm.clinicians.Where(c => c.POSITION != null).ToList();                
                _lvm.specialities = _externalClinicianData.GetClinicianTypeList();

                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int dID, string status, string letterTo, string letterFromCode, string letterContent, string letterContentBold, 
            bool isAddresseeChanged, string secTeam, string consultant, string gc, string dateDictated, string letterToCode, string enclosures, string comments)
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

                int success = _crud.CallStoredProcedure("Letter", "Update", dID, 0, 0, status, enclosures, letterContentBold, letterContent, User.Identity.Name, dDateDictated, null, false, false, 0, 0, 0, secTeam, consultant, gc, 0,0,0,0,0, comments);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-edit(SQL)" }); }

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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                int success = _crud.CallStoredProcedure("Letter", "Create", 0, id, 0, "", "", staffCode, "", User.Identity.Name);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "DictatedLetter-create(SQL)" }); }

                var dot = await _clinContext.DictatedLetters.OrderByDescending(l => l.CreatedDate).FirstOrDefaultAsync(l => l.RefID == id);
                int dID = dot.DoTID;

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
                var letter = _dictatedLetterData.GetDictatedLetterCopyDetails(id);
                
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
                _lvm.patientDetails = _patientData.GetPatientDetails(id);
                _lvm.activities = _activityData.GetActivityList(id);
                
                return View(_lvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "DictatedLetter-activityitems" });
            }

        }
    }
}
