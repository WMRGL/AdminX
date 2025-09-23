using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APIControllers.Controllers;
using APIControllers.Data;

namespace AdminX.Controllers
{
    public class RelativeController : Controller
    {
        
        private readonly ClinicalContext _clinContext;        
        private readonly APIContext _apiContext;        
        private readonly RelativeVM _rvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;        
        private readonly IPatientData _patientData;
        private readonly IRelativeData _relativeData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;
        private readonly APIController _api;
        private readonly ITitleData _titleData;

        public RelativeController(ClinicalContext context, APIContext apiContext, IConfiguration config)
        {
            
            _clinContext = context;            
            _apiContext = apiContext;
            _config = config;            
            _crud = new CRUD(_config);
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _relativeData = new RelativeData(_clinContext);            
            _rvm = new RelativeVM();
            _audit = new AuditService(_config);
            _api = new APIController(_apiContext, _config);
            _titleData = new TitleData(_clinContext);

        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RelativeDetails(int id)
        {
            try
            {
                _rvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - View Relative", "ID=" + id.ToString(), _ip.GetIPAddress());
                _rvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rvm.patient = _patientData.GetPatientDetailsByWMFACSID(_rvm.relativeDetails.WMFACSID);
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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Relative", "ID=" + id.ToString());

                _rvm.relativeDetails = _relativeData.GetRelativeDetails(id);
                _rvm.patient = _patientData.GetPatientDetailsByWMFACSID(_rvm.relativeDetails.WMFACSID);
                _rvm.MPI = _rvm.patient.MPI;
                _rvm.relationList = _relativeData.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rvm.genderList = _relativeData.GetGenderList();

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
                _rvm.relativeDetails = _relativeData.GetRelativeDetails(id);

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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Add Relative", "WMFACSID=" + wmfacsid.ToString(), _ip.GetIPAddress());

                _rvm.WMFACSID = wmfacsid;
                _rvm.MPI = _patientData.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
                _rvm.relationsList = _relativeData.GetRelationsList().OrderBy(r => r.ReportOrder).ToList();
                _rvm.genderList = _relativeData.GetGenderList();
                _rvm.titleList = _titleData.GetTitlesList();

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
            int isAffected, string sex)
        {
            try
            {
                _rvm.MPI = _patientData.GetPatientDetailsByWMFACSID(wmfacsid).MPI;
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
                    User.Identity.Name, birthDate, deathDate, false, false, 0, 0, 0, relation, sex);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Relative-add(SQL)" }); }

                var patient = _patientData.GetPatientDetailsByWMFACSID(wmfacsid);

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
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Import Revatives", "WMFACSID=" + id.ToString(), _ip.GetIPAddress());

                _rvm.patient = _patientData.GetPatientDetailsByWMFACSID(id);
                _rvm.cgudbRelativesList = _relativeData.GetRelativesList(_rvm.patient.MPI);
                _rvm.relationsList = _relativeData.GetRelationsList();

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