using Kendo.DynamicLinq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class AdminService : BaseService
    {
        public DataSourceResult GetAllUsers(DataSourceRequest request, List<string> roleIDs)
        {
            var ApiUserRole = UserRoles.TeleCareApi.ToDescription();
            // currently the query is based on assumption then only one role can be assigned  to a user. 
            var users = from u in _unitOfWork.ApplicationUsers
                        join r in _unitOfWork.ApplicationUserRoles
                        on u.Id equals r.UserId into userRole
                        from role in userRole.DefaultIfEmpty()
                        where u.IsDeleted == false
                        orderby u.LastName, u.FirstName
                        select new
                        {
                            u.Id,
                            u.FirstName,
                            u.LastName,
                            u.UserName,
                            u.Email,
                            u.IsActive,
                            u.IsDisable,
                            u.EnableFive9,
                            RoleName = role != null ? role.AspNetRole.Name : "",
                            role.RoleId,
                            ShowRemoteLogin = string.IsNullOrEmpty(u.PasswordHash) ? false : true,
                            u.CredentialIndex,
                            IsApiUser = ApiUserRole == (role != null ? role.AspNetRole.Name : ""),
                            u.IsEEG,
                            u.IsStrokeAlert,
                            u.TwoFactorEnabled
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
            return users.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        public HashSet<string> GetAllUsersIds(List<string> roleIDs)
        {
            var ApiUserRole = UserRoles.TeleCareApi.ToDescription();
            // currently the query is based on assumption then only one role can be assigned  to a user. 
            var users = from u in _unitOfWork.ApplicationUsers
                        join r in _unitOfWork.ApplicationUserRoles
                        on u.Id equals r.UserId into userRole
                        from role in userRole.DefaultIfEmpty()
                        where u.IsDeleted == false
                        orderby u.LastName, u.FirstName
                        select new
                        {
                            u.Id,
                            u.FirstName,
                            u.LastName,
                            u.UserName,
                            u.Email,
                            u.IsActive,
                            u.EnableFive9,
                            RoleName = role != null ? role.AspNetRole.Name : "",
                            role.RoleId,
                            ShowRemoteLogin = string.IsNullOrEmpty(u.PasswordHash) ? false : true,
                            u.CredentialIndex,
                            IsApiUser = ApiUserRole == (role != null ? role.AspNetRole.Name : ""),
                            u.IsEEG,
                            u.IsStrokeAlert
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
            HashSet<string> ids = new HashSet<string>(users.Select(s => s.Id.ToString()));
            return ids;
        }

      
        public AspNetRole GetRoleByName(string roleName)
        {
            return _unitOfWork.ApplicationRoles
                                            .Where(m => m.Name.ToLower() == roleName)
                                            .FirstOrDefault();
        }
        public AspNetUser GetUser(string Id)
        {
            return _unitOfWork.ApplicationUsers
                                    .Include(m => m.physician_status)
                                   .FirstOrDefault(m => m.Id == Id);
        }
        public IQueryable<AspNetUser> GetUsersByInitial(string userInitial, string userId = "")
        {
            return _unitOfWork.ApplicationUsers.Where(m => m.UserInitial.ToLower() == userInitial.ToLower())
                                               .Where(m => userId == "" || m.Id != userId);
        }
        public bool ValidateNPINumber(string npiNumber, string userId = "")
        {
            return _unitOfWork.ApplicationUsers.Where(m => string.IsNullOrEmpty(npiNumber) ? false : m.NPINumber.ToLower() == npiNumber.ToLower())
                                               .Where(m => userId == "" || m.Id != userId)
                                               .Where(m=> m.IsDeleted == false)
                                               .Any();
        }
        public IQueryable<AspNetUser> GetAspNetUsers()
        {
            return _unitOfWork.ApplicationUsers;
        }
        public IQueryable<AspNetRole> GetAllRoles()
        {
            return _unitOfWork.ApplicationRoles;
        }
    }
}
