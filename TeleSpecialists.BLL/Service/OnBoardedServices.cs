using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class OnBoardedServices : BaseService
    {

        public void Create(Onboarded entity)
        {
            _unitOfWork.OnBoardedRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(Onboarded entity)
        {
            _unitOfWork.OnBoardedRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public IEnumerable<Onboarded> GetAllBoardedData(string fac_key)
        {
            var list = _unitOfWork.OnBoardedRepository.Query().Where(x => x.Facility_Id == fac_key).OrderBy(x => x.SortNum).ToList();
            return list;
        }
        public void DeleteOnBoareded(int id)
        {
            _unitOfWork.OnBoardedRepository.Delete(id);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public DataSourceResult GetAll(DataSourceRequest request, string id)
        {
            var caseTypelist = (from m in _unitOfWork.OnBoardedRepository.Query().Where(x => x.Facility_Id == id)
                                select new
                                {
                                    m.SortNum,
                                    m.Onboarded_ID,
                                    m.ParameterName,
                                    m.ParameterName_Info,
                                    m.Facility_Name,
                                    m.Facility_Id,
                                    m.ParameterName_Image,
                                    m.Parameter_Add_Date
                                }).OrderBy(x => x.SortNum);
            
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort,request.Filter);
        }
        public Onboarded GetDetails(int id)
        {
            var model = _unitOfWork.OnBoardedRepository.Query()
                                   .FirstOrDefault(m => m.Onboarded_ID == id);
            return model;
        }
        public IEnumerable<Onboarded> GetDetailsForPOpUP(string id)
        {
            var model = _unitOfWork.OnBoardedRepository.Query().Where(m => m.Facility_Id == id).OrderBy(x => x.SortNum).ToList();
            return model;
        }
        public Onboarded GetDetailForOnboarded(int id)
        {
            var model = _unitOfWork.OnBoardedRepository.Query()
                                   .FirstOrDefault(m => m.Onboarded_ID == id);
            return model;
        }
        public bool Delete(Onboarded entity)
        {
                _unitOfWork.OnBoardedRepository.Delete(entity.Onboarded_ID);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
        }
        public bool DeleteRange(IEnumerable<Onboarded> entities)
        {
            _unitOfWork.OnBoardedRepository.DeleteRange(entities);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        
        public int? GetSortNumMaxByFacility(string id)
        {
            var model =  _unitOfWork.OnBoardedRepository.Query().Where(x=>x.Facility_Id == id).Select(x => x.SortNum).Max();
            return model;
        }
    }
}
