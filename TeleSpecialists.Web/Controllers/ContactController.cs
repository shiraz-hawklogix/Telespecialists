using System;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using System.Linq;

namespace TeleSpecialists.Controllers
{
    public class ContactController : BaseController
    {
        // GET: Contact
        private readonly ContactService _contactService;
        private readonly UCLService _uclService;
        public ContactController()
        {
            _contactService = new ContactService();
            _uclService = new UCLService();
        }
        public ActionResult Index(Guid fac_key)
        {
            ViewBag.fac_key = fac_key;
            return PartialView();
        }
        // GET: contact/Create
        public ActionResult Create(Guid fac_key)
        { 
            var cnt_role_list = _uclService.GetUclData(UclTypes.ContactRole).Where(c => c.ucd_is_active).OrderBy(s=>s.ucd_title)
                 .Select(m => new SelectListItem
                 {
                     Text = m.ucd_title,
                     Value = m.ucd_key.ToString(),
                     Selected= m.ucd_is_default
                 }).ToList();
            ViewBag.cnt_role_list = cnt_role_list;


            if (Request.IsAjaxRequest())
            {
                return PartialView(new contact { cnt_is_active = true, cnt_fac_key = fac_key });
            }
            else
            {
                return View(new contact { cnt_is_active = true, cnt_fac_key = fac_key });
            }

        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _contactService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        // POST: contact/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(contact contact)
        {

            if (string.IsNullOrEmpty(contact.cnt_mobile_phone) && string.IsNullOrEmpty(contact.cnt_primary_phone))
                ModelState.AddModelError("", "Enter Primary Phone or Mobile Number");

            if (ModelState.IsValid)
            {

                contact.cnt_created_by = User.Identity.GetUserId();
                contact.cnt_created_by_name = loggedInUser.FullName;
                contact.cnt_created_date = DateTime.Now.ToEST();

                contact.cnt_mobile_phone = Functions.ClearPhoneFormat(contact.cnt_mobile_phone);
                contact.cnt_primary_phone = Functions.ClearPhoneFormat(contact.cnt_primary_phone);
                _contactService.Create(contact);
                return Json(new { success = true });

            }
            else
            {

                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }


        }
        // GET: contact/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            contact contact = _contactService.GetDetails(id.Value);
            if (contact == null)
            {
                return HttpNotFound();
            }

            var cnt_role_list = _uclService.GetUclData(UclTypes.ContactRole).Where(c=>c.ucd_is_active).OrderBy(s => s.ucd_title)
                  .Select(m => new SelectListItem
                  {
                      Text = m.ucd_title,
                      Value = m.ucd_key.ToString(), 
                  }).ToList();
            ViewBag.cnt_role_list = cnt_role_list;


            return PartialView(contact);
        }
        // POST: CaseType/Edit/5               
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(contact contact)
        {
            if (string.IsNullOrEmpty(contact.cnt_mobile_phone) && string.IsNullOrEmpty(contact.cnt_primary_phone))
                ModelState.AddModelError("", "Enter Primary Phone or Mobile Number");

            if (ModelState.IsValid)
            {

                contact.cnt_modified_by = User.Identity.GetUserId();
                contact.cnt_modified_date = DateTime.Now.ToEST();
                contact.cnt_modified_by_name = loggedInUser.FullName;

                contact.cnt_mobile_phone = Functions.ClearPhoneFormat(contact.cnt_mobile_phone);
                contact.cnt_primary_phone = Functions.ClearPhoneFormat(contact.cnt_primary_phone);
                

                _contactService.Edit(contact);
                return Json(new { success = true });
            }
            else
            {

                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
        }
        public ActionResult Remove(int id)
        {
            try
            {
                contact contact = _contactService.GetDetails(id);
                
                contact.cnt_modified_by =  this.loggedInUser.Id;
                contact.cnt_modified_by_name = this.loggedInUser.FullName;
                contact.cnt_modified_date = DateTime.Now.ToEST(); 
                ///
                ///Soft Deleting contact
                /// 
                contact.cnt_is_deleted = true;
                _contactService.Edit(contact);
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
                    _contactService?.Dispose();
                    _uclService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}