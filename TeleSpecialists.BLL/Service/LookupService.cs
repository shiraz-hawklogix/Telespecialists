using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class LookupService : BaseService
    {
        #region Added by Ahmad

        public IQueryable<facility> GetAllFacilityByState(string phoneNumber,IQueryable<int?> state)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_is_active && state.Contains(f.fac_stt_key));
        }
        public IQueryable<facility> GetAllFacilityBySystem(string phoneNumber, IQueryable<int?> system)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_is_active && system.Contains(f.fac_ucd_key_system));
        }
        public IQueryable<facility> GetAllFacilityByRegion(string phoneNumber, IQueryable<int?> region)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_is_active && region.Contains(f.fac_ucd_region_key));
        }
        public IQueryable<facility> GetAllFacilityByQPS(string phoneNumber, IQueryable<string> qps)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_is_active && qps.Contains(f.qps_number));
        }

        #endregion

        public IQueryable<facility> GetAllFacility(string phoneNumber)
        {
            return GetFacilities(phoneNumber).Where(f=>f.fac_is_active);
        }

        public IQueryable<facility> GetAll(string phoneNumber)
        {
            return GetFacilities(phoneNumber);
        }
        public IQueryable<facility> GetAllActnNonActFacility(string phoneNumber)


		public IQueryable<facility> GetAllActnNonActFacility(string phoneNumber)

        {
            return GetFacilities(phoneNumber);
        }
     
        public IQueryable<facility> GetAllLiveFacility(string phoneNumber)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_go_live);
        }
        public IQueryable<facility> GetAllLiveTeleStrokeFacility(string phoneNumber)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_go_live && f.facility_contract.fct_service_calc.Contains(ContractServiceTypes.TeleStroke.ToString()));
        }
        public IQueryable<facility> GetStrokeFacilitiesForOthercasetypes(string phoneNumber)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_go_live);
        }
        public IQueryable<facility> GetFacilityAll(string phoneNumber)
        {
            return GetFacilities(phoneNumber);
        }
        public IQueryable<facility> GetAllLiveTeleNeuroFacility(string phoneNumber)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_go_live && f.facility_contract.fct_service_calc.Contains(ContractServiceTypes.TeleNeuro.ToString()));
        }

        private IQueryable<facility> GetFacilities(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                var result = _unitOfWork.ContactRepository.Query()
                                                    .Where(m => m.cnt_is_active && m.cnt_is_deleted == false)
                                                    .Where(m => m.cnt_mobile_phone == phoneNumber || m.cnt_primary_phone == phoneNumber)
                                                    //.Where(m => m.facility.fac_is_active)
                                                    .Select(m => m.facility)
                                                    .Distinct()
                                                    .OrderBy(m => m.fac_name);
                // if the calling contact is found in facility then return filter facility list
                if (result.Any())
                {
                    return result;
                }
            }
            // else return all facilities
            return _unitOfWork.FacilityRepository.Query()
                                                // .Where(m => m.fac_is_active)
                                                 .OrderBy(m => m.fac_name);
        }
        public IQueryable<facility> GetAllPACFacility(string phoneNumber)
        {
            return GetFacilities(phoneNumber).Where(f => f.fac_is_pac && f.fac_is_active == true && f.fac_go_live == true);
        }
        public IQueryable<facility> GetAllSleepFacility(string phoneNumber)
        {
            var query = from m in GetAllFacility(phoneNumber)
                        join r in _unitOfWork.FacilityContractServiceRepository.Query() on m.fac_key equals r.fcs_fct_key
                        where m.fac_go_live && r.fcs_srv_key == 335
                        select m;
            return query;

        }
        public List<SelectListItem> GetUserByRole(List<string> roleIDs)
        {
            var users = from u in _unitOfWork.ApplicationUsers
                        join r in _unitOfWork.ApplicationUserRoles
                        on u.Id equals r.UserId into userRole
                        from role in userRole.DefaultIfEmpty()
                        where u.IsDeleted == false
                        orderby u.LastName, u.FirstName
                        select new
                        {
                            Id = u.Id,
                            Name = u.FirstName + " " + u.LastName,
                            RoleId = role.RoleId,
                        };

            if (roleIDs != null)
            {
                if (roleIDs.Count > 0)
                {
                    if (roleIDs[0] != null && !roleIDs[0].Trim().Equals(string.Empty) && roleIDs[0] != "null")
                    {
                        users = users.Where(c => roleIDs.Contains(c.RoleId));
                    }
                }
            }
            var model = users.Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.Name
            }).ToList();
            return model;
        }

        public List<SelectListItem> GetLicensingSpecialist()
        {
            var GetData = _unitOfWork.PhysicianLicenseRepository.Query()
                                   .Select(x => new { x.phl_assigned_to_id, x.phl_assigned_to_name })
                                   .Distinct().ToList();
            GetData.RemoveAll(item => item.phl_assigned_to_id == null && item.phl_assigned_to_name == null);
            var model = GetData.Select(x => new SelectListItem { Text = x.phl_assigned_to_name, Value = x.phl_assigned_to_id.ToString() }).Prepend(new SelectListItem() { Text = "-- Select --", Value = "0" }).ToList();
            return model;
        }
        public List<SelectListItem> GetCredentialingSpecialist()
        {
            var GetData = _unitOfWork.FacilityPhysicianRepository.Query()
                                   .Select(x => new { x.fap_credential_specialist })
                                   .Distinct().ToList();
            GetData.RemoveAll(item => item.fap_credential_specialist == null);
            var model = GetData.Select(x => new SelectListItem { Text = x.fap_credential_specialist, Value = x.fap_credential_specialist }).Prepend(new SelectListItem() { Text = "-- Select --", Value = "0" }).ToList();
            return model;
        }

    }
}
