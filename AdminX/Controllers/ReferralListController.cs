using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AdminX.Controllers
{
    public class ReferralListController : Controller
    {
        //private readonly ClinicalContext _context;
        private readonly IConfiguration _config;
        private readonly IReferralDataAsync _referralData;
        private readonly ReferralListVM _rvm;

        public ReferralListController(IConfiguration config, IReferralDataAsync referral)
        {
            //_context = context;
            _config = config;
            _referralData = referral;
            _rvm = new ReferralListVM();
        }

        [Authorize]        
        public async Task<IActionResult> Index()
        {            
            _rvm.referralList = await _referralData.GetUnassignedReferrals();

            return View(_rvm);
        }
    }
}
