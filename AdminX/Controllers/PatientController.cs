using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using AdminX.Meta;
using AdminX.Data;
using AdminX.Models;
using System.Globalization;

namespace AdminX.Controllers
{
    public class PatientController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _documentContext;
        private readonly AdminContext _adminContext;
        private readonly PatientVM _pvm;
        private readonly ICRUD _crud;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly IPatientData _patientData;
        private readonly IPatientSearchData _patientSearchData;
        private readonly IPedigreeData _pedigreeData;
        private readonly ITitleData _titleData;
        private readonly IEthnicityData _ethnicityData;
        private readonly IRelativeData _relativeData;
        private readonly IPathwayData _pathwayData;
        private readonly IAlertData _alertData;
        private readonly IReferralData _referralData;
        private readonly IAppointmentData _appointmentData;
        private readonly IDiaryData _diaryData;
        private readonly IExternalClinicianData _gpData;
        private readonly IExternalFacilityData _gpPracticeData;
        private readonly IAuditService _audit;
        private readonly ILanguageData _languageData;
        private readonly IPatientAlertData _patientAlertData;
        //private readonly ReviewVM _rvm;
        private readonly IReviewData _reviewData;
        private readonly ICityData _cityData;
        private readonly IAreaNamesData _areaNamesData;
        private readonly IGenderData _genderData;
        private readonly IConstantsData _constantsData;


        public PatientController(ClinicalContext context, IConfiguration config, AdminContext adminContext, DocumentContext documentContext)
        {
            _clinContext = context;
            _adminContext = adminContext;
            _documentContext = documentContext;
            _config = config;
            _crud = new CRUD(_config);
            _pvm = new PatientVM();
            _staffUser = new StaffUserData(_clinContext);
            _patientData = new PatientData(_clinContext);
            _patientSearchData = new PatientSearchData(_clinContext);
            _pedigreeData = new PedigreeData(_clinContext);
            _relativeData = new RelativeData(_clinContext);
            _pathwayData = new PathwayData(_clinContext);
            _alertData = new AlertData(_clinContext);
            _referralData = new ReferralData(_clinContext);
            _appointmentData = new AppointmentData(_clinContext);
            _diaryData = new DiaryData(_clinContext);
            _gpData = new ExternalClinicianData(_clinContext);
            _gpPracticeData = new ExternalFacilityData(_clinContext);
            _titleData = new TitleData(_clinContext);
            _ethnicityData = new EthnicityData(_clinContext);
            _audit = new AuditService(_config);
            _languageData = new LanguageData(_adminContext);
            _patientAlertData = new PateintAlertData(_clinContext);
            //_rvm = new ReviewVM();
            _reviewData = new ReviewData(_clinContext);
            _cityData = new CityData(_adminContext);
            _areaNamesData = new AreaNamesData(_clinContext);
            _genderData = new GenderData(_clinContext);
            _constantsData = new ConstantsData(_documentContext);
        }

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id, string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "MPI=" + id.ToString(), _ip.GetIPAddress());

                if (message != null && message != "")
                {
                    _pvm.message = message;
                    _pvm.success = success.GetValueOrDefault();
                }

                _pvm.patient = _patientData.GetPatientDetails(id);

                //List<Patient> patients = new List<Patient>();
                _pvm.patientsList = _patientData.GetPatientsInPedigree(_pvm.patient.PEDNO).OrderBy(p => p.RegNo).ToList();
                //patients = _patientData.GetPatientsInPedigree(_pvm.patient.PEDNO);

                if (_pvm.patientsList.Count > 0)
                {
                    int regNo;
                    string cguno = _pvm.patient.CGU_No;

                    if (Int32.TryParse(cguno.Substring(cguno.LastIndexOf('.') + 1), out regNo))
                    {
                        int prevRegNo = regNo - 1;
                        int nextRegNo = regNo + 1;

                        _pvm.previousPatient = _patientData.GetPatientDetailsByCGUNo(_pvm.patient.PEDNO + "." + prevRegNo.ToString());
                        _pvm.nextPatient = _patientData.GetPatientDetailsByCGUNo(_pvm.patient.PEDNO + "." + nextRegNo.ToString());
                    }

                }

                if (_pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }
                _pvm.relatives = _relativeData.GetRelativesList(id).Distinct().ToList();
                List<Referral> referrals = _referralData.GetReferralsList(id);
                _pvm.activeReferrals = referrals.Where(r => r.COMPLETE == "Active").ToList();
                _pvm.inactiveReferrals = referrals.Where(r => r.COMPLETE == "Complete").ToList();
                _pvm.appointments = _appointmentData.GetAppointmentListByPatient(id);
                _pvm.patientPathway = _pathwayData.GetPathwayDetails(id);
                _pvm.alerts = _alertData.GetAlertsList(id);
                _pvm.diary = _diaryData.GetDiaryList(id);
                _pvm.languages = _languageData.GetLanguages();
                _pvm.protectedAddress = _patientAlertData.GetProtectedAdress(id);
                _pvm.GP = _gpData.GetClinicianDetails(_pvm.patient.GP_Code);
                _pvm.GPPractice = _gpPracticeData.GetFacilityDetails(_pvm.patient.GP_Facility_Code);
                //_pvm.referral = _referralData.GetReferralDetails(id);
                _pvm.reviewList = _reviewData.GetReviewsListForPatient(id);
                _pvm.edmsLink = _constantsData.GetConstant("GEMRlink", 1);

                if (_pvm.patient.DECEASED == -1)
                {
                    _pvm.diedage = CalculateDiedAge(_pvm.patient.DOB.Value, _pvm.patient.DECEASED_DATE.Value);
                }
                if (_pvm.patient.DOB != null)
                {
                    _pvm.currentage = CalculateAge(_pvm.patient.DOB.Value);
                }

                if (_pvm.activeReferrals.Count == 0 && _pvm.inactiveReferrals.Count == 0)
                {
                    _pvm.message = "There is no activity for this patient, please rectify by adding a referral or temp-reg.";
                }

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                    new BreadcrumbItem { Text = "Patient" }
                };

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientDetails(bool? DECEASED, DateTime? DECEASED_DATE, int mpi, bool? isSuccess, string? sMessage)
        {
            string deceased = Request.Form["DECEASED"];
            string interpreterRequired = Request.Form["IsInterpreterReqd"];
            string language = Request.Form["LanguageName"];
            bool deseasedStatus = false;
            bool interpreter = false;
            if (deceased == "on")
            {
                deseasedStatus = true;
            }

            if (interpreterRequired == "on")
            {
                interpreter = true;
            }

            if (language == null)
            {
                _pvm.patient = _patientData.GetPatientDetails(mpi);
                language = _pvm.patient.PrimaryLanguage;
            }

            _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Update");

            int success = _crud.CallStoredProcedure("Patient", "Update", mpi, 0, 0, "", language, "", "", User.Identity.Name, DECEASED_DATE, null, deseasedStatus, interpreter);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "PatientDetails-edit(SQL)" }); }

            return RedirectToAction("PatientDetails", new { id = mpi });
        }

        public static string CalculateAge(DateTime dob)
        {
            int years = DateTime.Now.Year - dob.Year;
            int months = DateTime.Now.Month - dob.Month;
            if (months < 0)
            {
                months += 12;
                years -= 1;
            }

            return $"{years} years, {months} months";
        }

        public static string CalculateDiedAge(DateTime dob, DateTime deceasedDate)
        {
            if (deceasedDate < dob)
            {
                return "Invalid Date";
            }

            int years = deceasedDate.Year - dob.Year;
            int months = deceasedDate.Month - dob.Month;
            int days = deceasedDate.Day - dob.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(deceasedDate.Year, deceasedDate.Month == 1 ? 12 : deceasedDate.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }
            return $"{years} years {months} months";
        }

        [HttpGet]
        public async Task<IActionResult> AddNew(string firstname, string lastname, DateTime DOB, string postcode, string nhs, string? fileNumber,
            string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;


                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New", _ip.GetIPAddress());
                string cguNumber = "";

                if (fileNumber == null || fileNumber == "")
                {
                    cguNumber = _pedigreeData.GetNextPedigreeNumber() + ".0";
                }
                else
                {
                    List<Patient> patList = _patientSearchData.GetPatientsListByCGUNo(fileNumber); //get the next CGU point number
                    int patientNumber = patList.Count();

                    if (_patientData.GetPatientDetailsByCGUNo(fileNumber + "." + patientNumber.ToString()) == null)
                    {
                        cguNumber = fileNumber + "." + patientNumber.ToString();
                    }
                }

                _pvm.cguNumber = cguNumber;
                _pvm.firstName = firstname;
                _pvm.lastName = lastname;
                _pvm.dob = DOB;
                _pvm.postCode = postcode;
                _pvm.nhs = nhs;
                _pvm.ethnicCode = "NA";
                _pvm.titles = _titleData.GetTitlesList();
                _pvm.ethnicities = _ethnicityData.GetEthnicitiesList();
                _pvm.GPList = _gpData.GetGPList();
                _pvm.GPPracticeList = _gpPracticeData.GetGPPracticeList();
                _pvm.cityList = _cityData.GetAllCities();
                _pvm.areaNamesList = _areaNamesData.GetAreaNames().OrderBy(a => a.AreaName).ToList();
                _pvm.genders = _genderData.GetGenderList();

                if (success.HasValue)
                {
                    _pvm.success = success.GetValueOrDefault();

                    if (message != null)
                    {
                        _pvm.message = message;
                    }
                }

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                    new BreadcrumbItem
                    {
                        Text = "Patient",


                    },

                    new BreadcrumbItem { Text = "New" }
                };

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNew(string title, string firstname, string lastname, string nhsno, DateTime dob,
            string language, bool isInterpreterReqd, bool isConsentToEmail, string postcode, string address1, string address2, string address3, string address4, string areaCode,
            string gpCode, string gpFacilityCode, string email, string prevName, string maidenName, string preferredName, string ethnicCode, string sex,
            string middleName, string tel, string workTel, string mobile, string cguNumber)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                int day = dob.Day;
                int month = dob.Month;

                var patient = _patientData.GetPatientDetailsByDemographicData(firstname, lastname, nhsno, dob);

                if (patient != null)
                {
                    _pvm.success = false;
                    _pvm.message = "Patient already exists";

                }
                else
                {
                    TextInfo textInfo = new CultureInfo("en-GB", false).TextInfo; //to convert to proper case

                    firstname = textInfo.ToTitleCase(firstname);
                    lastname = textInfo.ToTitleCase(lastname);
                    postcode = textInfo.ToUpper(postcode);
                    address1 = textInfo.ToTitleCase(address1);
                    if (address2 != null) { address2 = textInfo.ToTitleCase(address2); }
                    if (address3 != null) { address3 = textInfo.ToTitleCase(address3); }
                    if (address4 != null) { address4 = textInfo.ToTitleCase(address4); }
                    if (prevName != null) { prevName = textInfo.ToTitleCase(prevName); }
                    if (maidenName != null) { maidenName = textInfo.ToTitleCase(maidenName); }
                    if (preferredName != null) { preferredName = textInfo.ToTitleCase(preferredName); }
                    if (middleName != null) { middleName = textInfo.ToTitleCase(middleName); }


                    int success = _crud.PatientDetail("Patient", "Create", User.Identity.Name, 0, title, firstname, "", lastname, nhsno, postcode, gpCode, address1, address2, address3,
                    address4, email, prevName, dob, null, maidenName, isInterpreterReqd, isConsentToEmail, preferredName, ethnicCode, sex, middleName, tel, workTel, mobile, areaCode, cguNumber);
                    _pvm.success = true;
                    _pvm.message = "Patient saved.";
                }

                _pvm.patient = _patientData.GetPatientDetailsByCGUNo(cguNumber);

                return RedirectToAction("PatientDetails", "Patient", new { mpi = _pvm.patient.MPI, success = _pvm.success, message = _pvm.message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPatientDetails(int mpi, string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                _pvm.patient = _patientData.GetPatientDetails(mpi);
                _pvm.titles = _titleData.GetTitlesList();
                _pvm.ethnicities = _ethnicityData.GetEthnicitiesList();
                _pvm.GPList = _gpData.GetGPList();
                _pvm.currentGPList = _pvm.GPList.Where(g => g.FACILITY == _pvm.patient.GP_Facility_Code).ToList();
                _pvm.GPPracticeList = _gpPracticeData.GetGPPracticeList();
                _pvm.cityList = _cityData.GetAllCities();
                _pvm.areaNamesList = _areaNamesData.GetAreaNames().OrderBy(a => a.AreaName).ToList();
                _pvm.genders = _genderData.GetGenderList();

                if (success.HasValue)
                {
                    _pvm.success = success.GetValueOrDefault();

                    if (message != null)
                    {
                        _pvm.message = message;
                    }
                }

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditPatientDetails" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditPatientDetails(int mpi, string title, string firstname, string lastname, string nhsno, DateTime dob, string postcode,
            string address1, string address2, string address3, string address4, string areaCode, string gpCode, string gpFacilityCode, string email, string prevName,
            string maidenName, string preferredName, string ethnicCode, string sex, string middleName, string tel, string workTel, string mobile,
            bool isInterpreterRequired, bool isConsentToEmail)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New");

                _pvm.patient = _patientData.GetPatientDetails(mpi);

                int success = _crud.PatientDetail("Patient", "Update", User.Identity.Name, mpi, title, firstname, "", lastname, nhsno, postcode, gpCode, address1, address2, address3,
                    address4, email, prevName, dob, null, maidenName, isInterpreterRequired, isConsentToEmail, preferredName, ethnicCode, sex, middleName, tel, workTel, mobile);


                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Patient-edit(SQL)" }); }


                return RedirectToAction("PatientDetails", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditPatientDetails" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangeCGUNumber(int mpi)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                _pvm.patient = _patientData.GetPatientDetails(mpi);
                _pvm.patientsList = new List<Patient>();

                return View(_pvm);

            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditPatientDetails" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCGUNumber(int mpi, string newFileNo)
        {
            try
            {
                _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New");

                _pvm.patient = _patientData.GetPatientDetails(mpi);
                //_pvm.patientsList = _patientSearchData.GetPatientsListByCGUNo(newFileNo);
                _pvm.patientsList = _patientData.GetPatientsInPedigree(newFileNo);
                _pvm.cguNumber = newFileNo;

                return View(_pvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditPatientDetails" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCGUNumber(int mpi, string newFileNumber)
        {

            _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Update");

            List<Patient> patList = _patientSearchData.GetPatientsListByCGUNo(newFileNumber); //get the next CGU point number

            int patientNumber = patList.Count();

            string cguNumber = "";
            string sMessage = "";
            bool isSuccess = false;
            int sourceDCTM = 0;
            int destDCTM = 0;

            if (_patientData.GetPatientDetailsByCGUNo(newFileNumber + "." + patientNumber.ToString()) == null)
            {
                sourceDCTM = _patientData.GetPatientDetails(mpi).Patient_Dctm_Sts;

                if (_pedigreeData.GetPedigree(newFileNumber) != null)
                {
                    destDCTM = _pedigreeData.GetPedigree(newFileNumber).File_Dctm_Sts;
                }

                if (sourceDCTM <= destDCTM && _pedigreeData.GetPedigree(newFileNumber) != null)
                {

                    int success = _crud.CallStoredProcedure("Patient", "ChangeFileNumber", mpi, patientNumber, destDCTM, newFileNumber, "", "", "", User.Identity.Name);

                    if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "PatientDetails-ChangeCGUNo(SQL)" }); }

                    sMessage = "CGU number updated, please check the new file for integrity.";
                    isSuccess = true;
                }
                else
                {
                    sMessage = "Destination file is not electronic.";
                    isSuccess = false;
                }
            }
            else
            {
                sMessage = "Destination file number unavailable, please check the integrity of the file you are merging into.";
            }

            return RedirectToAction("PatientDetails", new { id = mpi, message = sMessage, success = isSuccess });
        }

        public async Task<IActionResult> MakePatientElectronic(int mpi)
        {
            _pvm.staffMember = _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Update");



            int success = _crud.CallStoredProcedure("Patient", "MakeElectronic", mpi, 0, 0, "", "", "", "", User.Identity.Name);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "PatientDetails-ChangeCGUNo(SQL)" }); }

            string sMessage = "File made electronic. Please wait a few minutes to allow the EDMS structure to complete.";
            bool isSuccess = true;

            return RedirectToAction("PatientDetails", new { id = mpi, message = sMessage, success = isSuccess });
        }

        public async Task<IActionResult> JumpToPatient(int id)
        {
            return RedirectToAction("PatientDetails", "Patient", new { id = id });
        }
    }
}
