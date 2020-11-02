using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;

namespace TeleSpecialists.Web.Controllers
{
    public class MenuController : BaseController
    {
        private readonly MenuService _menuService;
        public MenuController() : base()
        {
            _menuService = new MenuService();
        }
        // GET: Menu
        public ActionResult Index()
        {
            return GetViewResult();
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
          var res = _menuService.GetAll(request);
          return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create()
        {
            ViewBag.Menues = _menuService.GetAllMenues()
                                             .Select(m => new { Value = m.com_key, Text = m.com_module_name })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return GetViewResult(new component {com_status = true });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(component model)
        {
            if (string.IsNullOrEmpty(model.com_module_name))
                ModelState.AddModelError("com_module_name", "Module Name is required.");

            else if (string.IsNullOrEmpty(model.com_page_url))
                ModelState.AddModelError("com_page_url", "page Url is required.");

            if (ModelState.IsValid)
            {
                if (_menuService.IsAlreadyExists(model))
                    ModelState.AddModelError("com_module_name", $" {model.com_module_name} already exists");
                else
                {
                    model.com_addedby = User.Identity.GetUserId();
                    model.com_addedon = DateTime.Now.ToEST();
                    model.com_modifiedby = User.Identity.GetUserId();
                    model.com_modifiedon = DateTime.Now.ToEST();
                    if (model.com_parentcomponentid == 0)
                    {
                        model.com_parentcomponentid = null;
                    }
                    _menuService.Create(model);

                    return GetSuccessResult(Url.Action("Index", new { @id = model.com_key }));
                }
            }

            return GetErrorResult(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Menues = _menuService.GetAllMenues()
                                            .Select(m => new { Value = m.com_key, Text = m.com_module_name })
                                            .ToList()
                                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            component com_data = _menuService.GetDetails(id.Value);

            if (com_data == null)
            {
                return HttpNotFound();
            }
            return GetViewResult(com_data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(component model)
        {
            if (string.IsNullOrEmpty(model.com_module_name))
                ModelState.AddModelError("com_module_name", "Module Name is required.");

            else if (string.IsNullOrEmpty(model.com_page_url))
                ModelState.AddModelError("com_page_url", "page Url is required.");
            if (ModelState.IsValid)
            {
                if (model.com_key > 0)
                {
                    if (_menuService.IsAlreadyExists(model))
                        ModelState.AddModelError("com_module_name", $" {model.com_module_name} already exists");
                }
                
                else
                {
                    model.com_modifiedby = User.Identity.GetUserId();
                    model.com_modifiedon = DateTime.Now.ToEST();


                    _menuService.Edit(model);
                   
                    return GetSuccessResult(Url.Action("Index", new { @id = model.com_key }));
                }
            }
            return GetErrorResult(model);
        }
        public ActionResult Remove(int id)
        {
            try
            {
                var model = _menuService.GetDetails(id);
                int type = model.com_key;
                var result = _menuService.Delete(model);
                if (!result)
                    ModelState.AddModelError("", "Record can not be deleted. Data is linked with it");
                else
                   

                if (ModelState.IsValid)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
        }
    }
}