using Microsoft.AspNetCore.Mvc;
using AdminX.Data;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using AdminX.Meta;

namespace AdminX.Controllers
{
    public class PatientSearchController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly PatientSearchVM _pvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientSearchData _patientSearchData;
        private readonly IAuditService _audit;
        

        public PatientSearchController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _pvm = new PatientSearchVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientSearchData = new PatientSearchData(_clinContext);            
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
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient Search", searchTerm);

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "PatientSearch" });
            }
        }             
    }
}
