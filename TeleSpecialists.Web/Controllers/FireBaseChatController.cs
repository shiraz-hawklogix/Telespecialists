using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.Controllers
{
    public class FireBaseChatController : BaseController
    {
        private readonly FireBaseUserMailService _fireBaseUserMailService;
        private FireBaseData _fireBaseData;
        private readonly AdminService _adminService;
        private readonly SchedulerService _schedulerService;

        public FireBaseChatController()
        {
            _fireBaseUserMailService = new FireBaseUserMailService();
            _fireBaseData = new FireBaseData();
            _adminService = new AdminService();
            _schedulerService = new SchedulerService();
        }
        #region firebase chat
        public ActionResult HawkLogixChat()
        {
            //GetUsersAndSave();
            #region Code for get user all users
            /*
            string userinfo = ""; string schoolinfo = ""; string roless = "";
            List<SP_Get_Application_Users_Detail_With_Search_OR_With_Out_Search_Result> UsersList = new List<SP_Get_Application_Users_Detail_With_Search_OR_With_Out_Search_Result>();
            UsersList = db.SP_Get_Application_Users_Detail_With_Search_OR_With_Out_Search(role, schoolid, userinfo, schoolinfo, roless).ToList();

            List<FireBaseData> list = new List<FireBaseData>();
            foreach (var item in UsersList)
            {
                FireBaseData obj = new FireBaseData();
                obj.user_id = item.ID;
                obj.SchoolId = schoolid;
                obj.name = item.UserFullName;
                bool isValid = IsValidEmail(item.Email);
                if (isValid)
                {
                    obj.email = item.Email;
                }
                else
                {
                    obj.email = item.Email + "@gmail.com";
                }
                obj.UserName = item.Email;
                obj.password = obj.email;
                obj.ImgPath = item.ProfilePicture;
                list.Add(obj);
            }
            List<FireBaseData> _List = FireBaseUsers(list, schoolid);

            List<string> objlist = new List<string>();


            var classList = db.Classes.Where(x => x.School_Id == schoolid).ToList();
            foreach (var item in classList)
            {
                objlist.Add(item.ClasssName);
            }
            ViewBag.classList = objlist;
            return View(list);
            */
            #endregion 
            var result = _fireBaseUserMailService.GetDetails(loggedInUser.Id);
            if (result != null)
            {
                _fireBaseData.user_id = loggedInUser.Id;
                _fireBaseData.teleid = 1;
                _fireBaseData.name = result.fre_firstname;
                _fireBaseData.ImgPath = result.fre_profileimg;
                _fireBaseData.email = result.fre_firebase_email;
                _fireBaseData.password = result.fre_firebase_email;
                _fireBaseData.UserName = result.fre_email;

                //var file = RenderImage("");
            }
            return GetViewResult(_fireBaseData);
        }

        public ActionResult Chat()
        {
            var result = _fireBaseUserMailService.GetDetails(loggedInUser.Id);
            if (result != null)
            {
                _fireBaseData.user_id = loggedInUser.Id;
                _fireBaseData.teleid = 1;
                _fireBaseData.name = result.fre_firstname;
                _fireBaseData.ImgPath = result.fre_profileimg;
                _fireBaseData.email = result.fre_firebase_email;
                _fireBaseData.password = result.fre_firebase_email;
                _fireBaseData.UserName = result.fre_email;

                //var file = RenderImage("");
            }
            return GetViewResult(_fireBaseData);
        }
        public ActionResult Chat2()
        {
            var result = _fireBaseUserMailService.GetDetails(loggedInUser.Id);
            if (result != null)
            {
                _fireBaseData.user_id = loggedInUser.Id;
                _fireBaseData.teleid = 1;
                _fireBaseData.name = result.fre_firstname;
                _fireBaseData.ImgPath = result.fre_profileimg;
                _fireBaseData.email = result.fre_firebase_email;
                _fireBaseData.password = result.fre_firebase_email;
                _fireBaseData.UserName = result.fre_email;

                //var file = RenderImage("");
            }
            return GetViewResult(_fireBaseData);
        }

        public bool RenderImage(string id)
        {
            try
            {
                id = "df79577f-3d6c-40f2-a752-85dc1ce21ca1";
                string base64String = _schedulerService.GetImage(id)?.Replace("data:image/jpeg;base64,", "");
                if (base64String != null)
                {
                    byte[] bytes = Convert.FromBase64String(base64String);
                    Image image;
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        image = Image.FromStream(ms);
                    }
                    string mergedPath = "A";

                    string filepath = Path.Combine(
                             HttpContext.Server.MapPath("~/Content/ChatFile"),
                             Path.GetFileName(mergedPath));

                    image.Save(mergedPath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //public ActionResult RenderImage(string id)
        //{
        //    id = "df79577f-3d6c-40f2-a752-85dc1ce21ca1";
        //    string base64String = _schedulerService.GetImage(id)?.Replace("data:image/jpeg;base64,", "");
        //    if (base64String != null)
        //        return File(Convert.FromBase64String(base64String), "image/jpeg");
        //    else
        //        return Json(null, JsonRequestBehavior.AllowGet);
        //}

        // Call to save all the user in firebase table

        private void GetUsersAndSave()
        {
            var users = _fireBaseUserMailService.GetAllUser();
            firebase_usersemail _firebase_Usersemail;
            foreach (var item in users)
            {
                _firebase_Usersemail = new firebase_usersemail();
                _firebase_Usersemail.fre_userId = item.user_id;
                //_firebase_Usersemail.sch = item.SchoolId;
                _firebase_Usersemail.fre_firstname = item.name;
                _firebase_Usersemail.fre_email = item.UserName;
                _firebase_Usersemail.fre_firebase_email = item.email;
                _firebase_Usersemail.fre_profileimg = item.ImgPath;
                _fireBaseUserMailService.Create(_firebase_Usersemail);
            }
        }

        public ActionResult Anon()
        {
            var users = _fireBaseUserMailService.GetAllUser();
            return GetViewResult(users);
        }

        [HttpPost]
        public JsonResult SaveAttachement(System.Web.HttpPostedFileBase Attachement, string senderId, string timeStamp)
        {
            if (Request.Files.Count > 0)
            {
                foreach (string file in Request.Files)
                {
                    var _file = Request.Files[file];

                    #region Save Files

                    string fileType = _file.ContentType;
                    string fileName = _file.FileName;

                    string mergedPath = senderId + "-" + timeStamp + "-" + fileName;

                    string filepath = Path.Combine(
                             HttpContext.Server.MapPath("~/Content/ChatFile"),
                             Path.GetFileName(mergedPath)
                         );

                    _file.SaveAs(filepath);


                    #endregion
                }
            }

            return Json(0);
        }

        public bool SavePic(FileContentResult img, string senderId, string timeStamp)
        {
            try
            {
                //byte[] bytes = 
                //Image image;
                //using (MemoryStream ms = new MemoryStream(img))
                //{
                //    image = Image.FromStream(ms);
                //}

                //string mergedPath = senderId + "-" + timeStamp + "-" + fileName;

                //string filepath = Path.Combine(
                //         HttpContext.Server.MapPath("~/Content/ChatFile"),
                //         Path.GetFileName(mergedPath)
                //     );

                //_file.SaveAs(filepath);
                return true;
            }
            catch
            {
                return false;
            }

        }
        [HttpPost]
        public JsonResult SaveId(string id)
        {
            try
            {
                var result = _fireBaseUserMailService.GetDetails(loggedInUser.Id);
                if (result != null)
                {
                    result.fre_firebase_uid = id;
                    _fireBaseUserMailService.Edit(result);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveCustomUserId(string firebaseId, string sqlid)
        {
            try
            {
                var result = _fireBaseUserMailService.GetDetails(sqlid);
                if (result != null)
                {
                    result.fre_firebase_uid = firebaseId;
                    _fireBaseUserMailService.Edit(result);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveIdAuto(string id, string userid)
        {
            try
            {
                var result = new firebase_usersemail();
                if (!string.IsNullOrEmpty(userid))
                {
                    result = _fireBaseUserMailService.GetDetails(userid);
                }
                else
                {
                    //result = _fireBaseUserMailService.GetDetails(loggedInUser.Id);
                }
                if (result != null)
                {
                    result.fre_firebase_uid = id;
                    _fireBaseUserMailService.Edit(result);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult GetUser(string id)
        {
            try
            {
                var result = _fireBaseUserMailService.GetDetails(id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult SignInRoleBase()
        {
            DataSourceResult request = new DataSourceResult();
            List<string> roleIDs = new List<string>();
            //roleIDs.Add("43299308-3a6c-4385-acb4-4c354fd5758d"); // navigators
            roleIDs.Add("0029737b-f013-4e0b-8a31-1b09524194f9"); // physicians
            //roleIDs.Add("684c8b74-216a-48bb-a9c1-c9cd4c1014fc"); // partner physicians
            var res = _adminService.GetAllUsersIds(roleIDs);
            var list = _fireBaseUserMailService.GetAllSpecificUserForAuto(res);
            ViewBag.navigators = list;
            return GetViewResult();
        }

        public ActionResult UserWithStatus()
        {
            return GetViewResult();
        }

        [HttpPost]
        public JsonResult GetUserId()
        {
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region User's in firebase database
        [HttpPost]
        public JsonResult SaveMuteDuration(string muteDuration, string firebaseuid)
        {
            try
            {
                var result = _fireBaseUserMailService.SaveUpdateMuteDuration(muteDuration, loggedInUser.Id, firebaseuid);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(e, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult GetMuteStatus()
        {
            bool status = false;
            try
            {
                status = _fireBaseUserMailService.CheckUserMuteStatus(loggedInUser.Id);
                return Json(status, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(status, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region Commented Code
        /*
       public ActionResult checkIdentity()
       {
           ApplicationDbContext context = new ApplicationDbContext();
           var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
           var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

           var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
           role.Name = "husnainHAwkLogix";
           roleManager.Create(role);

           return View();

       }

       // GET: FireBaseChat
       public ActionResult Index()
       {
           string filePath = Server.MapPath("/auth/hawkschool-2feda-users-export.json");
           using (StreamReader r = new StreamReader(filePath))
           {
               string json = r.ReadToEnd();
               //string[] arr = json.Split(',');
               JsonClass items = JsonConvert.DeserializeObject<JsonClass>(json);
               string abc = "";
           }
           return View();
       }

      



       bool IsValidEmail(string email)
       {
           try
           {
               var addr = new System.Net.Mail.MailAddress(email);
               return addr.Address == email;
           }
           catch
           {
               return false;
           }
       }

       public ActionResult CustomAuth()
       {
           return View();
       }

       public ActionResult Login()
       {
           return View();
       }

       public ActionResult Chat()
       {
           return View();
       }

       public ActionResult Success()
       {
           return View();
       }

       public ActionResult TestChat()
       {
           int userid = cc.GetCurrentUserID();
           int schoolid = cc.GetSchoolID(userid);
           var usrList = db.FireBaseUsersEmails.Where(x => x.SchoolId == schoolid && x.FireBaseUid != null && x.UserId != userid).ToList();
           return View(usrList);
       }

       public ActionResult MyChat()
       {
           int userid = cc.GetCurrentUserID();
           int schoolid = cc.GetSchoolID(userid);
           var usrList = db.FireBaseUsersEmails.Where(x => x.SchoolId == schoolid && x.FireBaseUid != null && x.UserId != userid).ToList();
           return View(usrList);
       }

       public ActionResult Fire_chat()
       {
           return View();
       }

       [HttpPost]
       public JsonResult GetUserName(string email, string user_id)
       {
           string name = "";
           var data = db.FireBaseUsersEmails.Where(x => x.FireBaseEmail == email).FirstOrDefault();
           if (data != null)
           {
               name = data.FirstName;
              bool status =  UpdateFireBaseId(data.Id, user_id);
           }
           return Json(name, JsonRequestBehavior.AllowGet);
       }

       public bool UpdateFireBaseId(int id ,string user_id)
       {
           try
           {
               var getRecord = db.FireBaseUsersEmails.Where(x => x.Id == id).FirstOrDefault();
               getRecord.FireBaseUid = user_id;               
               db.Entry(getRecord).State = System.Data.Entity.EntityState.Modified;
               db.SaveChanges();
               return true;
           }
           catch
           {
               return false;
           }

       }

       public ActionResult ChatWithFileAttachement()
       {
           int userid = cc.GetCurrentUserID();
           int schoolid = cc.GetSchoolID(userid);
           var role = db.sp_GetRoleAgainsUser(userid).FirstOrDefault();
           string userinfo = ""; string schoolinfo = ""; string roless = "";
           List<SP_Get_Application_Users_Detail_With_Search_OR_With_Out_Search_Result> UsersList = new List<SP_Get_Application_Users_Detail_With_Search_OR_With_Out_Search_Result>();
           UsersList = db.SP_Get_Application_Users_Detail_With_Search_OR_With_Out_Search(role, schoolid, userinfo, schoolinfo, roless).ToList();

           List<FireBaseData> list = new List<FireBaseData>();
           foreach (var item in UsersList)
           {
               FireBaseData obj = new FireBaseData();
               obj.user_id = item.ID;
               obj.SchoolId = schoolid;
               obj.name = item.UserFullName;
               bool isValid = IsValidEmail(item.Email);
               if (isValid)
               {
                   obj.email = item.Email;
               }
               else
               {
                   obj.email = item.Email + "@gmail.com";
               }
               obj.UserName = item.Email;
               obj.password = obj.email;
               obj.ImgPath = item.ProfilePicture;
               list.Add(obj);
           }
           List<FireBaseData> _List = FireBaseUsers(list, schoolid);
           return View(list);
       }

     
       [HttpPost]
       public ActionResult SaveAttachements(IEnumerable<HttpPostedFileBase> Attachement, string imgpath, string myName)
       {

           if (Request.Files.Count > 0)
           {
               foreach (string file in Request.Files)
               {
                   var _file = Request.Files[file];
               }
           }
           return Content("Success");
       }

       [HttpPost]
       public JsonResult UploadHomeReport(string id)
       {
           try
           {
               foreach (string file in Request.Files)
               {
                   var fileContent = Request.Files[file];
                   if (fileContent != null && fileContent.ContentLength > 0)
                   {
                       // get a stream
                       var stream = fileContent.InputStream;
                       // and optionally write the file to disk
                       var fileName = Path.GetFileName(file);
                       var path = Path.Combine(Server.MapPath("~/App_Data/Images"), fileName);
                       //using (var fileStream = File.Create(path))
                       //{
                       //    stream.CopyTo(fileStream);
                       //}
                   }
               }
           }
           catch (Exception)
           {
               Response.StatusCode = (int)HttpStatusCode.BadRequest;
               return Json("Upload failed");
           }

           return Json("File uploaded successfully");
       }

       [HttpPost]
       public JsonResult UpdateProfile(int id)
       {
           int userid = id;//cc.GetCurrentUserID();
           int schoolid = cc.GetSchoolID(userid);
           var role = db.sp_GetRoleAgainsUser(userid).FirstOrDefault();

           FireBaseData data = new FireBaseData();
           var result = db.FireBaseUsersEmails.Where(x => x.UserId == userid).FirstOrDefault();

           data.user_id = userid;
           data.name = result.FirstName;
           data.ImgPath = result.ProfileImg;
           data.email = result.FireBaseEmail;
           data.password = result.FireBaseEmail;
           data.SchoolId = (int)result.SchoolId;
           data.UserName = result.Email;


           return Json(data, JsonRequestBehavior.AllowGet);
       }

       public ActionResult CreateProfile()
       {
           return PartialView();
       }
   */
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
                    _fireBaseUserMailService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}