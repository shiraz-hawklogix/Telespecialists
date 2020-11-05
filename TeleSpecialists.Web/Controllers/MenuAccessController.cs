using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;

namespace TeleSpecialists.Web.Controllers
{
    public class MenuAccessController : BaseController
    {
        // GET: MenuAccess
        private readonly MenuService _menuService;
        private readonly AdminService _adminService;
        public MenuAccessController() : base()
            {
            _menuService = new MenuService();
            _adminService = new AdminService();
        }
        public ActionResult Index()
        {
            ViewBag.roles = _adminService.GetAllRoles()
                                            .Select(m => new { Value = m.Id, Text = m.Name})
                                            .ToList()
                                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return View();
        }
        public ActionResult _Index(string ID)
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = Session["userRole"].ToString();
            }
          
                ViewBag.roleId = ID;
           
            var result = _menuService.getMenuAccess(ID);
                                            
         
            return PartialView(result);
        }
        public ActionResult Create()
        {
            ViewBag.roles = _adminService.GetAllRoles()
                                           .Select(m => new { Value = m.Id, Text = m.Name })
                                           .ToList()
                                           .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            //var res = _menuService.DataClassificationTree().ToList();
            //return Json(res, JsonRequestBehavior.AllowGet);
            return View();
          
        }
        //public JsonResult ReturnTreeData(string ID)
        //{
        //    var res = _menuService.DataClassificationTree(ID).ToList();
        //    return Json(res, JsonRequestBehavior.AllowGet);
        //}
     
        [HttpPost]
        public JsonResult CreateComponentAccess(int MenuId, bool CheckboxStatus, string RoleId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _menuService.Edit(MenuId, CheckboxStatus, RoleId, userId);
                Session["userRole"] = RoleId;
                ViewBag.roleId = RoleId;
                var result = _menuService.getMenuAccess(RoleId);
               // return PartialView("_Index", result);
                // return RedirectToAction("_Index", new { RoleId });
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }
    }
}