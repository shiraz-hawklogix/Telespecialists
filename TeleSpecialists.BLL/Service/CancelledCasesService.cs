using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.Reports;

namespace TeleSpecialists.BLL.Service
{
    public class CancelledCasesService : BaseService
    {
        public List<CancelledCases> CancelledCases(DataSourceRequest request,
                                                   List<string> facilities,
                                                   DateTime startDate,
                                                   DateTime endDate,
                                                   List<string> casetype,
                                                   List<string> cancelledType)
        {
            DateTime _startTime = startDate.AddDays(-1);
            DateTime _endTime = endDate.AddDays(1);

            var _listcancelledcases = _cancelcases(_startTime, _endTime, facilities, casetype, cancelledType);

            return _listcancelledcases;
        }
        List<CancelledCases> _cancelcases(DateTime startDate, DateTime endDate, List<string> facilities, List<string> casetype,List<string> cancelledType)
        {
            List<CancelledCases> onShiftCasesList = null;
            var onShiftQuery = (from c in _unitOfWork.CaseRepository.Query()
                                join u in _unitOfWork.FacilityRepository.Query() on c.cas_fac_key equals u.fac_key
                                join m in _unitOfWork.UCL_UCDRepository.Query() on c.cas_ctp_key equals m.ucd_key
                                join n in _unitOfWork.UserRepository.Query() on c.cas_phy_key equals n.Id
                                where c.cas_fac_key != null && c.cas_cst_key == 140 && c.cas_billing_date_of_consult != null
                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(c.cas_billing_date_of_consult) <= DbFunctions.TruncateTime(endDate)
                                orderby (c.cas_case_number)
                                select new { c, u, m, n });

            if (facilities != null)
                onShiftQuery = onShiftQuery.Where(x => facilities.Contains(x.c.cas_fac_key.ToString()));
            if (casetype != null)
                onShiftQuery = onShiftQuery.Where(x => casetype.Contains(x.c.cas_ctp_key.ToString()));
            if (cancelledType != null)
                onShiftQuery = onShiftQuery.Where(x => cancelledType.Contains(x.c.cas_cancelled_type.ToString()));

            onShiftCasesList = (from onShiftModel in onShiftQuery
                                group
                                    new { onShiftModel.c } by
                                        new
                                        {
                                            AssignDate = DBHelper.FormatDateTime(DbFunctions.TruncateTime(onShiftModel.c.cas_billing_date_of_consult).Value, false),
                                            Facility = onShiftModel.u.fac_name,
                                            FacilityKey = onShiftModel.c.cas_fac_key.ToString(),
                                            caseID = onShiftModel.c.cas_case_number,
                                            casetype = onShiftModel.m.ucd_title,
                                            PhysicianId = onShiftModel.n.PhysicianId,
                                            physician = onShiftModel.n.FirstName + " " + onShiftModel.n.LastName,
                                            cancelReason = onShiftModel.c.cas_cancelled_text,
                                            cancelType = onShiftModel.c.cas_cancelled_type,
                                        } into g
                                select new CancelledCases
                                {
                                    AssignDate = g.Key.AssignDate,
                                    Facility = g.Key.Facility,
                                    FacilityKey = g.Key.FacilityKey,
                                    caseID = g.Key.caseID.ToString(),
                                    casetype = g.Key.casetype.ToString(),
                                    PhysicianId = g.Key.PhysicianId,
                                    physician = g.Key.physician,
                                    cancelReason = g.Key.cancelReason,
                                    cancelType = g.Key.cancelType,
                                }).ToList();

            return onShiftCasesList;
        }
    }
}
