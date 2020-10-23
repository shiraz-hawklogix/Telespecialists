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
    public class PhysicianPercentageRateService : BaseService
    {

        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.PhysicianPercentageRateRepository.Query()
                               join n in _unitOfWork.ApplicationUsers on m.ppr_phy_key equals n.Id into physicians
                               orderby m.ppr_key descending
                               select new
                               {
                                   m.ppr_key,
                                   m.ppr_phy_key,
                                   m.ppr_shift_name,
                                   m.ppr_start_date,
                                   m.ppr_end_date,
                                   name = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
                                   percentage = m.ppr_percentage + "%"
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public DataSourceResult GetAllIncentive(DataSourceRequest request, string phy_key)
        {
            var caseTypelist = from m in _unitOfWork.PhysicianPercentageRateRepository.Query()
                               join n in _unitOfWork.ApplicationUsers on m.ppr_phy_key equals n.Id into physicians
                               where m.ppr_phy_key == phy_key
                               orderby m.ppr_key
                               select new
                               {
                                   m.ppr_key,
                                   m.ppr_phy_key,
                                   m.ppr_shift_name,
                                   m.ppr_start_date,
                                   m.ppr_end_date,
                                   name = physicians.FirstOrDefault().FirstName + " " + physicians.FirstOrDefault().LastName,
                                   percentage = m.ppr_percentage + "%"
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public physician_percentage_rate GetDetails(int id)
        {
            var model = _unitOfWork.PhysicianPercentageRateRepository.Query()
                                   .FirstOrDefault(m => m.ppr_key == id);
            return model;
        }

        public physician_percentage_rate GetDetailsByKey(string id, DateTime dt, int shiftid)
        {
            var model = _unitOfWork.PhysicianPercentageRateRepository.Query()
                                   .FirstOrDefault(m => m.ppr_phy_key == id &&  m.ppr_shift_id == shiftid && (DbFunctions.TruncateTime(m.ppr_start_date) <= DbFunctions.TruncateTime(dt) && DbFunctions.TruncateTime(m.ppr_end_date) >= DbFunctions.TruncateTime(dt)));
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
            return _unitOfWork.PhysicianPercentageRateRepository.Query()
                                                 .Where(m => m.ppr_phy_key == Phy_key)
                                                 .Where(m => DbFunctions.TruncateTime(m.ppr_start_date) == DbFunctions.TruncateTime(Start_date))
                                                 .Where(m => DbFunctions.TruncateTime(m.ppr_end_date) == DbFunctions.TruncateTime(End_date))
                                                 .Where(m => m.ppr_shift_id == Shift_ID)
                                                 .Any();
        }
        public void Create(physician_percentage_rate entity)
        {
            _unitOfWork.PhysicianPercentageRateRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(physician_percentage_rate entity)
        {
            _unitOfWork.PhysicianPercentageRateRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
