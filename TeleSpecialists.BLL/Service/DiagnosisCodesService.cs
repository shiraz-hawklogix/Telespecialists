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
        static List<Icd10BillingCodes> listIcd10BillingCodes;
        static List<Icd10BillingCodeKeys> listIcd10BillingCodeKeys;


        public bool GetAllDiagnosisCodes()
        {
            listIcd10BillingCodes = new List<Icd10BillingCodes>();
            listIcd10BillingCodeKeys = new List<Icd10BillingCodeKeys>();
            Icd10BillingCodes ojIcd10BillingCodes;
            Icd10BillingCodeKeys objIcd10BillingCodeKeys;
            var result = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.is_active == true).ToList();
            foreach (var item in result)
            {
                ojIcd10BillingCodes = new Icd10BillingCodes();
                ojIcd10BillingCodes.code_id = item.code_id;
                ojIcd10BillingCodes.diag_cat_parent_id = item.diag_cat_parent_id;
                ojIcd10BillingCodes.icd_code = item.icd_code;
                ojIcd10BillingCodes.icd_code_description = item.icd_code_description;
                ojIcd10BillingCodes.icd_code_impression = item.icd_code_impression;
                ojIcd10BillingCodes.icd_code_title = item.icd_code_title;
                ojIcd10BillingCodes.sort_order = item.sort_order;

                listIcd10BillingCodes.Add(ojIcd10BillingCodes);
            }

            var codeKeys = _unitOfWork.Icd10CodesCalRepository.Query().Where(x => x.cod_is_active == true).ToList();
            foreach (var item in codeKeys)
            {
                objIcd10BillingCodeKeys = new Icd10BillingCodeKeys();
                objIcd10BillingCodeKeys.Id = item.Id;
                objIcd10BillingCodeKeys.cod_parent_id = item.cod_parent_id;
                objIcd10BillingCodeKeys.cod_name = item.cod_name;
                objIcd10BillingCodeKeys.cod_sort_order = item.cod_sort_order;
                objIcd10BillingCodeKeys.cod_class_name = item.cod_class_name;
                objIcd10BillingCodeKeys.cod_linked_id = item.cod_linked_id;
                listIcd10BillingCodeKeys.Add(objIcd10BillingCodeKeys);
            }
            
            return true;
        }
        public Icd10Codes SearchDiagnosisCodes(string Name, bool isImpressionChecked)
        {
            Name = Name.ToLower();
            Icd10Codes objIcd = new Icd10Codes();

            DiagnosisCodesViewModel obj;
            List<DiagnosisCodesViewModel> _list = new List<DiagnosisCodesViewModel>();

            Icd10CodeCalculator objIcd10cal = new Icd10CodeCalculator();
            List<Icd10CodeChilds> _listIcd10Childs = new List<Icd10CodeChilds>();
            Icd10CodeChilds objIcd10Child;

            #region commented
            //if (isImpressionChecked)
            //{
            //    var diagnosisCodes = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id != null && Name.Contains(x.icd_code_impression)).OrderBy(x => x.sort_order).ToList();
            //    foreach (var _code in diagnosisCodes)
            //    {
            //        obj = new DiagnosisCodesViewModel();
            //        obj.Id = _code.code_id;
            //        obj.icd_code = _code.icd_code;
            //        obj.title = _code.icd_code + " - " + _code.icd_code_title;
            //        _list.Add(obj);
            //    }
            //}
            //else
            //{                
            //var categoryList = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id == null && x.icd_code_title.Contains(Name)).OrderBy(x => x.sort_order).ToList();
            #endregion

            var sp_value_diag = new SqlParameter("value", SqlDbType.VarChar) { Value = Name.ToString().Trim() };
            var sp_table_name_diag = new SqlParameter("@table_name", SqlDbType.VarChar) { Value = "" };
            var categoryList = _unitOfWork.ExecuteStoreProcedure<int>("sp_get_search_parent_id @value,@table_name", sp_value_diag, sp_table_name_diag).FirstOrDefault();

            string ids = "";
            if (categoryList > 0)
            {
                var codeList = listIcd10BillingCodes.Where(x => x.diag_cat_parent_id == categoryList).OrderBy(x => x.sort_order).ToList();
                //var codeList = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id == categoryList).OrderBy(x => x.sort_order).ToList();
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

            var otherList = listIcd10BillingCodes.Where(x => x.diag_cat_parent_id != null && !ids.Contains(x.code_id.ToString()) && (x.icd_code_title.ToLower().Contains(Name) || x.icd_code.Contains(Name))).OrderBy(x => x.sort_order).ToList();
            //var otherList = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.diag_cat_parent_id != null && !ids.Contains(x.code_id.ToString()) && (x.icd_code_title.Contains(Name) || x.icd_code.Contains(Name))).OrderBy(x => x.sort_order).ToList();
            foreach (var otherCode in otherList)
            {
                obj = new DiagnosisCodesViewModel();
                obj.Id = otherCode.code_id;
                obj.icd_code = otherCode.icd_code;
                obj.title = otherCode.icd_code + " - " + otherCode.icd_code_title;
                _list.Add(obj);
            }

            //  }

            //var icd10cal = _unitOfWork.Icd10CodesCalRepository.Query().Where(x => x.cod_parent_id == null && x.cod_name.Contains(Name.Trim())).FirstOrDefault();
            var sp_value = new SqlParameter("value", SqlDbType.VarChar) { Value = Name.ToString().Trim() };
            var sp_table_name = new SqlParameter("table_name", SqlDbType.VarChar) { Value = "Billing_Cal" };
            var icd10cal = _unitOfWork.ExecuteStoreProcedure<int>("sp_get_search_parent_id @value,@table_name", sp_value, sp_table_name).FirstOrDefault();

            if (icd10cal > 0)
            {
                //var icd10Calresult = _unitOfWork.Icd10CodesCalRepository.Query().Where(x => x.cod_parent_id == icd10cal).FirstOrDefault();
                var icd10Calresult = listIcd10BillingCodeKeys.Where(x => x.cod_parent_id == icd10cal).FirstOrDefault();

                if (icd10Calresult != null)
                {
                    objIcd10cal.Id = icd10Calresult.Id;
                    objIcd10cal.name = icd10Calresult.cod_name;
                    objIcd10cal.class_name = icd10Calresult.cod_class_name;

                    //var icd10CalChilds = _unitOfWork.Icd10CodesCalRepository.Query().Where(x => x.cod_parent_id == icd10Calresult.Id).ToList();
                    var icd10CalChilds = listIcd10BillingCodeKeys.Where(x => x.cod_parent_id == icd10Calresult.Id).ToList();
                    foreach (var item in icd10CalChilds)
                    {
                        objIcd10Child = new Icd10CodeChilds();
                        objIcd10Child.Id = item.Id;
                        objIcd10Child.name = item.cod_name;
                        objIcd10Child.class_name = item.cod_class_name;
                        objIcd10Child.sort_order = item.cod_sort_order;
                        _listIcd10Childs.Add(objIcd10Child);
                    }

                    objIcd10cal._icd10CodeChilds = _listIcd10Childs;

                }
            }

            objIcd._DiagnosisCodesViewModel = _list;
            objIcd._Icd10CodeCalculator = objIcd10cal;

            return objIcd;

        }

        public List<DiagnosisCodesViewModel> SearchRecentDiagnosisCodes(string UserId)
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

        public Icd10CodeCalculator searchChildIcd10CalCodes(string Id, string Name, string code_id)
        {
            Icd10CodeCalculator objIcd10cal = new Icd10CodeCalculator();
            List<Icd10CodeChilds> _listIcd10Childs = new List<Icd10CodeChilds>();
            Icd10CodeChilds objIcd10Child;

            var result = _unitOfWork.DiagnosisCodesRepoistory.Query().Where(x => x.icd_code_impression == code_id.Trim()).FirstOrDefault();

            if (result == null)
            {
                var sp_value = new SqlParameter("value", SqlDbType.VarChar) { Value = Name.ToString().Trim() };
                var sp_table_name = new SqlParameter("table_name", SqlDbType.VarChar) { Value = "Billing_Cal" };
                var icd10cal = _unitOfWork.ExecuteStoreProcedure<int>("sp_get_search_parent_id @value,@table_name", sp_value, sp_table_name).FirstOrDefault();
                if (icd10cal > 0)
                {
                    var sp_value_P = new SqlParameter("value", SqlDbType.VarChar) { Value = icd10cal };
                    var sp_table_P = new SqlParameter("table_name", SqlDbType.VarChar) { Value = "Billing_Cal_Child" };
                    var sp_linked_code_id_P = new SqlParameter("linked_code_id", SqlDbType.VarChar) { Value = Id };
                    var icd10Calresult = _unitOfWork.ExecuteStoreProcedure<Icd10CodeCalParent>("sp_get_search_parent_id @value,@table_name,@linked_code_id", sp_value_P, sp_table_P, sp_linked_code_id_P).FirstOrDefault();

                    if (icd10Calresult != null)
                    {
                        objIcd10cal.Id = icd10Calresult.Id;
                        objIcd10cal.name = icd10Calresult.cod_name;
                        objIcd10cal.class_name = icd10Calresult.cod_class_name;

                        var sp_value_Child = new SqlParameter("value", SqlDbType.VarChar) { Value = objIcd10cal.Id };
                        var sp_table_Child = new SqlParameter("table_name", SqlDbType.VarChar) { Value = "Billing_Cal_Child" };
                        var sp_linked_code_id = new SqlParameter("linked_code_id", SqlDbType.VarChar) { Value = Id };
                        var icd10CalChilds = _unitOfWork.ExecuteStoreProcedure<Icd10CodeCalParent>("sp_get_search_parent_id @value,@table_name,@linked_code_id", sp_value_Child, sp_table_Child, sp_linked_code_id).ToList();

                        foreach (var item in icd10CalChilds)
                        {
                            objIcd10Child = new Icd10CodeChilds();
                            objIcd10Child.Id = item.Id;
                            objIcd10Child.name = item.cod_name;
                            objIcd10Child.class_name = item.cod_class_name;
                            objIcd10Child.sort_order = item.cod_sort_order;
                            _listIcd10Childs.Add(objIcd10Child);
                        }

                        objIcd10cal._icd10CodeChilds = _listIcd10Childs;

                    }
                }
            }
            else
            {
                objIcd10cal.code_name = result.icd_code + " - " + result.icd_code_title;
            }
            return objIcd10cal;
        }

        public List<Icd10SearchKeys> getIcd10SearchKeys()
        {
            List<Icd10SearchKeys> _list = _unitOfWork.ExecuteStoreProcedure<Icd10SearchKeys>("sp_get_icd10_search_keys").ToList();
            return _list;
        }
    }
}
