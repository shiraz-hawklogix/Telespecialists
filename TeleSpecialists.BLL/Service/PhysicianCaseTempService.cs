using System;
using System.Linq;
using System.Text;
using TeleSpecialists.BLL.Extensions;

namespace TeleSpecialists.BLL.Service
{
    public class PhysicianCaseTempService : BaseService
    {
        public void SavePhysicianCaseTemp(string pct_guid, string phy_key, int pct_cst_key, int pct_ctp_key, string saved_by)
        {
            var query = new StringBuilder();
            query.AppendSqlParam("@pct_guid", pct_guid);
            query.Append(",@pct_cst_key=" + pct_cst_key);
            query.Append(",@pct_ctp_key=" + pct_ctp_key);
            query.AppendSqlParam(",@pct_phy_key", phy_key);
            query.AppendSqlParam(",@pct_saved_by", saved_by);

            _unitOfWork.SqlQuery<object>("Exec usp_physician_case_temp_save " + query.ToString());

        }

        public void DeleteByPhysicianKey(string phy_key)
        {
            var list = _unitOfWork.Physician_Case_TempRepository
                                  .Query()
                                  .Where(m => m.pct_phy_key == phy_key)
                                  .ToList();

            _unitOfWork.Physician_Case_TempRepository.DeleteRange(list);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public void DeleteById(Guid pct_key)
        {
            var obj = _unitOfWork.Physician_Case_TempRepository
                                  .Query()
                                  .Where(m => m.pct_key == pct_key)
                                  .FirstOrDefault();

            if (obj != null)
            {
                _unitOfWork.Physician_Case_TempRepository.Delete(obj.pct_key);
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
        }
    }
}
