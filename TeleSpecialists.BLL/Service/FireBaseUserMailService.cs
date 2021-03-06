﻿using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.BLL.Service
{
   public class FireBaseUserMailService : BaseService
    {
        public List<FireBaseData> GetAllUser()
        {
            var ApiUserRole = UserRoles.TeleCareApi.ToDescription();
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
                            u.IsStrokeAlert
                        };
           var user = users.ToList();
            List<FireBaseData> list = new List<FireBaseData>();
            FireBaseData obj;
            foreach (var item in user)
            {
                obj = new FireBaseData();
                obj.email = item.Email;
                obj.name = item.FirstName + ' ' + item.LastName;
                obj.password = item.Email;
                obj.teleid = 1;
                obj.UserName = item.UserName;
                obj.user_id = item.Id;
                list.Add(obj);
            }
            return list;
        }
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.FireBaseUserMailRepository.Query()
                               orderby m.fre_firstname
                               select new
                               {
                                   m.fre_key,
                                   m.fre_userId,
                                   m.fre_firstname,
                                   m.fre_lastname,
                                   m.fre_firebase_uid,
                                   m.fre_firebase_email,
                                   m.fre_email,
                                   m.fre_profileimg
                               };
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
            // return _unitOfWork.CaseCancelledRepository.Query().ToList();

        }
        public IQueryable<firebase_usersemail> GetAllRecords()
        {

            return _unitOfWork.FireBaseUserMailRepository.Query();

        }
        public List<FireBaseData> GetAllSpecificUser(HashSet<string> list)
        {
            var record = (from m in GetAllRecords()
                          where m.fre_firebase_uid != null && list.Contains(m.fre_userId)
                          select new FireBaseData
                          {
                              user_id = m.fre_userId,
                              name = m.fre_firstname,
                              email = m.fre_firebase_email,
                              UserName = m.fre_firebase_uid,
                              ImgPath = m.fre_profileimg

                         }).ToList();
            return record;
        }
        public List<FireBaseData> GetAllSpecificUserForAuto(HashSet<string> list)
        {
            var record = (from m in GetAllRecords()
                          where list.Contains(m.fre_userId)
                          select new FireBaseData
                          {
                              user_id = m.fre_userId,
                              name = m.fre_firstname,
                              email = m.fre_firebase_email,
                              UserName = m.fre_firebase_uid,
                              ImgPath = m.fre_profileimg

                          }).ToList();
            return record;
        }
        public firebase_usersemail GetDetails(string id)
        {
            var model = _unitOfWork.FireBaseUserMailRepository.Query()
                                   .FirstOrDefault(m => m.fre_userId == id);
            return model;
        }
        public void Create(firebase_usersemail entity)
        {
            _unitOfWork.FireBaseUserMailRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(firebase_usersemail entity)
        {
            _unitOfWork.FireBaseUserMailRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(firebase_usersemail entity)
        {
            try
            {
                _unitOfWork.FireBaseUserMailRepository.Delete(entity.fre_key);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
