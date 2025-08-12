using AdminX.Data;
using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class patientDQController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly IPatientDQData _patientDQData;
        private readonly IPedigreeData _pedigreeData;
        private readonly IPatientSearchData _patientSearchData;
        private readonly IPatientData _patientData;
        private readonly AdminContext _adminContext;
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
        private readonly IStaffUserData _staffUser;

        public patientDQController(IPatientDQData patientDQData, AdminContext adminContext, ClinicalContext context) 
        {
            _clinContext = context;
            _patientDQData = patientDQData;
            _adminContext = adminContext;
            _pedigreeData = _pedigreeData;
            _patientSearchData = _patientSearchData;
            _patientData = _patientData;
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
           // _audit = new AuditService(_config);
            _languageData = new LanguageData(_adminContext);
            _patientAlertData = new PateintAlertData(_clinContext);
            _reviewData = new ReviewData(_clinContext);
            _cityData = new CityData(_adminContext);
            _areaNamesData = new AreaNamesData(_clinContext);
            _genderData = new GenderData(_clinContext);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.Name is null)
            {
                return RedirectToAction("UserLogin", "Login");
            }

            var viewModel = new DiscrepancyReportVM
            {
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today
            };

            try
            {
           
                viewModel.GPList = _gpData.GetGPList();

                viewModel.ethnicCode = "NA";
                viewModel.titles = _titleData.GetTitlesList();
                viewModel.ethnicities = _ethnicityData.GetEthnicitiesList();
                viewModel.GPList = _gpData.GetGPList();
                viewModel.GPPracticeList = _gpPracticeData.GetGPPracticeList();
                viewModel.cityList = _cityData.GetAllCities();
                viewModel.areaNamesList = _areaNamesData.GetAreaNames().OrderBy(a => a.AreaName).ToList();
                viewModel.genders = _genderData.GetGenderList();
                //viewModel.cguNumber = cguNumber;
            }

            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Index" });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime startDate, DateTime endDate)
        {
            if (User.Identity.Name is null)
            {
                return RedirectToAction("UserLogin", "Login");
            }

            if (endDate < startDate)
            {
                ViewBag.ErrorMessage = "End date cannot be earlier than start date.";
                return View();
            }
            if (startDate < new DateTime(2025, 5, 15))
            {
                ViewBag.ErrorMessage = "Dates must be after May 15, 2025.";
                return View();
            }

            var inclusiveEndDate = endDate.AddDays(1);
            var viewModel = new DiscrepancyReportVM
            {
                StartDate = startDate,
                EndDate = endDate
            };

            try
            {
                viewModel.CgundbPatients = await _patientDQData.GetPatientsCGUDBNotInEPICAsync(startDate, inclusiveEndDate);
                viewModel.EpicPatients = await _patientDQData.GetPatientsEPICNotInCGUDBAsync(startDate, inclusiveEndDate);
                viewModel.PatientMismatches = await _patientDQData.GetPatientMismatchesAsync(startDate, inclusiveEndDate);
                viewModel.GPList = _gpData.GetGPList();

                viewModel.ethnicCode = "NA";
                viewModel.titles = _titleData.GetTitlesList();
                viewModel.ethnicities = _ethnicityData.GetEthnicitiesList();
                viewModel.GPList = _gpData.GetGPList();
                viewModel.GPPracticeList = _gpPracticeData.GetGPPracticeList();
                viewModel.cityList = _cityData.GetAllCities();
                viewModel.areaNamesList = _areaNamesData.GetAreaNames().OrderBy(a => a.AreaName).ToList();
                viewModel.genders = _genderData.GetGenderList();
                //viewModel.cguNumber = cguNumber;
            }

            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Index" });
            }

            return View(viewModel);
        }
    }
}
