using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class PhysicianStatusService : BaseService
    {
        public IQueryable<physician_status> GetAll()
        {
            return _unitOfWork.PhysicianStatusRepository.Query()
                                                 .Where(m => m.phs_is_active)                                                 
                                                 .OrderBy(m => m.phs_sort_order);
        }
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = _unitOfWork.PhysicianStatusRepository.Query()
                                                  .Select(m => new {
                                                      m.phs_key,
                                                      m.phs_name,
                                                      m.phs_description,
                                                      m.phs_is_active,
                                                      m.phs_is_default,
                                                      m.phs_sort_order,
                                                      m.phs_assignment_priority,
                                                      ShowDelete = m.AspNetUsers.Any() ? false : true
                                                  })
                                                 .OrderBy(m => m.phs_sort_order);
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public physician_status GetDetails(int id)
        {
            var model = _unitOfWork.PhysicianStatusRepository.Query()
                                   .FirstOrDefault(m => m.phs_key == id);
            return model;
        }
        public physician_status GetDefault()
        {
            var model = _unitOfWork.PhysicianStatusRepository
                                   .Query()
                                   .Where(m => m.phs_is_active && m.phs_is_default)
                                   .FirstOrDefault();
            return model;
        }
        public void Create(physician_status entity)
        {
            #region Removing IsDefault check from existing records in case of IsDefault true
            if (entity.phs_is_default)
            {
                var removeDefaultCheck = _unitOfWork.PhysicianStatusRepository
                                                    .Query()
                                                    .Where(m => m.phs_is_default)
                                                    .ToList();

                removeDefaultCheck.ForEach(m => {
                    m.phs_is_default = false;
                });

            }
            #endregion

            #region Finding Sort Order Value
            var caseTypes = _unitOfWork.PhysicianStatusRepository.Query();
            int sortOrder = caseTypes.Count() > 0 ? caseTypes.Max(m => m.phs_sort_order) : 0;

            entity.phs_sort_order = (sortOrder + 1);
            #endregion

            _unitOfWork.PhysicianStatusRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public bool IsTypeAlreadyExists(physician_status entity)
        {
            return _unitOfWork.PhysicianStatusRepository.Query()
                                                 .Where(m => m.phs_name.ToLower().Trim() == entity.phs_name.ToLower().Trim())
                                                 .Where(m => m.phs_key != entity.phs_key)
                                                 .Any();
        }
        public bool Delete(physician_status entity)
        {
            if (!entity.AspNetUsers.Any())
            {
                _unitOfWork.PhysicianStatusRepository.Delete(entity.phs_key);
                _unitOfWork.Save();
                _unitOfWork.Commit();
                return true;
            }
            return false;
        }
        public bool IsColorCodeAlreadyExists(physician_status entity)
        {
            return _unitOfWork.PhysicianStatusRepository.Query()
                                                 .Where(m => m.phs_color_code.ToLower().Trim() == entity.phs_color_code.ToLower().Trim())
                                                 .Where(m => m.phs_key != entity.phs_key)
                                                 .Any();
        }
        public void Edit(physician_status entity)
        {
            #region Removing IsDefault check from existing records in case of IsDefault true
            if (entity.phs_is_default)
            {
                var removeDefaultCheck = _unitOfWork.PhysicianStatusRepository
                                                    .Query()
                                                    .Where(m => m.phs_is_default)
                                                    .Where(m => m.phs_key != entity.phs_key)
                                                    .ToList();

                removeDefaultCheck.ForEach(m => {
                    m.phs_is_default = false;
                });

            }
            #endregion

            _unitOfWork.PhysicianStatusRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public physician_status_snooze_option GetStatusSnoozeOption(int pso_key)
        {
            return _unitOfWork.PhysicianStatusSnoozeOptionRepository.Query().FirstOrDefault(m => m.pso_key == pso_key);
        }

        public void SaveStatusSnoozeOptions(int phs_key, List<physician_status_snooze_option> list)
        {
            string query = "DELETE FROM physician_status_snooze_option where pso_phs_key = " + phs_key + Environment.NewLine;

            foreach (var item in list)
            {
                var subQuery = $@"
INSERT INTO physician_status_snooze_option
           ([pso_message]
           ,[pso_snooze_time]
           ,[pso_is_active]
           ,[pso_created_date]
           ,[pso_created_by]
           ,[pso_created_by_name]          
           ,[pso_phs_key])
     VALUES
           ('{item.pso_message}'
           ,'{item.pso_snooze_time}'
           ,1
           ,'{DateTime.Now.ToEST()}'
           ,'{item.pso_created_by}'
           ,'{item.pso_created_by_name}'
           ,{phs_key})
                        ";

                query += Environment.NewLine + subQuery;

             
            }

            this._unitOfWork.ExecuteSqlCommand(query);

        }
    }
}
