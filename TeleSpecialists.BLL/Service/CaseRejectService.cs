using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class CaseRejectService : BaseService
    {
        public List<case_rejection_reason> GetDropdownReasons()
        {
            var caseRejectlist = from m in _unitOfWork.CaseRejectRepository.Query()
                                 where m.crr_parent_key == null
                                 orderby m.crr_reason
                                 select m;

            return caseRejectlist.ToList();

        }
        public List<case_rejection_reason> GetRejectionReasonsForDispatch()
        {
            var caseRejectlist = from m in _unitOfWork.CaseRejectRepository.Query()
                                 orderby m.crr_reason
                                 select m;

            return caseRejectlist.ToList();

        }
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseRejectlist = from m in _unitOfWork.CaseRejectRepository.Query()
                                 orderby m.crr_reason
                                 select m;
                                 //select new
                                 //{
                                 //    m.crr_key,
                                 //    m.crr_reason,
                                 //    m.crr_is_main,
                                 //    m.crr_parent_key
                                 //};
            return caseRejectlist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            // return _unitOfWork.CaseCancelledRepository.Query().ToList();

        }
        public IQueryable<case_rejection_reason> GetAllRecords()
        {

            return _unitOfWork.CaseRejectRepository.Query();

        }
        public case_rejection_reason GetDetails(int id)
        {
            var model = _unitOfWork.CaseRejectRepository.Query()
                                   .FirstOrDefault(m => m.crr_key == id);
            return model;
        }
        public void Create(case_rejection_reason entity)
        {
            _unitOfWork.CaseRejectRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(case_rejection_reason entity)
        {
            _unitOfWork.CaseRejectRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(case_rejection_reason entity)
        {
            try
            {
                _unitOfWork.CaseRejectRepository.Delete(entity.crr_key);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<AspNetUser> GetAllUsers()
        {
            var users = _unitOfWork.SqlQuery<AspNetUser>("Exec usp_get_aspnetusers ").AsQueryable();

            return users.ToList();
        }
    }
}
