using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClinicalXPDataConnections.Models;

namespace AdminX.Controllers
{
    public class OtherCaseloadController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly CaseloadVM _cvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserData _staffUser;
        private readonly ICaseloadData _caseloadData;
        private readonly IAuditService _audit;
        private readonly IPAddressFinder _ip;

        public OtherCaseloadController(ClinicalContext context, IConfiguration config)
        {
            _clinContext = context;
            _config = config;
            _cvm = new CaseloadVM();            
            _staffUser = new StaffUserData(_clinContext);
            _caseloadData = new CaseloadData(_clinContext);
            _audit = new AuditService(_config);
            _ip = new IPAddressFinder(HttpContext);
        }

        [Authorize]
        public IActionResult Index(string? staffCode)
        {
            try
            {
                string userStaffCode = _staffUser.GetStaffMemberDetails(User.Identity.Name).STAFF_CODE;

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - Caseloads", "StaffCode=" + staffCode, _ip.GetIPAddress());

                _cvm.caseLoad = new List<Caseload>();
                if (staffCode != null)
                {
                    _cvm.staffCode = staffCode;
                    _cvm.caseLoad = _caseloadData.GetCaseloadList(staffCode).OrderBy(c => c.BookedDate).ThenBy(c => c.BookedTime).ToList();
                }
                
                _cvm.clinicians = _staffUser.GetClinicalStaffList();
                if (_cvm.caseLoad.Count() > 0)
                {
                    _cvm.name = _cvm.caseLoad.FirstOrDefault().Clinician;
                }

                return View(_cvm);
            }
            catch (Exception ex) 
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "OtherCaseload" });
            }
        }
    }
}
