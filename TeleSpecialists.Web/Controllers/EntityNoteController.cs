using System;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Helpers;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.ViewModels.EntityNotes;
using System.Linq;

namespace TeleSpecialists.Controllers
{
    public class EntityNoteController : BaseController
    {
        // GET: EntityNote
        private  readonly EntityNoteService _entityNoteService;
        private readonly UCLService _uCLService;
        public EntityNoteController()
        {
            _entityNoteService = new EntityNoteService();
            _uCLService = new UCLService();
        }

        public ActionResult Index(string etn_entity_key, EntityTypes entity_type)
        {
            ViewBag.etn_entity_key = etn_entity_key;
            ViewBag.ent_key = entity_type.ToInt();
            return PartialView();
        }
        // GET: entity_note/Create
        public ActionResult Create(string etn_entity_key, EntityTypes entity_type)
        {
            ViewBag.IsSignOutNotes = entity_type == EntityTypes.SignOutNotes;
                ViewBag.etn_ntt_key = _entityNoteService.GetAll()
               .Select(m => new SelectListItem
               {
                   Text = m.ucd_title,
                   Value = m.ucd_key.ToString()
               });

                var noteTypeKey = 0;
                var nt = _uCLService.GetDefault(UclTypes.NoteType);
                if (nt != null)
                    noteTypeKey = nt.ucd_key;
                if (Request.IsAjaxRequest())
                {
                    return PartialView(new entity_note { etn_is_active = true, etn_entity_key = etn_entity_key, etn_ent_key = entity_type.ToInt(), etn_ntt_key = noteTypeKey });
                }
                else
                {
                    return View(new entity_note { etn_is_active = true, etn_entity_key = etn_entity_key, etn_ent_key = entity_type.ToInt() });
                } 
        }

        [HttpPost]
        public ActionResult GetSignOutNotes(DataSourceRequest request)
        {
            var res = _entityNoteService.GetEnityNotes(request,EntityTypes.SignOutNotes);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _entityNoteService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        // POST: entity_note/Create
        // To protect from over posting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(entity_note entity_note)
        {
            if (ModelState.IsValid)
            {
                entity_note.etn_created_by = User.Identity.GetUserId();
                entity_note.etn_created_by_name = loggedInUser.FullName;
                entity_note.etn_created_date = DateTime.Now.ToEST();
                _entityNoteService.Create(entity_note);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }
        // GET: entity_note/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.etn_ntt_key = _entityNoteService.GetAll()
                .Select(m => new SelectListItem
                {
                    Text = m.ucd_title,
                    Value = m.ucd_key.ToString()
                });

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            entity_note entity_note = _entityNoteService.GetDetails(id.Value);
            if (entity_note == null)
            {
                return HttpNotFound();
            }
            return PartialView(entity_note);
        }
        // POST: CaseType/Edit/5               
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(entity_note entity_note)
        {
            if (ModelState.IsValid)
            {
                entity_note.etn_modified_by = User.Identity.GetUserId();
                entity_note.etn_modified_by_name = loggedInUser.FullName;
                entity_note.etn_modified_date = DateTime.Now.ToEST();

                _entityNoteService.Edit(entity_note);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }

        [HttpPost]
        public ActionResult EditNotes(SignOutNoteViewModel vm)
        {
            try
            {  
                var entity_note = _entityNoteService.GetDetails(vm.Id);

                entity_note.etn_notes = vm.Notes;
                entity_note.etn_modified_by = User.Identity.GetUserId();
                entity_note.etn_modified_by_name = loggedInUser.FullName; 
                entity_note.etn_modified_date = DateTime.Now.ToEST();

                _entityNoteService.Edit(entity_note);


                var noteVM = _entityNoteService.GetSingOutNotes(vm.Id);

                /// Assing the current user's Full Name here. To reduce CPU cycles and joinings.
                noteVM.Name = loggedInUser.FullName;

                return Json(noteVM);
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }

        public ActionResult Remove(int id)
        {
            try
            {
                var model = _entityNoteService.GetDetails(id);
                model.etn_modified_by = User.Identity.GetUserId();
                model.etn_modified_date = DateTime.Now.ToEST();
                model.etn_is_active = false;
                _entityNoteService.Edit(model);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
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
                    _entityNoteService?.Dispose();
                    _uCLService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}