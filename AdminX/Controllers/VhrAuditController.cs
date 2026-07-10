using Microsoft.AspNetCore.Mvc;
using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdminX.Controllers
{
    public class VhrAuditController : Controller
    {

      
        private readonly IConfiguration _config;
        private readonly VhrAuditVM _vAVM;
        private readonly IVHRAuditDataAsync _vhrAuditData;

        public VhrAuditController( IConfiguration config, VhrAuditVM vavm, IVHRAuditDataAsync vhrAuditData)
        {
           
            _config = config;
            _vAVM = vavm;
            _vhrAuditData = vhrAuditData;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            _vAVM.vhrAuditList = await _vhrAuditData.GetVHRAuditList();
            return View(_vAVM);
        }
    }
}
