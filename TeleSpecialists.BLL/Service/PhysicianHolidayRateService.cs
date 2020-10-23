using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Helpers;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace TeleSpecialists.BLL.Service
{
    public class PhysicianHolidayRateService : BaseService
    {

        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.PhysicianHolidayRateRepository.Query()
                               join n in _unitOfWork.ApplicationUsers on m.phr_phy_key equals n.Id into physicians
                               //from physician_shift_rate in physicians.DefaultIfEmpty()
                               orderby m.phr_phy_key descending
                               //from m in _unitOfWork.PhysicianRateRepository.Query()
                               //               join n in _unitOfWork.ApplicationUsers on m.psr_phy_key equals n.Id into physicians
                               //join type in GetUclData(UclTypes.CaseType) on m.psr_rate equals type.ucd_key into CaseTypeEntity
                               //from case_type in physicians.DefaultIfEmpty()
                               //orderby m.psr_Id descending
                               select new
                               {
                                   m.phr_key,
                                   m.phr_phy_key,
                                   name = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
                                   //m.psr_rate,
                                   //casename = Enum.GetName(typeof(CaseType), m.psr_rate), //((CaseType)m.case_id).ToString(),
                                   //CaseType = case_type != null ? case_type.ucd_title : "",
                                   //time = DbFunctions.Right("00" + SqlFunctions.DateName("hour", m.starttimeslot.Value), 2)
                                   //    + ":"
                                   //    + DbFunctions.Right("00" + SqlFunctions.DateName("minute", m.starttimeslot.Value), 2)
                                   //    + " - "
                                   //    + DbFunctions.Right("00" + SqlFunctions.DateName("hour", m.endtimeslot.Value), 2)
                                   //    + ":"
                                   //    + DbFunctions.Right("00" + SqlFunctions.DateName("minute", m.endtimeslot.Value), 2),
                                   rate = "$" + m.phr_rate
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public physician_holiday_rate GetDetails(int id)
        {
            var model = _unitOfWork.PhysicianHolidayRateRepository.Query()
                                   .FirstOrDefault(m => m.phr_key == id);
            return model;
        }

        public physician_holiday_rate GetCustomRate(long id)
        {
            var model = _unitOfWork.PhysicianHolidayRateRepository.Query().Where(m => m.phr_uss_key == id).FirstOrDefault();
                         //.FirstOrDefault(m => m.phr_uss_key == id);
            return model;
        }

        public physician_holiday_rate GetCustomRateById(int id)
        {
            var model = _unitOfWork.PhysicianHolidayRateRepository.Query()
                         .FirstOrDefault(m => m.phr_key == id);
            return model;
        }

        public physician_holiday_rate CheckExistingRecord(string id, DateTime scheduleDate)
        {
            var model = _unitOfWork.PhysicianHolidayRateRepository.Query()
                                   .FirstOrDefault(m => m.phr_phy_key == id && DbFunctions.TruncateTime(m.phr_date) == DbFunctions.TruncateTime(scheduleDate));
            return model;
        }

        public string GetPhysicianName(string id)
        {
            var getRecord = _unitOfWork.ApplicationUsers.Where(x => x.Id == id).FirstOrDefault();
            string name = getRecord.FirstName + " " + getRecord.LastName;
            return name;
        }

        public bool IsAlreadyExists(facility entity)
        {
            return _unitOfWork.FacilityRepository.Query()
                                                 .Where(m => m.fac_name.ToLower().Trim() == entity.fac_name.ToLower().Trim())
                                                 .Where(m => m.fac_key != entity.fac_key)
                                                 .Any();
        }
        public void Create(physician_holiday_rate entity)
        {
            _unitOfWork.PhysicianHolidayRateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(physician_holiday_rate entity)
        {
            _unitOfWork.PhysicianHolidayRateRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Remove(int id)
        {
            physician_holiday_rate del = _unitOfWork.PhysicianHolidayRateRepository.Query().Where(m => m.phr_uss_key == id).FirstOrDefault();
            _unitOfWork.PhysicianHolidayRateRepository.DeleteRate(del);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
    }
}
