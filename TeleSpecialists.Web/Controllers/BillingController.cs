using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;
using TeleSpecialists.Models;
using TeleSpecialists.Web.Hubs;
using TeleSpecialists.Web.Models;



namespace TeleSpecialists.Web.Controllers
{
    public class BillingController : BaseController
    {
        private readonly RateService _rateService;
        public BillingController()
        {
            _rateService = new RateService();
        }

        // GET: Billing
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult _getBill(string physicians, DateTime startDate, DateTime endDate)
        {
            string user = User.Identity.GetUserId();
            //startDate = startDate.AddDays(-18);
            //endDate = endDate.AddDays(-18);
            Kendo.DynamicLinq.DataSourceRequest request = null;
            List<int> caseStatus = new List<int>();
            List<string> physiciansList = new List<string>();
            physiciansList.Add(user);
            decimal amount = 0;
            try
            {
                var result = _rateService.GetPhysicianBillingAmount(request, physiciansList, startDate, endDate, caseStatus, ShiftType.All, requestFor: "load");
                if (result != null)
                    amount = result.Where(x => x.AssignDate == "Total Earned").Single().Amount;
                        //result.Sum(x => x.Amount); use this code when multiple physician open
                //amount = _rateService.getBill(user, startDate, endDate); default code
                return Json(amount, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(amount, JsonRequestBehavior.AllowGet);
            }
            
        }

        #region ----- Disposable -----

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _rateService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}