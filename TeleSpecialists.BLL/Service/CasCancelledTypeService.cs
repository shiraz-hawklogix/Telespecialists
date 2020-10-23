using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CasCancelledTypeService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.CaseCancelledRepository.Query()
                               orderby m.cct_name
                               select new
                               {
                                   m.cct_key,
                                   m.cct_name
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
           // return _unitOfWork.CaseCancelledRepository.Query().ToList();
                
        }
        public IQueryable<case_cancelled_type> GetAllRecords()
        {

            return _unitOfWork.CaseCancelledRepository.Query();

        }
        public case_cancelled_type GetDetails(int id)
        {
            var model = _unitOfWork.CaseCancelledRepository.Query()
                                   .FirstOrDefault(m => m.cct_key == id);
            return model;
        }
        public void Create(case_cancelled_type entity)
        {
            _unitOfWork.CaseCancelledRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(case_cancelled_type entity)
        {
            _unitOfWork.CaseCancelledRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(case_cancelled_type entity)
        {
            try
            {
                _unitOfWork.CaseCancelledRepository.Delete(entity.cct_key);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}
