using System;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CaseAssignHistoryService : BaseService
    {
        public void Create(case_assign_history entity)
        {
            _unitOfWork.CaseAssignHistoryRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public case_assign_history GetDetails(int id)
        {
            return _unitOfWork.CaseAssignHistoryRepository.Query()
                                                    .AsQueryable()
                                                    .Where(m => m.cah_is_active)
                                                    .FirstOrDefault(m => m.cah_key == id);
        }
        public IQueryable<case_assign_history> GetAll()
        {
            return _unitOfWork.CaseAssignHistoryRepository.Query()
                                                    .AsQueryable()
                                                    .Where(m => m.cah_is_active);
        }

        public bool Check15MinuteRule(string phy_key, int cas_key = 0)
        {
            bool bshowPopup = false;
          

            var cancelledStatus = CaseStatus.Cancelled.ToInt();
            var completedStatus = CaseStatus.Complete.ToInt();

            var strCancelledStatus = CaseStatus.Cancelled.ToString();
            var strCompletedStatus = CaseStatus.Complete.ToString();
            var acceptedQueueStatus = PhysicianCaseAssignQueue.Accepted.ToString();

            var strokeAlert = CaseType.StrokeAlert.ToInt();

            var historyQuery = (from h in _unitOfWork.CaseAssignHistoryRepository.Query()
                           join c in _unitOfWork.CaseRepository.Query() on h.cah_cas_key equals c.cas_key
                           where h.cah_is_active
                                         && h.@case.cas_ctp_key == strokeAlert
                                         && h.cah_phy_key == phy_key
                                         && c.cas_phy_key == phy_key // the case must be currently assigned to the physician 
                                         && (c.cas_cst_key != cancelledStatus && c.cas_cst_key != completedStatus)
                                         && (h.cah_action.ToLower() != strCancelledStatus.ToLower() && h.cah_action.ToLower() != strCompletedStatus.ToLower())
                                         && (h.cah_cas_key != cas_key)
                           select h);

            var strokeAlertCaseKey = historyQuery.OrderByDescending(m => m.cah_key).FirstOrDefault(); // intially sorting it by descending, so we get the latest case from history against the physician


            if (strokeAlertCaseKey != null)
            {
                var history = historyQuery.Where(m => m.cah_cas_key == strokeAlertCaseKey.cah_cas_key).OrderBy(m => m.cah_key).FirstOrDefault(); // second time sorting it in ascending, so we get initial record to companre the time with
                if (history.cah_action_time.HasValue)
                {
                    var diff = DateTime.Now.ToEST() - history.cah_action_time;
                    if (diff.HasValue)
                    {
                        if (diff.Value.TotalMinutes <= 15)
                            bshowPopup = true;
                    }

                }
            }


            return bshowPopup;
        }

        public IQueryable<case_assign_history> GetLog(PhysicianCaseAssignQueue status)
        {
            return _unitOfWork.CaseAssignHistoryRepository.Query().AsQueryable()
                                                                  .Where(m => m.cah_is_active)
                                                                  .Where(m => m.cah_action == status.ToString());
        }
        public IQueryable<case_assign_history> GetInQueuePhysicians(int cas_key)
        {
            return GetLog(PhysicianCaseAssignQueue.InQueue)
                          .Where(m => m.cah_action_time == null)
                          .Where(m => m.cah_cas_key == cas_key)
                          .Where(m => m.cah_phy_key != null)
                          .OrderBy(m => m.cah_sort_order);
        }
        public IQueryable<case_assign_history> GetRequests(int cas_key, PhysicianCaseAssignQueue status)
        {
            return GetLog(status)
                          .Where(m => m.cah_cas_key == cas_key)
                          .OrderBy(m => m.cah_sort_order);
        }
        public void Edit(case_assign_history entity)
        {
            _unitOfWork.CaseAssignHistoryRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
