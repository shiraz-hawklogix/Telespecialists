using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;

namespace TeleSpecialists.Web.Controllers
{
    public class PACPhysicianController : BaseController
    {
        private readonly PostAcuteCareService _postacutecareService;
        private readonly LookupService _lookUpService;
        private readonly PACGeneratedTemplateService _pacgenerateTemplateService;
        private readonly UCLService _uclService;


        public PACPhysicianController()
        {
            _postacutecareService = new PostAcuteCareService();
            _lookUpService = new LookupService();
            _pacgenerateTemplateService = new PACGeneratedTemplateService();
            _uclService = new UCLService();

        }

        // GET: PACPhysician
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            ViewBag.physicians = _postacutecareService.getPACPhysicains();
            ViewBag.PacCaseType = _uclService.GetUclData(UclTypes.PacCaseType).ToList();
            
            var tempstatus = _uclService.GetUclData(UclTypes.PacStatus).ToList();
            var statuswithoucomplete=tempstatus.Where(x => x.ucd_title != "Complete").ToList();
            ViewBag.PacStatus = statuswithoucomplete;


            ViewBag.IsEdit = false;
            post_acute_care _post_acute_care = new post_acute_care();
            _post_acute_care.pac_cst_key = PacStatus.Open.ToInt();
            return View(_post_acute_care);
        }
        [HttpPost]
        public ActionResult CreateNavigator(post_acute_care model)
        {

            model.pac_cst_key = PacStatus.Open.ToInt();
            model.pac_is_active = true;
            model.pac_callback = Functions.ClearPhoneFormat(model.pac_callback);
            model.pac_created_by = loggedInUser.Id;
            model.pac_created_by_name = loggedInUser.FullName;
            model.pac_created_date = DateTime.Now.ToEST();
            model.pac_date_of_completion = DateTime.Now.ToEST();
            _postacutecareService.Create(model);
            return ShowSuccessMessageOnly("Case Created Successfully", model);
        }
        [HttpPost]
        public ActionResult Create(post_acute_care model)
        {
            model.pac_is_active = true;
            model.pac_callback = Functions.ClearPhoneFormat(model.pac_callback);
            model.pac_created_by = loggedInUser.Id;
            model.pac_created_by_name = loggedInUser.FullName;
            model.pac_created_date = DateTime.Now.ToEST();
            model.pac_date_of_completion = DateTime.Now.ToEST(); 
            if (model.pac_patient != null)
            {
                model.pac_billing_patient_name = model.pac_patient;
            }
            if (model.pac_dob != null)
            {
                model.pac_billing_dob = model.pac_dob;
            }
            if (model.pac_identification_type != null)
            {
                model.pac_billing_identification_type = model.pac_identification_type;
            }
            if (model.pac_identification_number != null)
            {
                model.pac_billing_identification_number = model.pac_identification_number;
            }

            if(model.pac_cst_key == null)
            {
                model.pac_cst_key = PacStatus.Open.ToInt();
            }

            _postacutecareService.Create(model);
            return GetSuccessResult(Url.Action("EditPAC", new { id = model.pac_key }));
        }


        public ActionResult EditPAC(int id, bool isReadOnly = false)
        {
            ViewBag.IsReadOnlyCase = isReadOnly;
            ViewBag.PacCaseType = _uclService.GetUclData(UclTypes.PacCaseType).ToList();
            var tempstatus = _uclService.GetUclData(UclTypes.PacStatus).ToList();
            var statuswithoucomplete = tempstatus.Where(x => x.ucd_title != "Complete").ToList();
            if (User.IsInRole(UserRoles.Navigator.ToDescription()))
            {
                ViewBag.PacStatus = statuswithoucomplete;
            }
            else
            {
                ViewBag.PacStatus = tempstatus;
            }

            var model = new post_acute_care();
            try
            {
                ViewBag.physicians = _postacutecareService.getPACPhysicains();
                ViewBag.IsEdit = true;
                model = _postacutecareService.GetDetails(id);
                ViewBag.ctpNAme = _uclService.GetDetails((int)model.pac_ctp_key).ucd_title;
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            #region Get Billing Codes
            var types = new List<int>()
                    {
                        UclTypes.SleepCodes.ToInt()
                    };
            var uclDataList = _lookUpService.GetUclData(types)
                              .Where(m => m.ucd_is_active)
                               .OrderBy(c => c.ucd_ucl_key).ThenBy(c => c.ucd_sort_order)
                              .ToList();
            ViewBag.UclData = uclDataList.Select(m => new { m.ucd_key, m.ucd_title, m.ucd_description, m.ucd_ucl_key, m.ucd_sort_order }).OrderBy(o => o.ucd_sort_order);
            #endregion


            return GetViewResult(model);
        }

        [HttpPost]
        public ActionResult EditPAC(post_acute_care model)
        {
            try
            {
                post_acute_care dbObject = _postacutecareService.GetDetails(model.pac_key);

                #region sync billing and general tab fields

                if (dbObject.pac_patient == null && model.pac_patient != null)
                {
                    model.pac_billing_patient_name = model.pac_patient;
                } 

                if (dbObject.pac_patient == null && model.pac_billing_patient_name != null)
                {
                    model.pac_patient = model.pac_billing_patient_name;
                }

                if (dbObject.pac_patient != null && model.pac_patient != dbObject.pac_patient)
                {
                    model.pac_billing_patient_name = model.pac_patient;
                }

                if (dbObject.pac_patient != null && model.pac_billing_patient_name != dbObject.pac_patient)
                {
                    model.pac_patient = model.pac_billing_patient_name;
                }
                //end patient name

                if (dbObject.pac_dob == null && model.pac_dob != null)
                {
                    model.pac_billing_dob = model.pac_dob;
                }

                if (dbObject.pac_dob == null && model.pac_billing_dob != null)
                {
                    model.pac_dob = model.pac_billing_dob;
                }

                if (dbObject.pac_dob != null && model.pac_dob != dbObject.pac_dob)
                {
                    model.pac_billing_dob = model.pac_dob;
                }

                if (dbObject.pac_dob != null && model.pac_billing_dob != dbObject.pac_dob)
                {
                    model.pac_dob = model.pac_billing_dob;
                }
                // end dob

                if (dbObject.pac_identification_type == null && model.pac_identification_type != null)
                {
                    model.pac_billing_identification_type = model.pac_identification_type;
                }

                if (dbObject.pac_identification_type == null && model.pac_billing_identification_type != null)
                {
                    model.pac_identification_type = model.pac_billing_identification_type;
                }

                if (dbObject.pac_identification_type != null && model.pac_identification_type != dbObject.pac_identification_type)
                {
                    model.pac_billing_identification_type = model.pac_identification_type;
                }

                if (dbObject.pac_identification_type != null && model.pac_billing_identification_type != dbObject.pac_identification_type)
                {
                    model.pac_identification_type = model.pac_billing_identification_type;
                }
                //end identification type


                if (dbObject.pac_identification_number == null && model.pac_identification_number != null)
                {
                    model.pac_billing_identification_number = model.pac_identification_number;
                }

                if (dbObject.pac_identification_number == null && model.pac_billing_identification_number != null)
                {
                    model.pac_identification_number = model.pac_billing_identification_number;
                }

                if (dbObject.pac_identification_number != null && model.pac_identification_number != dbObject.pac_identification_number)
                {
                    model.pac_billing_identification_number = model.pac_identification_number;
                }

                if (dbObject.pac_identification_number != null && model.pac_billing_identification_number != dbObject.pac_identification_number)
                {
                    model.pac_identification_number = model.pac_billing_identification_number;
                }
                //end  identification number
                #endregion
                model.pac_date_of_completion = DateTime.Now.ToEST();
                model.pac_is_active = true;
                
                var isRedirect = Request.Params["RedirectPage"] == "0" ? false : true;
                //if (model.pac_seen_by_telespecialist == 2) // NO
                //{
                //    model.pac_seen_type = "Video Consult";
                //}
                _postacutecareService.Edit(model);
                if (isRedirect)
                    return GetSuccessResult(Url.Action("Index", "Case")); /* commented due to #411 - settings.aps_cas_facility_popup_on_load */
                else
                {
                    return GetSuccessResult(Url.Action("EditPAC", new { id = model.pac_key }), "Case has been updated successfully");
                }
            }
            catch (Exception ex)
            {
                ViewBag.PostAcuteModel = _postacutecareService.GetDetails(model.pac_key);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
                return GetErrorResult(model);
            }
        }

        #region Template Methods
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GeneratePACTemplate(post_acute_care model)
        {
            try
            {
                var physicians = _postacutecareService.getPACPhysicains();
                var name = physicians.Where(x => x.Id == model.pac_phy_key).Select(c=>c.FirstName).FirstOrDefault();
                ViewBag.PhysicianName = name;
                var result = RenderPartialViewToString("Templates/Preview/_PACCaseTemplate", model);
                return Json(new { success = true, data = result, showEditor = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public ActionResult LoadPACTemplate(int pacKey)
        {
            try
            {
                var template = _pacgenerateTemplateService.GetDetails(pacKey);
                if (template != null)
                {
                    var result = template.pct_template_html;
                    return Json(new { success = true, data = result, showEditor = (template.pct_finalize_date.HasValue ? false : true) });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false });
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveTemplate(int pac_key, bool is_finalized, string ptemplateData)
        {
            try
            {
                #region Check authorized user to save case
                if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
                {
                    var pacModel = _postacutecareService.GetDetailsWithoutTimeConversion(pac_key);
                    if (pacModel.pac_phy_key != User.Identity.GetUserId())
                    {
                        return Json(new { success = false, data = "Access Denied! <br/> You are not authorized to save or finalize this PAC template. This PAC case is reassigned." });
                    }
                }
                #endregion
                var templateData = Functions.DecodeFrom64(ptemplateData);
                // var templateData = HttpUtility.HtmlDecode(encodeFromBase64);

                var template = _pacgenerateTemplateService.GetDetails(pac_key);
                if (template == null)
                {
                    var obj = new pac_case_template
                    {
                        pct_pac_key = pac_key,
                        pct_template_html = templateData,
                        pct_created_by = loggedInUser.Id,
                        pct_created_by_name = loggedInUser.FullName,
                        pct_created_date = DateTime.Now.ToEST()
                    };
                    if (is_finalized)
                        obj.pct_finalize_date = DateTime.Now.ToEST();

                    _pacgenerateTemplateService.Create(obj);
                }
                else
                {
                    template.pct_modified_by = loggedInUser.Id;
                    template.pct_modified_by_name = loggedInUser.FullName;
                    template.pct_modified_date = DateTime.Now.ToEST();
                    template.pct_template_html = templateData;
                    if (is_finalized)
                        template.pct_finalize_date = DateTime.Now.ToEST();

                    _pacgenerateTemplateService.Edit(template);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "An error occured while processing your request, please try later" });
            }
        }
        #endregion
        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _postacutecareService?.Dispose();
                    _lookUpService?.Dispose();
                    _pacgenerateTemplateService?.Dispose();
                    _uclService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion

    }
}