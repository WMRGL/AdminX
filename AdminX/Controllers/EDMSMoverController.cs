using AdminX.ViewModels;
//using ClinicalXPDataConnections.Data;
using ClinicalXPDataConnections.Meta;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class EDMSMoverController : Controller
    {
        //private readonly ClinicalContext _clinicalContext;
        //private readonly DocumentContext _documentContext;
        private readonly IPatientDataAsync _patientData;
        private readonly IConstantsDataAsync _constantsData;
        private readonly IDocKindsDataAsync _docKindsData;
        private readonly EDMSVM _vm;
        private readonly IStaffUserDataAsync _staffUserData;
        private readonly IAuditServiceAsync _audit;
        private readonly IPAddressFinder _ip;
        private readonly IConfiguration _config;

        public EDMSMoverController(IConfiguration configuration, IPatientDataAsync patient, IConstantsDataAsync constants, IDocKindsDataAsync docKinds, IStaffUserDataAsync staffUser, 
            IAuditServiceAsync audit)
        {
            _config = configuration;            
            _patientData = patient;
            _constantsData = constants;
            _docKindsData = docKinds;
            _vm = new EDMSVM();
            _staffUserData = staffUser;
            _audit = audit;
            _ip = new IPAddressFinder(HttpContext);
        }

        [HttpGet]
        public async Task<IActionResult> Upload(int mpi, string? message, bool? success)
        {
            try
            {
                string staffCode = await _staffUserData.GetStaffCode(User.Identity.Name);
                _audit.CreateUsageAuditEntry(staffCode, "AdminX - Upload to EDMS", "MPI=" + mpi.ToString(), _ip.GetIPAddress());

                _vm.patient = await _patientData.GetPatientDetails(mpi);
                _vm.docKinds = await _docKindsData.GetDocumentKindsList();

                if (message != null) { _vm.Message = message; }
                _vm.isSuccess = success.GetValueOrDefault();

                return View(_vm);
            }
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EDMSUpload" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Upload(int mpi, string docType, int taskRouting, IFormFile fileToUpload)        
        {
            try
            {
                string destFilename = mpi.ToString() + "-" + docType + "-" + taskRouting.ToString() + "-" +
                    DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() +
                    DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() +
                    fileToUpload.FileName.Substring(fileToUpload.FileName.IndexOf("."), fileToUpload.FileName.Length - fileToUpload.FileName.IndexOf("."));


                string targetFileName = await _constantsData.GetConstant("FilePathEDMS", 1) + "\\" + destFilename;

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
            catch (Exception ex)
            {
                return RedirectToAction("ErrorHome", "Error", new { error = ex.Message, formName = "EDMSUpload" });
            }
        }
    }
}
