using System;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Extensions;
using System.Linq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Helpers;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    [AccessRoles(Roles = "Super Admin,Administrator,QPS,VP Quality,Quality Director")]
    public class FacilityPhysicianController : BaseController
    {
        private FacilityPhysicianService _facilityPhysicianService;
        private LookupService _LookupService;
        public FacilityPhysicianController()
        {
            _facilityPhysicianService = new FacilityPhysicianService();
            _LookupService = new LookupService();
        }

        public ActionResult Index(Guid fac_key)
        {
            ViewBag.fac_key = fac_key;
            return PartialView();
        }
        public ActionResult _ListPhysicianFacilities(string phy_key)
        {
            ViewBag.phy_key = phy_key;
            return PartialView();
        }
        public ActionResult EditFacility(int id)
        {
            var facility_physician = _facilityPhysicianService.GetDetails(id);

            ViewBag.fap_fac_key = _facilityPhysicianService.GetAvailableFacilities(facility_physician.fap_user_key, id)
              .Select(m => new SelectListItem
              {
                  Text = m.fac_name.ToString(),
                  Value = m.fac_key.ToString(),
                  Selected = m.fac_key == facility_physician.fap_fac_key
              }).ToList();

            return PartialView("_AddFacility", facility_physician);

        }

        public ActionResult EditPhysician(int id)
        {
            ViewBag.Facilities = _LookupService.GetAllFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
            var facility_physician = _facilityPhysicianService.GetDetails(id);

            ViewBag.fap_phy_key = _facilityPhysicianService.GetAvailableAllPhysicians(facility_physician.fap_fac_key, id)
              .Select(m => new SelectListItem
              {
                  Text = m.FirstName + " " + m.LastName,
                  Value = m.Id,
                  Selected = m.Id == facility_physician.fap_user_key
              }).ToList();


            var facility_id = facility_physician.fap_fac_key;
            var ss = _LookupService.GetPhysiciansByFacility(facility_id);
            ViewBag.AllPhycision = _LookupService.GetPhysiciansByFacility(facility_id)
                                    .Select(m => new SelectListItem
                                    {
                                        Text = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
                                        Value = m.fap_key.ToString()
                                    }).OrderBy(p => p.Text).ToList();
            return PartialView("_AddPhysician", facility_physician);
        }

        public JsonResult GetPhysicianByFacility(List<Guid> Facility)
        {
            try
            {
                var phyList = _facilityPhysicianService.GetAvailablePhysiciansByFacility(Facility).Select(m => new SelectListItem
                {
                    Text = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
                    Value = m.fap_user_key,
                }).Distinct().OrderBy(p=>p.Text).ToList();

                return Json(phyList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult _AddPhysician(Guid fac_key)
        {
            ViewBag.fap_user_key = _facilityPhysicianService.GetAvailablePhysicians(fac_key)
             .Select(m => new SelectListItem
             {
                 Text = m.FirstName + " " + m.LastName,
                 Value = m.Id
             }).ToList();



            return PartialView(new facility_physician { fap_fac_key = fac_key, fap_is_active = true });
        }
        public ActionResult _AddFacility(string phy_key)
        {
            ViewBag.fap_fac_key = _facilityPhysicianService.GetAvailableFacilities(phy_key)
               .Select(m => new SelectListItem
               {
                   Text = m.fac_name.ToString(),
                   Value = m.fac_key.ToString()
               });

            return PartialView(new facility_physician { fap_user_key = phy_key, fap_is_active = true });
        }
        public ActionResult Remove(int id)
        {
            try
            {
                var model = _facilityPhysicianService.GetDetails(id);
                model.fap_modified_by = User.Identity.GetUserId();
                model.fap_modified_date = DateTime.Now.ToEST();
                model.fap_modified_by_name = loggedInUser.FullName;

                model.fap_is_active = false;
                model.fap_end_date = DateTime.Now.ToEST();
                _facilityPhysicianService.Edit(model);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _facilityPhysicianService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetAllPhysicianFacilities(DataSourceRequest request)
        {
            var res = _facilityPhysicianService.GetPhysicianFacilities(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPhysicians(Guid fac_key)
        {
            try
            {
                var list = _facilityPhysicianService.GetAvailablePhysicians(fac_key)
                                                    .Select(m => new { phy_name = m.FirstName + " " + m.LastName, phy_key = m.Id });
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetFacilities(string phy_key)
        {
            try
            {
                var list = _facilityPhysicianService.GetAvailableFacilities(phy_key)
                                                    .Select(m => new { m.fac_name, m.fac_key });
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<facility_physician> UpdatePhysicians(List<facility_physician> existingEntries, facility_physician model)
        {
            List<facility_physician> updatedPhysicians = new List<facility_physician>();
            var exist_value = model.fap_Array_path;
            if (exist_value == "[]")
            {
                facility_physician _facility_Physician = new facility_physician();
                var dbModel = _facilityPhysicianService.GetDetails(model.fap_key);
                if (dbModel.fap_is_on_boarded != model.fap_is_on_boarded)
                {
                    _facility_Physician.fap_onboarded_by = User.Identity.GetUserId();
                    _facility_Physician.fap_onboarded_date = DateTime.Now.ToEST();
                    _facility_Physician.fap_onboarded_by_name = loggedInUser.FullName;

                }
                _facility_Physician.fap_is_active = true;
                _facility_Physician.fap_modified_by = User.Identity.GetUserId();
                _facility_Physician.fap_modified_by_name = loggedInUser.FullName;
                _facility_Physician.fap_modified_date = DateTime.Now.ToEST();
                _facility_Physician.fap_Password_BY = User.Identity.GetUserId();
                _facility_Physician.fap_Password_Date = DateTime.Now.ToEST();
                _facility_Physician.fap_onboarded_by = User.Identity.GetUserId();
                _facility_Physician.fap_onboarded_date = DateTime.Now.ToEST();
                _facility_Physician.fap_onboarded_by_name = loggedInUser.FullName;
                _facility_Physician.fap_is_on_boarded = model.fap_is_on_boarded;
                _facility_Physician.fap_key = dbModel.fap_key;
                _facility_Physician.fap_UserName = dbModel.fap_UserName;
                _facility_Physician.fap_Password = dbModel.fap_Password;
                _facility_Physician.fap_created_by = dbModel.fap_created_by;
                _facility_Physician.fap_fac_key = dbModel.fap_fac_key;
                _facility_Physician.fap_user_key = dbModel.fap_user_key;
                _facility_Physician.fap_created_date = dbModel.fap_created_date;
                _facility_Physician.fap_Credentials_confirmed_date = dbModel.fap_Credentials_confirmed_date;
                _facility_Physician.fap_date_assigned = dbModel.fap_date_assigned;
                _facility_Physician.fap_end_date = dbModel.fap_end_date;
                _facility_Physician.fap_onboarding_complete_provider_active_date = dbModel.fap_onboarding_complete_provider_active_date;
                _facility_Physician.fap_preapp_receive_date = dbModel.fap_preapp_receive_date;
                _facility_Physician.fap_preapp_request_date = dbModel.fap_preapp_request_date;
                _facility_Physician.fap_preapp_submitted_date = dbModel.fap_preapp_submitted_date;
                _facility_Physician.fap_start_date = dbModel.fap_start_date;
                _facility_Physician.fap_vcaa_date = dbModel.fap_vcaa_date;
                _facility_Physician.fap_is_active = dbModel.fap_is_active;
                _facility_Physician.fap_is_hide_pending_onboarding = dbModel.fap_is_hide_pending_onboarding;
                _facility_Physician.fap_created_by = dbModel.fap_created_by;
                _facility_Physician.fap_created_by_name = dbModel.fap_created_by_name;
                _facility_Physician.fap_created_date = dbModel.fap_created_date;
                _facility_Physician.fap_Credentials_confirmed_date = dbModel.fap_Credentials_confirmed_date;
                _facility_Physician.fap_credential_specialist = dbModel.fap_credential_specialist;
                _facilityPhysicianService.Edit(_facility_Physician, false);
                _facilityPhysicianService.Save();
                _facilityPhysicianService.Commit();
            }
            else
            {
                var dbModelForPhysicin = _facilityPhysicianService.GetDetails(model.fap_key);
                facility_physician facility_Physicians = new facility_physician();
                if (dbModelForPhysicin.fap_is_on_boarded != model.fap_is_on_boarded)
                {

                    facility_Physicians.fap_onboarded_by = User.Identity.GetUserId();
                    facility_Physicians.fap_onboarded_date = DateTime.Now.ToEST();
                    facility_Physicians.fap_onboarded_by_name = loggedInUser.FullName;
                    facility_Physicians.fap_is_on_boarded = model.fap_is_on_boarded;
                    facility_Physicians.fap_modified_by = User.Identity.GetUserId();
                    facility_Physicians.fap_modified_by_name = loggedInUser.FullName;
                    facility_Physicians.fap_modified_date = DateTime.Now.ToEST();
                    facility_Physicians.fap_Password_BY = User.Identity.GetUserId();
                    facility_Physicians.fap_Password_Date = DateTime.Now.ToEST();
                    if (model.fap_is_override) { facility_Physicians.fap_override_start = model.fap_modified_date; }
                    else { facility_Physicians.fap_override_start = null; facility_Physicians.fap_override_hours = null; }
                    if (facility_Physicians != null)
                    {
                        facility_Physicians.fap_key = model.fap_key;
                        facility_Physicians.fap_UserName = dbModelForPhysicin.fap_UserName;
                        facility_Physicians.fap_Password = dbModelForPhysicin.fap_Password;
                        facility_Physicians.fap_created_by = dbModelForPhysicin.fap_created_by;
                        facility_Physicians.fap_fac_key = dbModelForPhysicin.fap_fac_key;
                        facility_Physicians.fap_user_key = dbModelForPhysicin.fap_user_key;
                        facility_Physicians.fap_created_date = dbModelForPhysicin.fap_created_date;
                        facility_Physicians.fap_Credentials_confirmed_date = dbModelForPhysicin.fap_Credentials_confirmed_date;
                        facility_Physicians.fap_date_assigned = dbModelForPhysicin.fap_date_assigned;
                        facility_Physicians.fap_end_date = dbModelForPhysicin.fap_end_date;
                        facility_Physicians.fap_onboarding_complete_provider_active_date = dbModelForPhysicin.fap_onboarding_complete_provider_active_date;
                        facility_Physicians.fap_preapp_receive_date = dbModelForPhysicin.fap_preapp_receive_date;
                        facility_Physicians.fap_preapp_request_date = dbModelForPhysicin.fap_preapp_request_date;
                        facility_Physicians.fap_preapp_submitted_date = dbModelForPhysicin.fap_preapp_submitted_date;
                        facility_Physicians.fap_start_date = dbModelForPhysicin.fap_start_date;
                        facility_Physicians.fap_vcaa_date = dbModelForPhysicin.fap_vcaa_date;
                        facility_Physicians.fap_is_active = dbModelForPhysicin.fap_is_active;
                        facility_Physicians.fap_is_hide_pending_onboarding = dbModelForPhysicin.fap_is_hide_pending_onboarding;
                        facility_Physicians.fap_created_by = dbModelForPhysicin.fap_created_by;
                        facility_Physicians.fap_created_by_name = dbModelForPhysicin.fap_created_by_name;
                        facility_Physicians.fap_created_date = dbModelForPhysicin.fap_created_date;
                        facility_Physicians.fap_Credentials_confirmed_date = dbModelForPhysicin.fap_Credentials_confirmed_date;
                        facility_Physicians.fap_credential_specialist = dbModelForPhysicin.fap_credential_specialist;
                        _facilityPhysicianService.Edit(facility_Physicians, false);
                        _facilityPhysicianService.Save();
                        _facilityPhysicianService.Commit();
                    }
                }
                foreach (var item in existingEntries)
                {
                    var dbModel = _facilityPhysicianService.GetDetailsByFacKey(item.fap_fac_key, item.fap_user_key);
                   // var _facility_Physician = _facilityPhysicianService.GetDetailsByFacKey(item.fap_fac_key, item.fap_user_key);
                    facility_physician _facility_Physician = new facility_physician();
                    _facility_Physician.fap_is_active = true;

                    _facility_Physician.fap_onboarded_by = User.Identity.GetUserId();
                    _facility_Physician.fap_onboarded_date = DateTime.Now.ToEST();
                    _facility_Physician.fap_onboarded_by_name = loggedInUser.FullName;

                    _facility_Physician.fap_modified_by = User.Identity.GetUserId();
                    _facility_Physician.fap_modified_by_name = loggedInUser.FullName;
                    _facility_Physician.fap_modified_date = DateTime.Now.ToEST();
                    _facility_Physician.fap_Password_BY = User.Identity.GetUserId();
                    _facility_Physician.fap_Password_Date = DateTime.Now.ToEST();
                    _facility_Physician.fap_is_on_boarded = dbModel.fap_is_on_boarded;

                    if (model.fap_is_override)
                    {
                        _facility_Physician.fap_override_start = model.fap_modified_date;
                    }
                    else
                    {
                        _facility_Physician.fap_override_start = null;
                        _facility_Physician.fap_override_hours = null;
                    }
                    if (_facility_Physician != null)
                    {
                        _facility_Physician.fap_key = dbModel.fap_key;
                        _facility_Physician.fap_UserName = item.fap_UserName;
                        _facility_Physician.fap_Password = item.fap_Password;
                        _facility_Physician.fap_created_by = dbModel.fap_created_by;
                        _facility_Physician.fap_fac_key = dbModel.fap_fac_key;
                        _facility_Physician.fap_user_key = dbModel.fap_user_key;
                        _facility_Physician.fap_created_date = dbModel.fap_created_date;
                        _facility_Physician.fap_Credentials_confirmed_date = dbModel.fap_Credentials_confirmed_date;
                        _facility_Physician.fap_date_assigned = dbModel.fap_date_assigned;
                        _facility_Physician.fap_end_date = dbModel.fap_end_date;
                        _facility_Physician.fap_onboarding_complete_provider_active_date = dbModel.fap_onboarding_complete_provider_active_date;
                        _facility_Physician.fap_preapp_receive_date = dbModel.fap_preapp_receive_date;
                        _facility_Physician.fap_preapp_request_date = dbModel.fap_preapp_request_date;
                        _facility_Physician.fap_preapp_submitted_date = dbModel.fap_preapp_submitted_date;
                        _facility_Physician.fap_start_date = dbModel.fap_start_date;
                        _facility_Physician.fap_vcaa_date = dbModel.fap_vcaa_date;
                        _facility_Physician.fap_is_active = dbModel.fap_is_active;
                        _facility_Physician.fap_is_hide_pending_onboarding = dbModel.fap_is_hide_pending_onboarding;
                        _facility_Physician.fap_created_by = dbModel.fap_created_by;
                        _facility_Physician.fap_created_by_name = dbModel.fap_created_by_name;
                        _facility_Physician.fap_created_date = dbModel.fap_created_date;
                        _facility_Physician.fap_Credentials_confirmed_date = dbModel.fap_Credentials_confirmed_date;
                        _facility_Physician.fap_credential_specialist = dbModel.fap_credential_specialist;
                        updatedPhysicians.Add(_facility_Physician);
                    }
                }
            }

           
            return updatedPhysicians;
        }

        public ActionResult SaveFacility(facility_physician model, bool FacilityView)
        {
          
            try
            {
                if (ModelState.IsValid)
                {
                    if (ModelState.IsValid)
                    {
                        if (_facilityPhysicianService.IsAlreadyExists(model))
                        {
                            if (FacilityView)
                                ModelState.AddModelError("", "Facility already exists");
                            else
                                ModelState.AddModelError("", "physician already exists");
                        }

                        if (model.fap_key > 0)
                        {
                            var dbModel = _facilityPhysicianService.GetDetails(model.fap_key);
                            var chk_value_path = model.fap_Array_path;
                            if(chk_value_path== null)
                            {
                                model.fap_Array_path = "[]";
                            }
                            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<facility_physician>>(model.fap_Array_path);
                            if (items.Equals(0))
                            {

                            }
                            else
                            {
                                _facilityPhysicianService.Save();
                                _facilityPhysicianService.Commit();
                                var ssss = UpdatePhysicians(items, model);
                                foreach (var item in ssss)
                                {

                                    _facilityPhysicianService.Edit(item, false);
                                    _facilityPhysicianService.Save();
                                    _facilityPhysicianService.Commit();
                                }

                                //_facilityPhysicianService.EditForMultiple(ssss);
                            }
                            #region handling change log
                            var changes = _facilityPhysicianService.GetChangeTrackset("fap_created_date");
                            if (changes.Count() > 0)
                            {
                                AddChangeLog(changes, model.fap_key.ToString(), EntityTypes.Credentials);
                            }
                            #endregion


                        }
                        else
                        {
                            model.fap_created_by = User.Identity.GetUserId();
                            model.fap_is_active = true;
                            model.fap_created_date = DateTime.Now.ToEST();

                            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<facility_physician>>(model.fap_UserName);
                            if (items.Equals(0))
                            {

                            }
                            else
                            {
                                var ssss = UpdatePhysicians(items,model);
                                _facilityPhysicianService.EditForMultiple(ssss);
                            }


                            if (model.fap_is_on_boarded)
                            {
                                model.fap_onboarded_by = User.Identity.GetUserId();
                                model.fap_onboarded_date = DateTime.Now.ToEST();
                                model.fap_onboarded_by_name = loggedInUser.FullName;
                            }

                            if (model.fap_is_override)
                            {
                                model.fap_override_start = model.fap_created_date;
                            }

                            _facilityPhysicianService.Create(model);
                        }
                        return Json(new { success = true });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
        }


        //public ActionResult Update(PhysicianViewModel model, string SchType)
        //{
        //    _facilityPhysicianService.updatePhysicianPassword(model, true);
        //    return View();
        //}
        [HttpPost]
        public JsonResult SaveMultiplePasswords(List<PhysicianViewModel> model)
        {
           _facilityPhysicianService.updatePhysicianPassword(model, true);
            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetPhyPendingOnboardindFacDate()
        {
            int result = _facilityPhysicianService.SetPhyPendingOnboardindFacDate();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ToggleHide(int id)
        { 
            try
            {
                var model = _facilityPhysicianService.GetDetails(id);
                model.fap_hide = !model.fap_hide;
                model.fap_modified_by = User.Identity.GetUserId();
                model.fap_modified_date = DateTime.Now.ToEST();
                model.fap_modified_by_name = loggedInUser.FullName;
                _facilityPhysicianService.Edit(model);
                return Json(new { changed = true }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { changed = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PhysicianPassword()
        {
            ViewBag.Facilities = _LookupService.GetAll(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
            ViewBag.Physicians = _LookupService.GetPhysicians()
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            return GetViewResult();
        }
        public ActionResult GetAllPhysicianPassword(DataSourceRequest request, List<Guid> Facilities, List<string> Physicians)
        {
            var result = _facilityPhysicianService.GetAllPhysicianPassword(request, Facilities, Physicians);
            return JsonMax(result, JsonRequestBehavior.AllowGet);
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
                    _facilityPhysicianService?.Dispose();
                    _LookupService?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}