using AdminX.ViewModels;
using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class EDMSMoverController : Controller
    {
        private readonly ClinicalContext _clinicalContext;
        private readonly DocumentContext _documentContext;
        private readonly IPatientData _patientData;
        private readonly IConstantsData _constantsData;
        private readonly IDocKindsData _docKindsData;
        private readonly EDMSVM _vm;

        public EDMSMoverController(ClinicalContext clinicalContext, DocumentContext documentContext)
        {
            _clinicalContext = clinicalContext;
            _documentContext = documentContext;
            _patientData = new PatientData(_clinicalContext);
            _constantsData = new ConstantsData(_documentContext);
            _docKindsData = new DocKindsData(_documentContext);
            _vm = new EDMSVM();
        }

        [HttpGet]
        public async Task<IActionResult> Upload(int mpi, string? message, bool? success)
        {
            _vm.patient = _patientData.GetPatientDetails(mpi);
            _vm.docKinds = _docKindsData.GetDocumentKindsList();

            if(message != null) { _vm.Message = message; }
            _vm.isSuccess = success.GetValueOrDefault();

            return View(_vm);
        }
        [HttpPost]
        public async Task<IActionResult> Upload(int mpi, string docType, int taskRouting, IFormFile fileToUpload)        
        {
            string destFilename = mpi.ToString() + "-" + docType + "-" + taskRouting.ToString() + "-" +
                DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() +
                DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + 
                fileToUpload.FileName.Substring(fileToUpload.FileName.IndexOf("."), fileToUpload.FileName.Length - fileToUpload.FileName.IndexOf("."));


            string targetFileName = _constantsData.GetConstant("FilePathEDMS", 1) + "\\" + destFilename;
            
            string sMessage = "";
            bool isSuccess = false;
            
            using (var stream = new FileStream(targetFileName, FileMode.Create))
            {
                await fileToUpload.CopyToAsync(stream);
                sMessage = "Success - File uploaded.";
                isSuccess = true;
            }
            
            return RedirectToAction("Upload", new { mpi=mpi, message = sMessage, success = isSuccess });
        }
    }
}
