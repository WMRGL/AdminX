using ClinicalXPDataConnections.Data;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Meta;
using System.Diagnostics;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authentication;

namespace AdminX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly HomeVM _hvm;
        private readonly IConfiguration _config;        
        private readonly IStaffUserData _staffUser;
        private readonly IVersionData _version;
        private readonly INotificationData _notificationData;
        private readonly IAuditService _audit;
		private readonly IClinicData _clinicData;
		private readonly ITriageData _triageData;
		private readonly IReviewData _reviewData;
        private readonly IDictatedLetterData _dictatedLetterData;


		public HomeController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _hvm = new HomeVM();
            _staffUser = new StaffUserData(_clinContext);
            _version = new VersionData();
            _notificationData = new NotificationData(_clinContext);
            _audit = new AuditService(_config);
			_clinicData = new ClinicData(_clinContext);
			_triageData = new TriageData(_clinContext);
            _reviewData = new ReviewData(_clinContext);
			_dictatedLetterData = new DictatedLetterData(_clinContext);

		}

        public IActionResult Index()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("UserLogin", "Login");
                }
                else
                {
                    _hvm.notificationMessage = _notificationData.GetMessage("AdminXOutage");
                    var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);

                    IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                    _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Home", "", _ip.GetIPAddress());

                    _hvm.name = user.NAME;
                   
                    _hvm.isLive = bool.Parse(_config.GetValue("IsLive", ""));
                    _hvm.dllVersion = _version.GetDLLVersion();
                    _hvm.appVersion = _config.GetValue("AppVersion", "");
                    _hvm.contactOutcomes = _clinicData.GetAllOutstandingClinics().Count();
                    _hvm.triageOutcomes = _triageData.GetTriageListFull().Count();
                    _hvm.reviewOutcomes = _reviewData.GetReviewsListAll().Count();
                    _hvm.dictatedLetters = _dictatedLetterData.GetDictatedLettersList(user.STAFF_CODE).Count();


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
