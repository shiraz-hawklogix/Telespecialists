using System;
using TeleSpecialists.BLL.Repository;
using System.Data.Entity;
using System.Linq;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Extensions;
using System.Collections.Generic;
using TeleSpecialists.BLL.ViewModels;
using System.Reflection;

namespace TeleSpecialists.BLL.Service
{
    public class BaseService : IDisposable
    {
        #region Global Variable
        protected readonly UnitOfWork _unitOfWork;
        #endregion

        public BaseService()
        {
            this._unitOfWork = new UnitOfWork();
        }

        public void Save()
        {
            this._unitOfWork.Save();
        }
        public void Commit()
        {
            this._unitOfWork.Commit();
        }
        public IQueryable<facility_physician> GetPhysiciansByFacility(Guid facility_id)
        {

            //var user_list = _unitOfWork.FacilityPhysicianRepository.Query().Where(x => x.fap_fac_key == facility_id).Select(o => o.fap_user_key).ToList();
            //var query = _unitOfWork.UserRepository.Query().Where(x => user_list.Contains(x.Id)).ToList();
            //return query;
            var query = from m in _unitOfWork.FacilityPhysicianRepository.Query()
                           join n in _unitOfWork.UserRepository.Query() on m.fap_user_key equals n.Id
                           where
                           m.fap_fac_key == facility_id
                           select m;
            return query;
        }

        public IQueryable<contact> GetContactsByType(Guid fac_key, string designationType)
        {
            designationType = designationType.ToLower();

            var contacts = from m in _unitOfWork.ContactRepository.Query()
                           join n in _unitOfWork.UCL_UCDRepository.Query() on m.cnt_role_ucd_key equals n.ucd_key
                           where
                           n.ucd_title.ToLower() == designationType
                           && m.cnt_fac_key == fac_key
                           && m.cnt_is_active
                           && !m.cnt_is_deleted
                           select m ;
            return contacts;
        }
        public IQueryable<AspNetUser> GetPhysicians(List<Guid> facKeysList)
        {
            //_unitOfWork.FacilityRepository.Query().Where(m=> facKeysList.Contains(m.fac_key))
            _unitOfWork.FacilityPhysicianRepository.Query()
                                                   .Where(m => facKeysList.Contains(m.fap_fac_key))
                                                   .Select(m => m.AspNetUser);

            return null;
        }
        public IQueryable<AspNetUser> GetPhysicians(string FilterRoleNames = "")
        {
            if (string.IsNullOrEmpty(FilterRoleNames))
            {
                FilterRoleNames = UserRoles.Physician.ToDescription().ToLower() + "," + UserRoles.PartnerPhysician.ToDescription().ToLower();
            }
            //string strPhysician = UserRoles.Physician.ToDescription().ToLower();
            //string strPartnerPhysician = UserRoles.PartnerPhysician.ToDescription().ToLower();
            var roles = FilterRoleNames.ToLower().Split(',').ToArray();

            var physicianRoleIds = _unitOfWork.ApplicationRoles
                                            .Where(m => roles.Contains(m.Name.ToLower()))
                                            .Select(m => m.Id);

            var query = (from m in _unitOfWork.UserRepository.Query()
                         join role in _unitOfWork.UserRoleRepository.Query() on m.Id equals role.UserId
                         where physicianRoleIds.Contains(role.RoleId)
                         select m).Include(k => k.physician_status);

           
            return query;
        }

        public IQueryable<AspNetUser> GetMockPhysicians(string FilterRoleNames = "")
        {
            if (string.IsNullOrEmpty(FilterRoleNames))
            {
                FilterRoleNames = UserRoles.MockPhysician.ToDescription().ToLower();
            }
            var roles = FilterRoleNames.ToLower().Split(',').ToArray();

            var physicianRoleIds = _unitOfWork.ApplicationRoles
                                            .Where(m => roles.Contains(m.Name.ToLower()))
                                            .Select(m => m.Id);

            var query = (from m in _unitOfWork.UserRepository.Query()
                         join role in _unitOfWork.UserRoleRepository.Query() on m.Id equals role.UserId
                         where physicianRoleIds.Contains(role.RoleId)
                         select m).Include(k => k.physician_status);


            return query;
        }

        #region Husnain Code Block
        public IQueryable<AspNetUser> GetPacPhysicians(string FilterRoleNames = "")
        {
            if (string.IsNullOrEmpty(FilterRoleNames))
            {
                FilterRoleNames = UserRoles.AOC.ToDescription().ToLower();
            }
            //string strPhysician = UserRoles.Physician.ToDescription().ToLower();
            //string strPartnerPhysician = UserRoles.PartnerPhysician.ToDescription().ToLower();
            var roles = FilterRoleNames.ToLower().Split(',').ToArray();

            var physicianRoleIds = _unitOfWork.ApplicationRoles
                                            .Where(m => roles.Contains(m.Name.ToLower()))
                                            .Select(m => m.Id);
            

            var query = (from m in _unitOfWork.UserRepository.Query()
                         join role in _unitOfWork.UserRoleRepository.Query() on m.Id equals role.UserId
                         where physicianRoleIds.Contains(role.RoleId)
                         select m).Include(k => k.physician_status);

            
            return query;
        }
        #endregion

        public IQueryable<AspNetUser> GetPhysiciansForFacilityAdmin(string facilityAdminId, string FilterRoleNames = "")
        {
            if (string.IsNullOrEmpty(FilterRoleNames))
            {
                FilterRoleNames = UserRoles.Physician.ToDescription().ToLower() + "," + UserRoles.PartnerPhysician.ToDescription().ToLower();
            }
            //string strPhysician = UserRoles.Physician.ToDescription().ToLower();
            //string strPartnerPhysician = UserRoles.PartnerPhysician.ToDescription().ToLower();
            var roles = FilterRoleNames.ToLower().Split(',').ToArray();

            var physicianRoleIds = _unitOfWork.ApplicationRoles
                                            .Where(m => roles.Contains(m.Name.ToLower()))
                                            .Select(m => m.Id);

            var query = (from m in _unitOfWork.UserRepository.Query()
                         join role in _unitOfWork.UserRoleRepository.Query() on m.Id equals role.UserId
                         join phyFac in _unitOfWork.FacilityPhysicianRepository.Query() on m.Id equals phyFac.fap_user_key
                         join facAdminFacilities in _unitOfWork.EAlertFacilitiesRepository.Query() on phyFac.fap_fac_key equals facAdminFacilities.efa_fac_key
                         where (physicianRoleIds.Contains(role.RoleId) && facAdminFacilities.efa_user_key == facilityAdminId)
                         select m).Include(k => k.physician_status).Distinct();

            return query;
        }
        public IQueryable<ucl_data> GetUclData(UclTypes type)
        {
            int Type = type.ToInt();

            return _unitOfWork.UCL_UCDRepository.Query().Where(m => m.ucd_ucl_key == Type);
        }
        public IQueryable<ucl_data> GetUclDataSleep()
        {
            return _unitOfWork.UCL_UCDRepository.Query().Where(m => m.ucd_ucl_key == 11 || m.ucd_ucl_key == 37);
        }
        public IQueryable<ucl_data> GetUclData(List<int> types)
        {
            //var strType = type.ToString();

            var query = from m in _unitOfWork.UCL_UCDRepository.Query()
                        where
                        types.Contains(m.ucd_ucl_key.Value)
                        select m;
            return query;
        }
        public string GetImage(string id)
        {
            var user = _unitOfWork.AspNetUserDetailRepositorty.Query().Where(x => x.Id == id).FirstOrDefault();
            if (user != null)
            {
                return user.PhotoBase64;
            }
            else
            {
                return "";
            }
        }

        public List<ChangeTrackEntityVMFormatted> GetChangeTrackset(string excludedFields)
        {
            string _name = string.Empty;

            var result = new List<ChangeTrackEntityVM>();
            var KeysToTrack = new List<string>();
            var postedKeys = new List<string>();
            if (System.Web.HttpContext.Current != null)
            {
                var request = System.Web.HttpContext.Current.Request;
                try
                {
                    postedKeys.AddRange(request.Params.AllKeys);
                }
                catch
                {
                    postedKeys.AddRange(request.Unvalidated.Form.AllKeys);
                }
            }

            //if (AdditionalKeysToTrack != null)
            //{
            //    KeysToTrack.AddRange(AdditionalKeysToTrack);
            //}

            postedKeys = postedKeys.Where(k => k != null).ToList();
            if (postedKeys != null)
            {
                postedKeys = postedKeys.Select(m => m.ToLower()).ToList();

                if (postedKeys.Count() > 0)
                {
                    KeysToTrack.AddRange(postedKeys);
                }
                KeysToTrack = KeysToTrack.Select(s => s.Split('.').Last()).ToList();

                _unitOfWork.GetChangeSet()
                 .ForEach(e =>
                 {
                     PropertyInfo[] properties = e.Entity.GetType().GetProperties().Where(m => KeysToTrack.Contains(m.Name.ToLower())).ToArray();

                     if (e.State == EntityState.Modified)
                     {
                         var origional = e.GetDatabaseValues().ToObject();

                         foreach (var p in properties)
                         {
                             var info = p.PropertyType.GetTypeInfo();
                             if (info.Namespace.ToLower().Contains("telespecialists"))
                             {
                                 continue;
                             }

                             ChangeTrackEntityVM changetrack = new ChangeTrackEntityVM();

                             _name = e.Entity.GetType().BaseType.Name;
                             if (_name.ToLower() == "object")
                             {
                                 _name = e.Entity.GetType().Name;//.Split('_').ToList();
                                 //if (list.Count() > 1)
                                 //{
                                 //    list.Remove(list.Last());
                                 //    _name = string.Join("_", list.ToArray());
                                 //}
                             }



                             changetrack.entity = _name;

                             changetrack.field = p.Name;
                             //changetrack.PrimaryKey = e.GetPrimaryKeyValue(db).ToString();
                             changetrack.current = p.GetValue(e.Entity);
                             var previous = origional.GetType().GetProperty(p.Name).GetValue(origional);

                             if (previous != null)
                             {
                                 changetrack.previous = previous;
                             }
                             if (changetrack.current != null && changetrack.previous != null)
                             {
                                 if (!changetrack.current.Equals(changetrack.previous))
                                 {
                                     result.Add(changetrack);
                                 }
                             }
                             else if (changetrack.current == null && changetrack.previous != null)
                             {
                                 result.Add(changetrack);
                             }

                             else if (changetrack.current != null && changetrack.previous == null)
                             {
                                 result.Add(changetrack);
                             }
                         }
                     }
                     else if (e.State == EntityState.Added)
                     {
                         foreach (var p in properties)
                         {
                             var info = p.PropertyType.GetTypeInfo();
                             if (info.Namespace.ToLower().Contains("telespecialists"))
                             {
                                 continue;
                             }

                             ChangeTrackEntityVM changetrack = new ChangeTrackEntityVM();

                             _name = e.Entity.GetType().BaseType.Name;
                             if (_name.ToLower() == "object")
                             {
                                 var list = e.Entity.GetType().Name.Split('_').ToList();
                                 if (list.Count() > 1)
                                 {
                                     list.Remove(list.Last());
                                     _name = string.Join("_", list.ToArray());
                                 }
                             }

                             changetrack.entity = _name;

                             changetrack.field = p.Name;
                             // changetrack.PrimaryKey = e.GetPrimaryKeyValue(db).ToString();
                             changetrack.current = p.GetValue(e.Entity);

                             if (changetrack.current != null)
                             {
                                 result.Add(changetrack);
                             }
                         }
                     }
                 });

            }

            if (!string.IsNullOrEmpty(excludedFields))
                result = result.Where(m => !excludedFields.Split(',').Contains(m.field)).ToList();
            return GetFormattedList(result);
        }

        public List<ChangeTrackEntityVMFormatted> GetFormattedList(List<ChangeTrackEntityVM> list)
        {
            var groupByList = list.GroupBy(m => m.entity).ToList();
            var result = new List<ChangeTrackEntityVMFormatted>();
            foreach (var item in groupByList)
            {
                var childs = list.Where(m => m.entity == item.Key)
                    .Select(m => new ChangeTrackEntityVMFormattedDetail { current = m.current, field = m.field, previous = m.previous })
                    .ToList();
                result.Add(new ChangeTrackEntityVMFormatted { entity = item.Key, changes = childs });
            }

            return result;
        }

        #region Added By HawkLogix

        public IQueryable<AspNetUser> GetNavigators(string FilterRoleNames = "")
        {
            if (string.IsNullOrEmpty(FilterRoleNames))
            {
                //FilterRoleNames = UserRoles.Navigator.ToDescription().ToLower() + "," + UserRoles.FacilityNavigator.ToDescription().ToLower();
                FilterRoleNames = UserRoles.Navigator.ToDescription().ToLower();
            }
            //string strPhysician = UserRoles.Physician.ToDescription().ToLower();
            //string strPartnerPhysician = UserRoles.PartnerPhysician.ToDescription().ToLower();
            var roles = FilterRoleNames.ToLower().Split(',').ToArray();

            var physicianRoleIds = _unitOfWork.ApplicationRoles
                                            .Where(m => roles.Contains(m.Name.ToLower()))
                                            .Select(m => m.Id);

            var query = (from m in _unitOfWork.UserRepository.Query()
                         join role in _unitOfWork.UserRoleRepository.Query() on m.Id equals role.UserId
                         where physicianRoleIds.Contains(role.RoleId)
                         select m).Include(k => k.physician_status);


            return query;
        }


        #endregion

        #region ----- IDisposable -----

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Include objects to disposed
                    _unitOfWork.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
