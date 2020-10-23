using System;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.Web.Models;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Data.Entity.Validation;
using Microsoft.Azure.Storage;
using Microsoft.Azure;
using System.Configuration;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace TeleSpecialists.Controllers
{

    #region Shiraz Code for role
    [Authorize]
    [AccessRoles(Roles = "Super Admin,Administrator,QPS,VP Quality, Quality Director, Physician, RRC Manager, RRC Director")]
    #endregion
    public class FacilityController : BaseController
    {
        private FacilityService _facilityService;
        private readonly FacilityContractService _facilityContractService;
        private UCLService _uCLService;
        private OnBoardedServices _OnBoardedServices;
        private FacilityPhysicianService _facilityPhysicianService;
        private LookupService _lookUpService;
        public FacilityController()
        {

            _lookUpService = new LookupService();
            _facilityService = new FacilityService();
            _facilityContractService = new FacilityContractService();
            _uCLService = new UCLService();
            _OnBoardedServices = new OnBoardedServices();
            _facilityPhysicianService = new FacilityPhysicianService();
        }

        #region Facility 
        public ActionResult Index()
        {
            return GetViewResult();
        }
        // GET: facility/Create
        public ActionResult Create()
        {
            var facility = new facility { fac_is_active = true, fac_timezone = BLL.Settings.DefaultTimeZone };
            if (facility.fac_cst_key == null)
            {
                var sc = _uCLService.GetDefault(UclTypes.EMR);
                facility.fac_cst_key = sc != null ? sc.ucd_key : 0;
            }

            if (facility.fac_fct_key == null)
            {
                var ft = _uCLService.GetDefault(UclTypes.FacilityType);
                facility.fac_fct_key = ft != null ? ft.ucd_key : 0;
            }

            if (facility.fac_sct_key == null)
            {
                var scert = _uCLService.GetDefault(UclTypes.StrokeDesignation);
                facility.fac_sct_key = scert != null ? scert.ucd_key : 0;
            }
            // by defualt select "yes" radio button for template used.
            facility.fac_not_templated_used = true;
            facility.facility_contract = GetFacilityContract();

            return GetViewResult(facility);
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _facilityService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        // POST: facility/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(facility facility)
        {


            if (ModelState.IsValid)
            {
                if (_facilityService.IsAlreadyExists(facility))
                    ModelState.AddModelError("fac_name", $"Facility {facility.fac_name} already exists");
                else
                {
                    facility.facility_contract.fct_created_by = User.Identity.GetUserId();
                    facility.facility_contract.fct_created_date = DateTime.Now.ToEST();
                    facility.fac_key = Guid.NewGuid();
                    facility.fac_created_by = User.Identity.GetUserId();
                    facility.fac_created_date = DateTime.Now.ToEST();
                    facility.fac_created_by_name = loggedInUser.FullName;
                    if (!string.IsNullOrEmpty(facility.facility_contract.fct_selected_services))
                    {
                        try
                        {
                            string[] _services = facility.facility_contract.fct_selected_services.Split(',');
                            bool isFind = _services.Contains("335");
                            if (isFind)
                                facility.fac_is_pac = true;
                            else
                                facility.fac_is_pac = false;

                        }
                        catch (Exception e)
                        {
                            facility.fac_is_pac = false;
                        }
                    }
                    _facilityService.Create(facility);

                    // Adding default selected facility contract
                    SaveFacilityContract(facility.facility_contract);

                    return GetSuccessResult(Url.Action("Edit", new { id = facility.fac_key, showPopupOnLoad = User.IsInRole(UserRoles.FacilityAdmin.ToDescription()) ? false : ApplicationSetting.aps_cas_facility_popup_on_load }), "Facility successfully added");
                    //return GetSuccessResult();
                }
            }

            return GetErrorResult(facility);
        }
        // GET: facility/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            facility facility = _facilityService.GetDetails(id.Value);
            if (facility == null)
            {
                return HttpNotFound();
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //    Selected = facility.qps_number == n
            //}).Prepend(new SelectListItem() { Text = "-- Select --", Value = "0", Selected = facility.qps_number == null });
            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = facility.qps_number;
            List<string> roles = new List<string>();

            var QPS = UserRoles.QPS.ToDescription();
            var QualityDirector = UserRoles.QualityDirector.ToDescription();
            var VPQuality = UserRoles.VPQuality.ToDescription();

            var QPSId = RoleManager.Roles.Where(x => x.Description == QPS).Select(x => x.Id).FirstOrDefault();
            var QualityDirectorId = RoleManager.Roles.Where(x => x.Description == QualityDirector).Select(x => x.Id).FirstOrDefault();
            var VPQualityId = RoleManager.Roles.Where(x => x.Description == VPQuality).Select(x => x.Id).FirstOrDefault();

            roles.Add(QPSId);
            roles.Add(QualityDirectorId);
            roles.Add(VPQualityId);
            ViewBag.Facilities = _lookUpService.GetAllFacility(null).Select(m => new { Value = m.fac_key, Text = m.fac_name }).ToList().Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            ViewBag.QPS_Numbers_List = _facilityService.GetUserByRole(roles, selected);

            facility.facility_contract = GetFacilityContract(facility.fac_key.ToString());
            return GetViewResult(facility);
        }
        // POST: CaseType/Edit/5               
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(facility facility)
        {
            if (ModelState.IsValid)
            {
                
                if (_facilityService.IsAlreadyExists(facility))
                    ModelState.AddModelError("fac_name", $"Type {facility.fac_name} already exists");
                else
                {
                    facility.fac_modified_by = User.Identity.GetUserId();
                    facility.fac_modified_date = DateTime.Now.ToEST();
                    facility.fac_modified_by_name = loggedInUser.FullName;
                    if (!string.IsNullOrEmpty(facility.facility_contract.fct_selected_services))
                    {
                        try
                        {
                            string[] _services = facility.facility_contract.fct_selected_services.Split(',');
                            bool isFind = _services.Contains("335");
                            if (isFind)
                                facility.fac_is_pac = true;
                            else
                                facility.fac_is_pac = false;

                        }
                        catch (Exception e)
                        {
                            facility.fac_is_pac = false;
                        }
                    }
                    _facilityService.Edit(facility);

                    SaveFacilityContract(facility.facility_contract);

                    return GetSuccessResult();
                }
            }
            return GetErrorResult(facility);
        }

        private facility_contract GetFacilityContract(string fac_key = "")
        {
            ViewBag.ServiceTypes = _uCLService.GetUclData(UclTypes.ServiceType)
                                                    .Select(m => new { Key = m.ucd_key, Value = m.ucd_title })
                                                     .ToDictionary(m => m.Key.ToString(), m => m.Value);
            ViewBag.CoverageTypes = _uCLService.GetUclData(UclTypes.CoverageType)
                                                        .Select(m => new { Key = m.ucd_key, Value = m.ucd_title })
                                                        .ToDictionary(m => m.Key.ToString(), m => m.Value);

            facility_contract facilityContract = null;

            if (!string.IsNullOrEmpty(fac_key))
            {
                facilityContract = _facilityContractService.GetDetails(new Guid(fac_key));
            }

            if (facilityContract != null)
            {

                facilityContract.fct_selected_services = string.Join(",", facilityContract.facility_contract_service.Select(m => m.fcs_srv_key));
            }
            else
            {
                facilityContract = new facility_contract { fct_is_active = true };

                var serviceType = _uCLService.GetDefault(UclTypes.ServiceType);
                var coverageType = _uCLService.GetDefault(UclTypes.CoverageType);
                if (coverageType != null)
                    facilityContract.fct_cvr_key = coverageType.ucd_key;

                //This is for default selection of servicetype checkbox
                facilityContract.fct_selected_services = serviceType.ucd_key.ToString();
            }


            return facilityContract;

        }

        private void SaveFacilityContract(facility_contract facility_contract)
        {
            try
            {

                if (facility_contract.fct_start_date.HasValue && facility_contract.fct_end_date.HasValue)
                {
                    if (facility_contract.fct_start_date > facility_contract.fct_end_date)
                        ModelState.AddModelError("fct_start_date", "Start Date can not be greater then End Date");
                }
                //if (ModelState.IsValid) // re validating the model after custom validations
                //{
                if (_facilityContractService.Exists(facility_contract.fct_key))
                {
                    facility_contract.fct_modified_by = User.Identity.GetUserId();
                    facility_contract.fct_modified_date = DateTime.Now.ToEST();
                    _facilityContractService.Edit(facility_contract);
                    SaveSelectedServices(facility_contract, true);
                }
                else
                {
                    facility_contract.fct_created_by = User.Identity.GetUserId();
                    facility_contract.fct_created_date = DateTime.Now.ToEST();
                    _facilityContractService.Create(facility_contract);
                    SaveSelectedServices(facility_contract, false);
                }
                //   return Json(new { success = true });

                //  }
                // return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                // return Json(new { success = false, data = "An error occurred while processing your request, please try later." });
            }
        }

        private void SaveSelectedServices(facility_contract facility_contract, bool updateExisting)
        {
            if (!string.IsNullOrEmpty(facility_contract.fct_selected_services))
            {
                if (updateExisting)
                {
                    #region handling Edit Scenario
                    var list = facility_contract.fct_selected_services.Split(',').Select(m => m.ToInt());
                    var existingList = _facilityContractService.GetFacilityContractServices(facility_contract.fct_key).ToList();
                    var existingListKeys = existingList.Select(m => m.fcs_srv_key);
                    var entitiesToRemove = existingList.Where(m => !list.Contains(m.fcs_srv_key)).ToList();
                    var entitiesToAdd = list.Where(m => !existingListKeys.Contains(m)).ToList();
                    _facilityContractService.RemoveServices(entitiesToRemove);
                    entitiesToAdd.ForEach(m =>
                    {
                        facility_contract.facility_contract_service.Add(new facility_contract_service
                        {
                            fcs_srv_key = m,
                            fcs_fct_key = facility_contract.fct_key,
                            fcs_created_by = loggedInUser.Id,
                            fcs_created_by_name = loggedInUser.FullName,
                            fcs_is_active = true,
                            fcs_created_date = DateTime.Now.ToEST()
                        });
                    });
                    #endregion 
                }
                else
                {
                    #region Add New Facility Contract with Selected Services
                    facility_contract.fct_selected_services.Split(',').Select(m => m.ToInt()).ToList().ForEach(m =>
                    {
                        facility_contract.facility_contract_service.Add(new facility_contract_service
                        {
                            fcs_srv_key = m,
                            fcs_fct_key = facility_contract.fct_key,
                            fcs_created_by = loggedInUser.Id,
                            fcs_created_by_name = loggedInUser.FullName,
                            fcs_is_active = true,
                            fcs_created_date = DateTime.Now.ToEST()
                        });
                    });
                    #endregion
                }
                _facilityContractService.SaveChanges();
            }
        }

        #endregion

        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls

        public object CloudStorageAccount { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _facilityService?.Dispose();
                    _facilityContractService?.Dispose();
                    _uCLService?.Dispose();
                    _OnBoardedServices?.Dispose();
                    _facilityPhysicianService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion



        #region Shiraz Code For OnBoarding
        
        public ActionResult CreateOnboarded(string fac_key, string issave)
        {
            ViewBag.fac_keys = fac_key;
            ViewBag.issaves = issave;
            Onboarded model = new Onboarded();
            ViewBag.Facilities = _lookUpService.GetAllFacility(null)
                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                  .ToList()
                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            var onboardingdetails = _OnBoardedServices.GetDetailsForPOpUP(fac_key);
            ViewBag.onboardingdatas = onboardingdetails;

            ViewBag.FAC_value = TempData["message"];
            var result = RenderPartialViewToString("CreateOnboarded", model);
            return Json(new { success = true, data = result });
            //return GetViewResult(_Onboarding);
        }
        public ActionResult EditButton(string fac_key)
        {
            Onboarded model = new Onboarded();
            ViewBag.fac_keys = fac_key;
            ViewBag.Facilities = _lookUpService.GetAllFacility(null)
                                 .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                 .ToList()
                                 .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            var result = RenderPartialViewToString("EditButton", model);
            return Json(new { success = true, data = result });
            //return GetViewResult();
        }
        public ActionResult GetAllData(DataSourceRequest request, string id)
        {
            var res = _OnBoardedServices.GetAll(request, id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditOnboarded(int id)
        {

            if (id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Onboarded model = _OnBoardedServices.GetDetails(id);
            ViewBag.Facilities = _lookUpService.GetAllFacility(null)
                      .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                      .ToList()
                      .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            ViewBag.Fac_Id = model.Facility_Id;

            if (model == null)
            {
                return HttpNotFound();
            }
            //return GetViewResult(onboarded);
            var result = RenderPartialViewToString("EditOnboarded", model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditOnboarded(string ParameterName, string parameterinfo, string FacilityIdss, string FacilityNames, string onid, string OldImagePath, string Div_Image, int SortNum)
        {
            try
            {

                Onboarded On_boardeds = new Onboarded();
                string savenewimagepath = "";
                string connString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                var connStringArray = connString.Split(';');
                var dictionary = new Dictionary<string, string>();
                foreach (var item in connStringArray)
                {
                    var itemKeyValue = item.Split('=');
                    dictionary.Add(itemKeyValue[0], itemKeyValue[1]);
                }
                string accountname = dictionary["AccountName"];
                string accesskeys = dictionary["AccountKey"];
                string accesskey = accesskeys + "==";
                var onboarded = _OnBoardedServices.GetDetails(Convert.ToInt32(onid));
                On_boardeds = onboarded;
                var onboardedimges = onboarded.ParameterName_Image;

                foreach (var img in Div_Image.Split(','))
                {
                    foreach (var item in onboardedimges.Split(','))
                    {
                        if (img == item)
                        {
                            var imgpathbydb = item;
                            savenewimagepath += imgpathbydb + ",";
                            break;
                        }
                    }
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file.FileName == img)
                        {
                            var fileName = DateTime.Now.ToString("yyyymmddMMssfff") + System.IO.Path.GetExtension(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/"), fileName);
                            var imagepth = path;
                            file.SaveAs(path);
                            string BlobKey = WebConfigurationManager.AppSettings["BlobStroageKey"];
                            var OrigionalImagepath = BlobKey + fileName;
                            StorageCredentials creden = new StorageCredentials(accountname, accesskey);
                            CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                            CloudBlobClient client = acc.CreateCloudBlobClient();
                            CloudBlobContainer cont = client.GetContainerReference("onboarding");
                            cont.CreateIfNotExists();
                            cont.SetPermissions(new BlobContainerPermissions
                            {
                                PublicAccess = BlobContainerPublicAccessType.Blob

                            });
                            CloudBlockBlob cblob = cont.GetBlockBlobReference(fileName);
                            var LocalImagePath = path.ToString();
                            using (Stream ddd = System.IO.File.OpenRead(@LocalImagePath))

                            {
                                cblob.UploadFromStream(ddd);
                            }
                            savenewimagepath += OrigionalImagepath + ",";
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                            //newImagesLink = savenewimagepath;
                            break;
                        }
                    }
                }
                var getRecord = _OnBoardedServices.GetDetailsForPOpUP(FacilityIdss);
                int? highestnum = getRecord.Max(x => x.SortNum);
                if (SortNum > highestnum)
                {
                    SortNum = Convert.ToInt32(highestnum);
                }
                else if (SortNum < 1)
                {
                    SortNum = 1;
                }
                int onboardid = Convert.ToInt32(onid);
                int? oldsortnum = getRecord.Where(x => x.Onboarded_ID == onboardid).Select(x => x.SortNum).FirstOrDefault();
                if (oldsortnum != SortNum)
                {
                    foreach (var item in getRecord)
                    {
                        if (item.Onboarded_ID != onboardid)
                        {
                            var detail = _OnBoardedServices.GetDetails(item.Onboarded_ID);
                            if (oldsortnum > SortNum)
                            {
                                if (detail.SortNum >= SortNum && detail.SortNum <= oldsortnum)
                                {
                                    detail.SortNum = detail.SortNum + 1;
                                    _OnBoardedServices.Edit(detail);
                                }
                            }
                            else
                            {
                                if (detail.SortNum >= oldsortnum && detail.SortNum <= SortNum)
                                {
                                    detail.SortNum = detail.SortNum - 1;
                                    _OnBoardedServices.Edit(detail);
                                }
                            }

                        }
                    }
                }

                On_boardeds.Onboarded_ID = Convert.ToInt32(onid);
                On_boardeds.ParameterName = ParameterName;
                var encode_value = Functions.DecodeFrom64(parameterinfo);
                On_boardeds.ParameterName_Info = encode_value;
                On_boardeds.ParameterName_Image = savenewimagepath;
                On_boardeds.Facility_Name = FacilityNames;
                On_boardeds.Facility_Id = FacilityIdss;
                On_boardeds.Parameter_Add_Date = DateTime.Now.ToEST();
                On_boardeds.SortNum = SortNum;
                _OnBoardedServices.Edit(On_boardeds);
                ViewBag.Fac_Id = FacilityIdss;
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


            return Json(new { success = true, message = "OnBoarded SuccessFuly Added." });
        }

        public ActionResult Onboarded(int? Id, string fap_key, string facilityname)
        {
            var fn = loggedInUser?.FirstName;
            var ln = loggedInUser?.LastName;
            var fname = fn + " " + ln;
            var user_id = loggedInUser?.PasswordHash;
            var lsit = _OnBoardedServices.GetAllBoardedData(fap_key);
            var value_key = Id ?? 0;
            var dbModel = _facilityPhysicianService.GetDetails(value_key);
            ViewBag.fnames = fname;
            ViewBag.user_ids = user_id;
            IEnumerable<Onboarded> model = lsit;
            if (fap_key == null)
            {
                var ss = dbModel.fap_fac_key.ToString();
                ViewBag.fap_keys = ss;

                var sss = _facilityService.GetDetails(dbModel.fap_fac_key);
                ViewBag.facilityname = sss.fac_name;
            }
            else
            {
                ViewBag.fap_keys = fap_key;

            }
            if (Id != null)
            {
                ViewBag.usernames = dbModel.fap_UserName;
                ViewBag.passwords = dbModel.fap_Password;
                ViewBag.fap_key = dbModel.fap_fac_key.ToString(); ;
                ViewBag.fap_key_id = Id;

                ViewBag.isboungingvalue = dbModel.fap_is_on_boarded;
                if (model.Count() == 0)
                {
                    string fac_key = dbModel.fap_fac_key.ToString();
                    model = _OnBoardedServices.GetAllBoardedData(fac_key);
                }

            }
            var result = RenderPartialViewToString("Onboarded", model);
            return Json(new { success = true, data = result });
        }
        [HttpPost]
        public ActionResult Edit_Facility_Physician(int? fap_key_values, bool chk_value)
        {

            try
            {

                facility_physician model = new facility_physician();
                var value_key = fap_key_values ?? 0;
                var dbModel = _facilityPhysicianService.GetDetails(value_key);
                if (dbModel.fap_is_on_boarded == chk_value)
                {
                    model.fap_key = value_key;
                    model.fap_is_on_boarded = false;
                    model.fap_onboarded_by = User.Identity.GetUserId();
                    model.fap_onboarded_date = DateTime.Now.ToEST();
                    model.fap_onboarded_by_name = loggedInUser.FullName;
                    model.fap_created_by = User.Identity.GetUserId();
                    model.fap_user_key = dbModel.fap_user_key;
                    model.fap_created_date = DateTime.Now.ToEST();
                    model.fap_fac_key = dbModel.fap_fac_key;
                    model.fap_start_date = dbModel.fap_start_date;
                    model.fap_end_date = dbModel.fap_end_date;
                    model.fap_UserName = dbModel.fap_UserName;
                    model.fap_Password = dbModel.fap_Password;

                    if (model.fap_is_override)
                    {
                        model.fap_override_start = model.fap_modified_date;
                    }
                    else
                    {
                        model.fap_override_start = null;
                        model.fap_override_hours = null;
                    }
                    return Json(false);
                }
                else
                {
                    model.fap_key = value_key;
                    model.fap_is_active = true;
                    model.fap_is_on_boarded = chk_value;
                    model.fap_modified_by = User.Identity.GetUserId();
                    model.fap_modified_by_name = loggedInUser.FullName;
                    model.fap_modified_date = DateTime.Now.ToEST();
                    model.fap_created_by = User.Identity.GetUserId();
                    model.fap_user_key = dbModel.fap_user_key;
                    model.fap_created_date = DateTime.Now.ToEST();
                    model.fap_start_date = dbModel.fap_start_date;
                    model.fap_end_date = dbModel.fap_end_date;
                    model.fap_fac_key = dbModel.fap_fac_key;
                    model.fap_UserName = dbModel.fap_UserName;
                    model.fap_Password = dbModel.fap_Password;
                    model.fap_onboarded_by = User.Identity.GetUserId();
                    model.fap_onboarded_date = DateTime.Now.ToEST();
                    model.fap_onboarded_by_name = loggedInUser.FullName;
                    if (model.fap_is_override)
                    {
                        model.fap_override_start = model.fap_modified_date;
                    }
                    else
                    {
                        model.fap_override_start = null;
                        model.fap_override_hours = null;
                    }
                }


                _facilityPhysicianService.Edit(model, false);
                _facilityPhysicianService.Save();
                _facilityPhysicianService.Commit();

            }
            catch (Exception ex)
            {

                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ModelState.AddModelError("", "Error! Please try again.");
            }
            return Json(new { success = true });
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Upload(string ParameterName, string parameterinfo, string cas_fac_key_arrays, string cas_fac_Name_array, string Div_Image)
        {

            Onboarded On_boardeds = new Onboarded();
            try
            {
                string connString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
                var connStringArray = connString.Split(';');
                var dictionary = new Dictionary<string, string>();
                foreach (var item in connStringArray)
                {
                    var itemKeyValue = item.Split('=');
                    dictionary.Add(itemKeyValue[0], itemKeyValue[1]);
                }
                string accountname = dictionary["AccountName"];
                string accesskeys = dictionary["AccountKey"];
                string accesskey = accesskeys + "==";
                string Imagepath = "";

                foreach (var items in Div_Image.Split(','))
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file.FileName.Equals(items))
                        {

                            var fileName = DateTime.Now.ToString("yyyymmddMMssfff") + System.IO.Path.GetExtension(file.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/"), fileName);
                            var imagepth = path;
                            file.SaveAs(path);
                            string BlobKey = WebConfigurationManager.AppSettings["BlobStroageKey"];
                            var OrigionalImagepath = BlobKey + fileName;
                            StorageCredentials creden = new StorageCredentials(accountname, accesskey);
                            CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                            CloudBlobClient client = acc.CreateCloudBlobClient();
                            CloudBlobContainer cont = client.GetContainerReference("onboarding");
                            cont.CreateIfNotExists();
                            cont.SetPermissions(new BlobContainerPermissions
                            {
                                PublicAccess = BlobContainerPublicAccessType.Blob

                            });
                            CloudBlockBlob cblob = cont.GetBlockBlobReference(fileName);
                            var LocalImagePath = path.ToString();
                            using (Stream ddd = System.IO.File.OpenRead(@LocalImagePath))

                            {
                                cblob.UploadFromStream(ddd);
                            }
                            Imagepath += OrigionalImagepath + ",";
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path);
                            }
                        }
                    }

                }
                foreach (var item in cas_fac_key_arrays.Split(','))
                {
                    var sortnum = _OnBoardedServices.GetSortNumMaxByFacility(item);
                    var encode_value = Functions.DecodeFrom64(parameterinfo);
                    On_boardeds.ParameterName = ParameterName;
                    On_boardeds.ParameterName_Info = encode_value;
                    On_boardeds.ParameterName_Image = Imagepath;
                    On_boardeds.Facility_Name = _facilityService.GetFacilityName(item);
                    On_boardeds.Facility_Id = item;
                    On_boardeds.Parameter_Add_Date = DateTime.Now.ToEST();
                    On_boardeds.SortNum = sortnum == null ? 1 : sortnum + 1;
                    _OnBoardedServices.Create(On_boardeds);
                }
                TempData["message"] = cas_fac_key_arrays;
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

            return ShowSuccessMessageOnly("OnBoarding SuccessFuly Added.", On_boardeds);

        }
        [HttpPost]
        public ActionResult AddNewfacility(string cas_fac_key_arrays, string cas_fac_Name_array, string current_fac_key)
        {
            try
            {
                foreach (var item2 in cas_fac_key_arrays.Split(','))
                {
                    var getDetails = _OnBoardedServices.GetAllBoardedData(item2);
                    if (getDetails != null)
                    {
                        var result = _OnBoardedServices.DeleteRange(getDetails);
                    }
                }
                Onboarded On_boarded = new Onboarded();
                var lsit = _OnBoardedServices.GetAllBoardedData(current_fac_key);
                foreach (var item in lsit)
                {
                    foreach (var item2 in cas_fac_key_arrays.Split(','))
                    {

                        var sortnum = _OnBoardedServices.GetSortNumMaxByFacility(item2);
                        On_boarded.ParameterName = item.ParameterName;
                        On_boarded.ParameterName_Info = item.ParameterName_Info;
                        On_boarded.ParameterName_Image = item.ParameterName_Image;
                        On_boarded.Facility_Name = _facilityService.GetFacilityName(item2);
                        On_boarded.Facility_Id = item2;
                        On_boarded.Parameter_Add_Date = DateTime.Now.ToEST();
                        On_boarded.SortNum = sortnum == null ? 1 : sortnum + 1;
                        _OnBoardedServices.Create(On_boarded);
                    }
                }
                return Json(true, JsonRequestBehavior.AllowGet);
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
            //return GetViewResult();
        }

        public ActionResult Remove(int id)
        {
            try
            {
                var model = _OnBoardedServices.GetDetailForOnboarded(id);
                int? oldsortnum = model.SortNum;
                var getRecord = _OnBoardedServices.GetDetailsForPOpUP(model.Facility_Id).Where(x => x.SortNum > oldsortnum);

                foreach (var item in getRecord)
                {
                    if (item.Onboarded_ID != id)
                    {
                        var detail = _OnBoardedServices.GetDetails(item.Onboarded_ID);

                        if (item.SortNum > oldsortnum)
                        {
                            detail.SortNum = detail.SortNum - 1;
                            _OnBoardedServices.Edit(detail);
                        }
                    }
                }
                var result = _OnBoardedServices.Delete(model);
                if (!result)
                    ModelState.AddModelError("", "Record can not be deleted. OnBoarding are linked with it");

                if (ModelState.IsValid)
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult CheckHeaderName(string ParameterName, string cas_fac_key_arrays)
        {
            bool isValid = true;
            var checkheader = _facilityService.ChKName(ParameterName, cas_fac_key_arrays);
            if (checkheader == 0)
            {
                isValid = true;
            }
            else
            {
                isValid = false;
            }
            return Json(isValid);
        }

        public ActionResult CopyOnboardTab(int onboardid, List<string> facilities)
        {
            try
            {
                var model = _OnBoardedServices.GetDetailForOnboarded(onboardid);
                
                foreach (var item in facilities)
                {
                    Onboarded On_boarded = new Onboarded();
                    var sortnum = _OnBoardedServices.GetSortNumMaxByFacility(item);
                    On_boarded.ParameterName = model.ParameterName;
                    On_boarded.ParameterName_Info = model.ParameterName_Info;
                    On_boarded.ParameterName_Image = model.ParameterName_Image;
                    On_boarded.Facility_Name = _facilityService.GetFacilityName(item);
                    On_boarded.Facility_Id = item;
                    On_boarded.Parameter_Add_Date = DateTime.Now.ToEST();
                    On_boarded.SortNum = sortnum == null ? 1 : sortnum + 1;
                    _OnBoardedServices.Create(On_boarded);
                }
                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        #endregion


    }




}
