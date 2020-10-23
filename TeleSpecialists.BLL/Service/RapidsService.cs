using Kendo.DynamicLinq;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.ViewModels.Rapids;

namespace TeleSpecialists.BLL.Service
{
    public class RapidsService : BaseService
    {
        private readonly AppSettingService _appSettingService;

        public RapidsService()
        {
            _appSettingService = new AppSettingService();
        }

        public IQueryable<EmailInfo> GetAll()
        {
            return
                (
                from r in _unitOfWork.RapidsMailboxRepository.Query()
                select new EmailInfo
                {
                    Id = r.rpd_key,
                    DateTimeSent = r.rpd_date,
                    From = r.rpd_from,
                    Subject = r.rpd_subject,
                    Attachments = r.rpd_attachments,
                    UId = r.rpd_uid,
                    CreatedDate = r.rpd_created_date,
                    isRead = r.rpd_is_read
                });
        }

        public DataSourceResult GetAll(DataSourceRequest request)
        {
            var query = GetAll();

            /*
            if (request.Filter != null)
            {
                var fromFilter = request.Filter.Filters.Where(m => m.Field.Contains("From")).FirstOrDefault();

                if (fromFilter != null)
                    query = query.Where(x => x.From == Convert.ToString(fromFilter.Value));

                var subjectFilter = request.Filter.Filters.Where(m => m.Field.Contains("Subject")).FirstOrDefault();

                if (subjectFilter != null)
                    query = query.Where(x => x.Subject == Convert.ToString(subjectFilter.Value));

                var messageIdFilter = request.Filter.Filters.Where(m => m.Field.Contains("MessageId")).FirstOrDefault();

                if (messageIdFilter != null)
                    query = query.Where(x => x.UId == Convert.ToString(messageIdFilter.Value));
            }
            */
            return query.OrderByDescending(x => x.DateTimeSent).ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }

        public rapids_mailbox GetDetails(int id)
        {
            return _unitOfWork.RapidsMailboxRepository.Find(id);
        }

        public void Create(rapids_mailbox entity)
        {
            _unitOfWork.RapidsMailboxRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public void Edit(rapids_mailbox entity)
        {
            _unitOfWork.RapidsMailboxRepository.Update(entity);

            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public bool Delete(int id)
        {
            _unitOfWork.RapidsMailboxRepository.Delete(id);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }

        public bool FindByUID(string uid)
        {
            return _unitOfWork.RapidsMailboxRepository.Query().Where(x => x.rpd_uid == uid).Any();
        }
    }
}
