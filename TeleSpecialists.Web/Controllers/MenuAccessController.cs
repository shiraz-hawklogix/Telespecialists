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
            return GetViewResult();
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

        public ActionResult UserBasedMenuAccess()
        {
            ViewBag.roles = _adminService.GetAllRoles()
                                           .Select(m => new { Value = m.Id, Text = m.Name })
                                           .ToList()
                                           .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            ViewBag.vbusers = new SelectList("", "Value", "Text");
            return GetViewResult();
            
        }
        public JsonResult GetUsers(string RoleId)
        {
           var result = _menuService.GetRoleBasedUser(RoleId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _GetUserBasedAccess(string roleId, string userId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                roleId = Session["userRole"].ToString();
            }

            ViewBag.roleId = roleId;

            var result = _menuService.getMenuAccess(roleId);
            var useraccess = _menuService.getUserBasedMenu(roleId, userId);

            for (int i = 0; i < useraccess.Count; i++)
            {
                var bit = useraccess[i].user_isAllowed;
                var result2 = result.Where(x => x.com_key == useraccess[i].user_com_key).FirstOrDefault();
                result.Where(x => x.cac_key == result2.cac_key).FirstOrDefault().cac_isAllowed = bit;
            }
          
            return PartialView(result);
        }
        [HttpPost]
        public ActionResult CreateUserbasedAccess(int MenuId, bool CheckboxStatus, string RoleId, string userId)
        {
            try
            {
                var loggedInUserId = User.Identity.GetUserId();

                _menuService.updateUserAccess(MenuId, CheckboxStatus, RoleId, userId, loggedInUserId);
                Session["userRole"] = RoleId;
                ViewBag.roleId = RoleId;
                var result = _menuService.getMenuAccess(RoleId);
                return RedirectToAction("_GetUserBasedAccess", RoleId, userId);
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);

            }
        }
    }
}