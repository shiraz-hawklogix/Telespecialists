using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Extensions;
using System;

namespace TeleSpecialists.BLL.Service
{
    public class ContactService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            //Removing filtering cnt_is_active, As active and InActive both should be displayed in grid but not deleted.

            var cntlist = _unitOfWork.ContactRepository.Query().Where(c => c.cnt_is_deleted == false).ApplyFilters(request);
            request.Filter = null;
            var caseTypelistOrderBy = cntlist.Select(m => new
            {
                m.cnt_key,
                m.cnt_first_name,
                m.cnt_last_name,
                m.cnt_mobile_phone,
                m.cnt_primary_phone,
                m.cnt_is_active,
                m.cnt_email,
                m.cnt_role,
                m.cnt_fac_key,
                m.ucl_data.ucd_title,
                m.cnt_extension
            })
            .OrderBy(m => m.cnt_first_name);
            return caseTypelistOrderBy.ToDataSourceResult(request.Take, request.Skip, request.Sort, null);
        }

        public IQueryable<contact> GetContactsByType(Guid fac_key, string designationType)
        {
            designationType = designationType.ToLower();

            var contacts = from m in _unitOfWork.ContactRepository.Query()
                           join n in _unitOfWork.UCL_UCDRepository.Query() on m.cnt_role_ucd_key equals n.ucd_key
                           where
                           n.ucd_title.ToLower() == designationType
                           && m.cnt_fac_key == fac_key
                           && m.cnt_is_active
                           && !m.cnt_is_deleted
                           select m
            ;
            return contacts;
        }


        public contact GetDetails(int id)
        {
            var model = _unitOfWork.ContactRepository.Query()
                                   .FirstOrDefault(m => m.cnt_key == id);
            return model;
        }
        public void Create(contact entity)
        {
            _unitOfWork.ContactRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(contact entity)
        {
            _unitOfWork.ContactRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public IQueryable<facility> GetAllContactFacilities(string phoneNumber)
        {
            var result = _unitOfWork.ContactRepository.Query()
                                                .Where(m => m.cnt_is_active)
                                                .Where(m => m.cnt_mobile_phone == phoneNumber || m.cnt_primary_phone == phoneNumber)
                                                .Where(m => m.facility.fac_go_live && m.cnt_is_deleted == false)
                                                .Select(m => m.facility)
                                                .Distinct()
                                                .OrderBy(m => m.fac_name);
            return result;
        }
    }
}


