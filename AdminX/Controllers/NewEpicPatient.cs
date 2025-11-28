using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class NewEpicPatient : Controller
    {
        ClinicalContext _context;
        AdminContext _adminContext;
        IPatientData _patientData;
        IPedigreeData _pedigreeData;
        INewPatientSearchData _searchData;
        IPatientSearchData _patientSearchData;
        IStaffUserData _staffUserData;
        IConfiguration _configuration;
        ICRUD _crud;
        NewEpicPatientVM _epvm;
        IPAddressFinder _ip;
        IAuditService _audit;

        public NewEpicPatient(ClinicalContext context, AdminContext adminContext, IConfiguration configuration)
        {
            _context = context;
            _adminContext = adminContext;
            _patientData = new PatientData(_context);
            _searchData = new NewPatientSearchData(_adminContext);
            _epvm = new NewEpicPatientVM();
            _configuration = configuration;
            _crud = new CRUD(_configuration);
            _staffUserData = new StaffUserData(_context);
            _pedigreeData = new PedigreeData(_context);
            _patientSearchData = new PatientSearchData(_context);
            _audit = new AuditService(_configuration);
            _ip = new IPAddressFinder(HttpContext);
        }

        public IActionResult Index()
        {
            try
            {
                string staffCode = _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Epic Patients", "", _ip.GetIPAddress());
                _epvm.patientList = _patientData.GetPatientsWithoutCGUNumbers();

                return View(_epvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "NewEpicPatients" });
            }
        }

        [HttpGet]
        public IActionResult EpicPatientSearch(int id)
        {
            try
            { 
                string staffCode = _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Epic Patient Search", "MPI=" + id.ToString(), _ip.GetIPAddress());

                _epvm.patient = _patientData.GetPatientDetails(id);

                _crud.NewPatientSearch(_epvm.patient.FIRSTNAME, _epvm.patient.LASTNAME, _epvm.patient.DOB.GetValueOrDefault(), _epvm.patient.POSTCODE, _epvm.patient.SOCIAL_SECURITY, staffCode);

                int searchID = _searchData.GetPatientSearchID(staffCode);

                List<PatientSearchResults> searchResults = _searchData.GetPatientSearchResults(searchID);

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
                string staffCode = _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - New Epic Patient Search", "MPI=" + id.ToString(), _ip.GetIPAddress());

                string cguNumber = "";

                if (fileNumber == null || fileNumber == "")
                {
                    cguNumber = _pedigreeData.GetNextPedigreeNumber() + ".0";
                }
                else
                {
                    string pedno = fileNumber.Substring(0, fileNumber.IndexOf("."));

                    List<Patient> patList = _patientSearchData.GetPatientsListByCGUNo(pedno); //get the next CGU point number
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
