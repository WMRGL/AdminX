using AdminX.Data;
using AdminX.Meta;
using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class AlertController : Controller
    {
        private readonly ClinicalContext _context;
        private readonly AdminContext _adminContext;
        private readonly IPatientData _patientData;
        private readonly IAlertData _alertData;
        private readonly IAlertTypeData _alertTypeData;
        private readonly AlertVM _avm;

        public AlertController(ClinicalContext context, AdminContext adminContext)
        {
            _context = context;
            _adminContext = adminContext;
            _patientData = new PatientData(_context);
            _alertData = new AlertData(_context);
            _alertTypeData = new AlertTypeData(_adminContext);
            _avm = new AlertVM();
        }

        public IActionResult Index(int mpi)
        {
            _avm.patient = _patientData.GetPatientDetails(mpi);
            _avm.alertList = _alertData.GetAlertsListAll(mpi);

            return View(_avm);
        }

        public IActionResult AlertDetails(int alertID)
        {
            _avm.alert = _alertData.GetAlertDetails(alertID);
            _avm.patient = _patientData.GetPatientDetails(_avm.alert.MPI);

            return View(_avm);
        }

        [HttpGet]
        public IActionResult Create(int mpi)
        {            
            _avm.patient = _patientData.GetPatientDetails(mpi);
            _avm.alertList = _alertData.GetAlertsListAll(mpi);
            _avm.alertTypes = _alertTypeData.GetAlertTypes();

            return View(_avm);
        }

        [HttpPost]
        public IActionResult Create(int mpi, string alertType, bool isProtectedAddress, DateTime? startDate, string? comments)
        {            
            //CRUD etc
            int alertID = _alertData.GetAlertsList(mpi).OrderByDescending(a => a.AlertID).FirstOrDefault().AlertID;

            return RedirectToAction("AlertDetails", "Alert", new { alertID = alertID });
        }




        [HttpGet]
        public IActionResult Edit(int alertID)
        {
            _avm.alert = _alertData.GetAlertDetails(alertID);
            _avm.patient = _patientData.GetPatientDetails(_avm.alert.MPI);
            _avm.alertList = _alertData.GetAlertsListAll(_avm.alert.MPI);
            _avm.alertTypes = _alertTypeData.GetAlertTypes();

            return View(_avm);
        }

        [HttpPost]
        public IActionResult Edit(int alertID, string alertType, bool isProtectedAddress, DateTime? endDate, string? comments)
        {
            if(endDate == null)
            {
                endDate = DateTime.Parse("1900-01-01"); //because SQL can't take a null date so we have to convert it, then change it back again
            }
            //CRUD etc
            
            return RedirectToAction("AlertDetails", "Alert", new { alertID = alertID });
        }
    }
}
