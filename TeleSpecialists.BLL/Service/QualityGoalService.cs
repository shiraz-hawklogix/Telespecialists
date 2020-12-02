using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Helpers;
using System;
using TeleSpecialists.BLL.Extensions;
using System.Collections.Generic;

namespace TeleSpecialists.BLL.Service
{
    public class QualityGoalService : BaseService
    {
        public quality_goals GetDetailByYear(Guid key,string year)
        {
            var model = _unitOfWork.QualityGoalsRepository.Query().Where(x => x.qag_fac_key == key && x.qag_time_frame == year).FirstOrDefault();
            return model;
        }
        public List<rca_counter_measure> GetRca_Counter_Measure(int id)
        {
            var model = _unitOfWork.RootCauseRepository.Query().Where(x => x.rca_key_id == id).ToList();
            return model;
        }
        public quality_goals GetDetails(int id)
        {
            var model = _unitOfWork.QualityGoalsRepository.Query()
                                   .FirstOrDefault(m => m.qag_key == id);
            return model;
        }
        public goals_data GetGoalDataById(int id)
        {
            var model = _unitOfWork.GoalsDataRepository.Query()
                                   .FirstOrDefault(m => m.gd_key == id);
            return model;
        }
        public List<goals_data> GetGoalsDataDetails(int id)
        {
            var model = _unitOfWork.GoalsDataRepository.Query()
                                   .Where(m => m.gd_qag_key == id).ToList();
            return model;
        }
        public IQueryable<quality_goals> GetQualityGoals(List<Guid> fct_key, string timeframe)
        {
            return _unitOfWork.QualityGoalsRepository.Query().Where(x => fct_key.Contains(x.qag_fac_key.Value) && x.qag_time_frame == timeframe);
        }
        public quality_goals GetQualityGoalsByFacility(Guid fct_key,string timeframe)
        {
            var model = _unitOfWork.QualityGoalsRepository.Query().Where(x => x.qag_fac_key.Value == fct_key && x.qag_time_frame == timeframe).FirstOrDefault();
            return model;
        }
        public DataSourceResult GetAll(DataSourceRequest request,Guid fac_key)
        {

            var Getqualitygoals = _unitOfWork.QualityGoalsRepository.Query().Where(x=>x.qag_fac_key == fac_key).ToList();
            List<QualityGoalscls> qualities = new List<QualityGoalscls>();
            
            foreach(var item in Getqualitygoals)
            {
                int count = 0;
                foreach (var data in item.goals_data)
                {
                    QualityGoalscls goalscls = new QualityGoalscls();
                    goalscls.facility = item.facility.fac_name;
                    goalscls.qag_key = item.qag_key;
                    goalscls.qag_fac_key = item.qag_fac_key.Value;
                    goalscls.qag_time_frame = item.qag_time_frame + "-Quarter " + data.gd_quater;
                    goalscls.gd_key = data.gd_key;
                    goalscls.qag_door_to_TS_notification_ave_minutes = data.qag_door_to_TS_notification_ave_minutes;
                    goalscls.qag_door_to_TS_notification_median_minutes = data.qag_door_to_TS_notification_median_minutes;
                    goalscls.qag_percent10_min_or_less_activation_EMS = data.qag_percent10_min_or_less_activation_EMS;
                    goalscls.qag_percent10_min_or_less_activation_PV = data.qag_percent10_min_or_less_activation_PV;
                    //goalscls.qag_percent10_min_or_less_activation_Inpt = data.qag_percent10_min_or_less_activation_Inpt;
                    goalscls.qag_TS_notification_to_response_average_minute = data.qag_TS_notification_to_response_average_minute;
                    goalscls.qag_TS_notification_to_response_median_minute = data.qag_TS_notification_to_response_median_minute;
                    goalscls.qag_percent_TS_at_bedside_grterthan10_minutes = data.qag_percent_TS_at_bedside_grterthan10_minutes;
                    goalscls.qag_alteplase_administered = data.qag_alteplase_administered;
                    goalscls.qag_door_to_needle_average = data.qag_door_to_needle_average;
                    goalscls.qag_door_to_needle_median = data.qag_door_to_needle_median;
                    goalscls.qag_verbal_order_to_administration_average_minutes = data.qag_verbal_order_to_administration_average_minutes;
                    goalscls.qag_DTN_grter_or_equal_30minutes_percent = data.qag_DTN_grter_or_equal_30minutes_percent;
                    goalscls.qag_DTN_grter_or_equal_45minutes_percent = data.qag_DTN_grter_or_equal_45minutes_percent;
                    goalscls.qag_DTN_grter_or_equal_60minutes_percent = data.qag_DTN_grter_or_equal_60minutes_percent;
                    goalscls.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent = data.qag_TS_notification_to_needle_grter_or_equal_30minutes_percent;
                    goalscls.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent = data.qag_TS_notification_to_needle_grter_or_equal_45minutes_percent;
                    goalscls.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent = data.qag_TS_notification_to_needle_grter_or_equal_60minutes_percent;
                    goalscls.qag_cpoe_order_to_needle_ave_min = data.qag_cpoe_order_to_needle_ave_min;
                    goalscls.qag_cpoe_order_to_needle_med_min = data.qag_cpoe_order_to_needle_med_min;
                    qualities.Add(goalscls);
                    count++;
                }
            }
            var qualitygoals = qualities.AsQueryable();
                                    
            return qualitygoals.ToDataSourceResult(request.Take, request.Skip, request.Sort, null);
        }
        public bool Exists(int id)
        {
            return _unitOfWork.QualityGoalsRepository.Query()
                                   .Any(m => m.qag_key == id);
        }
        public void Create(quality_goals entity)
        {
            _unitOfWork.QualityGoalsRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void CreateGoalData(goals_data entity)
        {
            _unitOfWork.GoalsDataRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(quality_goals entity)
        {
            _unitOfWork.QualityGoalsRepository.Delete(entity.qag_key);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        public bool DeleteRange(int id)
        {
            var list = _unitOfWork.GoalsDataRepository.Query().Where(m => m.gd_qag_key == id).ToList();
            _unitOfWork.GoalsDataRepository.DeleteRange(list);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        public void Edit(quality_goals entity)
        {
            _unitOfWork.QualityGoalsRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void EditGoalData(goals_data entity)
        {
            _unitOfWork.GoalsDataRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void SaveChanges()
        {
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
