using System.Linq;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;
using Kendo.DynamicLinq;

namespace TeleSpecialists.BLL.Service
{
    public class UCLService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var uclTypeslist = _unitOfWork.UCL_UCDRepository.Query().ApplyFilters(request)
                                                  .Select(m => new
                                                  {
                                                      m.ucd_key,
                                                      m.ucd_title,
                                                      m.ucd_description,
                                                      m.ucd_sort_order,
                                                      m.ucd_unique_id,
                                                      m.ucd_is_default,
                                                      m.ucd_is_active,
                                                      m.ucd_ucl_key,
                                                      m.ucd_is_deleted,
                                                      m.ucd_is_locked,
                                                      m.ucl.ucl_title,
                                                      ShowDelete = true // m.cases.Any() ? false : true
                                                  })
                                                 .OrderBy(m => m.ucd_sort_order);
            return uclTypeslist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public ucl_data GetDetails(int id)
        {
            return _unitOfWork.UCL_UCDRepository.Query().FirstOrDefault(m => m.ucd_key == id);
        }
        public ucl_data GetDetails(string name, UclTypes type)
        {
            var model = GetUclData(type)
                                     .Where(m => m.ucd_is_active && m.ucd_title.ToLower() == name.ToLower())
                                     .FirstOrDefault();
            return model;
        }
        public ucl_data GetDefault(UclTypes type)
        {
            var model = GetUclData(type)
                                   .Where(m => m.ucd_is_active && m.ucd_is_default)
                                   .FirstOrDefault();
            return model;
        }
        public ucl GetParent(int id)
        {
            return _unitOfWork.UCLRepository.Find(id);
        }
        public bool IsAlreadyExists(ucl_data entity)
        {
            return _unitOfWork.UCL_UCDRepository.Query()
                                                 .Where(m => m.ucd_title.ToLower().Trim() == entity.ucd_title.ToLower().Trim())
                                                 .Where(m => m.ucd_ucl_key == entity.ucd_ucl_key)
                                                 .Where(m => m.ucd_key != entity.ucd_key)
                                                 .Any();
        }
        public void Create(ucl_data entity)
        {
            #region Removing IsDefault check from existing records in case of IsDefault true
            if (entity.ucd_is_default)
            {
                var removeDefaultCheck = _unitOfWork.UCL_UCDRepository
                                                    .Query()
                                                    .Where(m => m.ucd_ucl_key == entity.ucd_ucl_key)
                                                    .Where(m => m.ucd_is_default)
                                                    .ToList();

                removeDefaultCheck.ForEach(m =>
                {
                    m.ucd_is_default = false;
                });

            }
            #endregion

            #region Finding Sort Order Value
            var ucdTypes = _unitOfWork.UCL_UCDRepository.Query().Where(x => x.ucd_ucl_key == entity.ucd_ucl_key);
            int sortOrder = 1;
            if (ucdTypes.Count() > 0)
                sortOrder =  ucdTypes.Max(x => x.ucd_sort_order);

            entity.ucd_sort_order = (sortOrder + 1);
            #endregion

            _unitOfWork.UCL_UCDRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(ucl_data entity)
        {
            #region Removing IsDefault check from existing records in case of IsDefault true

            if (entity.ucd_is_default)
            {
                var removeDefaultCheck = _unitOfWork.UCL_UCDRepository
                                                    .Query()
                                                    .Where(m => m.ucd_is_default)
                                                    .Where(m => m.ucd_key != entity.ucd_key)
                                                    .Where(m => m.ucd_ucl_key == entity.ucd_ucl_key)
                                                    .ToList();

                removeDefaultCheck.ForEach(m =>
                {
                    m.ucd_is_default = false;
                });

            }
            #endregion

            _unitOfWork.UCL_UCDRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(ucl_data entity)
        {

            bool CanDelete = true;
            UclTypes type = (UclTypes)entity.ucd_ucl_key;

            #region Validations for Delete

            // checking case data
            if (type == UclTypes.CaseStatus)
                CanDelete = _unitOfWork.CaseRepository.Query().Any(m => m.cas_cst_key == entity.ucd_key) == false;

            if (type == UclTypes.CaseType)
                CanDelete = _unitOfWork.CaseRepository.Query().Any(m => m.cas_ctp_key == entity.ucd_key) == false;

            if (type == UclTypes.BillingCode)
                CanDelete = _unitOfWork.CaseRepository.Query().Any(m => m.cas_billing_bic_key == entity.ucd_key) == false;

            if (type == UclTypes.LoginDelay)
                CanDelete = _unitOfWork.CaseRepository.Query().Any(m => m.cas_billing_lod_key == entity.ucd_key) == false;

            // checking facility data
            if (type == UclTypes.EMR)
                CanDelete = _unitOfWork.FacilityRepository.Query().Any(m => m.fac_cst_key == entity.ucd_key) == false;

            if (type == UclTypes.FacilityType)
                CanDelete = _unitOfWork.FacilityRepository.Query().Any(m => m.fac_fct_key == entity.ucd_key) == false;

            if (type == UclTypes.StrokeDesignation)
                CanDelete = _unitOfWork.FacilityRepository.Query().Any(m => m.fac_sct_key == entity.ucd_key) == false;

            if (type == UclTypes.State)
            {
                CanDelete = _unitOfWork.FacilityRepository.Query().Any(m => m.fac_stt_key == entity.ucd_key) == false;
                if (CanDelete)
                    CanDelete =  _unitOfWork.PhysicianLicenseRepository.Query().Any(m => m.phl_license_state == entity.ucd_key) == false;
            }                

            // checking data in facility contract

            if (type == UclTypes.ServiceType)
                CanDelete = _unitOfWork.FacilityContractServiceRepository.Query().Any(m => m.fcs_srv_key == entity.ucd_key) == false;

            if (type == UclTypes.CoverageType)
                CanDelete = _unitOfWork.FacilityContractRepository.Query().Any(m => m.fct_cvr_key == entity.ucd_key) == false;

            
            // checking data in entity notes

            if (type == UclTypes.NoteType)
                CanDelete = _unitOfWork.EntityNoteRepository.Query().Any(m => m.etn_ntt_key == entity.ucd_key) == false;

            #endregion

            if (CanDelete)
            {
                _unitOfWork.UCL_UCDRepository.Delete(entity.ucd_key);
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
            return CanDelete;
        }
    }
}
