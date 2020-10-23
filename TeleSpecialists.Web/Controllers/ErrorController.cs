using System.Web.Mvc;

namespace TeleSpecialists.Controllers
{
    public class ErrorController : BaseController
    {
        // GET: Error
        public ActionResult NotFound()
        {
            ViewBag.Title = "Page Not Found";
            ViewBag.Message = "The page you request could not be found on our server.";
            return GetViewResult("Error");
        }
        public ActionResult ErrorOccured()
        {
            ViewBag.Title = "Server Error";
            ViewBag.Message = "Oops, Something went wrong. <br/> Try to refresh the page or feel free to contact us if the problem still persist";
            return GetViewResult("Error");
        }
    }
}