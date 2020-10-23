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
    public class PhysicianRateService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.PhysicianRateRepository.Query()
                               join n in _unitOfWork.ApplicationUsers on m.psr_phy_key equals n.Id into physicians
                               //join type in GetUclData(UclTypes.BillingCode) on m.rat_cas_id equals type.ucd_key into CaseTypeEntity
                               //from case_type in CaseTypeEntity.DefaultIfEmpty()
                               orderby m.psr_key descending
                               select new
                               {
                                   m.psr_key,
                                   m.psr_phy_key,
                                   m.psr_shift_name,
                                   m.psr_start_date,
                                   m.psr_end_date,
                                   name = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
                                   rate = "$" + m.psr_rate
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetAllRecordByPhyscian(DataSourceRequest request, string phy_key)
        {
            var caseTypelist = from m in _unitOfWork.PhysicianRateRepository.Query()
                               join n in _unitOfWork.ApplicationUsers on m.psr_phy_key equals n.Id into physicians
                               where m.psr_phy_key == phy_key
                               //join type in GetUclData(UclTypes.BillingCode) on m.rat_cas_id equals type.ucd_key into CaseTypeEntity
                               //from case_type in CaseTypeEntity.DefaultIfEmpty()
                               orderby m.psr_key
                               select new
                               {
                                   m.psr_key,
                                   m.psr_phy_key,
                                   m.psr_shift_name,
                                   m.psr_start_date,
                                   m.psr_end_date,
                                   name = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
                                   rate = "$" + m.psr_rate
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public physician_shift_rate GetDetails(int id)
        {
            var model = _unitOfWork.PhysicianRateRepository.Query()
                                   .FirstOrDefault(m => m.psr_key == id);
            return model;
        }
        public string GetPhysicianName(string id)
        {
            var getRecord = _unitOfWork.ApplicationUsers.Where(x => x.Id == id).FirstOrDefault();
            string name = getRecord.FirstName + " " + getRecord.LastName;
            return name;
        }
        public bool IsAlreadyExists(string Phy_key, DateTime Start_date, DateTime End_date, int Shift_ID)
        {
            return _unitOfWork.PhysicianRateRepository.Query()
                                                 .Where(m => m.psr_phy_key == Phy_key)
                                                 .Where(m => DbFunctions.TruncateTime(m.psr_start_date) == DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => DbFunctions.TruncateTime(m.psr_end_date) ==DbFunctions.TruncateTime(End_date))
                                                 .Where(m => m.psr_shift == Shift_ID)
                                                 .Any();
        }
        public void Create(physician_shift_rate entity)
        {
            _unitOfWork.PhysicianRateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(physician_shift_rate entity)
        {
            _unitOfWork.PhysicianRateRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
