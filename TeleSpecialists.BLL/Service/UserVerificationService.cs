
using System.Collections.Generic;
using System.Linq;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.BLL.Service
{
    public class UserVerificationService : BaseService
    {

        public List<UserLoginVerifyViewModel> userVerifications(string Id)
        {
            List<UserLoginVerifyViewModel> _list = new List<UserLoginVerifyViewModel>();
            UserLoginVerifyViewModel obj;
            var result = _unitOfWork.userVerificationRepoistory.Query().Where(x => x.UserId == Id).ToList();
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    obj = new UserLoginVerifyViewModel();
                    obj.Id = item.Id;
                    obj.UserId = item.UserId;
                    obj.IsLoggedIn = item.IsLoggedIn;
                    obj.IsTwoFactRememberChecked = item.IsTwoFactRememberChecked;
                    obj.TwoFactVerifyExpiryDate = item.TwoFactVerifyExpiryDate;
                    obj.MachineName = item.MachineName;
                    obj.MachineIpAddress = item.MachineIpAddress;
                    _list.Add(obj);
                }
            }
            return _list;
        }

        public void addVerificationEntry(user_login_verify model)
        {
            var result = _unitOfWork.userVerificationRepoistory.Query().Where(x => x.UserId == model.UserId.ToString() && x.MachineName == model.MachineName).FirstOrDefault();
            if(result != null)
            {
                result.IsLoggedIn = true;
                if (!string.IsNullOrEmpty(model.MachineIpAddress))
                {
                    result.MachineIpAddress = model.MachineIpAddress;
                }
               
                if(model.TwoFactVerifyExpiryDate != null)
                {
                    result.TwoFactVerifyExpiryDate = model.TwoFactVerifyExpiryDate;
                }
                if(model.IsTwoFactRememberChecked != null)
                {
                    result.IsTwoFactRememberChecked = model.IsTwoFactRememberChecked;
                }
                
                _unitOfWork.userVerificationRepoistory.Update(result);
            }
            else
            {
                model.IsLoggedIn = true;
                _unitOfWork.userVerificationRepoistory.Insert(model);
              
            }
            _unitOfWork.Save();
        }

        public void userSignOut(string UserId,string MachineName,string isLogout)
        {
            if (!string.IsNullOrEmpty(isLogout) && isLogout.ToLower().ToString() == "true")
            {
               
            }
            else
            {
                var result = _unitOfWork.userVerificationRepoistory.Query().Where(x => x.UserId == UserId && x.MachineName == MachineName).FirstOrDefault();
                if (result != null)
                {
                    result.IsLoggedIn = false;
                    _unitOfWork.userVerificationRepoistory.Update(result);
                }
            }
            _unitOfWork.Save();
        }

        public void SignOutAllUsers(string UserId)
        {
            var result = _unitOfWork.userVerificationRepoistory.Query().Where(x => x.UserId == UserId && x.IsLoggedIn == true).ToList();
            if (result.Count() > 0)
            {
                foreach (var item in result)
                {
                    item.IsLoggedIn = false;
                    _unitOfWork.userVerificationRepoistory.Update(item);
                }

            }
            _unitOfWork.Save();
        }

        public void LogOutOtherUserLoggedIn(string UserId, string MachineName)
        {
            var result = _unitOfWork.userVerificationRepoistory.Query().Where(x => x.UserId == UserId && x.MachineName != MachineName).ToList();
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    item.IsLoggedIn = false;
                    _unitOfWork.userVerificationRepoistory.Update(item);
                }
            }
            _unitOfWork.Save();
        }

        public bool CheckUserLogOut(string UserId, string MachineName)
        {
            var isLogout = false;
            var result = _unitOfWork.userVerificationRepoistory.Query().Where(x => x.UserId == UserId && x.MachineName == MachineName && x.IsLoggedIn == false).FirstOrDefault();
            if (result != null)
            {
                isLogout = true;
            }
            return isLogout;
        }

        public List<string> LogoutUserList(string UserId, string MachineName)
        {
            List<string> _list = new List<string>();
            var result = _unitOfWork.userVerificationRepoistory.Query().Where(x => x.UserId == UserId && x.IsLoggedIn == true).ToList();
            foreach (var item in result)
            {
                string obj = item.UserId + '_' + item.MachineName;
                _list.Add(obj);
            }
            return _list;
        }
    }
}
