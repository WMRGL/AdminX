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
        //private readonly ClinicalContext _clinContext;
        private readonly CaseloadVM _cvm;
        private readonly IConfiguration _config;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly ICaseloadDataAsync _caseloadData;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;

        public OtherCaseloadController(IConfiguration config, IStaffUserDataAsync staffUser, ICaseloadDataAsync caseload, IAuditServiceAsync audit)
        {
            //_clinContext = context;
            _config = config;
            _cvm = new CaseloadVM();            
            _staffUser = staffUser;
            _caseloadData = caseload;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
        }

        [Authorize]
        public async Task<IActionResult> Index(string? staffCode)
        {
            try
            {
                string userStaffCode = await _staffUser.GetStaffCode(User.Identity.Name);

                //IPAddressFinder _ip = new IPAddressFinder(HttpContext);
                _audit.CreateUsageAuditEntry(userStaffCode, "AdminX - Caseloads", "StaffCode=" + staffCode, _ip.GetIPAddress());

                _cvm.caseLoad = new List<Caseload>();
                if (staffCode == null)
                {
                    staffCode = userStaffCode;
                }
                else
                {
                    _cvm.staffCode = staffCode;
                    var caseLoad = await _caseloadData.GetCaseloadList(staffCode);
                    _cvm.caseLoad = caseLoad.OrderBy(c => c.BookedDate).ThenBy(c => c.BookedTime).ToList();
                }
                
                _cvm.clinicians = await _staffUser.GetClinicalStaffList();                                  
                _cvm.name = await _staffUser.GetStaffNameFromStaffCode(staffCode);                

                return View(_cvm);
            }
            catch (Exception ex) 
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "OtherCaseload" });
            }
        }
    }
}
