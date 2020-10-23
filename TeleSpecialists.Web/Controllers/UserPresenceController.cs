using System;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using System.Collections.Generic;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Super Admin, Navigator")]
    public class UserPresenceController : BaseController
    {
        private readonly PhysicianStatusService _PhysicianStatusService;
        public UserPresenceController()
        {
            _PhysicianStatusService = new PhysicianStatusService();
        }
        // GET: physician_status
        public ActionResult Index()
        {
            return GetViewResult();
        }
        // GET: physician_status/Create
        public ActionResult Create()
        {
            return GetViewResult(new physician_status { phs_is_active = true });
        }
        // GET: physician_status/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            physician_status physician_status = _PhysicianStatusService.GetDetails(id.Value);
            if (physician_status == null)
            {
                return HttpNotFound();
            }
            return GetViewResult(physician_status);
        }
        public ActionResult Remove(int id)
        {
            try
            {
                var model = _PhysicianStatusService.GetDetails(id);
                var result = _PhysicianStatusService.Delete(model);
                if (!result)
                    ModelState.AddModelError("", "Record can not be deleted. Physicians are linked with it");

                if (ModelState.IsValid)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
        }

        // POST: physician_status/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Exclude = "phs_key, phs_sort_order")]physician_status physician_status)
        {
            if (ModelState.IsValid)
            {
                if (physician_status.phs_move_status_key.HasValue && physician_status.phs_move_threshhold_time == null)
                    ModelState.AddModelError("phs_move_threshhold_time", "Select the status move time");

                if (physician_status.phs_move_status_key == null && physician_status.phs_move_threshhold_time.HasValue)
                    ModelState.AddModelError("phs_move_status_key", "Select Move to State");

                if (_PhysicianStatusService.IsTypeAlreadyExists(physician_status))
                    ModelState.AddModelError("phs_name", $"Type {physician_status.phs_name} already exists");

                if (_PhysicianStatusService.IsColorCodeAlreadyExists(physician_status))
                    ModelState.AddModelError("phs_color_code", $"Color {physician_status.phs_color_code} is already assigned");

                if (ModelState.IsValid) // re validating model after custom validations
                {
                    physician_status.phs_created_by = User.Identity.GetUserId();
                    physician_status.phs_created_date = DateTime.Now.ToEST();
                    _PhysicianStatusService.Create(physician_status);

                    #region handling snooze data
                    var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<physician_status_snooze_option>>(Request.Params["hdnSnoozeJson"]);
                    if (jsonData.Count > 0)
                    {
                        jsonData.ForEach(m => {
                            m.pso_created_by = loggedInUser.Id;
                            m.pso_created_by_name = loggedInUser.FullName;
                        });
                    }
                    _PhysicianStatusService.SaveStatusSnoozeOptions(physician_status.phs_key, jsonData);

                    #endregion

                    return GetSuccessResult();
                }
            }

            return GetErrorResult(physician_status);
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _PhysicianStatusService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(physician_status physician_status)
        {
            if (ModelState.IsValid)
            {
                if (physician_status.phs_move_status_key.HasValue && physician_status.phs_move_threshhold_time == null)
                    ModelState.AddModelError("phs_move_threshhold_time", "Select the status move time");

                if (physician_status.phs_move_status_key == null && physician_status.phs_move_threshhold_time.HasValue)
                    ModelState.AddModelError("phs_move_status_key", "Select Move to State");


                if (_PhysicianStatusService.IsTypeAlreadyExists(physician_status))
                    ModelState.AddModelError("phs_name", $"Type {physician_status.phs_name} already exists");
                else if (_PhysicianStatusService.IsColorCodeAlreadyExists(physician_status))
                    ModelState.AddModelError("phs_color_code", $"Color {physician_status.phs_color_code} is already assigned");
                if (ModelState.IsValid)
                {

                    #region handling snooze data
                    var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<physician_status_snooze_option>>(Request.Params["hdnSnoozeJson"]);
                    if (jsonData.Count > 0)
                    {
                        jsonData.ForEach(m => {
                            m.pso_created_by = loggedInUser.Id;
                            m.pso_created_by_name = loggedInUser.FullName;
                        });
                    }
                    _PhysicianStatusService.SaveStatusSnoozeOptions(physician_status.phs_key,jsonData);

                    #endregion

                    physician_status.phs_modified_by = User.Identity.GetUserId();
                    physician_status.phs_modified_date = DateTime.Now.ToEST();

                    _PhysicianStatusService.Edit(physician_status);
                    return GetSuccessResult();
                }
            }
            return GetErrorResult(physician_status);
        }

        public ActionResult SnoozeOptionPopup(int phs_key)
        {
            return PartialView("_SnoozeOptionPopup", new physician_status_snooze_option { pso_phs_key = phs_key });
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
                    _PhysicianStatusService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }

}