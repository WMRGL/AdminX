﻿using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Meta;
using AdminX.Data;
using AdminX.Models;

namespace AdminX.Controllers
{
    public class PatientSearchController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly AdminContext _adminContext;
        private readonly PatientSearchVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientSearchData _patientSearchData;
        private readonly INewPatientSearchData _newPatientSearchData;
        private readonly IPedigreeData _pedigreeData;
        private readonly ICRUD _crud;
        private readonly IAuditService _audit;


        public PatientSearchController(ClinicalContext context, AdminContext adminContext, IConfiguration config)
        {
            _clinContext = context;
            _adminContext = adminContext;
            _config = config;
            _pvm = new PatientSearchVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientSearchData = new PatientSearchData(_clinContext);
            _newPatientSearchData = new NewPatientSearchData(_adminContext);
            _pedigreeData = new PedigreeData(_clinContext);
            _crud = new CRUD(_config);
            _audit = new AuditService(_config);
        }

        [Authorize]
        public async Task<IActionResult> Index(string? cguNo, string? firstname, string? lastname, string? nhsNo, DateTime? dob)
        {
            try
            {
                string staffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;
                string searchTerm = "";

                if (cguNo != null || firstname != null || lastname != null || nhsNo != null || (dob != null && dob != DateTime.Parse("0001-01-01")))
                {
                    if (cguNo != null)
                    {
                        _pvm.patientsList = _patientSearchData.GetPatientsListByCGUNo(cguNo);
                        searchTerm = "CGU_No=" + cguNo;
                        _pvm.cguNumberSearch = cguNo;
                    }
                    if (nhsNo != null)
                    {
                        if (searchTerm == "")
                        {
                            _pvm.patientsList = _patientSearchData.GetPatientsListByNHS(nhsNo);
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
                            _pvm.patientsList = _patientSearchData.GetPatientsListByName(firstname, null);
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
                            _pvm.patientsList = _patientSearchData.GetPatientsListByName(null, lastname);
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
                            _pvm.patientsList = _patientSearchData.GetPatientsListByDOB(dob.GetValueOrDefault());
                        }
                        else
                        {
                            _pvm.patientsList = _pvm.patientsList.Where(p => p.DOB == dob).ToList();
                        }
                        searchTerm = searchTerm + "," + "DOB=" + dob.ToString();
                        _pvm.dobSearch = dob.GetValueOrDefault();
                    }

                    _pvm.patientsList = _pvm.patientsList.OrderBy(p => p.LASTNAME).ThenBy(p => p.FIRSTNAME).ToList();
                }

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
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
        public async Task<IActionResult> NewPatientSearch(string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - NewPatientSearch", "", _ip.GetIPAddress());

                _pvm.patientSearchResultsList = new List<PatientSearchResults>();
                _pvm.relativeSearchResultsList = new List<PatientSearchResults>();
                _pvm.pedigreeSearchResultsList = new List<PatientSearchResults>();

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
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                if (firstname == "" || lastname == "" || postcode == "" || nhs == "" || dobToSearch == "0001-01-01")
                {
                    return RedirectToAction("NewPatientSearch", new { message = "Missing data, please rectify", success = false });
                }
                else
                {
                    DateTime DOB = DateTime.Parse(dobToSearch);
                    _crud.NewPatientSearch(firstname, lastname, DOB, postcode, nhs, staffCode);

                    int searchID = _newPatientSearchData.GetPatientSearchID(staffCode);

                    List<PatientSearchResults> searchResults = _newPatientSearchData.GetPatientSearchResults(searchID);

                    _pvm.patientSearchResultsList = searchResults.Where(r => r.ResultSource == "Patient").ToList();
                    _pvm.relativeSearchResultsList = searchResults.Where(r => r.ResultSource == "Relative").ToList();
                    _pvm.pedigreeSearchResultsList = searchResults.Where(r => r.ResultSource == "Pedigree").ToList();

                    _pvm.forenameSearch = firstname;
                    _pvm.surnameSearch = lastname;
                    _pvm.dobSearch = DOB;
                    _pvm.postcodeSearch = postcode;
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
        public async Task<IActionResult> SelectFile(string firstname, string lastname, DateTime dob, string postcode, string nhs)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
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
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "NewPatientSearch");

                _pvm.pedigree = _pedigreeData.GetPedigree(fileNumber);

                _pvm.patientsList = _patientSearchData.GetPatientsListByCGUNo(_pvm.pedigree.PEDNO);

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
