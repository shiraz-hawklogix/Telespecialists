using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels;
using TeleSpecialists.Controllers;

namespace TeleSpecialists.Web.Controllers
{
    public class CaseRejectController : BaseController
    {
        private readonly CaseRejectService _casRejectService;

        public CaseRejectController()
        {
            _casRejectService = new CaseRejectService();
        }

        public ActionResult Index()
        {
            return GetViewResult();
        }

        [HttpGet]
        public ActionResult GetReasonsForDropdown()
        {
            var res = _casRejectService.GetDropdownReasons();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUsersForDropdown()
        {
            var users = _casRejectService.GetAllUsers();
            return Json(users, JsonRequestBehavior.AllowGet);
        }
        #region Case Rejection Reasons
        public ActionResult RejectionReason()
        {
            return GetViewResult();
        }
        [HttpPost]
        public ActionResult GetAllReasons(DataSourceRequest request)
        {
            var res =  _casRejectService.GetAll(request);
            List<case_rejection_reason> maping = (List<case_rejection_reason>)res.Data;
            List<CaseRejectionReasonsViewModel> ObjectList = new List<CaseRejectionReasonsViewModel>();
            foreach (var r in maping)
            {
                CaseRejectionReasonsViewModel TempObj = new CaseRejectionReasonsViewModel();

                if (r.crr_users != null)
                {
                    string usr = r.crr_users;
                    var list = usr.Split(',');
                    foreach (var li in list)
                    {
                        TempObj.crr_users += " " + _casRejectService.GetAllUsers().Where(xr => xr.Id == li).Select(m => m.FirstName).FirstOrDefault() +",";
                    }
                }
                TempObj.crr_key = r.crr_key;
                TempObj.crr_parent_reason = maping.Where(x => x.crr_key == r.crr_parent_key).Select(s => s.crr_reason).FirstOrDefault();
                TempObj.crr_sub_reason = r.crr_reason;
                TempObj.crr_troubleshoot = r.crr_troubleshoot;

                ObjectList.Add(TempObj);
            }
            res.Data = ObjectList;
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateReason()
        {
            ViewBag.dropdownUsers = _casRejectService.GetAllUsers().Select(m => new SelectListItem { Value = m.Id, Text = m.FirstName }).ToList();

            return GetViewResult();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReason(case_rejection_reason case_Reason)
        {
            ModelState.Remove("crr_key");
            if (ModelState.IsValid)
            {

                case_Reason.crr_created_by = loggedInUser.Id;
                case_Reason.crr_created_on = DateTime.Now;
                _casRejectService.Create(case_Reason);

                //return ShowSuccessMessageOnly("Case Rejection Reason Successfully Added", case_Reason);
                return GetSuccessResult("", "Case Rejection Reason Added Successfully.");
            }
            return GetErrorResult(case_Reason);
        }

        public ActionResult EditReason(int id)
        {
            var result = _casRejectService.GetDetails(id);
            List<SelectListItem> temp = _casRejectService.GetAllUsers().Select(m => new SelectListItem { Value = m.Id, Text = m.FirstName }).ToList();

            if (result.crr_users != null)
            {
                string usr = result.crr_users;
                var list = usr.Split(',');
                foreach(var li in list)
                {
                    temp.Where(t => t.Value == li).First().Selected = true;
                }
            }
            ViewBag.dropdownUsers = temp;

            return GetViewResult(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditReason(case_rejection_reason case_Rejection_Reason)
        {
            if (ModelState.IsValid)
            {

                case_Rejection_Reason.crr_modified_by = loggedInUser.Id;
                case_Rejection_Reason.crr_modified_on = DateTime.Now;
                _casRejectService.Edit(case_Rejection_Reason);

                //return ShowSuccessMessageOnly("Case Rejection Reason Successfully Updated", case_Rejection_Reason);
                return GetSuccessResult("", "Case Rejection Reason Updated Successfully.");
            }
            return GetErrorResult(case_Rejection_Reason);
        }
        public ActionResult Remove(int id)
        {
            try
            {
                var model = _casRejectService.GetDetails(id);
                var result = _casRejectService.Delete(model);
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
                    this._casRejectService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}