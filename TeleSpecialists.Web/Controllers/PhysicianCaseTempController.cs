using System;
using System.Web.Mvc;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;
using TeleSpecialists.Web.Models;

namespace TeleSpecialists.Web
{
    public class PhysicianCaseTempController : BaseController
    {
        // GET: PhysicianCaseTemp
        private readonly PhysicianCaseTempService _physicianCaseTempService;

        public PhysicianCaseTempController()
        {
            _physicianCaseTempService = new PhysicianCaseTempService();
        }
        public ActionResult Save(PhysicianCaseTempViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                _physicianCaseTempService.SavePhysicianCaseTemp(model.pct_guid, model.pct_phy_key, model.pct_cst_key, model.pct_ctp_key, loggedInUser.Id);
                return GetSuccessResult();
            }
            else
            {
                return GetErrorResult(model, true);
            }
        }

        public ActionResult Delete(string Id)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                _physicianCaseTempService.DeleteById(new Guid(Id));
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message =  "Id is required" });
            }
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
                    _physicianCaseTempService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}