using System.Linq;
using System.Web.Mvc;
using TeleSpecialists.BLL.Service;
using System;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;


namespace TeleSpecialists.Controllers
{
    [Authorize]
    public class LookupController : BaseController
    {
        private readonly LookupService _lookupService;
        private readonly FacilityPhysicianService _physicianService;
        private readonly PhysicianStatusService _physicianStatusService;
        private readonly UCLService _uCLService;
        private readonly CasCancelledTypeService _casCancelledTypeService;
       


        public LookupController()
        {
            _lookupService = new LookupService();
            _physicianService = new FacilityPhysicianService();
            _physicianStatusService = new PhysicianStatusService();
            _uCLService = new UCLService();
            _casCancelledTypeService = new CasCancelledTypeService();
        }

        [OutputCache(Duration = 300, VaryByParam = "type")]
        public JsonResult GetAll(UclTypes type)
        {
            try
            {
                var list = _uCLService.GetUclData(type)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description });

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllSleep()
        {
            try
            {
                 
                var list = _uCLService.GetUclDataSleep()
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description });

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCaseStatus(bool allowCompleteOption)
        {
            try
            {
                if (!allowCompleteOption)
                {
                    var completeId = CaseStatus.Complete.ToInt();
                    var list = _uCLService.GetUclData(UclTypes.CaseStatus)
                                       .Where(m => m.ucd_is_active && m.ucd_key != completeId)
                                       .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description });
                    var test = list.ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list = _uCLService.GetUclData(UclTypes.CaseStatus)
                                        .Where(m => m.ucd_is_active)
                                        .Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description });

                    return Json(list, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetAllPhysicians()
        {
            try
            {
                var list = _physicianService.GetAllPhysicians()
                                         .Select(m => new { Id = m.Id, Name = m.FirstName + " " + m.LastName })
                                         .ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetPhysicians(Guid? fac_key, int? fac_Type_key = 0)
        {
            try
            {
                var list = _physicianService.GetPhysiciansByFacility(fac_key, false, fac_Type_key)
                                         .Select(m => new { Id = m.Id, Name = m.FirstName + " " + m.LastName })
                                         .OrderBy(x => x.Name)
                                         .ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetFacility(string phoneNumber)
        {
            try
            {

                if (User.IsInRole(UserRoles.Physician.ToDescription()))
                {
                    var phyFacList = _physicianService.GetPhsicianFacilities(loggedInUser.Id, phoneNumber).Where(f => f.fac_go_live)
                                     .Select(m =>
                                                         new
                                                         {
                                                             fac_key = m.fac_key,
                                                             fac_name = m.fac_name
                                                         });
                    return Json(phyFacList, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    var list = _lookupService.GetAllLiveFacility(phoneNumber)
                                             .Select(m =>
                                                        new
                                                        {
                                                            fac_key = m.fac_key,
                                                            // Will be used in future
                                                            //fac_name = !string.IsNullOrEmpty(m.fac_city) && !string.IsNullOrEmpty(m.state.stt_code) ? m.fac_name+" ("+m.fac_city+", "+m.state.stt_code+")" : m.fac_name
                                                            fac_name = m.fac_name
                                                        });
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetAllMockFacilities()
        {
            var phyFacList = _physicianService.GetAllFacilities().Where(f => f.fac_is_active && f.fac_go_live == false)
                                 .Select(m =>
                                                     new
                                                     {
                                                         fac_key = m.fac_key,
                                                         fac_name = m.fac_name
                                                     }).ToList();
            return Json(phyFacList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllMockPhyscians()
        {
            var phyList = _physicianService.GetAllPhysicians().Where(x => x.Id == "46cb0702-4fd9-433b-bdb3-1729047a41c5")
                                 .Select(m =>
                                                     new
                                                     {
                                                         phy_key = m.Id,
                                                         phy_name = m.FirstName + " " + m.LastName
                                                     }).ToList();
            return Json(phyList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStrokeFacilities(string phoneNumber)
        {
            try
            {

                if (User.IsInRole(UserRoles.Physician.ToDescription()))
                {
                    var phyFacList = _physicianService.GetPhsicianFacilities(loggedInUser.Id, phoneNumber).Where(f => f.fac_go_live)
                                     .Select(m =>
                                                         new
                                                         {
                                                             fac_key = m.fac_key,
                                                             fac_name = m.fac_name
                                                         });
                    return Json(phyFacList, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    var list = _lookupService.GetAllLiveTeleStrokeFacility(phoneNumber)
                                             .Select(m =>
                                                        new
                                                        {
                                                            fac_key = m.fac_key,
                                                            // Will be used in future
                                                            //fac_name = !string.IsNullOrEmpty(m.fac_city) && !string.IsNullOrEmpty(m.state.stt_code) ? m.fac_name+" ("+m.fac_city+", "+m.state.stt_code+")" : m.fac_name
                                                            fac_name = m.fac_name
                                                        });
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetStrokeFacilitiesForOthercasetypes(string phoneNumber)
        {
            try
            {

                if (User.IsInRole(UserRoles.Physician.ToDescription()))
                {
                    var phyFacList = _physicianService.GetPhsicianFacilities(loggedInUser.Id, phoneNumber).Where(f => f.fac_go_live)
                                     .Select(m =>
                                                         new
                                                         {
                                                             fac_key = m.fac_key,
                                                             fac_name = m.fac_name
                                                         });
                    return Json(phyFacList, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    var list = _lookupService.GetStrokeFacilitiesForOthercasetypes(phoneNumber)
                                             .Select(m =>
                                                        new
                                                        {
                                                            fac_key = m.fac_key,
                                                            // Will be used in future
                                                            //fac_name = !string.IsNullOrEmpty(m.fac_city) && !string.IsNullOrEmpty(m.state.stt_code) ? m.fac_name+" ("+m.fac_city+", "+m.state.stt_code+")" : m.fac_name
                                                            fac_name = m.fac_name
                                                        });
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetTeleNeuroFacility(string phoneNumber)
        {
            try
            {

                if (User.IsInRole(UserRoles.Physician.ToDescription()))
                {
                    var phyFacList = _physicianService.GetPhsicianFacilities(loggedInUser.Id, phoneNumber).Where(f => f.fac_go_live
                    && f.facility_contract.fct_service_calc.Contains(ContractServiceTypes.TeleNeuro.ToString()))
                                     .Select(m =>
                                                         new
                                                         {
                                                             fac_key = m.fac_key,
                                                             fac_name = m.fac_name
                                                         });
                    return Json(phyFacList, JsonRequestBehavior.AllowGet);

                }
                else
                {

                    var list = _lookupService.GetAllLiveTeleNeuroFacility(phoneNumber)
                                             .Select(m =>
                                                        new
                                                        {
                                                            fac_key = m.fac_key,
                                                            // Will be used in future
                                                            //fac_name = !string.IsNullOrEmpty(m.fac_city) && !string.IsNullOrEmpty(m.state.stt_code) ? m.fac_name+" ("+m.fac_city+", "+m.state.stt_code+")" : m.fac_name
                                                            fac_name = m.fac_name
                                                        });
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetPACFacility(string phoneNumber)
        {
            try
            {

                var list = _lookupService.GetAllSleepFacility(phoneNumber) // GetAllPACFacility(phoneNumber)  // old Funcation when column in the table
                                             .Select(m =>
                                                        new
                                                        {
                                                            fac_key = m.fac_key,
                                                            fac_name = m.fac_name
                                                        });
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetSystemFacility(int Id)
        {
            try
            {

                var list = _lookupService.GetAllFacility("")
                                             .Where(m=> m.fac_ucd_key_system == Id)
                                             .Select(m =>
                                                        new
                                                        {
                                                            fac_key = m.fac_key,
                                                            fac_name = m.fac_name
                                                        });
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetAllPhysicianStatuses(int keyToIgnore = 0)
        {
            try
            {
                var list = _physicianStatusService.GetAll()
                                                  .Where(m => m.phs_key != keyToIgnore)
                                         .Select(m => new
                                         {
                                             m.phs_key,
                                             m.phs_name
                                         });
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetAllTimeZones()
        {
            try
            {
                var list = TimeZoneInfo.GetSystemTimeZones()
                                         .Select(m => new
                                         {
                                             tmz_key = m.Id,
                                             tmz_name = m.DisplayName.Replace("'", "")
                                         });
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetUsers()
        {
            var users = UserManager.Users.OrderBy(r => r.UserName).ToList()
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.FullName,
                                       Value = x.Id
                                   });

            return Json(users, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRoles()
        {
            try
            {
                var roles = RoleManager.Roles.OrderBy(r => r.Name)
                                   .Select(x => new SelectListItem
                                   {
                                       Text = x.Name,
                                       Value = x.Id
                                   }).ToList();
                return Json(new { success = true, data = roles, error = "" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetCancellCasTypes()
        {
            try
            {
                var list = _casCancelledTypeService.GetAllRecords()
                                         .Select(m => new { Id = m.cct_key, Name = m.cct_name })
                                         .ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
                    this._lookupService?.Dispose();
                    this._physicianService?.Dispose();
                    this._physicianStatusService?.Dispose();
                    this._uCLService?.Dispose();
                    this._casCancelledTypeService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
