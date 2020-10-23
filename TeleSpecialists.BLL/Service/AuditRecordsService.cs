using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class AuditRecordsService : BaseService
    {
        public void Create(audit_records entity)
        {
            _unitOfWork.AuditRecordsRepository.Insert(entity);
            _unitOfWork.Save();
             _unitOfWork.Commit(); 
        }
    }
}
