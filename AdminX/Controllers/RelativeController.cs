using AdminX.Meta;
using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APIControllers.Controllers;
//using APIControllers.Data;

namespace AdminX.Controllers
{
    public class RelativeController : Controller
    {
        
        //private readonly ClinicalContext _clinContext;        
        //private readonly APIContext _apiContext;        
        private readonly RelativeVM _rvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;        
        private readonly IPatientDataAsync _patientData;
        private readonly IRelativeDataAsync _relativeData;
        private readonly ITitleDataAsync _titleData;
        private readonly ICRUD _crud;
        private readonly IAuditServiceAsync _audit;
        private readonly APIController _api;
        private readonly IPAddressFinder _ip;

        public RelativeController(IConfiguration config, IStaffUserDataAsync staffUser, IPatientDataAsync patient, IRelativeDataAsync relative, ITitleDataAsync title, ICRUD crud, 
            IAuditServiceAsync audit, APIController api)
        {
            
            //_clinContext = context;            
            //_apiContext = apiContext;
            _config = config;
            _crud = crud;
            _staffUser = staffUser;
            _patientData = patient;
            _relativeData = relative;
            _titleData = title;
            _rvm = new RelativeVM();
            _audit = audit;
            _api = api;
            _ip = new IPAddressFinder(HttpContext);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RelativeDetails(int id)
        {
            try
            {
                _rvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _rvm.staffMember.STAFF_CODE;
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - View Relative", "ID=" + id.ToString(), _ip.GetIPAddress());
                _rvm.relativeDetails = await _relativeData.GetRelativeDetails(id);
                _rvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rvm.relativeDetails.WMFACSID);
                _rvm.MPI = _rvm.patient.MPI;

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "RelativeDetails" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Relative", "ID=" + id.ToString());

                _rvm.relativeDetails = await _relativeData.GetRelativeDetails(id);
                _rvm.patient = await _patientData.GetPatientDetailsByWMFACSID(_rvm.relativeDetails.WMFACSID);
                _rvm.MPI = _rvm.patient.MPI;
                var rel = await _relativeData.GetRelationsList();
                _rvm.relationList = rel.OrderBy(r => r.ReportOrder).ToList();
                _rvm.genderList = await _relativeData.GetGenderList();

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-edit" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string? title, string forename1, string? forename2, string surname, string relation, string dob, string dod,
            int isAffected, string sex, string? prevSurname)
        {
            try
            {
                _rvm.relativeDetails = await _relativeData.GetRelativeDetails(id);

                //making sure all the nulls have values

                DateTime birthDate = new DateTime();
                DateTime deathDate = new DateTime();

                if (dob != null)
                {
                    birthDate = DateTime.Parse(dob);
                }
                else
                {
                    birthDate = DateTime.Parse("1900-01-01");
                }

                if (dod != null)
                {
                    deathDate = DateTime.Parse(dod);
                }
                else
                {
                    deathDate = DateTime.Parse("1900-01-01");
                }

                if (title == null)
                {
                    title = "";
                }

                if (forename2 == null)
                {
                    forename2 = "";
                }

                int success = _crud.CallStoredProcedure("Relative", "Edit", id, isAffected, 0, title, forename1, forename2, surname,
                        User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex, prevSurname);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Relative-edit(SQL)" }); }

                return RedirectToAction("RelativeDetails", "Relative", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-edit" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(int wmfacsid)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add Relative", "WMFACSID=" + wmfacsid.ToString(), _ip.GetIPAddress());

                _rvm.WMFACSID = wmfacsid;
                var pat = await _patientData.GetPatientDetailsByWMFACSID(wmfacsid);
                _rvm.MPI = pat.MPI;
                var rels = await _relativeData.GetRelationsList();
                _rvm.relationList = rels.OrderBy(r => r.ReportOrder).ToList();
                _rvm.genderList = await _relativeData.GetGenderList();
                _rvm.titleList = await _titleData.GetTitlesList();
                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-add" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> AddNew(int wmfacsid, string title, string forename1,
            string forename2, string surname, string relation, string sDOB, string sDOD,
            int isAffected, string sex, string? RelAKA, string? RelSurnameBirth, string? RelSurnamePrevious, string RelAdd1, string? RelAdd2,
            string? RelAdd3, string? RelAdd4, string? RelTel, string? RelSalutation, string? RelNHSNo, string? RelPC1, string? DeathAge, string? RelAlive, string? Notes)
        {
            try
            {
                var pat = await _patientData.GetPatientDetailsByWMFACSID(wmfacsid);
                _rvm.MPI = pat.MPI;
                DateTime birthDate = new DateTime();
                DateTime deathDate = new DateTime();

                if (sDOB != null)
                {
                    birthDate = DateTime.Parse(sDOB);
                }
                else
                {
                    birthDate = DateTime.Parse("1/1/1900");
                }

                if (sDOD != null)
                {
                    deathDate = DateTime.Parse(sDOD);
                }
                else
                {
                    deathDate = DateTime.Parse("1/1/1900");
                }

                if (forename2 == null)
                {
                    forename2 = "";
                }

                int success = _crud.CallStoredProcedure("Relative", "Create", wmfacsid, isAffected, 0, title, forename1, forename2, surname,
                    User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex, RelAKA, 0,0,0,0,0, RelSurnameBirth, RelSurnamePrevious,
                    RelAdd1, RelAdd2, RelAdd3, RelAdd4, RelTel, RelSalutation, RelNHSNo, RelPC1, DeathAge, RelAlive, Notes);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Relative-add(SQL)" }); }

                var patient = _patientData.GetPatientDetailsByWMFACSID(wmfacsid);
                TempData["SuccessMessage"] = "New relative added";
                return RedirectToAction("PatientDetails", "Patient", new { id = _rvm.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-add" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ImportRelatives(int id)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Import Revatives", "WMFACSID=" + id.ToString(), _ip.GetIPAddress());

                _rvm.patient = await _patientData.GetPatientDetailsByWMFACSID(id);
                _rvm.cgudbRelativesList = await _relativeData.GetRelativesList(_rvm.patient.MPI);
                _rvm.relationsList = await _relativeData.GetRelationsList();

                List<APIControllers.Models.Relative> relList = new List<APIControllers.Models.Relative>();
                _rvm.phenotipsRelativesList = new List<ClinicalXPDataConnections.Models.Relative>();

                relList = await _api.ImportRelativesFromPhenotips(_rvm.patient.MPI);

                foreach(var r in relList)
                {
                    _rvm.phenotipsRelativesList.Add(new ClinicalXPDataConnections.Models.Relative { WMFACSID = r.WMFACSID, RelTitle = r.RelTitle, RelForename1 = r.RelForename1,
                    RelForename2 = r.RelForename2, RelSurname = r.RelSurname, DOB = r.DOB, DOD = r.DOD, RelSex = r.RelSex });
                }

                //_rvm.phenotipsRelativesList = await _api.ImportRelativesFromPhenotips(_rvm.patient.MPI);

                return View(_rvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Relative-add" });
            }
        }        
    }
}