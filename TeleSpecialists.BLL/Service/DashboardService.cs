using System.Linq;
using System.Collections.Generic;
using TeleSpecialists.BLL.Model;
using System;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;
using System.Data.Entity;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.BLL.Service
{
    public class DashboardService : BaseService
    {
        public DashboardService()
        {

        }
        public List<ChartDataModel> LoadCaseStatusSummary(string filter)
        {
            return _unitOfWork.SqlQuery<ChartDataModel>(string.Format("Exec usp_dashboard_cases_by_status '{0}'", filter)).ToList();
        }
        public List<ChartDataModel> LoadPhysicianStatusSummary(string filter)
        {
            return _unitOfWork.SqlQuery<ChartDataModel>(string.Format("Exec usp_dashboard_physician_by_status '{0}'", filter)).ToList();
        }
        public List<ChartDataModel> LoadCasesTypeSummary(string filter)
        {
            return _unitOfWork.SqlQuery<ChartDataModel>(string.Format("Exec usp_dashboard_cases_by_type '{0}'", filter)).ToList();
        }
        public dynamic LoadCasesStats(string filter)
        {
            return _unitOfWork.SqlToList(string.Format("Exec usp_dashboard_stats '{0}'", filter));
        }
        public dynamic GetStrokeAlertChartStats()
        {
            List<StrokeAlertChartViewModel> chartDataList = new List<StrokeAlertChartViewModel>();
            var currentTimeEst = DateTime.Now.ToEST();

            List<StrokeStatChartViewModel> CasesCount = _unitOfWork.SqlQuery<StrokeStatChartViewModel>(string.Format("Exec sp_get_casescount_by_casetype @Date = '{0}'", currentTimeEst)).ToList();
            /*
            var query = _unitOfWork.CaseRepository.Query()
                    .Where(x => DbFunctions.TruncateTime(x.cas_created_date) == DbFunctions.TruncateTime(currentTimeEst))
                    .Where(x => x.cas_ctp_key == (int)CaseType.StrokeAlert)
                    .Where(x => x.cas_cst_key != 140)
                    .Select(x => x.cas_created_date).ToList();

            var casesList = query.GroupBy(x => x.Hour)
            .Select(g => new { hour = g.Key, count = g.Count() });
            */
            
            for (int i = 0; i < 24; i++)
            {
                //int currentCount = casesList.Where(x => x.hour == i).Select(x => x.count).FirstOrDefault();
                
                int StrokeCount = CasesCount.Where(x => x.Hour == i).Select(x => x.Count).FirstOrDefault();
                int PhysicianCount = CasesCount.Where(x => x.Hour == i).Select(x => x.PhysicianBlastCount).FirstOrDefault();
                int NavigatorCount = CasesCount.Where(x => x.Hour == i).Select(x => x.NavigatorBlastCount).FirstOrDefault();
                int StatCount = CasesCount.Where(x => x.Hour == i).Select(x => x.STATCount).FirstOrDefault();

                chartDataList.Add(new StrokeAlertChartViewModel()
                {
                    Hour = i,
                    Count = StrokeCount,
                    PhysicianBlastCount = PhysicianCount,
                    NavigatorBlastCount = NavigatorCount,
                    STATCount = StatCount
                });
            }
            return chartDataList;
        }
    }
}
