using System;
using System.Web.Mvc;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels.FacilityQuestionnaire;

namespace TeleSpecialists.Controllers
{
    public class FacilityQuestionnaireController : BaseController
    {
        private readonly FacilityQuestionnaireService _facilityQuestionnaireService;
        private readonly FacilityService _facilityService;
        private readonly UCLService _uclService;
        private readonly FacilityContractService _facilityContractService;
        public FacilityQuestionnaireController()
        {
            _facilityQuestionnaireService = new FacilityQuestionnaireService();
            _facilityService = new FacilityService();
            _uclService = new UCLService();
            _facilityContractService = new FacilityContractService();
        }
        public ActionResult Index()
        {
            return GetViewResult();
        }

        public ActionResult _PreLiveForm(Guid id)
        {
            var obj = _facilityQuestionnaireService.GetDetails(id);
            var contacts = _facilityQuestionnaireService.GetContacts(id);

         
            var vm = new PreLiveVM { questionnaireModel = obj, contactList = contacts };
            vm.FacilityKey = id;
            return PartialView(vm);
        }

        public ActionResult GenerateDoc(PreLiveVM model)
        {
            _facilityQuestionnaireService.SaveChanges(model, loggedInUser.Id, loggedInUser.FullName);

            var path = Server.MapPath("~/Documents");
            var fileName = Guid.NewGuid() + ".docx";
            path = path + "\\" +  fileName;
            model.contactList = _facilityQuestionnaireService.GetContacts(model.FacilityKey);

            model.facilityModel = _facilityService.GetDetails(model.FacilityKey);
            if (model.facilityModel.fac_stt_key.HasValue)
                model.facilityState = _uclService.GetDetails(model.facilityModel.fac_stt_key.Value);

            if (model.facilityModel.fac_ucd_key_system.HasValue)
                model.facilityHealthSystem = _uclService.GetDetails(model.facilityModel.fac_ucd_key_system.Value);

            if (model.facilityModel.fac_ucd_bed_size.HasValue)
                model.bedSizeUCL = _uclService.GetDetails(model.facilityModel.fac_ucd_bed_size.Value);

            model.faclityContract = _facilityContractService.GetDetails(model.FacilityKey);
            var html = RenderPartialViewToString("_PrevLivePreview", model);
            BLL.Helpers.Functions.GenerateDocument(html, path);
            return Json(new { fileName = fileName }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(PreLiveVM model)
        {
            _facilityQuestionnaireService.SaveChanges(model, loggedInUser.Id, loggedInUser.FullName);
            
            return ShowSuccessMessageOnly("Changes successfully updated", model);
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
                    _facilityQuestionnaireService?.Dispose();
                    _facilityService?.Dispose();
                    _uclService?.Dispose();
                    _facilityContractService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion

    }
}