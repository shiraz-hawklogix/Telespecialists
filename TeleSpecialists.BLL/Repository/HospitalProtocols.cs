using System;
using System.Collections.Generic;

   


using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Repository
{
    public interface IHospitalProtocols : IGenericRepository<Hospital_Protocols> { }
    class HospitalProtocols : GenericRepository<Hospital_Protocols>, IHospitalProtocols
    {
        public HospitalProtocols(TeleSpecialistsContext dbContext) : base(dbContext)
        {
        }
    }
}
