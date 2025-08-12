using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class patientDQController : Controller
    {
        private readonly IPatientDQData _patientDQData;
        
        public patientDQController(IPatientDQData patientDQData)
        {
            _patientDQData = patientDQData;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.Name is null)
            {
                return RedirectToAction("UserLogin", "Login");
            }

            var viewModel = new DiscrepancyReportVM
            {
                StartDate = DateTime.Today.AddDays(-7),
                EndDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime startDate, DateTime endDate)
        {
            if (User.Identity.Name is null)
            {
                return RedirectToAction("UserLogin", "Login");
            }

            if (endDate < startDate)
            {
                ViewBag.ErrorMessage = "End date cannot be earlier than start date.";
                return View();
            }
            if (startDate < new DateTime(2025, 5, 15))
            {
                ViewBag.ErrorMessage = "Dates must be after May 15, 2025.";
                return View();
            }

            var inclusiveEndDate = endDate.AddDays(1);
            var viewModel = new DiscrepancyReportVM
            {
                StartDate = startDate,
                EndDate = endDate
            };

            try
            {
                viewModel.CgundbPatients = await _patientDQData.GetPatientsCGUDBNotInEPICAsync(startDate, inclusiveEndDate);
                viewModel.EpicPatients = await _patientDQData.GetPatientsEPICNotInCGUDBAsync(startDate, inclusiveEndDate);
                viewModel.PatientMismatches = await _patientDQData.GetPatientMismatchesAsync(startDate, inclusiveEndDate);
            }

            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Index" });
            }

            return View(viewModel);
        }
    }
}
