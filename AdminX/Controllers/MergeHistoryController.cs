using AdminX.Meta;
using AdminX.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ClinicalXPDataConnections.Meta;
using ClinicalXPDataConnections.Models;
using AdminX.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdminX.Controllers
{
    public class MergeHistoryController : Controller
    {
        //private readonly ClinicalContext _clinContext;
        //private readonly AdminContext _context;
        private readonly IMergeHistoryDataAsync _mhData;
        private readonly IPatientDataAsync _patientData;
        private readonly MergeHistoryVM _mhvm;

        public MergeHistoryController(IConfiguration config, IMergeHistoryDataAsync mergeHistory, IPatientDataAsync patient)
        {
            //_clinContext = clinContext;
            //_context = context;
            _mhData = mergeHistory;
            _patientData = patient;
            _mhvm = new MergeHistoryVM();
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string? oldFileNo, string? newCGUNo, string? nhsNo, string? firstName, string? lastName, DateTime? dob, bool? isPostback = false)
        {
            try
            {

                if (isPostback.GetValueOrDefault()) //obviously we can't get the list and send it to the calling form. That would be too easy.
                {
                    _mhvm.mergeHistory = new List<MergeHistory>();

                    if (firstName != null || lastName != null || dob != null || nhsNo != null)
                    {
                        Patient patient = new Patient();
                        patient = await _patientData.GetPatientDetailsByDemographicData(firstName, lastName, nhsNo, dob.GetValueOrDefault());
                        if (patient != null)
                        {
                            _mhvm.mergeHistory = await _mhData.GetMergeHistoryByMPI(patient.MPI);
                        }
                    }

                    if (oldFileNo != null)
                    {
                        _mhvm.mergeHistory = await _mhData.GetMergeHistoryByOldFileNo(oldFileNo);
                    }

                    if (newCGUNo != null)
                    {
                        _mhvm.mergeHistory = await _mhData.GetMergeHistoryByNewFileNo(newCGUNo);
                    }

                    if (_mhvm.mergeHistory.Count == 0)
                    {
                        _mhvm.message = "No merge history found";
                    }
                }

                return View(_mhvm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "MergeHistory" });
            }
        }

        [HttpPost]
        public IActionResult Search(string? oldFileNo, string? newCGUNo, string? nhsNo, string? firstName, string? lastName, DateTime? dob)
        {
            try
            {


                return RedirectToAction("Index", new
                {
                    oldFileNo = oldFileNo,
                    newCGUNo = newCGUNo,
                    nhsNo = nhsNo,
                    firstName = firstName,
                    lastName = lastName,
                    dob = dob,
                    isPostback = true
                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "MergeHistorySearch" });
            }
        }
    }
}
