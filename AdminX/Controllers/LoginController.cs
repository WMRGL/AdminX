using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AdminX.Models;

namespace AdminX.Controllers
{
    public class LoginController : Controller
    {
        UserDataAccessLayer objUser = new  UserDataAccessLayer();
        [HttpGet]
        public IActionResult UserLogin()
        {           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogin([Bind] UserDetails user)
        {
            //ModelState.Remove("FIRSTNAME");
            //ModelState.Remove("LASTNAME");
           
            if (ModelState.IsValid)
            {
               
                string LoginStatus = objUser.ValidateLogin(user);
               
                if (LoginStatus == "Success") 
                {
                   
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.EMPLOYEE_NUMBER)
                    };
                    ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                   
                    TempData["UserLoginFailed"] = "Login failed. Please try again.";
                    return View();
                }
            }
            return View();
        }
    }
}
