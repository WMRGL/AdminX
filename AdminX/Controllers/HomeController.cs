using ClinicalXPDataConnections.Data;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Meta;
using System.Diagnostics;
using ClinicalXPDataConnections.Models;

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


        public HomeController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _hvm = new HomeVM();
            _staffUser = new StaffUserData(_clinContext);
            _version = new VersionData();
            _notificationData = new NotificationData(_clinContext);
            _audit = new AuditService(_config);
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
                    _hvm.notificationMessage = _notificationData.GetMessage();
                    var user = _staffUser.GetStaffMemberDetails(User.Identity.Name);
                    _audit.CreateUsageAuditEntry(user.STAFF_CODE, "AdminX - Home");

                    _hvm.name = user.NAME;
                   
                    _hvm.isLive = bool.Parse(_config.GetValue("IsLive", ""));
                    _hvm.dllVersion = _version.GetDLLVersion();
                    _hvm.appVersion = _config.GetValue("AppVersion", "");


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

        
    }
}
