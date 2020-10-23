using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using TeleSpecialists.BLL.Process.MDStaff;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Super Admin")]
    public class MDStaffController : BaseController
    {
        private readonly MDStaffImport _mdStaffService;

        public MDStaffController()
        {
            _mdStaffService = new MDStaffImport();
        }
        // GET: MDStaff
        public ActionResult Import()
        {
            ViewBag.RequestId = Guid.NewGuid().ToString();
            return GetViewResult();
        }
        public async Task<JsonResult> SyncData(string requestId)
        {
            try
            {
                await _mdStaffService.GetAll(requestId, loggedInUser.Id, loggedInUser.FullName);
              
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { error = ex.ToString(), success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetLogs(string requestId)
        {
            var result = _mdStaffService.GetImportLogs(requestId);
            return Json(new { success = true, Data = result });
        }

        #region ----- IDispose -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _mdStaffService?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}