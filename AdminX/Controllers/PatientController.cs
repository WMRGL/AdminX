//using AdminX.Data;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using APIControllers.Controllers;
//using APIControllers.Data;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Diagnostics;

namespace AdminX.Controllers
{
    public class PatientController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly DocumentContext _documentContext;
        //private readonly AdminContext _adminContext;
        //private readonly APIContext _apiContext;
        private readonly PatientVM _pvm;
        private readonly ICRUD _crud;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IPatientDataAsync _patientData;
        private readonly IPatientSearchDataAsync _patientSearchData;
        private readonly IPedigreeDataAsync _pedigreeData;
        private readonly ITitleDataAsync _titleData;
        private readonly IEthnicityDataAsync _ethnicityData;
        private readonly IRelativeDataAsync _relativeData;
        private readonly IPathwayDataAsync _pathwayData;
        private readonly IAlertDataAsync _alertData;
        private readonly IReferralDataAsync _referralData;
        private readonly IAppointmentDataAsync _appointmentData;
        private readonly IDiaryDataAsync _diaryData;
        private readonly IExternalClinicianDataAsync _gpData;
        private readonly IExternalFacilityDataAsync _gpPracticeData;
        private readonly IAuditServiceAsync _audit;
        private readonly ILanguageDataAsync _languageData;
        private readonly IPatientAlertDataAsync _patientAlertData;
        private readonly IReviewDataAsync _reviewData;
        private readonly ICityDataAsync _cityData;
        private readonly IAreaNamesDataAsync _areaNamesData;
        private readonly IGenderDataAsync _genderData;
        private readonly IConstantsDataAsync _constantsData;
        private readonly APIController _api;
        private readonly IPhenotipsMirrorDataAsync _phenotipsMirrorData;
        private readonly IAlertTypeDataAsync _alertTypeData;
        private readonly IDiaryActionDataAsync _diaryActionData;
        private readonly IDocumentsDataAsync _docsData;
        private readonly IGenderIdentityDataAsync _genderIdentityData;
        private readonly IReferralStagingDataAsync _referralStagingData;
        private readonly IEpicPatientReferenceDataAsync _epicPatientReferenceData;
        private readonly IEpicReferralReferenceDataAsync _epicReferralReferenceData;
        private readonly IPAddressFinder _ip;

        public PatientController(IConfiguration config, ICRUD crud, IStaffUserDataAsync staffUser, IPatientDataAsync patient, IPatientSearchDataAsync patientSearch, IPedigreeDataAsync pedigree,
            ITitleDataAsync title, IEthnicityDataAsync ethnicity, IRelativeDataAsync relative, IPathwayDataAsync pathway, IAlertDataAsync alert, IReferralDataAsync referral, IAppointmentDataAsync appointment,
            IDiaryDataAsync diary, IExternalClinicianDataAsync extClinician, IExternalFacilityDataAsync extFacility, IAuditServiceAsync audit, ILanguageDataAsync language, IPatientAlertDataAsync patientAlert,
            IReviewDataAsync review, ICityDataAsync city, IAreaNamesDataAsync areaNames, IGenderDataAsync gender, IConstantsDataAsync constants, IPhenotipsMirrorDataAsync phenotipsMirror, 
            IAlertTypeDataAsync alertType, IDiaryActionDataAsync diaryAction, IDocumentsDataAsync documents, IGenderIdentityDataAsync genderIdentity, IReferralStagingDataAsync referralStaging, 
            IEpicPatientReferenceDataAsync epicPatientReference, IEpicReferralReferenceDataAsync epicReferralReference, APIController api)
        {
            //_clinContext = context;
            //_adminContext = adminContext;
            //_documentContext = documentContext;
            //_apiContext = apiContext;
            _config = config;
            _crud = crud;
            _pvm = new PatientVM();
            _staffUser = staffUser;
            _patientData = patient;
            _patientSearchData = patientSearch;
            _pedigreeData = pedigree;
            _relativeData = relative;
            _pathwayData = pathway;
            _alertData = alert;
            _referralData = referral;
            _appointmentData = appointment;
            _diaryData = diary;
            _gpData = extClinician;
            _gpPracticeData = extFacility;
            _titleData = title;
            _ethnicityData = ethnicity;
            _audit = audit;
            _languageData = language;
            _patientAlertData = patientAlert;
            _reviewData = review;
            _cityData = city;
            _areaNamesData = areaNames;
            _genderData = gender;
            _constantsData = constants;
            _api = api;
            _phenotipsMirrorData = phenotipsMirror;
            _alertTypeData = alertType;
            _diaryActionData = diaryAction;
            _docsData = documents;
            _genderIdentityData = genderIdentity;
            _referralStagingData = referralStaging;
            _epicPatientReferenceData = epicPatientReference;
            _epicReferralReferenceData = epicReferralReference;
            _ip = new IPAddressFinder(HttpContext); //IP Address is how it gets the computer name when on the server
        }

        [Authorize]
        public async Task<IActionResult> PatientDetails(int id, string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "MPI=" + id.ToString(), _ip.GetIPAddress()); 

                if (message != null && message != "")
                {
                    _pvm.message = message;
                    _pvm.success = success.GetValueOrDefault();
                }

                _pvm.patient = await _patientData.GetPatientDetails(id);                

                _pvm.patientsList = new List<Patient>(); //because null

                if (_pvm.patient.CGU_No != "." && _pvm.patient.CGU_No != "" && _pvm.patient.CGU_No != null)
                {
                    _pvm.epicPatient = await _epicPatientReferenceData.GetEpicPatient(id);

                    if (_pvm.epicPatient != null)
                    {
                        _pvm.epicReferralStaging = await _referralStagingData.GetParkedReferralUpdates(_pvm.epicPatient.ExternalPatientID); //check and process parked updates

                        if (_pvm.epicReferralStaging.Count > 0)
                        {
                            foreach (var item in _pvm.epicReferralStaging)
                            {
                                var updateStatus = item.UpdateSts;

                                while (updateStatus < 5) //cycle through them all until they hit 5
                                {
                                    _crud.EpicReferralStaging(item.ID, item.PatientID, item.ReferralID, item.ReferralDate, item.ReferredBy, item.ReferredTo, item.Speciality, item.Pathway, item.ReferralStatus,
                                        item.CreatedDate.GetValueOrDefault());

                                    var stagedUpdate = await _referralStagingData.GetParkedUpdate(item.ID);
                                    updateStatus = stagedUpdate.UpdateSts;
                                }
                            }
                        }

                        /*if ((_pvm.patient.Title != _pvm.epicPatient.Title || _pvm.epicPatient.FirstName != _pvm.patient.FIRSTNAME || 
                            _pvm.epicPatient.LastName != _pvm.patient.LASTNAME || _pvm.epicPatient.PostCode != _pvm.patient.POSTCODE || 
                            (_pvm.epicPatient.NHSNo != _pvm.patient.SOCIAL_SECURITY && _pvm.epicPatient.NHSNo != null) ||
                            _pvm.patient.GP_Code != _pvm.epicPatient.GP || _pvm.patient.TEL != _pvm.epicPatient.PhoneHome || 
                            _pvm.patient.WORKTEL != _pvm.epicPatient.PhoneWork || _pvm.patient.PtTelMobile != _pvm.epicPatient.PhoneMobile ||
                            _pvm.patient.EthnicCode != _pvm.epicPatient.EthnicCode) && _pvm.epicPatient.UpdateSts == 1)*/

                        if(_pvm.epicPatient.UpdateSts == 1)
                        {
                            _pvm.isEpicPatientChanged = true;
                        }
                    }

                    var patientList = await _patientData.GetPatientsInPedigree(_pvm.patient.PEDNO);
                    _pvm.patientsList = patientList.OrderBy(p => p.RegNo).ToList(); //for the next/back buttons
                }

                List<Referral> referrals = await _referralData.GetReferralsList(id);
                _pvm.incompleteReferrals = referrals.Where(r => r.COMPLETE == "Missing Data").ToList();
                _pvm.activeReferrals = referrals.Where(r => r.COMPLETE == "Active" && !r.logicaldelete).ToList();
                _pvm.inactiveReferrals = referrals.Where(r => r.COMPLETE == "Complete" && !r.logicaldelete).ToList();
                _pvm.tempReges = await _referralData.GetTempRegList(id);
                _pvm.appointments = await _appointmentData.GetAppointmentListByPatient(id);
                _pvm.patientPathway = await _pathwayData.GetPathwayDetails(id);
                _pvm.alerts = await _alertData.GetAlertsList(id);
                _pvm.diary = await _diaryData.GetDiaryList(id);
                _pvm.languages = await _languageData.GetLanguages();
                _pvm.protectedAddress = await _patientAlertData.GetProtectedAdress(id);
                _pvm.GP = await _gpData.GetClinicianDetails(_pvm.patient.GP_Code);
                _pvm.GPPractice = await _gpPracticeData.GetFacilityDetails(_pvm.patient.GP_Facility_Code);
                _pvm.reviewList = await _reviewData.GetReviewsListForPatient(id);
                _pvm.edmsLink = await _constantsData.GetConstant("GEMRlink", 1);
                _pvm.alertTypes = await _alertTypeData.GetAlertTypes();
                _pvm.referralsList = await _referralData.GetActiveReferralsListForPatient(_pvm.patient.MPI);
                _pvm.diaryActionsList = await _diaryActionData.GetDiaryActions();
                _pvm.documentsList = await _docsData.GetDocumentsList();

                foreach (var item in referrals)
                {
                    EpicReferralReference epicRef = await _epicReferralReferenceData.GetEpicReferral(item.refid);
                    if (epicRef != null)
                    {
                        _pvm.epicReferral = epicRef;                        

                        /*if(item.PATIENT_TYPE_CODE != epicRef.Consultant || item.GC_CODE != epicRef.GC || item.PATHWAY != epicRef.Pathway) //etc*/
                        if(epicRef.LocalUpdateSts == 1)
                        {
                            _pvm.isEpicReferralChanged = true;
                            _pvm.referral = item;
                        }
                    }
                }

                if (_pvm.patientsList.Count > 0)
                {
                    int regNo;
                    string cguno = _pvm.patient.CGU_No;

                    if (Int32.TryParse(cguno.Substring(cguno.LastIndexOf('.') + 1), out regNo))
                    {
                        int prevRegNo = regNo - 1;
                        int nextRegNo = regNo + 1;

                        _pvm.previousPatient = await _patientData.GetPatientDetailsByCGUNo(_pvm.patient.PEDNO + "." + prevRegNo.ToString());
                        _pvm.nextPatient = await _patientData.GetPatientDetailsByCGUNo(_pvm.patient.PEDNO + "." + nextRegNo.ToString());
                    }
                }

                if (_pvm.patient == null)
                {
                    return RedirectToAction("NotFound", "WIP");
                }

                _pvm.relatives = new List<Relative>();
                if (_pvm.patient.PEDNO != null)
                {
                    var rels = await _relativeData.GetRelativesList(id);
                    _pvm.relatives = rels.Distinct().ToList();
                }
                
                _pvm.messages = new List<string>();

                if (_pvm.patient.DECEASED == -1)
                {
                    _pvm.diedage = CalculateDiedAge(_pvm.patient.DOB.Value, _pvm.patient.DECEASED_DATE.Value);
                }
                if (_pvm.patient.DOB != null)
                {
                    _pvm.currentage = CalculateAge(_pvm.patient.DOB.Value);
                }

                if (_pvm.patient.PtAreaCode == null)
                {
                    _pvm.messages.Add("This patient has no area code assigned.");
                }

                if (_pvm.activeReferrals.Count == 0 && _pvm.inactiveReferrals.Count == 0 && _pvm.tempReges.Count == 0 && _pvm.incompleteReferrals.Count == 0)
                {                    
                    _pvm.messages.Add("There is no activity for this patient, please rectify by adding a referral or temp-reg.");
                }

                string gpPractice = "";

                if (_pvm.patient.GP != null)
                {
                    var clin = await _gpData.GetClinicianDetails(_pvm.patient.GP_Code);

                    if (clin != null)
                    {
                        var gp = await _gpData.GetClinicianDetails(_pvm.patient.GP_Code);
                        gpPractice = gp.FACILITY;
                    }
                }

                if(gpPractice == "")
                {
                    _pvm.success = false;
                    if (_pvm.patient.GP_Code != null)
                    {                     
                        _pvm.messages.Add("The assigned GP code does not match a known GP. Please check in System Administration and add if necessary.");
                    }
                    else
                    {                        
                        _pvm.messages.Add("No GP is assigned to this patient.");
                    }
                }
                else if (gpPractice != _pvm.patient.GP_Facility_Code)
                {                 
                    _pvm.messages.Add("This patient's GP is no longer at this practice. Please check and select a new GP if necessary.");
                }

                string ptURL = await _constantsData.GetConstant("PhenotipsURL", 2);

                if (!ptURL.Contains("0"))
                {
                    _pvm.isPhenotipsAvailable = true;
                }

                if (_pvm.isPhenotipsAvailable) 
                {
                    //if (_api.GetPhenotipsPatientID(id).Result != "")
                    if(await _phenotipsMirrorData.GetPhenotipsPatientByID(id) != null) //don't ping the API every time we open a record!
                    {
                        //APIControllerLOCAL api = new APIControllerLOCAL(_apiContext, _config);

                        _pvm.isPatientInPhenotips = true;
                        _pvm.isCancerPPQScheduled = _api.CheckPPQExists(_pvm.patient.MPI, "Cancer").Result; //pings the Phenotips API to see if a PPQ is scheduled
                        _pvm.isGeneralPPQScheduled = _api.CheckPPQExists(_pvm.patient.MPI, "General").Result;

                        _pvm.isCancerPPQComplete = _api.CheckPPQSubmitted(_pvm.patient.MPI, "Cancer").Result;
                        _pvm.isGeneralPPQComplete = _api.CheckPPQSubmitted(_pvm.patient.MPI, "General").Result;
                        _pvm.phenotipsLink = _constantsData.GetConstant("PhenotipsURL", 1) + "/" + _api.GetPhenotipsPatientID(id).Result;
                    }                    
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
                _pvm.patient = await _patientData.GetPatientDetails(mpi);
                language = _pvm.patient.PrimaryLanguage;
            }

            _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Update");

            int success = _crud.CallStoredProcedure("Patient", "Update", mpi, 0, 0, "", language, "", "", User.Identity.Name, DECEASED_DATE, null, deseasedStatus, interpreter);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "PatientDetails-edit(SQL)" }); }

            TempData["SuccessMessage"] = "Patient details updated successfully";
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
        [Authorize]
        public async Task<IActionResult> AddNew(string firstname, string lastname, DateTime DOB, string postcode, string nhs, string? fileNumber,
            string? message, bool? success, string? sex, string? ADDRESS1, string? gpPracticeCode, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New", _ip.GetIPAddress());
                string cguNumber = "";                

                if (fileNumber == null || fileNumber == "")
                {
                    cguNumber = await _pedigreeData.GetNextPedigreeNumber() + ".0";
                }
                else
                {
                    List<Patient> patList = await _patientSearchData.GetPatientsListByCGUNo(fileNumber); //get the next CGU point number
                    int patientNumber = patList.Count();

                    if (await _patientData.GetPatientDetailsByCGUNo(fileNumber + "." + patientNumber.ToString()) == null)
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
                _pvm.titles = await _titleData.GetTitlesList();
                _pvm.ethnicities = await _ethnicityData.GetEthnicitiesList();
                _pvm.GPList = await _gpData.GetGPList();
                _pvm.GPPracticeList = await _gpPracticeData.GetGPPracticeList();
                _pvm.cityList = await _cityData.GetAllCities();
                var areaNames = await _areaNamesData.GetAreaNames();
                _pvm.areaNamesList = areaNames.OrderBy(a => a.AreaName).ToList();
                _pvm.genders = await _genderData.GetGenderList();
                _pvm.genderAtBirth = await _genderData.GetGenderList();
               _pvm.genderIdentities = await _genderIdentityData.GetGenderIdentities();              

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
            string middleName, string tel, string workTel, string mobile, string cguNumber, DateTime? startDate, DateTime? endDate, string? SALUTATION,
            string GenderIdentity)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                if (nhsno != null) { nhsno = nhsno.Replace(" ", ""); }
                int day = dob.Day;
                int month = dob.Month;

                var patient = await _patientData.GetPatientDetailsByDemographicData(firstname, lastname, nhsno, dob);

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

                    int success = _crud.PatientDetail("Patient", "Create", User.Identity.Name, 0, title, firstname, "", 
                        lastname, nhsno, postcode, gpCode, address1, address2, address3, address4, email, prevName, dob, 
                        null, maidenName, isInterpreterReqd, isConsentToEmail, preferredName, ethnicCode, sex, middleName, 
                        tel, workTel, mobile, areaCode, cguNumber, SALUTATION,  GenderIdentity);
                    _pvm.success = true;
                    _pvm.message = "Patient saved.";
                }                

                //_pvm.patient = _patientData.GetPatientDetailsByCGUNo(cguNumber);

                _pvm.patient = await _patientData.GetPatientDetailsByDemographicData(firstname, lastname, nhsno, dob);


                if(startDate != null && endDate != null)
                {                     
                    return RedirectToAction("PatientDQReport", "PatientDQ", new { startDate=startDate, endDate=endDate });
                }
                TempData["SuccessMessage"] = "Patient created successfully";
                return RedirectToAction("PatientDetails", "Patient", new { id = _pvm.patient.MPI, success = _pvm.success, message = _pvm.message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Patient" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditPatientDetails(int mpi, string? message, bool? success)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                _pvm.patient = await _patientData.GetPatientDetails(mpi);
                _pvm.titles = await _titleData.GetTitlesList();
                _pvm.ethnicities = await _ethnicityData.GetEthnicitiesList();
                _pvm.GPList = await _gpData.GetGPList();
                _pvm.currentGPList = _pvm.GPList.Where(g => g.FACILITY == _pvm.patient.GP_Facility_Code).ToList();
                var gpPracList = await _gpPracticeData.GetGPPracticeList();
                _pvm.GPPracticeList = gpPracList.OrderBy(p => p.MasterFacilityCode).ToList();
                _pvm.cityList = await _cityData.GetAllCities();
                var areaNames = await _areaNamesData.GetAreaNames();
                _pvm.areaNamesList = areaNames.OrderBy(a => a.AreaName).ToList();
                _pvm.genders = await _genderData.GetGenderList();
                _pvm.genderIdentities = await _genderIdentityData.GetGenderIdentities();

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
            bool isInterpreterRequired, bool isConsentToEmail, string SALUTATION, string GenderIdentity)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New");

                if (nhsno != null) { nhsno = nhsno.Replace(" ", ""); }

                _pvm.patient = await _patientData.GetPatientDetails(mpi);

                int success = _crud.PatientDetail("Patient", "Update", User.Identity.Name, mpi, title, firstname, "", lastname, nhsno.Replace(" ", ""),
                    postcode, gpCode, address1, address2, address3, address4, email, prevName, dob, null, maidenName, isInterpreterRequired,
                    isConsentToEmail, preferredName, ethnicCode, sex, middleName, tel, workTel, mobile, areaCode, null, SALUTATION,
                    GenderIdentity);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Patient-edit(SQL)" }); }

                TempData["SuccessMessage"] = "Patient updated successfully";
                return RedirectToAction("PatientDetails", new { id = mpi });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditPatientDetails" });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeCGUNumber(int mpi)
        {
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                _pvm.patient = await _patientData.GetPatientDetails(mpi);
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
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "New");

                _pvm.patient = await _patientData.GetPatientDetails(mpi);
                _pvm.patientsList = await _patientData.GetPatientsInPedigree(newFileNo);
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
            try
            {
                _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
                string staffCode = _pvm.staffMember.STAFF_CODE;
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Update");

                List<Patient> patList = await _patientSearchData.GetPatientsListByPedNo(newFileNumber); //get the next CGU point number

                int patientNumber = patList.Count();

                string cguNumber = "";
                string sMessage = "";
                bool isSuccess = false;
                int sourceDCTM = 0;
                int destDCTM = 0;

                Patient patToMergeTo = await _patientData.GetPatientDetailsByCGUNo(newFileNumber + "." + patientNumber.ToString());

                if (patToMergeTo == null)
                {
                    var pat = await _patientData.GetPatientDetails(mpi);
                    sourceDCTM = pat.Patient_Dctm_Sts;

                    var destPed = await _pedigreeData.GetPedigree(newFileNumber);

                    if (destPed != null)
                    {
                        var ped = await _pedigreeData.GetPedigree(newFileNumber);
                        destDCTM = ped.File_Dctm_Sts;
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
            catch(Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditPatientDetails" });
            }

        }

        [Authorize]
        public async Task<IActionResult> MakePatientElectronic(int mpi)
        {
            _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Update", _ip.GetIPAddress());

            int success = _crud.CallStoredProcedure("Patient", "MakeElectronic", mpi, 0, 0, "", "", "", "", User.Identity.Name);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "PatientDetails-ChangeCGUNo(SQL)" }); }

            string sMessage = "File made electronic. Please wait a few minutes to allow the EDMS structure to complete.";
            bool isSuccess = true;

            return RedirectToAction("PatientDetails", new { id = mpi, message = sMessage, success = isSuccess });
        }

        [Authorize]
        public async Task<IActionResult> JumpToPatient(int id)
        {
            return RedirectToAction("PatientDetails", "Patient", new { id = id });
        }

        [Authorize]
        public async Task<IActionResult> SynchronisePhenotips(int id)
        {            
            int result = _api.SynchroniseMirrorWithPhenotips(id).Result;

            string message = "Synch okay";
            bool success = true;
            
            if(result == 0)
            {
                message = "Record mismatch, please check both systems for inconsistencies.";
                success = false;
                return RedirectToAction("PhenotipsPatientRecord", new { id = id, message = message, success = success });
            }

            return RedirectToAction("PatientDetails", new { id = id, message = message, success = success });
        }

        [Authorize]
        public async Task<IActionResult> PhenotipsPatientRecord(int id, string? message, bool? success)
        {
            _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Phenotips Record", _ip.GetIPAddress());

            _pvm.patient = await _patientData.GetPatientDetails(id);
            _pvm.ptPatient = await _phenotipsMirrorData.GetPhenotipsPatientByID(id);
            _pvm.phenotipsLink = _constantsData.GetConstant("PhenotipsURL", 1) + "/" + _api.GetPhenotipsPatientID(id).Result;

            if (message != null && message != "")
            {
                _pvm.message = message;
                _pvm.success = success.GetValueOrDefault();
            }

            return View(_pvm);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EpicPatientChanges(int id)
        {
            _pvm.staffMember = await _staffUser.GetStaffMemberDetails(User.Identity.Name);
            string staffCode = _pvm.staffMember.STAFF_CODE;
            //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Patient", "Epic Changes", _ip.GetIPAddress());

            _pvm.patient = await _patientData.GetPatientDetails(id);
            _pvm.epicPatient = await _epicPatientReferenceData.GetEpicPatient(id);
            _pvm.referralsList = await _referralData.GetReferralsList(id);
            _pvm.epicReferrals = await _epicReferralReferenceData.GetEpicReferralsList(id);

            return View(_pvm);
        }

        [HttpPost]
        public async Task<IActionResult> EpicPatientChanges(int mpi, string epicID, string changeType)
        {
            int refID = 0;
            if(Int32.TryParse(changeType, out refID)) //if the value passed is a number, treat it as a RefID - then change it to "referral" (obtuse I know but how else can we get Javascript to
             //pass multiple values to a form?
            {
                changeType = "Referral";
            }

            int success = _crud.EpicAcceptChanges(mpi, epicID, User.Identity.Name, changeType, refID);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "PatientDetails-AcceptEpicChange(SQL)" }); }

            return RedirectToAction("PatientDetails", new { id = mpi, message = "Updated.", success = true });
        }

        [HttpGet]
        public IActionResult PedigreeAssistant(int id)
        {
            RunPedigreeAssistant();

            return RedirectToAction("PatientDetails", new { id = id });
        }


        public void RunPedigreeAssistant()
        {
            //Process.Start($"G:\\WMFACS databases\\Pedigree drawing\\GeneticPedigree.exe");
            Process.Start($"\\\\zion.matrix.local\\dfsrootbwh\\cling\\WMFACS databases\\Pedigree drawing\\GeneticPedigree.exe");
        }
    }
}
