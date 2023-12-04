using Microsoft.AspNetCore.Mvc;
using Tranning.Models;
using Tranning.Queries;

namespace Tranning.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            LoginModel model = new LoginModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(LoginModel model)
        {
            model = new LoginQueries().CheckLoginUser(model.Username, model.Password);
            if (string.IsNullOrEmpty(model.UserID) || string.IsNullOrEmpty(model.Username))
            {
                // Invalid login attempt - display an error message
                ViewData["MessageLogin"] = "Account invalid";
                return View(model);
            }

            // Save user information to session
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("SessionUserID")))
            {
                HttpContext.Session.SetString("SessionUserID", model.UserID);
                HttpContext.Session.SetString("SessionRoleID", model.RoleID);
                HttpContext.Session.SetString("SessionUsername", model.Username);
                HttpContext.Session.SetString("SessionEmail", model.EmailUser);
            }

            // Redirect to the appropriate home page based on user role and ID
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else if (User.IsInRole("User"))
            {
                // Check the user ID and redirect accordingly
                if (model.UserID == "1")
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                else if (model.UserID == "2")
                {
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                else
                {
                    // Handle unexpected user ID
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
            }
            else
            {
                // Handle unexpected role
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SessionUserID")))
            {
                // xoa cac session da dc tao ra
                HttpContext.Session.Remove("SessionUserID");
                HttpContext.Session.Remove("SessionRoleID");
                HttpContext.Session.Remove("SessionUsername");
                HttpContext.Session.Remove("SessionEmail");
            }
            return RedirectToAction(nameof(LoginController.Index), "Login");
        }
    }
}
