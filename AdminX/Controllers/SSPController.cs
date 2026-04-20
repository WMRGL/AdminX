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

        public SSPController(IPatientDataAsync patientDataAsync, ISSPDataAsync sspData)
        {
            _patientData = patientDataAsync;
            _sspData = sspData;
            _sspvm = new SSPVM();
        }

        public async Task<IActionResult> Index()
        {
            _sspvm.ssps = await _sspData.GetSSPList();

            return View(_sspvm);
        }
    }
}
