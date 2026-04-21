using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class SSPController : Controller
    {
        public IPatientDataAsync _patientData { get; set; }
        public SSPVM _sspvm;
        public ISSPDataAsync _sspData;
        public IStaffUserDataAsync _staffUser;
        public IAuditServiceAsync _audit;
        public IPAddressFinder _ip;
        private readonly ICRUD _crud;

        public SSPController(IPatientDataAsync patientDataAsync, ISSPDataAsync sspData, IStaffUserDataAsync staffUser, IAuditServiceAsync audit, ICRUD crud)
        {
            _patientData = patientDataAsync;
            _sspData = sspData;
            _sspvm = new SSPVM();
            _staffUser = staffUser;
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
            _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Clinic", "RefID=" + id.ToString(), _ip.GetIPAddress());

            _sspvm.ssp = await _sspData.GetSSPDetails(id);
            _sspvm.patient = await _patientData.GetPatientDetails(_sspvm.ssp.MPI);

            return View(_sspvm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
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
    }
}
