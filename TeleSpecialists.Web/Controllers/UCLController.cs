using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Super Admin")]
    public class UCLController : BaseController
    {
        UCLService _uclService = new UCLService();
        // GET: UCL
        public ActionResult Index(int id)
        {
            ViewBag.UclType = id;

            return GetViewResult();
        }

        public ActionResult Test()
        {
            var users = new List<string>();
            users.Add(loggedInUser.Id);
            var methodParams = new List<object>();
            methodParams.Add("4");
            methodParams.Add(false);
            new Web.Hubs.WebSocketEventHandler().CallJSMethod(users, new Web.Hubs.SocketResponseModel { MethodName = "showPhysicianStatusSnoozePopup_def", Data = methodParams });
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        

        // GET: ucl_data/Create
        public ActionResult Create(int type)
        {
            var parent = _uclService.GetParent(type);
            return GetViewResult(new ucl_data { ucd_ucl_key = type, ucd_is_active = true, ucl = parent });
        }
        // GET: ucl_data/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ucl_data ucl_data = _uclService.GetDetails(id.Value);
            if (ucl_data == null)
            {
                return HttpNotFound();
            }
            return GetViewResult(ucl_data);
        }
        public ActionResult Remove(int id)
        {
            try
            {
                var model = _uclService.GetDetails(id);
                int type = model.ucd_ucl_key.Value;
                var result = _uclService.Delete(model);
                if (!result)
                    ModelState.AddModelError("", "Record can not be deleted. Data is linked with it");
                else
                    clearLookUpCache(type);

                if (ModelState.IsValid)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _uclService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "ucd_key, ucd_sort_order")]ucl_data model)
        {
            if (string.IsNullOrEmpty(model.ucd_title))
                ModelState.AddModelError("ucd_title", "The Title field is required.");

            else if (string.IsNullOrEmpty(model.ucd_description))
                ModelState.AddModelError("ucd_description", "The Description field is required.");

            if (ModelState.IsValid)
            {
                if (_uclService.IsAlreadyExists(model))
                    ModelState.AddModelError("ucd_title", $"Type {model.ucd_title} already exists");
                else
                {
                    model.ucd_created_by = User.Identity.GetUserId();
                    model.ucd_created_date = DateTime.Now.ToEST();
                    model.ucl = null;
                    _uclService.Create(model);
                    clearLookUpCache(model.ucd_ucl_key.Value);
                    return GetSuccessResult(Url.Action("Index", new { @id = model.ucd_ucl_key }));
                }  
            }

            return GetErrorResult(model);
        }  
            

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ucl_data model)
        {
            if (ModelState.IsValid)
            {
                if (_uclService.IsAlreadyExists(model))
                    ModelState.AddModelError("ucd_title", $"Type {model.ucd_title} already exists");
                else
                {
                    model.ucd_modified_by = User.Identity.GetUserId();
                    model.ucd_modified_date = DateTime.Now.ToEST();
                    model.ucl = null;

                    _uclService.Edit(model);
                    clearLookUpCache(model.ucd_ucl_key.Value);

                    return GetSuccessResult(Url.Action("Index", new { @id = model.ucd_ucl_key }));
                }
            }
            return GetErrorResult(model);
        }

        private void clearLookUpCache(int type)
        {
            var uclType = Url.Action("GetAll", "Lookup", new
            {
                type = type
            });
            HttpRuntime.Cache.Remove(uclType);
        }

        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _uclService?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
