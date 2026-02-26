using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using APIControllers.Data;
using APIControllers.Controllers;

namespace AdminX.Controllers
{
    public class PatientSearchController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly AdminContext _adminContext;
        //private readonly APIContext _apiContext;
        private readonly PatientSearchVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientSearchDataAsync _patientSearchData;
        private readonly INewPatientSearchDataAsync _newPatientSearchData;
        private readonly IPedigreeDataAsync _pedigreeData;
        private readonly ICRUD _crud;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;
        //private readonly APIControllerLOCAL _api; //not sure what this was done for... to test?

        public PatientSearchController(IConfiguration config, IStaffUserDataAsync staffUser, IPatientSearchDataAsync patientSearch, INewPatientSearchDataAsync newPatientSearch, IPedigreeDataAsync pedigree,
            ICRUD crud, IAuditServiceAsync audit)
        {
            //_clinContext = context;
            //_adminContext = adminContext;
            //_apiContext = apiContext;
            _config = config;
            _pvm = new PatientSearchVM();
            _staffUser = staffUser;
            _patientSearchData = patientSearch;
            _newPatientSearchData = newPatientSearch;
            _pedigreeData = pedigree;
            _crud = crud;
            _audit = audit;
            //_api = api;
            _ip = new IPAddressFinder(HttpContext);
        }

        [Authorize]
        public async Task<IActionResult> Index(string? cguNo, string? firstname, string? lastname, string? nhsNo, DateTime? dob)
        {
            try
            {
                string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
                string username = User.Identity.Name;
                ViewBag.RecentPatients = await _newPatientSearchData.GetRecentlyViewedPatients(username); string searchTerm = "";
                if (nhsNo != null) { nhsNo = nhsNo.Replace(" ", ""); }

                if (cguNo != null || firstname != null || lastname != null || nhsNo != null || (dob != null && dob != DateTime.Parse("0001-01-01")))
                {
                    if (cguNo != null)
                    {
                        cguNo = cguNo.Replace(" ", "");
                        _pvm.patientsList = await _patientSearchData.GetPatientsListByCGUNo(cguNo);
                        searchTerm = "CGU_No=" + cguNo;
                        _pvm.cguNumberSearch = cguNo;
                    }
                    if (nhsNo != null)
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = await _patientSearchData.GetPatientsListByNHS(nhsNo);
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.SOCIAL_SECURITY == nhsNo).ToList();
                        }
                        searchTerm = searchTerm + "," + "NHSNo=" + nhsNo;
                        _pvm.nhsNoSearch = nhsNo;
                    }
                    if (firstname != null)
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = await _patientSearchData.GetPatientsListByName(firstname, null);
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.FIRSTNAME.ToUpper().Contains(firstname.ToUpper())).ToList();
                        }
                        searchTerm = searchTerm + "," + "Forename=" + firstname;
                        _pvm.forenameSearch = firstname;
                    }
                    if (lastname != null)
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = await _patientSearchData.GetPatientsListByName(null, lastname);
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.LASTNAME.ToUpper().Contains(lastname.ToUpper())).ToList();
                        }
                        searchTerm = searchTerm + "," + "Surname=" + lastname;
                        _pvm.surnameSearch = lastname;
                    }
                    if (dob != null && dob != DateTime.Parse("0001-01-01"))
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = await _patientSearchData.GetPatientsListByDOB(dob.GetValueOrDefault());
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.DOB == dob).ToList();
                        }
                        searchTerm = searchTerm + "," + "DOB=" + dob.ToString();
                        _pvm.dobSearch = dob.GetValueOrDefault();
                    }

                    //_pvm.patientsList = _pvm.patientsList.OrderBy(p => p.LASTNAME).ThenBy(p => p.FIRSTNAME).ToList();
                    _pvm.patientsList = _pvm.patientsList.OrderBy(p =>
                    {
                        if (string.IsNullOrWhiteSpace(p.CGU_No) || !p.CGU_No.Contains("."))
                            return 0;
                        string suffixString = p.CGU_No.Substring(p.CGU_No.LastIndexOf('.') + 1).Trim();
                        if (int.TryParse(suffixString, out int suffix))
                            return suffix;

                        return 0;
                    }).ToList();
                }

                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient Search", searchTerm, _ip.GetIPAddress());

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                new BreadcrumbItem { Text = "Search" }
            };

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "PatientSearch" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> NewPatientSearch(string? message, bool? success, 
            string? firstname, string? lastname, string? dobToSearch, string? postcode, string? nhs)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                _audit.CreateUsageAuditEntry(staffCode, "AdminX - NewPatientSearch", "", _ip.GetIPAddress());

                _pvm.patientSearchResultsList = new List<PatientSearchResults>();
                _pvm.relativeSearchResultsList = new List<PatientSearchResults>();
                _pvm.pedigreeSearchResultsList = new List<PatientSearchResults>();
                Patient patient = new Patient();
                if (firstname != null)
                {
                    _pvm.forenameSearch = firstname;
                }
                if (lastname != null)
                {
                    _pvm.surnameSearch = lastname;
                }
                if (dobToSearch != null && dobToSearch != "0001-01-01")
                {
                    _pvm.dobSearch = DateTime.Parse(dobToSearch);
                }
                else
                {
                    _pvm.dobSearch = DateTime.Parse("0001-01-01");
                }
                if (postcode != null)
                {
                    _pvm.postcodeSearch = postcode;
                }
                else
                {
                    _pvm.postcodeSearch = "";
                }
                if (nhs != null)
                {
                    _pvm.nhsNoSearch = nhs;
                }
                else
                {
                    _pvm.nhsNoSearch = "";
                }

                if (message != null)
                {
                    _pvm.success = success.GetValueOrDefault();
                    _pvm.message = message;
                }

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem
                    {
                        Text = "Search",
                        Controller = "PatientSearch",
                        Action = "Index",

                    },
                    new BreadcrumbItem { Text = "New" }
                };

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "NewPatientSearch" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewPatientSearch(string firstname, string lastname, string dobToSearch, string postcode, string nhs)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                if (firstname == "" || lastname == "" || postcode == "" || nhs == "" || dobToSearch == "0001-01-01")
                {
                    return RedirectToAction("NewPatientSearch", new { message = "Missing data, please rectify", success = false });
                }
                else
                {
                    DateTime DOB = DateTime.Parse(dobToSearch);
                    _crud.NewPatientSearch(firstname, lastname, DOB, postcode, nhs, staffCode);

                    int searchID = await _newPatientSearchData.GetPatientSearchID(staffCode);

                    List<PatientSearchResults> searchResults = await _newPatientSearchData.GetPatientSearchResults(searchID);

                    _pvm.patientSearchResultsList = searchResults.Where(r => r.ResultSource == "Patient").ToList();
                    _pvm.relativeSearchResultsList = searchResults.Where(r => r.ResultSource == "Relative").ToList();
                    _pvm.pedigreeSearchResultsList = searchResults.Where(r => r.ResultSource == "Pedigree").ToList();

                    TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo; //to convert to proper case

                    _pvm.forenameSearch = textInfo.ToTitleCase(firstname);
                    _pvm.surnameSearch = textInfo.ToTitleCase(lastname);
                    _pvm.dobSearch = DOB;
                    _pvm.postcodeSearch = textInfo.ToUpper(postcode);
                    _pvm.nhsNoSearch = nhs;

                    _pvm.success = true;
                    _pvm.message = "Search complete. Please check possible matches before creating a new patient record.";

                    return View(_pvm);
                }


            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "NewPatientSearch" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> NewPatientSearchAPI(string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                _audit.CreateUsageAuditEntry(staffCode, "AdminX - NewPatientSearch", "", _ip.GetIPAddress());
                
                if (message != null)
                {
                    _pvm.success = success.GetValueOrDefault();
                    _pvm.message = message;
                }

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Search",
                    Controller = "PatientSearch",
                    Action = "Index",

                },
                new BreadcrumbItem { Text = "New" }
            };
                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "APISearch" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> NewPatientSearchAPI(string firstname, string lastname, string dobToSearch, string postcode, string nhs)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                                
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - NewPatientSearch", "", _ip.GetIPAddress());
                
                //_pvm.jsonTest = _api.SearchSCRPatients(firstname, lastname).Result; //what's this for - to test?

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Search",
                    Controller = "PatientSearch",
                    Action = "Index",

                },
                new BreadcrumbItem { Text = "New" }
            };
                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "APISearch" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> SelectFile(string firstname, string lastname, DateTime dob, string postcode, string nhs)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                _audit.CreateUsageAuditEntry(staffCode, "AdminX - ExistingFileSelect", "", _ip.GetIPAddress());

                _pvm.patientsList = new List<Patient>();
                _pvm.forenameSearch = firstname;
                _pvm.surnameSearch = lastname;
                _pvm.dobSearch = dob;
                _pvm.postcodeSearch = postcode;
                _pvm.nhsNoSearch = nhs;

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ExistingFileSelect" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> SelectFile(string fileNumber, string firstname, string lastname, DateTime dob, string postcode, string nhs)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "NewPatientSearch");

                _pvm.pedigree = await _pedigreeData.GetPedigree(fileNumber);

                _pvm.patientsList = await _patientSearchData.GetPatientsListByCGUNo(_pvm.pedigree.PEDNO);

                _pvm.forenameSearch = firstname;
                _pvm.surnameSearch = lastname;
                _pvm.dobSearch = dob;
                _pvm.postcodeSearch = postcode;
                _pvm.nhsNoSearch = nhs;
                _pvm.cguNumberSearch = fileNumber;

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "ExistingFileSelect" });
            }
        }        
    }
}
