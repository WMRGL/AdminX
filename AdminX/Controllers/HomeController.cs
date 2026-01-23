//using ClinicalXPDataConnections.Data;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Authentication;
using AdminX.Meta;

namespace AdminX.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        private readonly HomeVM _hvm;
        private readonly IConfiguration _config;        
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IVersionData _version;
        private readonly INotificationDataAsync _notificationData;
        private readonly IAuditServiceAsync _audit;
		private readonly IClinicDataAsync _clinicData;
		private readonly ITriageDataAsync _triageData;
		private readonly IReviewDataAsync _reviewData;
        private readonly IDictatedLetterDataAsync _dictatedLetterData;
        private readonly IPAddressFinder _ip;
        private readonly IPatientDataAsync _patientData;
        private readonly IReferralDataAsync _referralData;

        public HomeController(IConfiguration config, IStaffUserDataAsync staffUser, IVersionData version, INotificationDataAsync notification, IAuditServiceAsync audit, IClinicDataAsync clinic,
            ITriageDataAsync triage, IReviewDataAsync review, IDictatedLetterDataAsync dictatedLetter, IPatientDataAsync epicPat, IReferralDataAsync epicRef)
        {
            //_clinContext = context;
            _config = config;
            _hvm = new HomeVM();
            _staffUser = staffUser;
            _version = version;
            _notificationData = notification;
            _audit = audit;
			_clinicData = clinic;
			_triageData = triage;
            _reviewData = review;
			_dictatedLetterData = dictatedLetter;
            _ip = new IPAddressFinder(HttpContext);
            _patientData = epicPat;
            _referralData = epicRef;
		}

        public async Task<IActionResult> Index()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _hvm.notificationMessage = await _notificationData.GetMessage("AdminXOutage");
                    var user = await _staffUser.GetStaffMemberDetails(User.Identity.Name);

                    //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                    _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Home", "", _ip.GetIPAddress());

                    _hvm.name = user.NAME;
                    var clinList = await _staffUser.GetClinicalStaffList();
                    _hvm.dutyClinicianList = clinList.Where(s => s.isDutyClinician == true).ToList();
                    _hvm.isLive = bool.Parse(_config.GetValue("IsLive", ""));
                    _hvm.dllVersion = _version.GetDLLVersion();
                    _hvm.appVersion = _config.GetValue("AppVersion", "");
                    var co = await _clinicData.GetAllOutstandingClinics();
                    _hvm.contactOutcomes = co.Count();
                    var tr = await _triageData.GetTriageListFull();
                    _hvm.triageOutcomes = tr.Count();
                    var rev = await _reviewData.GetReviewsListAll();
                    _hvm.reviewOutcomes = rev.Count();
                    var dl = await _dictatedLetterData.GetDictatedLettersListFull();
                    _hvm.dictatedLetters = dl.Count();
                    var nep = await _patientData.GetPatientsWithoutCGUNumbers();
                    _hvm.newEpicPatients = nep.Count();
                    var ner = await _referralData.GetUnassignedReferrals();
                    _hvm.newEpicReferrals = ner.Count();

					return View(_hvm);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Home" });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("UserLogin", "Login");
        }
    }
}
