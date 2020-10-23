using System;
using System.Collections.Generic;
using System.Linq;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.FacilityQuestionnaire;

namespace TeleSpecialists.BLL.Service
{
    public class FacilityQuestionnaireService : BaseService
    {
        public facility_questionnaire_pre_live GetDetails(Guid id)
        {
            var model = _unitOfWork.FacilityQuestionnairePreLiveRepository.Query()
                                   .FirstOrDefault(m => m.fqp_key == id);
            return model;
        }

        private bool IsQuestionnaireExists(Guid id)
        {
            return _unitOfWork.FacilityQuestionnairePreLiveRepository.Query().Any(m => m.fqp_key == id);
        }

        public List<contactVM> GetContacts(Guid id)
        {
            return  (from m in _unitOfWork.FacilityQuestionnaireContactDesignationRepository.Query()
                       join n in _unitOfWork.FacilityQuestionnaireContactRespository.Query().Where(k => k.fqc_fqp_key == id) on m.fqd_key equals n.fqc_fqd_key into designationContacts
                       from contact in designationContacts.DefaultIfEmpty()
                       select new contactVM {
                            fqd_key = m.fqd_key,
                            fqd_name = m.fqd_name,
                            fqc_fqd_key = contact != null ? contact.fqc_fqd_key : 0,
                            fqc_email = contact != null ? contact.fqc_email : "",
                            fqc_key = contact != null ? contact.fqc_key : 0,
                            fqc_name = contact != null ? contact.fqc_name : "",
                            fqc_phone = contact != null ? contact.fqc_phone : ""
                       }).ToList();            
        }

        public void SaveChanges(PreLiveVM model, string loggedInUserId, string loggedInUserFullName)
        {
            try
            {
                var currentDate = DateTime.Now.ToEST();

                #region Questionnaire Table
                if (!IsQuestionnaireExists(model.FacilityKey))
                {
                    model.questionnaireModel.fqp_created_by = loggedInUserId;
                    model.questionnaireModel.fqp_created_by_name = loggedInUserFullName;
                    model.questionnaireModel.fqp_created_date = currentDate;
                    model.questionnaireModel.fqp_key = model.FacilityKey;
                    _unitOfWork.FacilityQuestionnairePreLiveRepository.Insert(model.questionnaireModel);
                }
                else
                {
                    model.questionnaireModel.fqp_modified_by = loggedInUserId;
                    model.questionnaireModel.fqp_modified_by_name = loggedInUserFullName;
                    model.questionnaireModel.fqp_modified_date = currentDate;
                    _unitOfWork.FacilityQuestionnairePreLiveRepository.Update(model.questionnaireModel);
                }
                _unitOfWork.Save();

                #endregion

                #region Contacts Table
                var contacts = model.contactList?.Where(m => !string.IsNullOrEmpty(m.fqc_name));
                var existingList = _unitOfWork.FacilityQuestionnaireContactRespository.Query()
                                              .Where(m => m.fqc_fqp_key == model.FacilityKey)
                                              .ToList();
                if (existingList.Count() > 0)
                {
                    _unitOfWork.FacilityQuestionnaireContactRespository.DeleteRange(existingList);
                    _unitOfWork.Save();
                }

                var contactsToInsert = contacts.Select(m => new facility_questionnaire_contact
                {
                    fqc_fqp_key = model.FacilityKey,
                    fqc_fqd_key = m.fqd_key,
                    fqc_created_by = loggedInUserId,
                    fqc_created_date = currentDate,
                    fqc_created_by_name = loggedInUserFullName,
                    fqc_email = m.fqc_email,
                    fqc_name = m.fqc_name,
                    fqc_phone = m.fqc_phone,
                });
                _unitOfWork.FacilityQuestionnaireContactRespository.InsertRange(contactsToInsert);
                _unitOfWork.Save();
                _unitOfWork.Commit();

                #endregion
                
            }
            catch(Exception ex)
            {
                _unitOfWork.Rollback();
                throw ex;
            }
        }
       
    }
}
