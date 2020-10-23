using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Helpers;
using System;
using TeleSpecialists.BLL.Extensions;
using System.Collections.Generic;

namespace TeleSpecialists.BLL.Service
{
    public class FacilityContractService : BaseService
    {
        public facility_contract GetDetails(Guid id)
        {
            var model = _unitOfWork.FacilityContractRepository.Query()
                                   .FirstOrDefault(m => m.fct_key == id);
            return model;
        }
        public IQueryable<facility_contract_service> GetFacilityContractServices(Guid fct_key)
        {
            return _unitOfWork.FacilityContractServiceRepository.Query()
                                                                    .Where(m => m.fcs_fct_key == fct_key);                                                                 
            
        }
        public void RemoveServices(List<facility_contract_service> servicesList)
        {
            servicesList.ForEach(m => {
                _unitOfWork.FacilityContractServiceRepository.Delete(m.fcs_key);
            });
        }
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var facilityContracts = from m in _unitOfWork.FacilityContractRepository.Query().ApplyFilters(request)                        
                        join coverageType in GetUclData(UclTypes.CoverageType) on m.fct_cvr_key equals coverageType.ucd_key into FacilityCoverageTypes                        
                        from coverage_type in FacilityCoverageTypes.DefaultIfEmpty()

                        where m.fct_is_active == true
                        orderby m.fct_key descending
                        select new
                        {
                            m.fct_key,
                            fct_start_date = m.fct_start_date.HasValue ? DBHelper.FormatDateTime(m.fct_start_date.Value, false) : "",
                            fct_end_date = m.fct_end_date.HasValue ? DBHelper.FormatDateTime(m.fct_end_date.Value, false) : "",                            
                            fct_srv_key =  m.fct_service_calc,
                            fct_cvr_key = coverage_type != null ? coverage_type.ucd_title : "",
                            m.fct_is_active

                        };
            return facilityContracts.ToDataSourceResult(request.Take, request.Skip, request.Sort, null);
        }
        public bool Exists(Guid id)
        {
            return _unitOfWork.FacilityContractRepository.Query()
                                   .Any(m => m.fct_key == id);
        }
        public void Create(facility_contract entity)
        {
            _unitOfWork.FacilityContractRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(facility_contract entity)
        {
            _unitOfWork.FacilityContractRepository.Delete(entity.fct_key);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        public void Edit(facility_contract entity)
        {
            _unitOfWork.FacilityContractRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void SaveChanges()
        {
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
