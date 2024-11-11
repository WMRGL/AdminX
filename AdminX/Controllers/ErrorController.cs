using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace AdminX.Controllers
{
    public class ErrorController : Controller
    {       
        [HttpGet]
        public IActionResult ErrorHome(string error, string? formName="")
        {            

            using (StreamWriter sw = System.IO.File.AppendText($"wwwroot/ErrorLogs/Log.txt"))
            {
                sw.WriteLine($"{DateTime.Now}: {error} - logged in user: {User.Identity.Name} - form: {formName}");
            }

            return View("ErrorHome", error);
        }
    }
}
