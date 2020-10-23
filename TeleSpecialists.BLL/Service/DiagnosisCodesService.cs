using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.CasePage;

namespace TeleSpecialists.BLL.Service
{
    public class DiagnosisCodesService : BaseService
    {
        public List<DiagnosisCodesViewModel> SearchDiagnosisCodes(string Name, bool isImpressionChecked)
        {
            List<DiagnosisCodesViewModel> _list = new List<DiagnosisCodesViewModel>();
            DiagnosisCodesViewModel obj;
            if (isImpressionChecked)
            {
                var diagnosisCodes = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id != null && Name.Contains(x.icd_code_impression)).OrderBy(x => x.sort_order).ToList();
                foreach (var _code in diagnosisCodes)
                {
                    obj = new DiagnosisCodesViewModel();
                    obj.Id = _code.code_id;
                    obj.icd_code = _code.icd_code;
                    obj.title = _code.icd_code + " - " + _code.icd_code_title;
                    _list.Add(obj);
                }
            }
            else
            {
                var categoryList = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id == null && x.icd_code_title.Contains(Name)).OrderBy(x => x.sort_order).ToList();
                string ids = "";
                foreach (var cat in categoryList)
                {
                    //ids = ids + "," + cat.code_id;
                    //obj = new DiagnosisCodesViewModel();
                    //obj.Id = cat.code_id;
                    //obj.icd_code = cat.icd_code;
                    //obj.title = cat.icd_code + " - " + cat.icd_code_description;
                    //_list.Add(obj);

                    var codeList = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id == cat.code_id).OrderBy(x => x.sort_order).ToList();
                    foreach (var code in codeList)
                    {
                        ids = ids + "," + code.code_id;
                        obj = new DiagnosisCodesViewModel();
                        obj.Id = code.code_id;
                        obj.icd_code = code.icd_code;
                        obj.title = code.icd_code + " - " + code.icd_code_title;
                        _list.Add(obj);
                    }
                }
                var otherList = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id != null && !ids.Contains(x.code_id.ToString()) && (x.icd_code_title.Contains(Name) || x.icd_code.Contains(Name))).OrderBy(x => x.sort_order).ToList();
                foreach (var otherCode in otherList)
                {
                    obj = new DiagnosisCodesViewModel();
                    obj.Id = otherCode.code_id;
                    obj.icd_code = otherCode.icd_code;
                    obj.title = otherCode.icd_code + " - " + otherCode.icd_code_title;
                    _list.Add(obj);
                }

            }

            return _list;

        }

        public List<DiagnosisCodesViewModel> SearchRecentDiagnosisCodes(string UserId )
        {
            List<DiagnosisCodesViewModel> _list = new List<DiagnosisCodesViewModel>();
            DiagnosisCodesViewModel obj;
            string SemiColonSeperatedCode = "";

            var sp_user_id = new SqlParameter("UserId", SqlDbType.VarChar) { Value = UserId };
            List<DiagnosisCodesViewModel> _li_diagCodes = _unitOfWork.ExecuteStoreProcedure<DiagnosisCodesViewModel>("sp_recent_used_diagnosis_code @UserId", sp_user_id).ToList();
            foreach (var item in _li_diagCodes)
            {
                var diagnosisCodes = item.icd_code.Split(';');
                foreach (var code in diagnosisCodes)
                {
                    if (!SemiColonSeperatedCode.Contains(code))
                    {
                        SemiColonSeperatedCode += code + ",";
                        obj = new DiagnosisCodesViewModel();
                        obj.title = code.Trim();
                        _list.Add(obj);
                    }
                }
            }
            return _list;

        }

        public List<DiagnosisCodesViewModel> getBillingDiagnosisCode()
        {

            List<DiagnosisCodesViewModel> _list = new List<DiagnosisCodesViewModel>();
            DiagnosisCodesViewModel obj;

            var diagnosisCodes = _unitOfWork.DiagnosisCodesRepoistory.Query().OrderBy(x => x.code_id).ToList();
            foreach (var _code in diagnosisCodes)
            {
                obj = new DiagnosisCodesViewModel();
                obj.Id = _code.code_id;
                obj.icd_code = _code.icd_code;
                obj.title = _code.icd_code + " - " + _code.icd_code_title;
                _list.Add(obj);
            }

            return _list;
        }
    }
}
