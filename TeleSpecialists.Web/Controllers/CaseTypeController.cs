using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;

namespace TeleSpecialists.Web.Controllers
{
    public class CaseTypeController : BaseController
    {
        private readonly CasCancelledTypeService _casCancelledTypeService;

        public CaseTypeController()
        {
            _casCancelledTypeService = new CasCancelledTypeService();
        }
        // GET: CaseType
        public ActionResult Index()
        {
            return View();
        }
        #region Case Cancelled Type
        public ActionResult Cancelled()
        {
            return GetViewResult();
        }
        [HttpPost]
        public ActionResult GetAllCancelled(DataSourceRequest request)
        {
            var res = _casCancelledTypeService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreateCancelled()
        {
            return GetViewResult();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCancelled(case_cancelled_type case_Cancelled_Type)
        {
            ModelState.Remove("cct_key");
            if (ModelState.IsValid)
            {

                case_Cancelled_Type.cct_created_by = loggedInUser.Id;
                case_Cancelled_Type.cct_created_on = DateTime.Now;
                _casCancelledTypeService.Create(case_Cancelled_Type);

                return ShowSuccessMessageOnly("Case Cancelled Type Successfully Added", case_Cancelled_Type);
            }
            return GetErrorResult(case_Cancelled_Type);
        }

        public ActionResult EditCancelled(int id)
        {
            var result = _casCancelledTypeService.GetDetails(id);
            return GetViewResult(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCancelled(case_cancelled_type case_Cancelled_Type)
        {
            if (ModelState.IsValid)
            {

                case_Cancelled_Type.cct_modified_by = loggedInUser.Id;
                case_Cancelled_Type.cct_modified_on = DateTime.Now;
                _casCancelledTypeService.Edit(case_Cancelled_Type);

                return ShowSuccessMessageOnly("Case Cancelled Type Successfully Updated", case_Cancelled_Type);
            }
            return GetErrorResult(case_Cancelled_Type);
        }
        public ActionResult Remove(int id)
        {
            try
            {
                var model = _casCancelledTypeService.GetDetails(id);
                var result = _casCancelledTypeService.Delete(model);
                if (!result)
                    ModelState.AddModelError("", "Record can not be deleted");

                if (ModelState.IsValid)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _casCancelledTypeService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}