using System.Linq;
using TeleSpecialists.BLL.Model;
using Kendo.DynamicLinq;
using TeleSpecialists.BLL.Helpers;
using TeleSpecialists.BLL.ViewModels.EntityNotes;
using System.Data.Entity;
using System;
using TeleSpecialists.BLL.Extensions;

namespace TeleSpecialists.BLL.Service
{
    public class EntityNoteService : BaseService
    {
        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var caseTypelist = from m in _unitOfWork.EntityNoteRepository.Query()
                               join u in _unitOfWork.ApplicationUsers on m.etn_created_by equals u.Id
                               join note_type in GetUclData(UclTypes.NoteType) on m.etn_ntt_key equals note_type.ucd_key

                               orderby m.etn_key descending
                               where m.etn_is_active 
                               select new {
                                   m.etn_key,
                                   etn_ntt_type = note_type.ucd_title,
                                   m.etn_notes,
                                   etn_first_name = u.FirstName,
                                   etn_last_name = u.LastName,
                                   m.etn_created_date,
                                   m.etn_entity_key,
                                   m.etn_ent_key,
                                   m.etn_display_on_open
                               };

            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public IQueryable<ucl_data> GetAll()
        {
            return _unitOfWork.UCL_UCDRepository.Query()
                                                   .Where(m => m.ucd_ucl_key == (int)UclTypes.NoteType)
                                                   .Where(m => m.ucd_is_active)
                                                   .Where(m => !m.ucd_is_deleted)
                                                   .OrderBy(x => x.ucd_sort_order);
        }
        public IQueryable<entity_note> GetEnityNotes(string entityKey, EntityTypes entityType)
        {
            return _unitOfWork.EntityNoteRepository.Query()
                                                   .Where(m => m.etn_is_active)
                                                   .Where(m=>m.etn_entity_key == entityKey)
                                                   .Where(m=>m.etn_ent_key == (int)entityType)
                                                   .OrderBy(x => x.etn_created_date);
        }

        public IQueryable<SignOutNoteViewModel> GetEnityNotes(DataSourceRequest request, EntityTypes entityType)
        {
            var entityKey = string.Empty;
            var entityKey_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "etn_entity_key");
            if (entityKey_field != null)
            {
                var entityKeyValue = entityKey_field.Value?.ToString();
                if (!string.IsNullOrEmpty(entityKeyValue))
                {
                    entityKey = entityKeyValue;
                }
            }
            var notesdata = (from notes in _unitOfWork.EntityNoteRepository.Query()
                                                             .Where(m => m.etn_is_active)
                                                             .Where(m => m.etn_ent_key == (int)entityType)
                                                             .Where(m => m.etn_entity_key == entityKey)

                             select new SignOutNoteViewModel
                             {
                                 Id = notes.etn_key,
                                 Name = notes.etn_modified_by_name == null ? notes.etn_created_by_name : notes.etn_modified_by_name,
                                 Date = notes.etn_modified_date.HasValue ? DBHelper.FormatDateTime(notes.etn_modified_date.Value, true) : DBHelper.FormatDateTime(notes.etn_created_date, true),
                                 Notes = notes.etn_notes,

                                 /// Added Only for sorting by date.
                                 CreatedOrModified = notes.etn_modified_date.HasValue ? notes.etn_modified_date.Value : notes.etn_created_date,

                                 IsModified = notes.etn_modified_date.HasValue && notes.etn_modified_by != null && notes.etn_modified_by_name != null,
                                 CreatedBy = notes.etn_created_by
                                 

                             }); 

            #region Date Filter
            DateTime? StartDate = null;
            DateTime? EndDate = null;

            var currentDate = DateTime.Now.ToEST(); 
           
            var date_filter_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "date_filter");
            if (date_filter_field != null)
            {
                var date_filter = date_filter_field.Value?.ToString();
                if (!string.IsNullOrEmpty(date_filter))
                {
                    switch (date_filter)
                    {
                        case "Today":
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) == DbFunctions.TruncateTime(currentDate));
                            break;

                        case "Yesterday":
                            StartDate = currentDate.AddDays(-1);
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) == DbFunctions.TruncateTime(StartDate));
                            break;
                        case "Last24Hours":
                            StartDate = currentDate.AddDays(-1);
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate));
                            break;
                        case "Last48Hours":
                            StartDate = currentDate.AddDays(-2);
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate));
                            break;

                        case "LastSevenDays":
                            StartDate = currentDate.AddDays(-7);
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CreatedOrModified) <= DbFunctions.TruncateTime(currentDate));
                            break;

                        case "Last30Days":
                            StartDate = currentDate.AddDays(-30);
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CreatedOrModified) <= DbFunctions.TruncateTime(currentDate));
                            break;

                        case "PreviousWeek":
                            StartDate = currentDate.AddDays(-14);
                            EndDate = currentDate.AddDays(-7);
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CreatedOrModified) <= DbFunctions.TruncateTime(EndDate));
                            break;

                        case "PreviousMonth":
                            StartDate = currentDate.AddMonths(-2);
                            EndDate = currentDate.AddMonths(-1);
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CreatedOrModified) <= DbFunctions.TruncateTime(EndDate));
                            break;

                        case "MonthToDate":
                            StartDate = new DateTime(currentDate.Year, currentDate.Month, 01);
                            EndDate = currentDate;
                            notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate) && DbFunctions.TruncateTime(m.CreatedOrModified) <= DbFunctions.TruncateTime(EndDate));
                            break;
                        case "DateRange":
                            #region Date Range
                            var start_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "start_date");
                            if (start_date_field != null)
                            {
                                var start_date = start_date_field.Value?.ToString();
                                StartDate = start_date.ToDateTime();
                                
                            }

                            var end_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "end_date");
                            if (end_date_field != null)
                            {
                                var end_date = end_date_field.Value?.ToString();
                                EndDate = end_date.ToDateTime();
                                 
                            }
                            notesdata = notesdata.Where(m => (StartDate == null || DbFunctions.TruncateTime(m.CreatedOrModified) >= DbFunctions.TruncateTime(StartDate)) && (EndDate == null || DbFunctions.TruncateTime(m.CreatedOrModified) <= DbFunctions.TruncateTime(EndDate)));
                            #endregion
                            break;

                        case "SpecificDate":
                            #region Specific Date
                            var specific_date_field = request.Filter.Filters.FirstOrDefault(m => m.Field.ToLower() == "specific_date");
                            if (specific_date_field != null)
                            {
                                var specific_date = specific_date_field.Value?.ToString();
                                if (!string.IsNullOrEmpty(specific_date))
                                {
                                    StartDate = specific_date.ToDateTime();
                                    notesdata = notesdata.Where(m => DbFunctions.TruncateTime(m.CreatedOrModified) == DbFunctions.TruncateTime(StartDate));
                                }
                            }
                            #endregion
                            break;
                    }
                }
            }

            #endregion 

            return notesdata.OrderByDescending(m => m.CreatedOrModified); ;
        }

       


        public entity_note GetDetails(int id)
        {
            var model = _unitOfWork.EntityNoteRepository.Query()
                                   .FirstOrDefault(m => m.etn_key == id);
            return model;
        } 
        
        public SignOutNoteViewModel GetSingOutNotes(int id)
        {
            var signOutVM = _unitOfWork.EntityNoteRepository.Query().Select(m=> new SignOutNoteViewModel()
            {
                Id=m.etn_key,
                Notes = m.etn_notes,
                Name= m.etn_modified_by,
                Date = m.etn_modified_date.HasValue ? DBHelper.FormatDateTime(m.etn_modified_date.Value,true) : DBHelper.FormatDateTime(m.etn_created_date, true),
                CreatedBy = m.etn_created_by,
                CreatedOrModified = m.etn_modified_date.HasValue ? m.etn_modified_date.Value : m.etn_created_date,

            }).FirstOrDefault(m => m.Id == id);
            
            return signOutVM;
        }
        public void Create(entity_note entity)
        {   
            _unitOfWork.EntityNoteRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(entity_note entity)
        {    
            _unitOfWork.EntityNoteRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
    }
}
