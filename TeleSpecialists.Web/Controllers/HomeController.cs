using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly DashboardService _dashboardService;
        private readonly FacilityService _facilityService;
        private readonly string[] ChartColors = new string[] { "#3f5f29", "#7937ab", "#8e4311", "#0579cd", "#ffc105", "#d26317", "#ff0505", "#807a7a", "#734bf4", "#640d0d", "#fbabab", "#135589", "#3ddad7", "#c75300", "#ae3ec3", "#b48905", "#a2d01f", "#603f00", "#58ec4b", "#1d3f47" };
        private readonly LookupService _lookUpService;
        public HomeController()
        {
            _dashboardService = new DashboardService();
            _facilityService = new FacilityService();
            _lookUpService = new LookupService();
        }
        [HttpGet]
        public JsonResult GetImage(string User_Id)
        {
            try
            {
                var result = UserManager.Users.Where(x => x.Id == User_Id).Select(o => o.User_Image).FirstOrDefault();
                if (result == null)
                {
                    result = "2";
                }
                return Json(result.ToString(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Index()
        {
            if (User.IsInRole(UserRoles.SuperAdmin.ToDescription()) || User.IsInRole(UserRoles.CredentialingTeam.ToDescription()) || User.IsInRole(UserRoles.MedicalStaff.ToDescription()))
            {
                var facList = new List<SelectListItem>();
                var phyList = new List<SelectListItem>();
                facList = _lookUpService.GetAllFacility(null)
                                                      .Select(m => new { Value = m.fac_key, Text = m.fac_name })
                                                      .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text })
                                                      .ToList();
                phyList = _lookUpService.GetPhysicians().Where(m => m.IsActive == true)
                                              .OrderBy(m => m.LastName)
                                              .Select(m => new { Value = m.Id, Text = m.LastName + " " + m.FirstName })
                                              .ToList()
                                              .Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Text }).Distinct().ToList();

                ViewBag.Facilities = facList;
                ViewBag.Physicians = phyList;
            }

            if (User.IsInRole(UserRoles.SuperAdmin.ToDescription()) || User.IsInRole(UserRoles.QPS.ToDescription()) || User.IsInRole(UserRoles.QualityDirector.ToDescription()) || User.IsInRole(UserRoles.VPQuality.ToDescription()))
            {
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

                ViewBag.QPSList = _facilityService.GetUserByRole(roless, selected);
            }

            if (User.IsInRole(UserRoles.FacilityNavigator.ToDescription()) || User.IsInRole(UserRoles.PACNavigator.ToDescription()))
            {
                return RedirectToAction("FacilityNavigator", "FacilityUser");
            }
            if (User.IsInRole(UserRoles.Physician.ToDescription()) || User.IsInRole(UserRoles.PartnerPhysician.ToDescription()))
            {
                //return RedirectToAction("Dashboard", "Physician");
                // Replace '-----RedirectToAction("Dashboard", "Physician")----', becasue it wasn't working in IE ajx call.
                return GetViewResult("~/Views/Physician/Dashboard.cshtml");
            }
            if (User.IsInRole(UserRoles.AOC.ToDescription()))
            {
                //return RedirectToAction("Dashboard", "Physician");
                // Replace '-----RedirectToAction("Dashboard", "Physician")----', becasue it wasn't working in IE ajx call.
                return RedirectToAction("Index", "Case");
            }
            if (User.IsInRole(UserRoles.FacilityAdmin.ToDescription()))
            {
                return RedirectToAction("Index", "Case");
            }
            if (User.IsInRole(UserRoles.FacilityPhysician.ToDescription()))
            {
                return RedirectToAction("Index", "Case");
            }
            if (User.IsInRole(UserRoles.CapacityResearcher.ToDescription()))
            {
                return RedirectToAction("Index", "Reports");
            }
            //if (User.IsInRole(UserRoles.CredentialingTeam.ToDescription()))
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            if (User.IsInRole(UserRoles.RegionalMedicalDirector.ToDescription()))
            {
                return RedirectToAction("Index", "Case");
            }
            //if (User.IsInRole(UserRoles.QPS.ToDescription()))
            //{
            //    return RedirectToAction("Index", "Case");
            //}
            if (User.IsInRole(UserRoles.Finance.ToDescription()))
            {
                return RedirectToAction("Index", "Case");
            }
            if (User.IsInRole(UserRoles.AOC.ToDescription()))
            {
                return RedirectToAction("Index", "Case");
            }
            //if (User.IsInRole(UserRoles.MedicalStaff.ToDescription()))
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            return GetViewResult();
        }
        public JsonResult LoadCaseStatusSummary(string filter)
        {
            try
            {
                var model = _dashboardService.LoadCaseStatusSummary(filter);
                if (model != null && model.Count() > 0)
                {
                    int distinctDataItems = model.Select(x => x.Title).Distinct().ToList().Count();
                    Chart _chart = new Chart();
                    _chart.labels = model.Select(x => x.Title).Distinct().ToList().ToArray(); // new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "Novemeber", "December" };
                    List<Datasets> _dataSet = new List<Datasets>();
                    _dataSet.Add(new Datasets()
                    {
                        label = "Case Status",
                        data = model.Select(x => x.TitleCount).ToList().ToArray(), ////new int[] { 28, 48, 40, 19, 86, 27, 90, 20, 45, 65, 34, 22 },
                        backgroundColor = ChartColors.Take(distinctDataItems).ToArray(), //new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                        borderColor = ChartColors.Take(distinctDataItems).ToArray(), //new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                        borderWidth = "1",
                        fill = false,
                    });
                    _chart.datasets = _dataSet;
                    return Json(_chart, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            var error = new Chart { labels = new string[] { "ERROR" } };
            return Json(error, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadPhysicianStatusSummary(string filter)
        {
            try
            {
                var model = _dashboardService.LoadPhysicianStatusSummary(filter);
                if (model != null && model.Count() > 0)
                {
                    Chart _chart = new Chart();
                    _chart.labels = model.Select(x => x.Title).ToArray(); // new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "Novemeber", "December" };
                    List<Datasets> _dataSet = new List<Datasets>();
                    _dataSet.Add(new Datasets()
                    {
                        label = "Physician Status",
                        data = model.Select(x => x.TitleCount).ToArray(), ////new int[] { 28, 48, 40, 19, 86, 27, 90, 20, 45, 65, 34, 22 },
                        backgroundColor = model.Select(x => x.TitleColor).ToArray(), //new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                        borderColor = model.Select(x => x.TitleColor).ToArray(), //new string[] { "#FF0000", "#800000", "#808000", "#008080", "#800080", "#0000FF", "#000080", "#999999", "#E9967A", "#CD5C5C", "#1A5276", "#27AE60" },
                        borderWidth = "1",
                        fill = false,
                    });
                    _chart.datasets = _dataSet;
                    return Json(_chart, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            var error = new Chart { labels = new string[] { "ERROR" } };
            return Json(error, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCaseTypesSummary()
        {
            try
            {
                var model = _dashboardService.LoadPhysicianStatusSummary("");
                if (model != null && model.Count() > 0)
                {
                    var distinctTitles = model.Select(x => x.Title).Distinct().ToList();
                    Chart _chart = new Chart();
                    _chart.labels = distinctTitles.ToArray();
                    List<Datasets> _dataSet = new List<Datasets>();
                    //foreach (string title in distinctTitles)
                    for (int i = 0; i < distinctTitles.Count(); i++)
                    {
                        _dataSet.Add(new Datasets()
                        {
                            label = distinctTitles.ElementAt(i),
                            data = model.Where(x => x.Title == distinctTitles.ElementAt(i)).OrderBy(x => x.TitleDay).Select(x => x.TitleCount).ToArray(),
                            backgroundColor = new string[] { ChartColors.ElementAt(i) },
                            borderColor = new string[] { ChartColors.ElementAt(i) },
                            borderWidth = "1",
                            fill = false,
                        });
                    }
                    _chart.datasets = _dataSet;
                    return Json(_chart, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }
            var error = new Chart { labels = new string[] { "ERROR" } };
            return Json(error, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCaseStats(string filter)
        {
            var model = _dashboardService.LoadCasesStats(filter);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetStrokeAlertChartStats()
        {
            var model = _dashboardService.GetStrokeAlertChartStats();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #region TCARE-541
        public JsonResult GetAllSummaries(string filter, int currentCount = 0)
        {
            Chart caseStatusChart;
            Chart physicianStatusChart;
            dynamic caseStats;
            dynamic strokeAlertStats;
            try
            {
                caseStats = CaseStats(filter);

                if (caseStats != null)
                    if (caseStats[0]["TotalCases"] == currentCount)
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);
                    }
                caseStatusChart = CaseStatusSummary(filter);
                physicianStatusChart = PhysicianStatusSummary(filter);
                strokeAlertStats = StrokAlertStats();

                return Json(new { caseStatusChart, physicianStatusChart, caseStats, strokeAlertStats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
            }

            caseStatusChart = new Chart { labels = new string[] { "ERROR" } };
            physicianStatusChart = new Chart { labels = new string[] { "ERROR" } };
            caseStats = "ERROR";
            strokeAlertStats = "ERROR";

            return Json(new { caseStatusChart, physicianStatusChart, caseStats, strokeAlertStats }, JsonRequestBehavior.AllowGet);

        }

        private Chart CaseStatusSummary(string filter)
        {
            var model = _dashboardService.LoadCaseStatusSummary(filter);
            Chart _chart = new Chart();
            if (model != null && model.Count() > 0)
            {
                int distinctDataItems = model.Select(x => x.Title).Distinct().ToList().Count();

                _chart.labels = model.Select(x => x.Title).Distinct().ToList().ToArray();
                List<Datasets> _dataSet = new List<Datasets>();
                _dataSet.Add(new Datasets()
                {
                    label = "Case Status",
                    data = model.Select(x => x.TitleCount).ToList().ToArray(),
                    backgroundColor = ChartColors.Take(distinctDataItems).ToArray(),
                    borderColor = ChartColors.Take(distinctDataItems).ToArray(),
                    borderWidth = "1",
                    fill = false,
                });
                _chart.datasets = _dataSet;
            }
            return _chart;
        }

        private Chart PhysicianStatusSummary(string filter)
        {
            var model = _dashboardService.LoadPhysicianStatusSummary(filter);
            Chart _chart = new Chart();
            if (model != null && model.Count() > 0)
            {
                _chart.labels = model.Select(x => x.Title).ToArray();
                List<Datasets> _dataSet = new List<Datasets>();
                _dataSet.Add(new Datasets()
                {
                    label = "Physician Status",
                    data = model.Select(x => x.TitleCount).ToArray(),
                    backgroundColor = model.Select(x => x.TitleColor).ToArray(),
                    borderColor = model.Select(x => x.TitleColor).ToArray(),
                    borderWidth = "1",
                    fill = false,
                });
                _chart.datasets = _dataSet;

            }
            return _chart;
        }

        private Chart CaseTypeSummary(string filter)
        {
            var model = _dashboardService.LoadPhysicianStatusSummary("");
            Chart _chart = new Chart();

            if (model != null && model.Count() > 0)
            {
                var distinctTitles = model.Select(x => x.Title).Distinct().ToList();
                _chart.labels = distinctTitles.ToArray();
                List<Datasets> _dataSet = new List<Datasets>();
                //foreach (string title in distinctTitles)
                for (int i = 0; i < distinctTitles.Count(); i++)
                {
                    _dataSet.Add(new Datasets()
                    {
                        label = distinctTitles.ElementAt(i),
                        data = model.Where(x => x.Title == distinctTitles.ElementAt(i)).OrderBy(x => x.TitleDay).Select(x => x.TitleCount).ToArray(),
                        backgroundColor = new string[] { ChartColors.ElementAt(i) },
                        borderColor = new string[] { ChartColors.ElementAt(i) },
                        borderWidth = "1",
                        fill = false,
                    });
                }
                _chart.datasets = _dataSet;
            }
            return _chart;
        }

        private dynamic CaseStats(string filter)
        {
            return _dashboardService.LoadCasesStats(filter);
        }

        private dynamic StrokAlertStats()
        {
            return _dashboardService.GetStrokeAlertChartStats();
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
                    _dashboardService?.Dispose();
                }
                disposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}