using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Physician,Partner Physician,Administrator,Super Admin,Navigator,Outsourced Navigator,Regional Medical Director,Quality Team, RRC Manager, RRC Director, VP Quality, Quality Director")]
    public class RapidsController : BaseController
    {
        private readonly RapidsService _rapidsService;

        public RapidsController()
        {
            _rapidsService = new RapidsService();
        }

        // GET: Rapids
        public ActionResult Index()
        {
            return PartialView();
        }

        public ActionResult Details(int id)
        {
            if (id <= 0) return RedirectToAction("Index");

            var model = _rapidsService.GetDetails(id);
            try
            {
                // mark as read if its a new email
                if (!model.rpd_is_read)
                {
                    model.rpd_is_read = true;
                    _rapidsService.Edit(model);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            string result = RenderPartialViewToString("Details", model);

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAll(DataSourceRequest request)
        {             
            var res = _rapidsService.GetAll(request); 
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult CheckEmails()
        {
            var maxId = 0;
            var readEmails = 0;
            try
            {
                var mails = _rapidsService.GetAll();
                var count = mails.Count();
                if (count > 0)
                {
                    maxId = _rapidsService.GetAll().Max(m => m.Id);
                    readEmails = _rapidsService.GetAll().Count(m => m.isRead);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex); 
            } 

            return Json(new { id = maxId ,read= readEmails }, JsonRequestBehavior.AllowGet);
        }

        /*
        public JsonResult DownloadAll(string uid)
        {
            string archivePath = _rapidsService.DownloadAllAttachments(uid, Server.MapPath("/RapidsAttachments"));

            return Json(new { ArchivePath = archivePath }, JsonRequestBehavior.AllowGet);
        }
        */

        #region ----- Disposable -----

        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _rapidsService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
