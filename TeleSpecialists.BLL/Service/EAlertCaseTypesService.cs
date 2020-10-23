using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using TeleSpecialists.BLL.ViewModels.FacilityUser;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;

namespace TeleSpecialists.BLL.Service
{
    public class EAlertCaseTypesService : BaseService
    {
        public ealert_user_case_type GetDetails(int id)
        {
            var model = _unitOfWork.EAlertCaseTypesRepository.Query()
                                   .FirstOrDefault(m => m.ect_key == id);
            return model;
        }
        public DataSourceResult GetAllCaseTypes(DataSourceRequest request)
        {
            var eAlertFacilities = from m in _unitOfWork.EAlertCaseTypesRepository.Query() //.ApplyFilters(request)
                                   where m.ect_is_active == true
                                   orderby m.ect_key descending
                                   select new
                                   {
                                       m.ect_key,
                                       m.ect_case_type_key,
                                       m.ect_is_active,
                                       m.ect_is_default,
                                       m.ucl_data.ucd_title,
                                       m.ucl_data.ucd_ucl_key,
                                       m.ect_user_key
                                   };
            return eAlertFacilities.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }      
        public IQueryable<GetCaeTypeViewModel> GetAllAssignedCaseTypes(string userKey)
        {
            var caseTypes = from m in _unitOfWork.EAlertCaseTypesRepository.Query() //.ApplyFilters(request)
                             where m.ect_is_active == true
                             where m.ect_user_key == userKey
                             select new GetCaeTypeViewModel
                             {
                                 Id =  m.ect_key,
                                 CaseTypeKey = m.ect_case_type_key,
                                 CaseUCLKey = m.ucl_data.ucd_ucl_key,
                                 IsActive = m.ect_is_active,
                                 IsDefault = m.ect_is_default,
                                 CaseTypeName =  m.ucl_data.ucd_title,
                                 UserFullName = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName,
                                 UserKey = m.ect_user_key
                             };
            return caseTypes;
        }
        #region Husnain Block
        public List<GetCaeTypeViewModel> GetAllHomeHealthType(string userKey, string name)
        {
            var enumList = Enum.GetValues(typeof(PacCaseType)).Cast<PacCaseType>().Select(m => new { key = (int)m, title = m.ToDescription() });
            List<GetCaeTypeViewModel> list = new List<GetCaeTypeViewModel>();
            GetCaeTypeViewModel obj;
            foreach (var item in enumList)
            {
                obj = new GetCaeTypeViewModel();
                //obj.Id = 59;
                obj.CaseTypeKey = item.key;
                obj.CaseTypeName = item.title;
                obj.UserFullName = name;
                obj.UserKey = userKey;
                obj.IsActive = true;
                obj.IsDefault = false;
                list.Add(obj);
            }
            return list;
        }
        #endregion
        public bool IsAlreadyExists(string userKey, int caseTypeId, int id = 0)
        {
            if (id == 0)
            {
                return _unitOfWork.EAlertCaseTypesRepository.Query()
                                                .Where(m => m.ect_user_key == userKey)
                                                .Where(m => m.ect_case_type_key == caseTypeId)
                                                .Any();
            }
            else
            {
                return _unitOfWork.EAlertCaseTypesRepository.Query()
                                                .Where(m => m.ect_user_key == userKey)
                                                .Where(m => m.ect_case_type_key == caseTypeId)
                                                .Where(m => m.ect_key != id)
                                                .Any();
            }

        }       
        public void Create(ealert_user_case_type entity)
        {
            _unitOfWork.EAlertCaseTypesRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(ealert_user_case_type entity)
        {
            _unitOfWork.EAlertCaseTypesRepository.Delete(entity.ect_key);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        public void Edit(ealert_user_case_type entity)
        {
            _unitOfWork.EAlertCaseTypesRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void SaveChanges()
        {
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void AssginCaseTypes(string userKey, List<ealert_user_case_type> model)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.EAlertCaseTypesRepository.DeleteRange(_unitOfWork.EAlertCaseTypesRepository.Query().Where(m => m.ect_user_key == userKey));
                _unitOfWork.EAlertCaseTypesRepository.InsertRange(model);
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                _unitOfWork.Rollback();
            }
        }

     
    }
}
