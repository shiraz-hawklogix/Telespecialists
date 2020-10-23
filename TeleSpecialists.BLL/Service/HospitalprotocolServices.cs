using Kendo.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
   public class HospitalprotocolServices : BaseService
    {
        public void Create(Hospital_Protocols entity)
        {
            _unitOfWork.HospitalProtocols.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void Edit(Hospital_Protocols entity)
        {
            _unitOfWork.HospitalProtocols.Update(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public IEnumerable<Hospital_Protocols> GetAllProtocolData(string fap_key)
        {
            return _unitOfWork.HospitalProtocols.Query().Where(x => x.Facility_Id == fap_key).OrderBy(x => x.SortNum).ToList();
        }
        public void DeleteOnBoareded(int id)
        {
            _unitOfWork.HospitalProtocols.Delete(id);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public DataSourceResult GetAll(DataSourceRequest request, string id)
        {
            var caseTypelist = (from m in _unitOfWork.HospitalProtocols.Query().Where(x => x.Facility_Id == id)
                               select new
                               {
                                   m.SortNum,
                                   m.ID,
                                   m.ParameterName,
                                   m.ParameterName_Info,
                                   m.Facility_Name,
                                   m.Facility_Id,
                                   m.ParameterName_Image,
                                   m.Parameter_Add_Date
                               }).OrderBy(x => x.SortNum);
            return caseTypelist.ToDataSourceResult(request.Take, request.Skip, request.Sort, request.Filter);
        }
        public Hospital_Protocols GetDetails(int id)
        {
            return _unitOfWork.HospitalProtocols.Query()
                                   .FirstOrDefault(m => m.ID == id);
        }
        public IEnumerable<Hospital_Protocols> GetDetailsForPOpUP(string id)
        {
            var model = _unitOfWork.HospitalProtocols.Query().Where(m => m.Facility_Id == id).OrderBy(x => x.SortNum).ToList();
            return model;
        }

        public int ChKName(string ParameterName, string cas_fac_key_arrays)
        {
            string isValid = "";
            int isValidcount = 0;
            if (cas_fac_key_arrays == "")
            {
                isValidcount = _unitOfWork.HospitalProtocols.Query().Where(p => p.ParameterName == ParameterName).Select(a => a.ParameterName).Count();
            }
            else
            {
                foreach (var item in cas_fac_key_arrays.Split(','))
                {
                    var s = _unitOfWork.HospitalProtocols.Query().Where(p => p.ParameterName == ParameterName && p.Facility_Id == item.ToString()).Select(a => a.ParameterName).Count();
                    isValid += s + ",";
                }
                var wws = isValid.TrimEnd(',');
                var ssnew = wws.Split(',').ToArray();
                foreach (var digit in ssnew)
                {
                    isValidcount = isValidcount + Convert.ToInt32(digit);
                }
            }
            return isValidcount;
        }

        public Hospital_Protocols GetDetailForProtocols(int id)
        {
            return _unitOfWork.HospitalProtocols.Query()
                                   .FirstOrDefault(m => m.ID == id);
        }
        public bool Delete(Hospital_Protocols entity)
        {
            _unitOfWork.HospitalProtocols.Delete(entity.ID);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
        public int? GetSortNumMaxByFacility(string id)
        {
            var model = _unitOfWork.HospitalProtocols.Query().Where(x => x.Facility_Id == id).Select(x => x.SortNum).Max();
            return model;
        }
        public bool DeleteRange(IEnumerable<Hospital_Protocols> entities)
        {
            _unitOfWork.HospitalProtocols.DeleteRange(entities);
            _unitOfWork.Save();
            _unitOfWork.Commit();
            return true;
        }
    }
}
