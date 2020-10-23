using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;
using Microsoft.AspNet.Identity;
using TeleSpecialists.BLL.Extensions;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Helpers;

namespace TeleSpecialists.Controllers
{
    [Authorize] 
    public class FacilityContractController : BaseController
    {
        private readonly FacilityContractService _facilityContractService;
        private readonly UCLService _uCLService;
        
        public FacilityContractController()
        {
            _facilityContractService = new FacilityContractService();
            _uCLService = new UCLService();
        }

        public ActionResult Index(Guid fac_key)
        {
            ViewBag.fac_key = fac_key;
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetAll(DataSourceRequest request)
        {
            var res = _facilityContractService.GetAll(request);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add(Guid fac_key)
        {
            ViewBag.Title = "Add Contract";
            ViewBag.ServiceTypes = _uCLService.GetUclData(UclTypes.ServiceType)
                                                      .Select(m => new { Key = m.ucd_key, Value = m.ucd_title })
                                                       .ToDictionary(m => m.Key.ToString(), m => m.Value);
            ViewBag.CoverageTypes = _uCLService.GetUclData(UclTypes.CoverageType)
                                                        .Select(m => new { Key = m.ucd_key, Value = m.ucd_title })
                                                        .ToDictionary(m => m.Key.ToString(), m => m.Value);

            var facilityContract = new facility_contract { fct_is_active = true, fct_key = fac_key };
            var serviceType = _uCLService.GetDefault(UclTypes.ServiceType);
            var coverageType = _uCLService.GetDefault(UclTypes.CoverageType);
            if (coverageType != null)
                facilityContract.fct_cvr_key = coverageType.ucd_key;
            
            //This is for default selection of servicetype checkbox
            facilityContract.fct_selected_services = serviceType.ucd_key.ToString();

            return PartialView("_Form", facilityContract);
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
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.Title = "Edit Contract";
            facility_contract facility_contract = _facilityContractService.GetDetails(id.Value);
            ViewBag.ServiceTypes = _uCLService.GetUclData(UclTypes.ServiceType)
                                                      .Select(m => new { Key = m.ucd_key, Value = m.ucd_title })
                                                       .ToDictionary(m => m.Key.ToString(), m => m.Value);
            ViewBag.CoverageTypes = _uCLService.GetUclData(UclTypes.CoverageType)
                                                        .Select(m => new { Key = m.ucd_key, Value = m.ucd_title })
                                                        .ToDictionary(m => m.Key.ToString(), m => m.Value);
            if (facility_contract == null)
            {
                var contractType = _uCLService.GetDefault(UclTypes.ContractType);
                var serviceType = _uCLService.GetDefault(UclTypes.ServiceType);
                var coverageType = _uCLService.GetDefault(UclTypes.CoverageType);
                return PartialView(new facility_contract
                {
                    fct_is_active = true,
                    fct_key = id.Value,                   
                    fct_cvr_key = coverageType != null ? coverageType.ucd_key : 0,
                    fct_selected_services = string.Join(",", facility_contract.facility_contract_service.Select(m => m.fcs_srv_key))
                });
            }
            else
            {
                facility_contract.fct_selected_services = string.Join(",", facility_contract.facility_contract_service.Select(m => m.fcs_srv_key));
            }
            return PartialView("_Form", facility_contract);
        }           
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(facility_contract facility_contract)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (facility_contract.fct_start_date.HasValue && facility_contract.fct_end_date.HasValue)
                    {
                        if (facility_contract.fct_start_date > facility_contract.fct_end_date)
                            ModelState.AddModelError("fct_start_date", "Start Date can not be greater then End Date");
                    }
                    if (ModelState.IsValid) // re validating the model after custom validations
                    {
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
                        return Json(new { success = true });
                    }
                }
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) });
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "An error occurred while processing your request, please try later." });
            }
        }
        public ActionResult Remove(Guid id)
        {
            try
            {
                facility_contract contract = _facilityContractService.GetDetails(id);
                _facilityContractService.Delete(contract);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = string.Join("<br/>", this.GetModalErrors().Values) }, JsonRequestBehavior.AllowGet);
            }
        }


        
        public ActionResult CheckFacilityIsTeleneuro(Guid fct_key)
        {
            bool isFacilityTeleneuro = false;
            try
            {
                var contract = _facilityContractService.GetDetails(fct_key);
                if (contract != null)
                {
                    if (contract.fct_service_calc.Contains(ContractServiceTypes.TeleNeuro.ToString()))
                    {
                        isFacilityTeleneuro = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            return Json(new { success = isFacilityTeleneuro }, JsonRequestBehavior.AllowGet);
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
                    _facilityContractService?.Dispose();
                    _uCLService?.Dispose();

                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
