using Kendo.DynamicLinq;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.ViewModels.Reports;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.Controllers
{
    [Authorize]
    public class ReportsController : BaseController
    {
        private readonly ReportService _reportService;
        private readonly LookupService _lookUpService;
        private readonly CaseAssignHistoryService _caseAssignHistoryService;
        private readonly EAlertFacilitiesService _ealertFacilitiesService;
        private readonly UCLService _uCLService;
        private readonly RateService _rateService;
        private readonly RateService_new _rateServiceNew;
        private readonly FacilityBillingReportService _facilityBillingReportService;
        private readonly CancelledCasesService _cancelledCasesService;
        private readonly CasCancelledTypeService _casCancelledTypeService;
        private readonly FacilityService _facilityService;
        public ReportsController() : base()
        {
            _rateServiceNew = new RateService_new();
            _reportService = new ReportService();
            _lookUpService = new LookupService();
            _caseAssignHistoryService = new CaseAssignHistoryService();
            _uCLService = new UCLService();
            _ealertFacilitiesService = new EAlertFacilitiesService();
            _rateService = new RateService();
            _facilityBillingReportService = new FacilityBillingReportService();
            _cancelledCasesService = new CancelledCasesService();
            _casCancelledTypeService = new CasCancelledTypeService();
            _facilityService = new FacilityService();
        }

        public ActionResult Index()
        {
            bool allowListing = true;
            if (User.IsInRole(UserRoles.FacilityAdmin.ToDescription()))
            {
                var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                                        .Select(x => x.Facility).ToList();
                if (facilities.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }
            //if (User.IsInRole(UserRoles.QPS.ToDescription()))
            //{
            //    var facilities = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //                            .Select(x => x.Facility).ToList();
            //    if (facilities.Count < 1)
            //    {
            //        allowListing = false;
            //        ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
            //    }
            //}
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult PhysicianCredentials()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility(null)
                                             .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return GetViewResult();
        }
        public ActionResult FacilityCredentials()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility(null)
                                             .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return GetViewResult();
        }
        public ActionResult PhysicianLicensing()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.States = _lookUpService.GetUclData(UclTypes.State)
                                            .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                            .ToList()
                                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            return GetViewResult();
        }
        public ActionResult StateLicensing()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.States = _lookUpService.GetUclData(UclTypes.State)
                                            .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                            .ToList()
                                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            return GetViewResult();
        }
        public ActionResult FacilityBillingReport()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility("")
                                               .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                               .ToList()
                                               .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.BillingCodes = _lookUpService.GetUclData(UclTypes.BillingCode)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            return GetViewResult();
        }
        public ActionResult CaseAssignHistory()
        {
            return GetViewResult();
        }
        public ActionResult GetFacilityBillingReport(DataSourceRequest request,
                                                     List<Guid> facilities,
                                                     List<string> physicians,
                                                     List<int> billingCodes,
                                                     DateTime startDate,
                                                     DateTime endDate)
        {
            try
            {
                var list = _reportService.GetFacilityBillingReport(request, facilities, physicians, billingCodes, startDate, endDate);
                return JsonMax(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PhysicianBillingByShift()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.BillingCodes = _lookUpService.GetUclData(UclTypes.BillingCode).ToList();

            return GetViewResult();
        }

        #region Add by husnain
        public ActionResult PhysicianBillingByConsult()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.BillingCodes = _lookUpService.GetUclData(UclTypes.BillingCode).ToList();

            return GetViewResult();
        }

        [HttpPost]
        public ActionResult GetPhysicainBillingByConsult(DataSourceRequest request,
                                                       List<string> physicians,
                                                       DateTime startDate,
                                                       DateTime endDate,
                                                       List<int> caseStatus,
                                                       ShiftType shiftType)
        {
            try
            {
                var result = _reportService.GetPhysicainBillingByConsult(request, physicians, startDate, endDate, caseStatus, shiftType);
                result = DateWiseOrder(result);
                return JsonMax(new { Data = result, Total = result.Count() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult PhysicianBillingAmount()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.BillingCodes = _lookUpService.GetUclData(UclTypes.BillingCode).ToList();

            return GetViewResult();
        }

        [HttpPost]
        public ActionResult GetPhysicianBillingAmount(DataSourceRequest request,
                                                       List<string> physicians,
                                                       DateTime startDate,
                                                       DateTime endDate,
                                                       List<int> caseStatus,
                                                       ShiftType shiftType)
        {
            try
            {
                if (physicians == null)
                {
                    physicians = new List<string>();
                    string user = User.Identity.GetUserId();
                    physicians.Add(user);
                }

                var result = _rateService.GetPhysicianBillingAmount(request, physicians, startDate, endDate, caseStatus, shiftType);
                return JsonMax(new { Data = result, Total = result.Count() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #region New Physician Billing Report
        public ActionResult PhysicianBillingAmount_new()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.BillingCodes = _lookUpService.GetUclData(UclTypes.BillingCode).ToList();

            return GetViewResult();
        }

        [HttpPost]
        public ActionResult GetPhysicianBillingAmount_new(DataSourceRequest request,
                                                       List<string> physicians,
                                                       DateTime startDate,
                                                       DateTime endDate,
                                                       List<int> caseStatus,
                                                       ShiftType shiftType)
        {
            try
            {
                if (physicians == null)
                {
                    physicians = new List<string>();
                    string user = User.Identity.GetUserId();
                    physicians.Add(user);
                }

                var result = _rateServiceNew.GetPhysicianBillingAmount(request, physicians, startDate, endDate, caseStatus, shiftType);
                return JsonMax(new { Data = result, Total = result.Count() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        private List<PhysicianBillingByShiftViewModel> DateWiseOrder(List<PhysicianBillingByShiftViewModel> list)
        {
            var getListWithoutdates = list.Where(x => x.AssignDate == "Blast").ToList();
            var getListOfDates = list.Where(x => x.AssignDate != "Blast").ToList();
            foreach (var item in getListOfDates)
            {
                string date = item.AssignDate;
                DateTime _dt = Convert.ToDateTime(date);
                item.assign_date = _dt;
            }
            getListOfDates = getListOfDates.OrderBy(x => x.assign_date).ToList();
            getListOfDates.AddRange(getListWithoutdates);
            return getListOfDates;
        }


        // Billing for physicians
        public ActionResult PhysicianBillingAmountDetail()
        {
            ViewBag.BillingCodes = _lookUpService.GetUclData(UclTypes.BillingCode).ToList();
            return GetViewResult();
        }


        #region Quality Reports by Ahmad junaid
        public ActionResult BedsideResponseTime()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());
            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult ArivalToNeedleTime()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            var workflowtype = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });
            var Convertworkflowtype = workflowtype.ToList();
            Convertworkflowtype.RemoveAt(1);
            IEnumerable<SelectListItem> workflowlist = Convertworkflowtype;
            ViewBag.WorkflowType = workflowlist;

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult VerbalOrderTotPATime()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult OnScreenTime()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult HandleTime()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else if (isQPS)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetNavigators().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult ActivationTime()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            var workflowtype = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });
            var Convertworkflowtype = workflowtype.ToList();
            Convertworkflowtype.RemoveAt(1);
            IEnumerable<SelectListItem> workflowlist = Convertworkflowtype;
            ViewBag.WorkflowType = workflowlist;

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult SymptomstoNeedleTimeReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            var workflowtype = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });
            var Convertworkflowtype = workflowtype.ToList();
            Convertworkflowtype.RemoveAt(1);
            IEnumerable<SelectListItem> workflowlist = Convertworkflowtype;
            ViewBag.WorkflowType = workflowlist;

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult QualitySummaryReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult VolumeMetricsReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult LicensingPerformanceMetrics()
        {
            ViewBag.LicensingSpecialist = _lookUpService.GetLicensingSpecialist();

            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.States = _lookUpService.GetUclData(UclTypes.State)
                                            .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                            .ToList()
                                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            return GetViewResult();
        }
        public ActionResult CredentialingPerformanceMetrics()
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
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.CredentialingSpecialist = _lookUpService.GetCredentialingSpecialist();

            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            return GetViewResult();
        }
        public ActionResult HourlyVolumeMetricsReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.Hours = Enumerable.Range(00, 24).Select(i => (DateTime.MinValue.AddHours(i)).ToString("HH:mm"));
            ViewBag.Minutes = Enumerable.Range(00, 48).Select(i => (DateTime.MinValue.AddMinutes(i * 30)).ToString("HH:mm"));
            ViewBag.Minutes20 = Enumerable.Range(00, 72).Select(i => (DateTime.MinValue.AddMinutes(i * 20)).ToString("HH:mm"));
            ViewBag.Minutes40 = Enumerable.Range(00, 36).Select(i => (DateTime.MinValue.AddMinutes(i * 40)).ToString("HH:mm"));
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult HourlyMeanVolumeMetricsReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.Hours = Enumerable.Range(00, 24).Select(i => (DateTime.MinValue.AddHours(i)).ToString("HH:mm"));
            ViewBag.Minutes = Enumerable.Range(00, 48).Select(i => (DateTime.MinValue.AddMinutes(i * 30)).ToString("HH:mm"));
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;


            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult StatResponseReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else if (isQPS)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetNavigators().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            List<string> roles = new List<string>();
            var RRCDirector = UserRoles.RRCDirector.ToDescription();
            var RRCManager = UserRoles.RRCManager.ToDescription();
            var RRCDirectorId = RoleManager.Roles.Where(x => x.Description == RRCDirector).Select(x => x.Id).FirstOrDefault();
            var RRCManagerId = RoleManager.Roles.Where(x => x.Description == RRCManager).Select(x => x.Id).FirstOrDefault();
            roles.Add(RRCDirectorId);
            roles.Add(RRCManagerId);
            ViewBag.Facilities = facList;
            ViewBag.Physicians = _lookUpService.GetUserByRole(roles);
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
            List<string> roless = new List<string>();

            var QPS = UserRoles.QPS.ToDescription();
            var QualityDirector = UserRoles.QualityDirector.ToDescription();
            var VPQuality = UserRoles.VPQuality.ToDescription();

            var QPSId = RoleManager.Roles.Where(x => x.Description == QPS).Select(x => x.Id).FirstOrDefault();
            var QualityDirectorId = RoleManager.Roles.Where(x => x.Description == QualityDirector).Select(x => x.Id).FirstOrDefault();
            var VPQualityId = RoleManager.Roles.Where(x => x.Description == VPQuality).Select(x => x.Id).FirstOrDefault();

            roless.Add(QPSId);
            roless.Add(QualityDirectorId);
            roless.Add(VPQualityId);

            var QPSList = _facilityService.GetUserByRole(roless, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;


            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        public ActionResult STATResponseTrends()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else if (isQPS)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetNavigators().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            List<string> roles = new List<string>();
            var RRCDirector = UserRoles.RRCDirector.ToDescription();
            var RRCManager = UserRoles.RRCManager.ToDescription();
            var RRCDirectorId = RoleManager.Roles.Where(x => x.Description == RRCDirector).Select(x => x.Id).FirstOrDefault();
            var RRCManagerId = RoleManager.Roles.Where(x => x.Description == RRCManager).Select(x => x.Id).FirstOrDefault();
            roles.Add(RRCDirectorId);
            roles.Add(RRCManagerId);
            ViewBag.Facilities = facList;
            ViewBag.Physicians = _lookUpService.GetUserByRole(roles);
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;
            string selected = "";
            List<string> roless = new List<string>();

            var QPS = UserRoles.QPS.ToDescription();
            var QualityDirector = UserRoles.QualityDirector.ToDescription();
            var VPQuality = UserRoles.VPQuality.ToDescription();

            var QPSId = RoleManager.Roles.Where(x => x.Description == QPS).Select(x => x.Id).FirstOrDefault();
            var QualityDirectorId = RoleManager.Roles.Where(x => x.Description == QualityDirector).Select(x => x.Id).FirstOrDefault();
            var VPQualityId = RoleManager.Roles.Where(x => x.Description == VPQuality).Select(x => x.Id).FirstOrDefault();

            roless.Add(QPSId);
            roless.Add(QualityDirectorId);
            roless.Add(VPQualityId);

            var QPSList = _facilityService.GetUserByRole(roless, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;


            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        [HttpPost]
        public JsonResult GetFacilityByState(int?[] state)
        {
            try
            {
                //var facList = new List<SelectListItem>();
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                if (isFacilityAdmin)
                {
                    var facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();
                    return Json(facList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (state[0] == 0)
                    {
                        var facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name });
                        return Json(facList, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        IQueryable<int?> states = state.AsQueryable();
                        var facList = _lookUpService.GetAllFacilityByState(null, states)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name });
                        return Json(facList, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, data = "", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PhysicianVolumetricReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                     .Where(m => m.ucd_is_active)
                                     .OrderBy(c => c.ucd_sort_order)
                                     .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }
            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;

            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }

        #endregion


        #region Quality Reports by Ahmad junaid        
        public ActionResult GetVolumeMetricsReport(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                if (model.TimeFrame == "Select Cycle")
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string Status = "";
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _reportService.GetVolumeMetricsReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, Status);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetHourlyVolumeMetricsReport(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                if (model.TimeCycle == "Select Cycle")
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string Status = "";
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _reportService.GetHourlyVolumeMetricsReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, Status);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetHourlyMeanVolumeMetricsReport(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                if (model.TimeFrame == "Select Cycle")
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string Status = "";
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _reportService.GetHourlyMeanVolumeMetricsReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, Status);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetQualitySummaryReport(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetQualitySummaryReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetBedsideMetrics(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetBedsideMetrics(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetArivalToNeedle(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetArivalToNeedle(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetVerbalTotPA(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetVerbalTotPA(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetOnScreen(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetOnScreen(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetHandleTime(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetHandleTime(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetActication(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetActivationTime(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetSymptomstoNeedle(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetSymptomstoNeedle(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPerformanceLicense(DataSourceRequest request, QualityMetricsViewModel filterModel)
        {
            try
            {
                var result = _reportService.GetPerformanceLicense(request, filterModel);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetPerformanceCredentialing(DataSourceRequest request, QualityMetricsViewModel filterModel)
        {
            try
            {
                var result = _reportService.GetPerformanceCredentialing(request, filterModel);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetStatTime(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetStatTime(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetStatTrendsTime(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetStatTrendsTime(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCredentialsExpiringCasesList(DataSourceRequest request, List<Guid> Facilities, List<string> Physicians)
        {
            //var model = _reportService.GetCredentialsExpiringCaseslist(request);

            try
            {
                var result = _reportService.GetCredentialsExpiringCaseslist(request, Facilities, Physicians);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCasesPendingReviewList(DataSourceRequest request, List<string> QPS_Key, string period)
        {
            try
            {
                var result = _reportService.GetCasesPendingReviewList(request, QPS_Key, period);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetCasesCompletedReviewList(DataSourceRequest request, List<string> QPS_Key, string period)
        {
            try
            {
                var result = _reportService.GetCasesCompletedReviewList(request, QPS_Key, period);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetPhysicianVolumetricReport(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                if (model.TimeFrame == "Select Frame")
                {
                    DataSourceResult result = new DataSourceResult();
                    result.Data = new List<string>();
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                    var result = _reportService.GetPhysicianVolumetricReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                    return JsonMax(result, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetOperationsOutliersList(DataSourceRequest request, string period)
        {
            try
            {
                var result = _reportService.GetOperationsOutliersList(request, period);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult PhysicianColors(int cas_key,string physician)
        //{
        //    try
        //    {
        //        var result = _reportService.PhysicianColors(cas_key, physician);
        //        return JsonMax(result, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
        //        return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        #endregion

        #region Quality Reports Graph by Ahmad Junaid
        public ActionResult GetVolumeGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetVolumeGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVolumeBarGraphModal(DataSourceRequest request, QualityMetricsViewModel model, string Status)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetVolumeMetricsReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, Status);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetHourlyVolumeBarGraphModal(DataSourceRequest request, QualityMetricsViewModel model, string Status)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetHourlyVolumeMetricsReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, Status);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetHourlyMeanVolumeBarGraphModal(DataSourceRequest request, QualityMetricsViewModel model, string Status)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetHourlyMeanVolumeMetricsReport(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, Status);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBedsideGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetBedsideMetricsGraph(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetArivalToNeedleGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetArivalToNeedleGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVerbalTotPAGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetVerbalTotPAGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetOnScreenGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetOnScreenGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetHandleTimeGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetHandleTimeGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetActivationGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetActivationGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSymptomstoNeedleGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetSymptomstoNeedleGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetStatTimeGraphModal(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetStatTimeGraphModal(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPhysicianVolumeBarGraphModal(DataSourceRequest request, QualityMetricsViewModel model, string Status)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetPhysicianVolumetricGraph(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null, Status);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPhysicianVolumePieChart(DataSourceRequest request, QualityMetricsViewModel model)
        {
            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var result = _reportService.GetPhysicianVolumePieChart(model, isFacilityAdmin ? User.Identity.GetUserId() : null, request);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region Facility Report by Billing added by Axim

        public ActionResult FacilityReportByBilling()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility(null)
                                              .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.BillingCodes = _lookUpService.GetUclData(UclTypes.BillingCode).ToList();

            return GetViewResult();
        }

        [HttpPost]
        public ActionResult GetFacilityReportByBilling(DataSourceRequest request,
                                                       List<string> facilities,
                                                       DateTime startDate,
                                                       DateTime endDate)
        {
            try
            {
                var result = _facilityBillingReportService.GetFacilityReportByBilling(request, facilities, startDate, endDate);
                return JsonMax(new { Data = result, Total = result.Count() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Facility Settings Report added by Axim
        public ActionResult facilitySettingsReport()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility(null)
                                              .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            ViewBag.States = _uCLService.GetUclData(UclTypes.State)
                                    .Where(m => m.ucd_is_active)
                                    .OrderBy(c => c.ucd_sort_order)
                                    .Select(m => new SelectListItem { Value = m.ucd_key.ToString(), Text = m.ucd_title }).ToList();
            return GetViewResult();
        }
        public ActionResult GetfacilitySetingsReport(DataSourceRequest request, List<string> facilities,
                                                     List<string> states, int? system, int? region,
                                                     int? coverageType, List<string> serviceType,
                                                     bool? active, bool? goLive)
        {
            try
            {

                var result = _reportService.GetFacilitysettings(request, facilities, system, region, states, coverageType, serviceType, active, goLive);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #region User Presence Report Added by axim
        public ActionResult UserPresenceReport()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return GetViewResult();
        }
        [HttpPost]
        public ActionResult GetUserPresenceReport(DataSourceRequest request,
                                                       List<string> Physicians,
                                                       DateTime startDate,
                                                       DateTime endDate,
                                                       string ReportType
                                                       )
        {
            try
            {
                    var result = _reportService.GetUserPresence(request, Physicians, startDate, endDate, ReportType);
                    return JsonMax(/*new { Data = result, Total = result.Count()}*/result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetUserPresenceGraphReport(DataSourceRequest request,
                                                      string Physicians,
                                                      DateTime startDate
                                                      )
        {
            try
            {
                
                var result = _reportService.GetUserPresenceGraph(request, Physicians, startDate);
                return JsonMax(/*new { Data = result, Total = result.Count()}*/result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public ActionResult FacilityBillingWithMetrics()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility(null)
                                              .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return GetViewResult();
        }
        public ActionResult PhysicianBillingWithMetrics()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            return GetViewResult();
        }
        public ActionResult QualityMetricReport()
        {
            var facList = new List<SelectListItem>();
            var phyList = new List<SelectListItem>();

            var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
            var isQPS = User.IsInRole(UserRoles.QPS.ToDescription());

            if (isFacilityAdmin)
            {
                //  ViewBag.Facilities
                facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
                      .Select(m => new { Value = m.Facility, Text = m.FacilityName })
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                        .ToList();

                phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
                                          .OrderBy(m => m.LastName)
                                          .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                          .ToList()
                                          .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }
            //else if (isQPS)
            //{
            //    //  ViewBag.Facilities
            //    facList = _ealertFacilitiesService.GetAllAssignedFacilities(User.Identity.GetUserId())
            //          .Select(m => new { Value = m.Facility, Text = m.FacilityName })
            //                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
            //                            .ToList();

            //    phyList = _lookUpService.GetPhysiciansForFacilityAdmin(User.Identity.GetUserId()).Where(m => m.IsActive == true)
            //                              .OrderBy(m => m.LastName)
            //                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
            //                              .ToList()
            //                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            //}
            else
            {
                facList = _lookUpService.GetAllActnNonActFacility(null)
                                                  .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                  .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();
            }

            ViewBag.Facilities = facList;
            ViewBag.Physicians = phyList;

            ViewBag.BillingCode = _uCLService.GetUclData(UclTypes.BillingCode)
                                              .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseTypes = _uCLService.GetUclData(UclTypes.CaseType).Where(e => e.ucd_is_active)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CaseStatus = _uCLService.GetUclData(UclTypes.CaseStatus)
                                             .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                             .ToList()
                                             .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CallerSource = _uCLService.GetUclData(UclTypes.CallerSource)
                                        .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                        .ToList()
                                        .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });


            ViewBag.WorkflowType = Enum.GetValues(typeof(PatientType)).Cast<PatientType>()
                                      .Select(m => new
                                      {
                                          Key = Convert.ToInt32(m).ToString(),
                                          Value = m.ToDescription() == "Symptom Onset During ED Stay" ? "ED Onset" : m.ToDescription()
                                      }).ToList().Select(m => new SelectListItem { Value = m.Key, Text = m.Value });

            ViewBag.CallType = Enum.GetValues(typeof(CallType)).Cast<CallType>()
                                    .Select(m => new
                                    {
                                        Key = Convert.ToInt32(m).ToString(),
                                        Value = m.ToDescription()
                                    }).Select(m => new SelectListItem { Value = m.Key, Text = m.Value }).ToList();

            bool allowListing = true;
            if (isFacilityAdmin)
            {
                if (facList.Count < 1)
                {
                    allowListing = false;
                    ViewBag.FacilityAdminMessage = "No Facility assigned to the user. Please contact your system administrator.";
                }
            }


            //var QPS_Numbers_List = Enumerable.Range(1, 20).Select(n => new SelectListItem()
            //{
            //    Text = n.ToString(),
            //    Value = n.ToString(),
            //});

            //ViewBag.QPS_Numbers_List = QPS_Numbers_List;

            string selected = "";
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

            var QPSList = _facilityService.GetUserByRole(roles, selected);
            var NQPSList = QPSList.RemoveAll(c => c.Value == "");
            ViewBag.QPS_Numbers_List = QPSList;


            var tPA_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.tPA_List = tPA_List;


            #region TCARE-479
            var eAlert_List = new List<SelectListItem>()
            {
                new SelectListItem(){ Text="Yes",Value="true"},
                new SelectListItem(){ Text="No",Value="false"},
            };

            ViewBag.eAlert_List = eAlert_List;
            #endregion 
            ViewBag.AllowListing = allowListing;
            return GetViewResult();
        }
        [HttpPost]
        public ActionResult GetPhysicainBillingByShift(DataSourceRequest request,
                                                       List<string> physicians,
                                                       DateTime startDate,
                                                       DateTime endDate,
                                                       List<int> caseStatus,
                                                       ShiftType shiftType)
        {
            try
            {
                var result = _reportService.GetPhysicainBillingByShift(request, physicians, startDate, endDate, caseStatus, shiftType);
                result = DateWiseOrder(result);
                return JsonMax(new { Data = result, Total = result.Count() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetFacilityBillingWithMetrics(DataSourceRequest request,
                                                          List<Guid> facilities,
                                                          List<string> physicians,
                                                          DateTime startDate,
                                                          DateTime endDate)
        {
            try
            {
                var result = _reportService.GetFacilityBillingWithMetrics(request, facilities, physicians, startDate, endDate);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetPhysicianBillingWithMetrics(DataSourceRequest request,
                                                           List<string> physicians,
                                                           DateTime startDate,
                                                           DateTime endDate)
        {
            try
            {
                var result = _reportService.GetPhysicianBillingWithMetrics(request, physicians, startDate, endDate);
                return JsonMax(result, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetCredentials(DataSourceRequest request,
                                         List<string> facilities,
                                         List<string> physicians,
                                         bool? isStartDate,
                                         bool? isEndDate,
                                         bool? isOnBoarded,
                                         bool? fac_IsActive,
                                         bool? goLive,
                                         bool? phy_IsActive,
                                         DateTime? StartDate,
                                         DateTime? EndDate)
        {
            try
            {
                var result = _reportService.GetCredentials(request, facilities, physicians, isStartDate, isEndDate, isOnBoarded, fac_IsActive, goLive, phy_IsActive, StartDate, EndDate);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetLicense(DataSourceRequest request, List<string> physicians, List<string> states)
        {
            try
            {
                var result = _reportService.GetLicense(request, physicians, states);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetQualityMetrics(DataSourceRequest request, QualityMetricsViewModel model)
        {
            try
            {
                var isFacilityAdmin = User.IsInRole(UserRoles.FacilityAdmin.ToDescription());
                var result = _reportService.GetQualityMetrics(request, model, isFacilityAdmin ? User.Identity.GetUserId() : null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Case-Assignment-History
        public ActionResult GetCaseAssignHistory(DataSourceRequest request, DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = _reportService.GetCaseAssignHistory(request, startDate, endDate);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCaseAssignmentHistoryDetails(DataSourceRequest request, int id)
        {
            try
            {
                var accepted = PhysicianCaseAssignQueue.Accepted.ToString();
                var rejected = PhysicianCaseAssignQueue.Rejected.ToString();

                var history = _caseAssignHistoryService.GetAll()
                    .Where(m => m.cah_is_active)
                    .Where(m => m.cah_cas_key == id)
                    .Where(m => m.cah_is_manuall_assign || m.cah_action == accepted || m.cah_action == rejected)
                    .OrderByDescending(x => x.cah_created_date).ThenBy(x => x.cah_key)
                    .Select(m => new
                    {
                        m.cah_cas_key,
                        cah_created_date = DBHelper.FormatDateTime(m.cah_created_date, true),
                        cah_action_time = m.cah_action_time.HasValue ? DBHelper.FormatDateTime(m.cah_action_time.Value, true) : "",
                        physician = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
                        status = m.cah_action == PhysicianCaseAssignQueue.ManuallyAssigned.ToString() ? PhysicianCaseAssignQueue.Accepted.ToString() : m.cah_action
                    });
                var result = history.ToDataSourceResult(request.Take, request.Skip, request.Sort, null);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCompleteCaseAssignmentHistoryDetails(DataSourceRequest request)
        {
            try
            {
                var accepted = PhysicianCaseAssignQueue.Accepted.ToString();
                var rejected = PhysicianCaseAssignQueue.Rejected.ToString();

                var history = _caseAssignHistoryService.GetAll()
                    .Where(m => m.cah_is_active)
                    .Where(m => m.cah_is_manuall_assign || m.cah_action == accepted || m.cah_action == rejected)
                    .OrderByDescending(x => x.cah_created_date).ThenBy(x => x.cah_key)
                    .Select(m => new
                    {
                        m.cah_cas_key,
                        cah_created_date = DBHelper.FormatDateTime(m.cah_created_date, true),
                        cah_action_time = m.cah_action_time.HasValue ? DBHelper.FormatDateTime(m.cah_action_time.Value, true) : "",
                        physician = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
                        status = m.cah_action == PhysicianCaseAssignQueue.ManuallyAssigned.ToString() ? PhysicianCaseAssignQueue.Accepted.ToString() : m.cah_action
                    });
                var result = history.ToDataSourceResult(request);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        [HttpPost]
        public ActionResult ExportToExcel(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);
            return File(fileContents, contentType, fileName);
        }
        public ActionResult DistrictSummaryReport()
        {
            return GetViewResult();
        }
        #region Cancelled Cases Report Added by axim
        public ActionResult CancelledCasesReport()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility(null)
                                              .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.Casestype = _uCLService.GetUclData(UclTypes.CaseType).Where(m => m.ucd_is_active)
                                            .Select(m => new { Value = m.ucd_key, Text = m.ucd_title })
                                            .ToList()
                                            .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });

            ViewBag.CancelledType = _casCancelledTypeService.GetAllRecords().Select(c => new { c.cct_key, c.cct_name })
                                            .ToList()
                                            .Select(c => new SelectListItem { Value = c.cct_name, Text = c.cct_name });

            return GetViewResult();
        }
        [HttpPost]
        public ActionResult GetCancelledCasesReport(DataSourceRequest request,
                                                       List<string> facilities,
                                                       DateTime startDate,
                                                       DateTime endDate,
                                                       List<string> Casestype,
                                                       List<string> CancelledType
                                                       )
        {
            try
            {

                var result = _cancelledCasesService.CancelledCases(request, facilities, startDate, endDate, Casestype, CancelledType);
                return JsonMax(new { Data = result, Total = result.Count() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public JsonResult GetCWH(DataSourceRequest request, List<Guid> facilities, DateTime FromMonth, DateTime ToMonth)
        {
            try
            {
                var result = _reportService.GetCWHData(request, facilities, FromMonth, ToMonth);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }




        public ActionResult CWHReport()
        {
            ViewBag.Facilities = _lookUpService.GetAllActnNonActFacility(null)
                                 .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                 .ToList()
                                 .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return GetViewResult();
        }



        public ActionResult GetCWHGraphModal(DataSourceRequest request, CWHReport model)
        {
            var result = _reportService.GetCWHGraph(request, model);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRCI(DataSourceRequest request, List<Guid> Physicians, DateTime FromMonth, DateTime ToMonth)
        {
            try
            {
                var result = _reportService.GetRCIData(request, Physicians, FromMonth, ToMonth);
                return JsonMax(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return JsonMax(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RCIReport()
        {
            ViewBag.Physicians = _lookUpService.GetPhysicians().Where(m => m.IsActive == true && m.IsStrokeAlert == true)
                                  .OrderBy(m => m.LastName)
                                  .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                  .ToList()
                                  .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text });
            return GetViewResult();
        }
        public ActionResult GetRCIGraphModal(DataSourceRequest request, RCIReport model)
        {
            var result = _reportService.GetRCIGraph(request, model);
            var _result = result.Data;
            return Json(_result, JsonRequestBehavior.AllowGet);
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
                    _rateServiceNew?.Dispose();
                    _reportService?.Dispose();
                    _lookUpService?.Dispose();
                    _caseAssignHistoryService?.Dispose();
                    _uCLService?.Dispose();
                    _ealertFacilitiesService?.Dispose();
                    _facilityBillingReportService?.Dispose();
                    _cancelledCasesService?.Dispose();
                    _rateService?.Dispose();
                    _casCancelledTypeService?.Dispose();

                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}