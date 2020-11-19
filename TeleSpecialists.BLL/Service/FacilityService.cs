using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace TeleSpecialists.BLL.Service
{
    public class FacilityService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.FacilityRepository.Query()
                               join p in _unitOfWork.FacilityContractRepository.Query() on m.fac_key equals p.fct_key into facilites
                               from p in facilites.DefaultIfEmpty()
                               join n in GetUclData(UclTypes.State) on m.fac_stt_key equals n.ucd_key into FacilityStates                              
                               from state in FacilityStates.DefaultIfEmpty()
                               orderby m.fac_key descending
                               select new
                               {
                                   m.fac_key,
                                   m.fac_name,
                                   timezone = m.fac_timezone,                                   
                                   m.fac_address_line1,
                                   m.fac_address_line2,
                                   m.fac_city,
                                   fac_state = state != null ? state.ucd_title : "",
                                   m.fac_zip,
                                   m.fac_is_active,
                                   m.fac_go_live,
                                   serviceType=p.fct_service_calc
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public facility GetDetails(Guid id)
        {
            var model = _unitOfWork.FacilityRepository.Query()
                                   .FirstOrDefault(m => m.fac_key == id);
            return model;
        }


        public IEnumerable<cwh_data> GeCWHtDetails(DateTime FromMonth, DateTime ToMonth)
        {
            var model = _unitOfWork.cwhRepository.Query()
                                   .Where(x => x.cwh_date >= FromMonth && x.cwh_date <= ToMonth).OrderBy(x => x.cwh_fac_name).ToList();
            return model;
        }
        public void DeletePreviousRecords(IEnumerable<cwh_data> lsttodelete)
        {
            foreach (var item in lsttodelete)
            {
                _unitOfWork.cwhRepository.DeleteRange(_unitOfWork.cwhRepository.Query().Where(c => c.cwh_key == item.cwh_key));
            }

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void UpdateTableCWHData(cwh_data entities)
        {
            _unitOfWork.cwhRepository.Insert(entities);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public string GetFacilityName(string id)
        {
            var values = Guid.Parse(id);
            var model = _unitOfWork.FacilityRepository.Query().Where(x => x.fac_key == values).Select(s => s.fac_name).FirstOrDefault();
            return model;
        }
        public bool IsAlreadyExists(facility entity)
        {
            return _unitOfWork.FacilityRepository.Query()
                                                 .Where(m => m.fac_name.ToLower().Trim() == entity.fac_name.ToLower().Trim())
                                                 .Where(m => m.fac_key != entity.fac_key)
                                                 .Any();
        }
        public void Create(facility entity)
        {       
            _unitOfWork.FacilityRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(facility entity)
        {   
            _unitOfWork.FacilityRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        
        public AspNetUserRole GetUserRole(string phy_key)
        {
            var record = _unitOfWork.ApplicationUserRoles.Where(x => x.UserId == phy_key).FirstOrDefault();
            return record;
        }
        private List<string> rolesList()
        {
            List<string> list = new List<string>();
            list.Add("PAC Navigator");
            list.Add("Facility Navigator");
            return list;
        }
        public bool IsHomeHealth(string value)
        {
             var status = rolesList().Contains(value);
            return status;
        }
        public string GetFacilityNameForreport(Guid id)
        {
            var model = _unitOfWork.FacilityRepository.Query().Where(x => x.fac_key == id).Select(s => s.fac_name).FirstOrDefault();
            return model;
        }
        public string GetPhycisionName(string id)
        {
            var model = _unitOfWork.UserRepository.Query().Where(x => x.Id == id).Select(m => m.LastName + " " + m.FirstName).FirstOrDefault();
            return model;
        }

        public long GetPhycisionId(string id)
        {
            var model = _unitOfWork.UserRepository.Query().Where(x => x.Id == id).Select(m => m.PhysicianId).FirstOrDefault();
            return model;
        }
        public int ChKName(string ParameterName, string cas_fac_key_arrays)
        {
            string isValid = "";
            int isValidcount = 0;
            if (cas_fac_key_arrays == "")
            {
                isValidcount = _unitOfWork.OnBoardedRepository.Query().Where(p => p.ParameterName == ParameterName).Select(a => a.ParameterName).Count();
            }
            else
            {
                foreach (var item in cas_fac_key_arrays.Split(','))
                {
                    var s = _unitOfWork.OnBoardedRepository.Query().Where(p => p.ParameterName == ParameterName && p.Facility_Id == item.ToString()).Select(a => a.ParameterName).Count();
                    isValid += s + ",";
                }
                var wws = isValid.TrimEnd(',');
                var ssnew = wws.Split(',').ToArray();
                foreach (var digit in ssnew)
                {
                    isValidcount = isValidcount + Convert.ToInt32(digit);
                }
            }
            return isValidcount;
        }
        #region GetUserByRole Added by Ahmad
        public List<SelectListItem> GetUserByRole(List<string> roleIDs, string selected)
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
                Text = x.Name,
                Selected = x.Id == selected
            }).Prepend(new SelectListItem() { Text = "-- Select QPS --", Value = "", Selected = selected == null }).ToList();
            return model;
        }
        #endregion
    }
}
