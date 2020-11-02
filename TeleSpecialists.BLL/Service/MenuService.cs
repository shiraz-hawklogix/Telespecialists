using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels;

namespace TeleSpecialists.BLL.Service
{
    public class MenuService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var _result = _unitOfWork.MenuRepository.Query()
                            .Where(m => m.com_status == true)
                            .OrderBy(m => m.com_module_name);
            return _result.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public IQueryable<component> GetAllParentMenues()
        {
            return _unitOfWork.MenuRepository.Query()
                    .Where(x => !x.com_parentcomponentid.HasValue && x.com_status ==true);
        }
        public IQueryable<component> GetAllMenues()
        {
            return _unitOfWork.MenuRepository.Query()
                    .Where(x => x.com_status == true);
        }
        public component GetDetails(int id)
        {
            return _unitOfWork.MenuRepository.Query().FirstOrDefault(m => m.com_key == id);
        }
        public bool IsAlreadyExists(component entity)
        {
            return _unitOfWork.MenuRepository.Query()
                                                 .Where(m => m.com_module_name.ToLower().Trim() == entity.com_module_name.ToLower().Trim() && m.com_page_url.ToLower().Trim() == entity.com_page_url.ToLower().Trim())
                                                 .Any();
        }

        public void Create(component entity)
        {  
            _unitOfWork.MenuRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(component entity)
        {
            _unitOfWork.MenuRepository.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool Delete(component entity)
        {

            bool CanDelete = true;
          

            if (CanDelete)
            {
                _unitOfWork.MenuRepository.Delete(entity.com_key);
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
            return CanDelete;
        }

      
        public List<sp_getMenuAccess_Result> getMenuAccess(string Id)
        {
            var query = _unitOfWork.SqlQuery<sp_getMenuAccess_Result>(string.Format("Exec sp_getMenuAccess @Id = '{0}'", Id)).ToList();
            return query;
        }
        //public IEnumerable<DataClassifications> DataClassificationTree(string ID)
        //    {
        //    DataClassifications _dataClassifications = new DataClassifications();
        //    List<DataClassifications> _dataClassificationsList = new List<DataClassifications>();
        //    Classificationstate _state = new Classificationstate();
        //      //var ParentClassifications = _unitOfWork.MenuRepository.Query()
        //        //                            .Where(x => x.com_status == true)
        //        //                            .OrderBy(x => x.com_sortorder).ToList();
        //        var ParentClassifications = _unitOfWork.SqlQuery<sp_getMenuAccess_Result>(string.Format("Exec sp_getMenuAccess @Id = '{0}'", ID)).ToList();



        //        foreach (var item in ParentClassifications)
        //        {
        //            _dataClassifications = new DataClassifications();
        //            _state = new Classificationstate();

        //            _dataClassifications.id = item.com_key;
        //            _dataClassifications.text = "<span class='fa-folder-opens-icon' id ='span_" + _dataClassifications.id + "'></span> " + item.com_page_title + " <div id='action_2' class='action_tree'>";

        //            _dataClassifications.text = _dataClassifications.text + "</div>";
        //            if (item.com_parentcomponentid != null)
        //            {
        //                _dataClassifications.parent = item.com_parentcomponentid.ToString();
        //            }
        //            else
        //            {
        //                _dataClassifications.parent = "#";
        //            }

        //            _state.opened = true;
        //            _dataClassifications.state = _state;

        //            _dataClassificationsList.Add(_dataClassifications);

        //        }


        //        return _dataClassificationsList;
          
        //}
        public void Edit(int MenuId, bool CheckboxStatus, string RoleId, string userId)
        {
            List<int> parentid = new List<int>();
            parentid.Add(MenuId);
            UpdateNodes(parentid, RoleId, userId, CheckboxStatus);
            var _componentList = _unitOfWork.MenuRepository.Query().Where(x => x.com_parentcomponentid == MenuId).ToList().Select(x =>  x.com_key);
            
            if (_componentList != null)
            {
                UpdateNodes(_componentList, RoleId, userId, CheckboxStatus);
                foreach (var item in _componentList)
                {
                    var _firstLvlComponent = _unitOfWork.MenuRepository.Query().Where(x => x.com_parentcomponentid == item).ToList().Select(x => x.com_key);
                    if (_firstLvlComponent != null)
                    {
                        UpdateNodes(_firstLvlComponent, RoleId, userId, CheckboxStatus);
                        foreach (var item2 in _firstLvlComponent)
                        {
                            var _secondLvlComponent = _unitOfWork.MenuRepository.Query().Where(x => x.com_parentcomponentid == item2).ToList().Select(x => x.com_key);
                            UpdateNodes(_secondLvlComponent, RoleId, userId, CheckboxStatus);
                        }
                    }
                }
            }
        }

        private void UpdateNodes(IEnumerable<int> firstLvlComponent, string roleId, string userId, bool CheckboxStatus)
        {
            foreach (var item in firstLvlComponent)
            {   
                var result = _unitOfWork.MenuAccessRepository.Query().Where(x => x.cac_roleid == roleId && x.cac_com_key == item).FirstOrDefault();
                if (result != null)
                {
                    component_access entity = new component_access();
                    result.cac_roleid = roleId;
                    result.cac_isAllowed = CheckboxStatus;
                    result.cac_com_key = item;
                    result.cac_modifiedby = userId;
                    result.cac_modifiedon = DateTime.Now;
                    _unitOfWork.MenuAccessRepository.Update(result);
                    _unitOfWork.Save();
                    _unitOfWork.Commit();
                }
                else
                {
                    component_access entity = new component_access();
                    entity.cac_roleid = roleId;
                    entity.cac_isAllowed = CheckboxStatus;
                    entity.cac_com_key = item;
                    entity.cac_addeddate = DateTime.Now;
                    entity.cac_addedby = userId;
                   _unitOfWork.MenuAccessRepository.Insert(entity);
                    _unitOfWork.Save();
                    _unitOfWork.Commit();
                }
            }
        }
    }
}
