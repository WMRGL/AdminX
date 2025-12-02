using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AdminX.Controllers
{
    public class ReferralListController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly ReferralData _referralData;
        private readonly ReferralListVM _rvm;

        public ReferralListController(ClinicalContext context)
        {
            _context = context;
            _referralData = new ReferralData(_context);
            _rvm = new ReferralListVM();
        }

        [Authorize]        
        public IActionResult Index()
        {            
            _rvm.referralList = _referralData.GetUnassignedReferrals();

            return View(_rvm);
        }
    }
}
