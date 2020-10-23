using System.Web.Mvc;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    public class PhonebarController : BaseController
    {
        public PhonebarController()
        {
        }    
        public PartialViewResult _index()
        {
            return PartialView();
        }
    }
}