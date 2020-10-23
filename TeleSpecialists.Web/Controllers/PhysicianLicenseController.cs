using System;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using System.Linq;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Super Admin")]
    public class PhysicianLicenseController : BaseController
    {
        private readonly PhysicianLicenseService _physicianLicenseService;
        private readonly LookupService _lookupService;
        private readonly UCLService _uclService;
        
        public PhysicianLicenseController()
        {
            _physicianLicenseService = new PhysicianLicenseService();
            _lookupService = new LookupService();
            _uclService = new UCLService();
        }
        public ActionResult Index(string phy_key)
        {
            ViewBag.phy_key = phy_key;
            return PartialView();
        }
        public ActionResult Create(string phy_key)
        {
            ViewBag.phl_license_state = _uclService.GetUclData(UclTypes.State)
                .Select(m => new SelectListItem
                {
                    Text = m.ucd_description,
                    Value = m.ucd_key.ToString()
                });
            return PartialView(new physician_license { phl_user_key = phy_key,  phl_is_active = true });
        }
        public ActionResult Edit(Guid? id)
        {
            ViewBag.phl_license_state = _uclService.GetUclData(UclTypes.State)
                 .Select(m => new SelectListItem
                 {
                     Text = m.ucd_description,
                     Value = m.ucd_key.ToString()
                 });
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            physician_license physician_license = _physicianLicenseService.GetDetails(id.Value);
            if (physician_license == null)
            {
                return HttpNotFound();
            }
            return PartialView(physician_license);
        }
        // POST: CaseType/Edit/5               
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(physician_license physician_license)
        {
            if (ModelState.IsValid)
            {
                physician_license.phl_modified_by = User.Identity.GetUserId();
                physician_license.phl_modified_date = DateTime.Now.ToEST();
                physician_license.phl_modified_by_name = loggedInUser.FullName;

                _physicianLicenseService.Edit(physician_license);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }
        public ActionResult Remove(Guid id)
        {
            try
            {
                var model = _physicianLicenseService.GetDetails(id);
                model.phl_modified_by = User.Identity.GetUserId();
                model.phl_modified_date = DateTime.Now.ToEST();
                model.phl_modified_by_name = loggedInUser.FullName;

                model.phl_is_active = false;
                model.phl_expired_date = DateTime.Now.ToEST();
                _physicianLicenseService.Edit(model);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult Create(physician_license model)
        {
            if (_physicianLicenseService.IsAlreadyExists(model))
                ModelState.AddModelError("", "License already exists");

            if (ModelState.IsValid)
            {
                model.phl_key = Guid.NewGuid();
                model.phl_created_by_name = loggedInUser.FullName;
                model.phl_created_by = User.Identity.GetUserId();
                model.phl_is_active = true;
                model.phl_created_date = DateTime.Now.ToEST();
                _physicianLicenseService.Create(model);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _physicianLicenseService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
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
                    _physicianLicenseService?.Dispose();
                    _lookupService?.Dispose();
                    _uclService?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
