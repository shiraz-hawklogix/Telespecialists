using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class PatientDetailsService : BaseService
    {
        
        public List<patient> GetPatients(string fname, string lname, string facility,/* DateTime dob,*/ string mrn)
        {
            var sp_accountnumber = new SqlParameter("accountnumber", SqlDbType.Int) { Value =  DBNull.Value };
            var sp_firstname = new SqlParameter("firstname", SqlDbType.VarChar) { Value = fname };
            var sp_lastname = new SqlParameter("lastname", SqlDbType.VarChar) { Value = lname };
            var sp_dob = new SqlParameter("dob", SqlDbType.DateTime) { Value = DBNull.Value };
            var sp_sex = new SqlParameter("sex", SqlDbType.VarChar) { Value = DBNull.Value };
            var sp_mrn = new SqlParameter("mrn", SqlDbType.VarChar) { Value = DBNull.Value };
            var sp_facility = new SqlParameter("facility", SqlDbType.UniqueIdentifier) { Value = DBNull.Value };

            List<patient> _list = _unitOfWork.ExecuteStoreProcedure<patient>("usp_get_Patients @accountnumber,@firstname,@lastname,@dob,@sex,@mrn,@facility", sp_accountnumber, sp_firstname, sp_lastname, sp_dob, sp_sex, sp_mrn, sp_facility).ToList();

            return _list;
        }



    }
}
