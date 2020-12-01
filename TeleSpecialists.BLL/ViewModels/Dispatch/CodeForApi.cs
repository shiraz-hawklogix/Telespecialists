using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Model;
using TeleSpecialists.BLL.Service;

namespace TeleSpecialists.BLL.ViewModels.Dispatch
{
    public class CodeForApi : BaseService
    {
        private readonly CaseRejectService _caseRejectService;
        private readonly FireBaseUserMailService _fireBaseUserMailService;
        private readonly FacilityPhysicianService _facilityPhysicianService;
        private readonly CaseService _caseService;
        private readonly DispatchService _dispatchService;
        public CodeForApi()
        {
            _dispatchService = new DispatchService();
            _caseRejectService = new CaseRejectService();
            _fireBaseUserMailService = new FireBaseUserMailService();
            _facilityPhysicianService = new FacilityPhysicianService();
            _caseService = new CaseService();
        }
        #region Created Area for Api's by Husnain 
        public void RejectCase(int casKey, string casReasonId, string caseRejectionType)
        {
           
          

        }
        #endregion
    }
}
