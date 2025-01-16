using ClinicalXPDataConnections.Data;
using APIControllers.Controllers;
using APIControllers.Data;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.Controllers
{
    public class PhenotipsController : Controller
    {
        private readonly ClinicalContext _clinContext;
        private readonly DocumentContext _docContext;
        private readonly APIContext _apiContext;
        private readonly IConfiguration _config;

        public PhenotipsController(ClinicalContext clinContext, DocumentContext docContext, APIContext apiContext, IConfiguration config)
        {
            _clinContext = clinContext;
            _docContext = docContext;
            _apiContext = apiContext;
            _config = config;
        }

        public async Task<IActionResult> Confirm(int mpi)
        {
            string sMessage = "";
            bool isSuccess = false;

            APIController api = new APIController(_apiContext, _config);

            Int16 result = await api.PushPtToPhenotips(mpi);

            if(result==1)
            {
                isSuccess = true;
                sMessage = "Push to Phenotips successful";
            }
            else if (result == 0)
            {
                sMessage = "Patient already exists in Phenotips!";
            }
            else
            {
                sMessage = "Push to Phenotips failed :(";
            }

            return RedirectToAction("PatientDetails", "Patient", new { id = mpi, success = isSuccess, message = sMessage });
        }
    }
}
