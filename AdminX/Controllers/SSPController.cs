using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class SSPController : Controller
    {
        private readonly IPatientDataAsync _patientData;
        private readonly SSPVM _sspvm;
        private readonly ISSPDataAsync _sspData;
        private readonly IStaffUserDataAsync _staffUser;
        private readonly IAuditServiceAsync _audit;
        private readonly ITitleDataAsync _titleData;
        private readonly IPAddressFinder _ip;
        private readonly ICRUD _crud;

        public SSPController(IPatientDataAsync patientDataAsync, ISSPDataAsync sspData, IStaffUserDataAsync staffUser, IAuditServiceAsync audit, ITitleDataAsync titleData, ICRUD crud)
        {
            _patientData = patientDataAsync;
            _sspData = sspData;
            _sspvm = new SSPVM();
            _staffUser = staffUser;
            _titleData = titleData;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
            _crud = crud;
        }

        public async Task<IActionResult> Index()
        {
            _sspvm.ssps = await _sspData.GetSSPList();

            return View(_sspvm);
        }

        [HttpGet]
        public async Task<IActionResult> SSPDetails(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - SSP Details", "SSPID=" + id.ToString(), _ip.GetIPAddress());

            _sspvm.ssp = await _sspData.GetSSPDetails(id);
            _sspvm.patient = await _patientData.GetPatientDetails(_sspvm.ssp.MPI);

            return View(_sspvm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string staffCode = await _staffUser.GetStaffCode(User.Identity.Name);
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit SSP", "SSPID=" + id.ToString(), _ip.GetIPAddress());

            _sspvm.ssp = await _sspData.GetSSPDetails(id);
            _sspvm.patient = await _patientData.GetPatientDetails(_sspvm.ssp.MPI);
            _sspvm.socialServices = await _sspData.GetSocialServices();
            _sspvm.socialWorkers = await _sspData.GetSocialWorkers();
            _sspvm.sspOutcomes = await _sspData.GetSSPOutcomes();
            _sspvm.sspStatuses = await _sspData.GetSSPStatuses();

            return View(_sspvm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, int status, int? outcome, string? socialWorkerID, string? parentalResponsibility, DateTime? dateRequested, DateTime? dateReRequested,
            DateTime? dateReceived, string? outcomeDetails)
        {
            int success = _crud.SSPCRUD("SSP", "Update", id, status, outcome.GetValueOrDefault(), socialWorkerID, parentalResponsibility, outcomeDetails, User.Identity.Name, dateRequested, dateReRequested, dateReceived);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Clinic-edit(SQL)" }); }


            return RedirectToAction("SSPDetails", "SSP", new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> NewSocialWorker()
        {
            _sspvm.socialServices = await _sspData.GetSocialServices();
            _sspvm.titles = await _titleData.GetTitlesList();

            return View(_sspvm);
        }

        [HttpPost]
        public async Task<IActionResult> NewSocialWorker(string ssFacility, string title, string firstName, string lastName, string jobTitle)
        {
            int success = 0;//_crud.SocialWorkerCRUD("SocialWorker", "Create", service, title, User.Identity.Name);
            
            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Social Worker Creation(SQL)" }); }

            return RedirectToAction("Index", "SSP");
        }

        [HttpGet]
        public async Task<IActionResult> EditSocialWorker(string swid)
        {
            
            _sspvm.socialServices = await _sspData.GetSocialServices();
            _sspvm.titles = await _titleData.GetTitlesList();

            return View(_sspvm);
        }

        [HttpPost]
        public async Task<IActionResult> EditSocialWorker(string swid, string ssFacility, string title, string firstName, string lastName, string jobTitle)
        {
            int success = 0;//_crud.SocialWorkerCRUD("SocialWorker", "Update", service, title, User.Identity.Name);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Social Worker Creation(SQL)" }); }

            return RedirectToAction("Index", "SSP");
        }

        [HttpGet]
        public async Task<IActionResult> NewSocialService()
        {            

            return View(_sspvm);
        }

        [HttpPost]
        public async Task<IActionResult> NewSocialService(string ssFacility, string name)
        {
            int success = 0;//_crud.SocialWorkerCRUD("SocialWorker", "Create", service, title, User.Identity.Name);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Social Worker Creation(SQL)" }); }

            return RedirectToAction("Index", "SSP");
        }

        [HttpGet]
        public async Task<IActionResult> EditSocialService(string ssid)
        {
            

            return View(_sspvm);
        }

        [HttpPost]
        public async Task<IActionResult> EditSocialService(string ssid, string ssFacility, string title, string firstName, string lastName, string jobTitle)
        {
            int success = 0;//_crud.SocialWorkerCRUD("SocialWorker", "Update", service, title, User.Identity.Name);

            if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Social Worker Creation(SQL)" }); }

            return RedirectToAction("Index", "SSP");
        }
    }
}
