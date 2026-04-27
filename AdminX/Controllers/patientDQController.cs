
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
            try
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
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "PatientDQ" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(DateTime startDate, DateTime endDate)
        {

            try
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

           
                viewModel.CgundbPatients = await _patientDQData.GetPatientsCGUDBNotInEPICAsync(startDate, inclusiveEndDate);
                viewModel.EpicPatients = await _patientDQData.GetPatientsEPICNotInCGUDBAsync(startDate, inclusiveEndDate);
                viewModel.PatientMismatches = await _patientDQData.GetPatientMismatchesAsync(startDate, inclusiveEndDate);

                return View(viewModel);
            }

            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Index" });
            }           
        }

        [HttpGet]
        public async Task<IActionResult> PatientDQReport(DateTime startDate, DateTime endDate) 
        {
            try
            {
                if (User.Identity.Name is null)
                {
                    return RedirectToAction("UserLogin", "Login");
                }

                var viewModel = new DiscrepancyReportVM
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                if (endDate < startDate)
                {
                    ModelState.AddModelError(string.Empty, "End date cannot be earlier than start date.");
                    return View("Index", viewModel); 
                }
                if (startDate < new DateTime(2025, 5, 15))
                {
                    ModelState.AddModelError(string.Empty, "Dates must be after May 15, 2025.");
                    return View("Index", viewModel);
                }

                var inclusiveEndDate = endDate.AddDays(1);

            
                viewModel.CgundbPatients = await _patientDQData.GetPatientsCGUDBNotInEPICAsync(startDate, inclusiveEndDate);
                viewModel.EpicPatients = await _patientDQData.GetPatientsEPICNotInCGUDBAsync(startDate, inclusiveEndDate);
                viewModel.PatientMismatches = await _patientDQData.GetPatientMismatchesAsync(startDate, inclusiveEndDate);

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "Index" });
            }    
        }
    }
}
