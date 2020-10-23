using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;

namespace TeleSpecialists.BLL.Service
{
    public class PhysicianStatusLogService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = _unitOfWork.PhysicianStatusLogRepository.Query()
                                                  .Select(m => new {
                                                      m.psl_key,                                                      
                                                      m.psl_status_name,
                                                      m.psl_start_date,
                                                      m.psl_end_date,
                                                      physican = m.AspNetUser.FirstName                                                     
                                                  })
                                                 .OrderByDescending(m => m.psl_key);
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }      
        public physician_status_log GetDetails(int id)
        {
            var model = _unitOfWork.PhysicianStatusLogRepository.Query()
                                   .FirstOrDefault(m => m.psl_key == id);
            return model;
        }   
        public void Create(physician_status_log entity)
        {
            _unitOfWork.PhysicianStatusLogRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public physician_status_log GetExistingLog(string phy_key)
        {
            return _unitOfWork.PhysicianStatusLogRepository.Query()
                                                 .Where(m => m.psl_user_key == phy_key)
                                                 .Where(m => m.psl_end_date == null)
                                                 .FirstOrDefault();
        }
        public void Edit(physician_status_log entity)
        {
            _unitOfWork.PhysicianStatusLogRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
