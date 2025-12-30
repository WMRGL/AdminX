using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class NewEpicPatientController : Controller
    {
        //ClinicalContext _context;
        //AdminContext _adminContext;
        IPatientDataAsync _patientData;
        IPedigreeDataAsync _pedigreeData;
        INewPatientSearchDataAsync _searchData;
        IPatientSearchDataAsync _patientSearchData;
        IStaffUserDataAsync _staffUserData;
        IConfiguration _configuration;
        ICRUD _crud;
        NewEpicPatientVM _epvm;
        IPAddressFinder _ip;
        IAuditServiceAsync _audit;

        public NewEpicPatientController(IConfiguration configuration, IPatientDataAsync patient, INewPatientSearchDataAsync newSearch, IStaffUserDataAsync staffUser, IPedigreeDataAsync pedigree,
            IAuditServiceAsync audit, ICRUD crud, IPatientSearchDataAsync patSearch)
        {
            //_context = context;
            //_adminContext = adminContext;
            _patientData = patient;
            _searchData = newSearch;
            _epvm = new NewEpicPatientVM();
            _configuration = configuration;
            _crud = crud;
            _staffUserData = staffUser;
            _pedigreeData = pedigree;
            _patientSearchData = patSearch;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                string staffCode = await _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Epic Patients", "", _ip.GetIPAddress());
                _epvm.patientList = await _patientData.GetPatientsWithoutCGUNumbers();

                return View(_epvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "NewEpicPatients" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EpicPatientSearch(int id)
        {
            try
            { 
                string staffCode =  await _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Epic Patient Search", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _epvm.patient = await _patientData.GetPatientDetails(id);

                _crud.NewPatientSearch(_epvm.patient.FIRSTNAME, _epvm.patient.LASTNAME, _epvm.patient.DOB.GetValueOrDefault(), _epvm.patient.POSTCODE, _epvm.patient.SOCIAL_SECURITY, staffCode);

                int searchID = await _searchData.GetPatientSearchID(staffCode);

                List<PatientSearchResults> searchResults = await _searchData.GetPatientSearchResults(searchID);

                _epvm.patientSearchResultsList = searchResults.Where(r => r.ResultSource == "Patient").Where(p => p.MPI != id).ToList();
                _epvm.relativeSearchResultsList = searchResults.Where(r => r.ResultSource == "Relative").ToList();
                _epvm.pedigreeSearchResultsList = searchResults.Where(r => r.ResultSource == "Pedigree").ToList();

                return View(_epvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "NewEpicPatientSearch" });
            }
        }

        
        public async Task<IActionResult> AssignCGUNumber(int id, string? fileNumber)
        {
            try
            {
                string staffCode = await _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Epic Patient Search", "MPI=" + id.ToString(), _ip.GetIPAddress());

                string cguNumber = "";

                if (fileNumber == null || fileNumber == "")
                {
                    cguNumber = _pedigreeData.GetNextPedigreeNumber() + ".0";
                }
                else
                {
                    string pedno = fileNumber.Substring(0, fileNumber.IndexOf("."));

                    List<Patient> patList = await _patientSearchData.GetPatientsListByCGUNo(pedno); //get the next CGU point number
                    int patientNumber = patList.Count();
                                      

                    if (_patientData.GetPatientDetailsByCGUNo(pedno + "." + patientNumber.ToString()) == null)
                    {
                        cguNumber = pedno + "." + patientNumber.ToString();
                    }
                }

                int success = await _crud.PatientAssignCGUNumber(id, cguNumber, User.Identity.Name);

                return RedirectToAction("PatientDetails", "Patient", new { id = id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "AssignCGUNumber" });
            }
        }
    }
}
