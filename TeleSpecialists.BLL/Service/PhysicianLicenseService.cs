using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Helpers;
using System.Data.Entity;
using System;
using TeleSpecialists.BLL.Extensions;

namespace TeleSpecialists.BLL.Service
{
    public class PhysicianLicenseService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var query = from m in _unitOfWork.PhysicianLicenseRepository.Query()
                        join n in GetUclData(UclTypes.State) on m.phl_license_state equals n.ucd_key into FacilityStates
                        from state in FacilityStates.DefaultIfEmpty()
                        where m.phl_is_active
                        select new
                        {
                            m.phl_key,
                            m.phl_license_number,
                            phl_issued_date = DBHelper.FormatDateTime(m.phl_issued_date, false),
                            phl_expired_date = DBHelper.FormatDateTime(m.phl_expired_date, false),
                            m.phl_is_active,
                            m.phl_user_key,
                            m.phl_license_state,
                            phl_state = state != null ? state.ucd_title : "",
                            m.phl_created_date
                        };
                        
            foreach (var filter in request.Filter.Filters)
            {
                if (filter.Value != null)
                {
                    if (!string.IsNullOrEmpty(filter.Value?.ToString()))
                    {
                        query = query.WhereEquals(filter.Field, filter.Value);
                    }
                }
            }
            return query.OrderByDescending(m => m.phl_state).ToDataSourceResult(request.Take, request.Skip, request.Sort, null);
        }
        public physician_license GetDetails(Guid id)
        {
            var model = _unitOfWork.PhysicianLicenseRepository.Query()
                                   .FirstOrDefault(m => m.phl_key == id);
            return model;
        }
        public bool IsAlreadyExists(physician_license physician_license)
        {
            var end_date = DateTime.Now.AddYears(100);
            var phl_end_date = physician_license.phl_expired_date.HasValue ? physician_license.phl_expired_date : physician_license.phl_issued_date.AddMonths(1);
            var query = _unitOfWork.PhysicianLicenseRepository.Query()
                                                                 .Where(m => m.phl_user_key == physician_license.phl_user_key)
                                                                 .Where(m => m.phl_is_active)
                                                                 .Where(m => m.phl_license_state == physician_license.phl_license_state)
                                                                 .Where(m => m.phl_key != physician_license.phl_key)
                                                                 .Where(m => DbFunctions.TruncateTime(phl_end_date) >= DbFunctions.TruncateTime(m.phl_issued_date))
                                                                 .Where(m => DbFunctions.TruncateTime(m.phl_expired_date.HasValue ? m.phl_expired_date.Value : end_date) >= DbFunctions.TruncateTime(physician_license.phl_issued_date));

            return query.Any();
        }
        public void Create(physician_license entity)
        {
            _unitOfWork.PhysicianLicenseRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(physician_license entity)
        {
            _unitOfWork.PhysicianLicenseRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
