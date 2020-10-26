
using System.Linq;
using TeleSpecialists.BLL.Model;
using System.Collections.Generic;
using System.Text;

namespace TeleSpecialists.BLL.Service
{
    public class TokenService:BaseService
    {
        public List<token> GetAll(string token)
        {
            var model = _unitOfWork.TokenRepository.Query().Where(x => x.tok_phy_key == token).ToList();
            return model;
        }

        public token GetDetailById(string phy_key, string phy_token_key,string PCName)
        {
            var model = _unitOfWork.TokenRepository.Query().Where(x => x.tok_phy_key == phy_key && x.tok_phy_token == phy_token_key && x.tok_machine_name == PCName).FirstOrDefault();
            return model;
        }

        public void Create(token entity)
        {
            _unitOfWork.TokenRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public void DeleteRange(IEnumerable<token> id, bool commit = true)
        {
            _unitOfWork.TokenRepository.DeleteRange(id);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
        }

        public void Delete(int id, bool commit = true)
        {
            _unitOfWork.TokenRepository.Delete(id);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
        }

        public List<token> deleteToken(string UserId, string MachineName)
        {
            var model = _unitOfWork.TokenRepository.Query().Where(x => x.tok_phy_key == UserId && x.tok_machine_name == MachineName).ToList();
            return model;
        }

        public List<token> GetAllLoggedInUserToken(string phy_key)
        {
            var model = (from t in _unitOfWork.TokenRepository.Query()
                        join u in _unitOfWork.userVerificationRepoistory.Query() on t.tok_machine_name equals u.MachineName
                        where u.UserId == phy_key 
                        && u.IsLoggedIn == true
                        select t).ToList();

            return model;
        }
    }
}
