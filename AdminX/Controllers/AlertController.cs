using AdminX.Meta;
using AdminX.Models;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class AlertController : Controller
    {
        private readonly IConfiguration _config;
        //private readonly ClinicalContext _context;
        //private readonly AdminContext _adminContext;
        private readonly IPatientDataAsync _patientData;
        private readonly IAlertDataAsync _alertData;
        private readonly IAlertTypeDataAsync _alertTypeData;
        private readonly ICRUD _crud;
        private readonly AlertVM _avm;
        private readonly IStaffUserDataAsync _staffUserData;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;

        public AlertController(IConfiguration config, IPatientDataAsync patient, IAlertDataAsync alert, IAlertTypeDataAsync alerttype, ICRUD crud, IStaffUserDataAsync staffUser, 
            IAuditServiceAsync audit)
        {
            _config = config;
            //_context = context;
            //_adminContext = adminContext;
            _patientData = patient;
            _alertData = alert;
            _alertTypeData = alerttype;
            _crud = crud;
            _avm = new AlertVM();
            _staffUserData = staffUser;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext); //IP Address is how it gets the computer name when on the server
        }

        public async Task<IActionResult> Index(int mpi)
        {
            try
            {
                _avm.patient = await _patientData.GetPatientDetails(mpi);
                _avm.alertList = await _alertData.GetAlertsListAll(mpi);

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },

                    new BreadcrumbItem { Text = "Alert" }
                };

                return View(_avm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Alert" });
            }
        }

        public async Task<IActionResult> AlertDetails(int alertID)
        {
            try
            {
                string staffCode = await _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - AlertDetails", "MPI=" + alertID.ToString(), _ip.GetIPAddress());

                _avm.alert = await _alertData.GetAlertDetails(alertID);
                _avm.patient = await _patientData.GetPatientDetails(_avm.alert.MPI);

                ViewBag.Breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Controller = "Home", Action = "Index" },
                new BreadcrumbItem
                {
                    Text = "Alert",
                    Controller = "Alert",
                    Action = "Index",
                    RouteValues = new Dictionary<string, string> { { "mpi", _avm.patient.MPI.ToString() } }
                },
                new BreadcrumbItem { Text = "Details" }
            };

                return View(_avm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "AlertDetails" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int mpi)
        {
            try
            {
                string staffCode = await _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Alert", "New Alert", _ip.GetIPAddress());

                _avm.patient = await _patientData.GetPatientDetails(mpi);
                _avm.alertList = await _alertData.GetAlertsListAll(mpi);
                _avm.alertTypes = await _alertTypeData.GetAlertTypes();

                return View(_avm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "CreateAlert" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(int mpi, string alertType, bool isProtectedAddress, DateTime? startDate, string? comments)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Alert", "Create", mpi, 0, 0, alertType, comments, "", "", User.Identity.Name, startDate, null, isProtectedAddress, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Alert-edit(SQL)" }); }

                var alert = await _alertData.GetAlertsList(mpi);
                int alertID = alert.OrderByDescending(a => a.AlertID).FirstOrDefault().AlertID;

                _avm.patient = await _patientData.GetPatientDetails(mpi);
                _avm.success = true;
                _avm.message = "New alert added.";
                TempData["SuccessMessage"] = "New alert added";
                return RedirectToAction("PatientDetails", "Patient", new { id = _avm.patient.MPI });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "CreateAlert" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int alertID)
        {
            try
            {
                string staffCode = await _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Edit Alert", "MPI=" + alertID.ToString(), _ip.GetIPAddress());

                _avm.alert = await _alertData.GetAlertDetails(alertID);
                _avm.patient = await _patientData.GetPatientDetails(_avm.alert.MPI);
                _avm.alertList = await _alertData.GetAlertsListAll(_avm.alert.MPI);
                _avm.alertTypes = await _alertTypeData.GetAlertTypes();

                return View(_avm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditAlert" });
            }
        }

        [HttpPost]
        public IActionResult Edit(int alertID, string alertType, bool isProtectedAddress, DateTime? endDate, string? comments)
        {
            try
            {
                if (endDate == null)
                {
                    endDate = DateTime.Parse("1900-01-01"); //because SQL can't take a null date so we have to convert it, then change it back again
                }

                int success = _crud.CallStoredProcedure("Alert", "Edit", alertID, 0, 0, alertType, comments, "", "", User.Identity.Name, endDate, null, isProtectedAddress, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Alert-edit(SQL)" }); }

                return RedirectToAction("AlertDetails", "Alert", new { alertID = alertID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EditAlert" });
            }
        }

        public IActionResult StandDown(int alertID)
        {
            try
            {
                int success = _crud.CallStoredProcedure("Alert", "StandDown", alertID, 0, 0, "", "", "", "", User.Identity.Name, null, null, false, false);

                if (success == 0) { return RedirectToAction("ErrorHome", "Error", new { error = "Something went wrong with the database update.", formName = "Alert-end(SQL)" }); }

                return RedirectToAction("AlertDetails", "Alert", new { alertID = alertID });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "StandDownAlert" });
            }
        }
    }
}
