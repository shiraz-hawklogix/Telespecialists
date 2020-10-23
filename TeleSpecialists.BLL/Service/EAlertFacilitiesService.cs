using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using TeleSpecialists.BLL.ViewModels.FacilityUser;

namespace TeleSpecialists.BLL.Service
{
    public class EAlertFacilitiesService : BaseService
    {
        public ealert_user_facility GetDetails(int id)
        {
            var model = _unitOfWork.EAlertFacilitiesRepository.Query()
                                   .FirstOrDefault(m => m.efa_key == id);
            return model;
        }
        public DataSourceResult GetAllFacilities(DataSourceRequest request)
        {
            var eAlertFacilities = from m in _unitOfWork.EAlertFacilitiesRepository.Query() //.ApplyFilters(request)
                                   where m.efa_is_active == true
                                   orderby m.efa_key descending
                                   select new
                                   {
                                       m.efa_key,
                                       m.efa_fac_key,
                                       m.efa_is_active,
                                       m.efa_is_default,
                                       m.facility.fac_name,
                                       m.efa_user_key
                                   };


            return eAlertFacilities.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }     
        public IQueryable<GetFacilityViewModel> GetAllAssignedFacilities(string userKey)
        {
            var facilities = from m in _unitOfWork.EAlertFacilitiesRepository.Query() //.ApplyFilters(request)
                             where m.efa_is_active == true
                             where m.efa_user_key == userKey
                             select new GetFacilityViewModel
                             {
                                 Id = m.efa_key,
                                 Facility = m.efa_fac_key,
                                 FacilityName = m.facility.fac_name,
                                 UserKey = m.efa_user_key,
                                 UserFullName = m.AspNetUser.FirstName + " "+ m.AspNetUser.LastName
                             };
            return facilities;
        }
        // Code For Sleep
        public IQueryable<GetFacilityViewModel> GetAllAssignedFacilitiesSleep(string userKey)
        {
            var facilities = from m in _unitOfWork.EAlertFacilitiesRepository.Query() //.ApplyFilters(request)
                             where m.efa_is_active == true && m.facility.fac_is_pac
                             where m.efa_user_key == userKey
                             select new GetFacilityViewModel
                             {
                                 Id = m.efa_key,
                                 Facility = m.efa_fac_key,
                                 FacilityName = m.facility.fac_name,
                                 UserKey = m.efa_user_key,
                                 UserFullName = m.AspNetUser.FirstName + " " + m.AspNetUser.LastName
                             };
            return facilities;
        }
        // Sleep Code end
        public bool IsAlreadyExists(string userKey, Guid facilityId, int id = 0)
        {
            if (id == 0)
            {
                return _unitOfWork.EAlertFacilitiesRepository.Query()
                                                .Where(m => m.efa_user_key == userKey)
                                                .Where(m => m.efa_fac_key == facilityId)
                                                .Any();
            }
            else
            {
                return _unitOfWork.EAlertFacilitiesRepository.Query()
                                                .Where(m => m.efa_user_key == userKey)
                                                .Where(m => m.efa_fac_key == facilityId)
                                                .Where(m => m.efa_key != id)
                                                .Any();
            }

        }
        public void Edit(ealert_user_facility entity)
        {
            _unitOfWork.EAlertFacilitiesRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(ealert_user_facility entity)
        {
            _unitOfWork.EAlertFacilitiesRepository.Delete(entity.efa_key);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        public void SaveChanges()
        {
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void AssginFacilities(string userKey, List<ealert_user_facility> model)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                _unitOfWork.EAlertFacilitiesRepository.DeleteRange(_unitOfWork.EAlertFacilitiesRepository.Query().Where(m => m.efa_user_key == userKey));
                _unitOfWork.EAlertFacilitiesRepository.InsertRange(model);
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
