using System;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Helpers;
using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using Microsoft.Azure.Storage.Auth;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage;
using System.Web.Configuration;
using System.Configuration;

namespace TeleSpecialists.Controllers
{
    public class HospitalProtocolsController : BaseController
    {
        private readonly LookupService _lookUpService;
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private readonly FacilityService _facilityService;
        private readonly HospitalprotocolServices _Protocols;
        private readonly FacilityPhysicianService _facilityPhysicianService;
        public HospitalProtocolsController()
        {
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _lookUpService = new LookupService();
            _Protocols = new HospitalprotocolServices();
            _facilityService = new FacilityService();
            _facilityPhysicianService = new FacilityPhysicianService();
        }
        // GET: HospitalProtocols
        public ActionResult Index(int? Id, string fap_key, string facilityname)
        {
            var fn = loggedInUser?.FirstName;
            var ln = loggedInUser?.LastName;
            var fname = fn + " " + ln;
            var user_id = loggedInUser?.PasswordHash;
            var lsit = _Protocols.GetAllProtocolData(fap_key);
            var value_key = Id ?? 0;
            var dbModel = _facilityPhysicianService.GetDetails(value_key);
            ViewBag.fnames = fname;
            ViewBag.user_ids = user_id;
            IEnumerable<Hospital_Protocols> model = lsit;
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
            }
            
            var result = RenderPartialViewToString("Index", model);
            return Json(new { success = true, data = result });
        }
        public ActionResult Create(string fac_key, string issave)
        {
            var facList = new List<SelectListItem>();
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();
            }
            else
            {
                facList = _lookUpService.GetAllFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.fac_keys = fac_key;
            ViewBag.issaves = issave;
            Hospital_Protocols model = new Hospital_Protocols();
            var protocolsdetails = _Protocols.GetDetailsForPOpUP(fac_key);
            ViewBag.protocolsdataforpopup = protocolsdetails;

            ViewBag.FAC_value = TempData["message"];
            var result = RenderPartialViewToString("Create", model);
            return Json(new { success = true, data = result });
            //return GetViewResult(protocols);
        }
        public ActionResult Edit(string fac_key)
        {
            Hospital_Protocols model = new Hospital_Protocols();
            ViewBag.fac_keys = fac_key;
            ViewBag.Facilities = _lookUpService.GetAllFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .Where(x => x.Value != fac_key)
                                                  .ToList();
            var result = RenderPartialViewToString("Edit", model);
            return Json(new { success = true, data = result });
        }
        [HttpPost]
        public ActionResult Upload(string ParameterName, string parameterinfo, string cas_fac_key_arrays, string cas_fac_Name_array, string Div_Image)
        {

            Hospital_Protocols Protocols = new Hospital_Protocols();
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
                            string BlobKey = WebConfigurationManager.AppSettings["BlobStroageKey1"];
                            var OrigionalImagepath = BlobKey + fileName;
                            StorageCredentials creden = new StorageCredentials(accountname, accesskey);
                            CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                            CloudBlobClient client = acc.CreateCloudBlobClient();
                            CloudBlobContainer cont = client.GetContainerReference("hospitalprotocol");
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
                    var sortnum = _Protocols.GetSortNumMaxByFacility(item);
                    var encode_value = Functions.DecodeFrom64(parameterinfo);
                    Protocols.ParameterName = ParameterName;
                    Protocols.ParameterName_Info = encode_value;
                    Protocols.ParameterName_Image = Imagepath;
                    Protocols.Facility_Name = _facilityService.GetFacilityName(item);
                    Protocols.Facility_Id = item;
                    Protocols.Parameter_Add_Date = DateTime.Now.ToEST();
                    Protocols.SortNum = sortnum == null ? 1 : sortnum + 1;
                    _Protocols.Create(Protocols);
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

            return ShowSuccessMessageOnly("Hospital Protocols SuccessFuly Added.", Protocols);

        }
        public ActionResult GetAllData(DataSourceRequest request, string id)
        {
            var result = _Protocols.GetAll(request, id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditProtocols(int id)
        {
            int? Id = id;
            if (Id == null)
            {
                new SelectListItem { };
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Hospital_Protocols model = _Protocols.GetDetails(id);
            ViewBag.Facilities = _lookUpService.GetAllFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
            ViewBag.Fac_Id = model.Facility_Id;
            if (model == null)
            {
                return HttpNotFound();
            }
            var result = RenderPartialViewToString("EditProtocols", model);
            return Json(result, JsonRequestBehavior.AllowGet);
            //return GetViewResult(Protocols);
        }

        [HttpPost]
        public ActionResult EditProtocols(string ParameterName, string parameterinfo, string FacilityIdss, string FacilityNames, string onid, string OldImagePath, string Div_Image, int SortNum)
        {
            try
            {

                Hospital_Protocols Protocols = new Hospital_Protocols();
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
                var protocols = _Protocols.GetDetails(Convert.ToInt32(onid));
                Protocols = protocols;
                var protocolsimges = protocols.ParameterName_Image;

                foreach (var img in Div_Image.Split(','))
                {
                    foreach (var item in protocolsimges.Split(','))
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
                            string BlobKey = WebConfigurationManager.AppSettings["BlobStroageKey1"];
                            var OrigionalImagepath = BlobKey + fileName;
                            StorageCredentials creden = new StorageCredentials(accountname, accesskey);
                            CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);
                            CloudBlobClient client = acc.CreateCloudBlobClient();
                            CloudBlobContainer cont = client.GetContainerReference("hospitalprotocol");
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

                var getRecord = _Protocols.GetDetailsForPOpUP(FacilityIdss);
                int? highestnum = getRecord.Max(x => x.SortNum);
                if (SortNum > highestnum)
                {
                    SortNum = Convert.ToInt32(highestnum);
                }
                else if (SortNum < 1)
                {
                    SortNum = 1;
                }
                int id = Convert.ToInt32(onid);
                int? oldsortnum = getRecord.Where(x => x.ID == id).Select(x => x.SortNum).FirstOrDefault();
                if (oldsortnum != SortNum)
                {
                    foreach (var item in getRecord)
                    {
                        if (item.ID != id)
                        {
                            var detail = _Protocols.GetDetails(item.ID);
                            if (oldsortnum > SortNum)
                            {
                                if (detail.SortNum >= SortNum && detail.SortNum <= oldsortnum)
                                {
                                    detail.SortNum = detail.SortNum + 1;
                                    _Protocols.Edit(detail);
                                }
                            }
                            else
                            {
                                if (detail.SortNum >= oldsortnum && detail.SortNum <= SortNum)
                                {
                                    detail.SortNum = detail.SortNum - 1;
                                    _Protocols.Edit(detail);
                                }
                            }

                        }
                    }
                }

                Protocols.ID = Convert.ToInt32(onid);
                Protocols.ParameterName = ParameterName;
                var encode_value = Functions.DecodeFrom64(parameterinfo);
                Protocols.ParameterName_Info = encode_value;
                Protocols.ParameterName_Image = savenewimagepath;
                Protocols.Facility_Name = FacilityNames;
                Protocols.Facility_Id = FacilityIdss;
                Protocols.Parameter_Add_Date = DateTime.Now.ToEST();
                Protocols.SortNum = SortNum;
                _Protocols.Edit(Protocols);
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


            return Json(new { success = true, message = "Hospital Protocols SuccessFuly Added." });
        }
        [HttpPost]
        public JsonResult CheckHeaderName(string ParameterName, string cas_fac_key_arrays)
        {
            bool isValid = true;
            var checkheader = _Protocols.ChKName(ParameterName, cas_fac_key_arrays);
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
        public ActionResult Remove(int id)
        {
            try
            {
                var model = _Protocols.GetDetailForProtocols(id);
                int? oldsortnum = model.SortNum;
                var getRecord = _Protocols.GetDetailsForPOpUP(model.Facility_Id).Where(x => x.SortNum > oldsortnum);
                
                foreach (var item in getRecord)
                {
                    if (item.ID != id)
                    {
                        var detail = _Protocols.GetDetails(item.ID);

                        if (item.SortNum > oldsortnum)
                        {
                            detail.SortNum = detail.SortNum - 1;
                            _Protocols.Edit(detail);
                        }
                    }
                }
                var result = _Protocols.Delete(model);
                if (!result)
                    ModelState.AddModelError("", "Record can not be deleted. Protocols are linked with it");

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
        public ActionResult AddNewfacility(string cas_fac_key_arrays, string cas_fac_Name_array, string current_fac_key)
        {
            try
            {
                foreach (var item2 in cas_fac_key_arrays.Split(','))
                {
                    var getDetails = _Protocols.GetAllProtocolData(item2);
                    if (getDetails != null)
                    {
                        var result = _Protocols.DeleteRange(getDetails);
                    }
                }
                Hospital_Protocols On_Protocols = new Hospital_Protocols();
                var lsit = _Protocols.GetAllProtocolData(current_fac_key);
                foreach (var item in lsit)
                {
                    foreach (var item2 in cas_fac_key_arrays.Split(','))
                    {
                        var sortnum = _Protocols.GetSortNumMaxByFacility(item2);
                        On_Protocols.ParameterName = item.ParameterName;
                        On_Protocols.ParameterName_Info = item.ParameterName_Info;
                        On_Protocols.ParameterName_Image = item.ParameterName_Image;
                        On_Protocols.Facility_Name = _facilityService.GetFacilityName(item2);
                        On_Protocols.Facility_Id = item2;
                        On_Protocols.Parameter_Add_Date = DateTime.Now.ToEST();
                        On_Protocols.SortNum = sortnum == null ? 1 : sortnum + 1;
                        _Protocols.Create(On_Protocols);
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
        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    _lookUpService?.Dispose();
                    _facilityService?.Dispose();
                    _facilityPhysicianService?.Dispose();
                    _ealertFacilitiesService?.Dispose();
                    _Protocols?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}