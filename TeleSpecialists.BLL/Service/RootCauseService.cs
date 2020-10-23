using Kendo.DynamicLinq;
using System.Linq;
using TeleSpecialists.BLL.Model;
using System.Data.Entity;
using System;
using TeleSpecialists.BLL.Extensions;
using TeleSpecialists.BLL.Helpers;
using System.Reflection;
using TeleSpecialists.BLL.ViewModels;
using System.Collections.Generic;
using System.Text;

namespace TeleSpecialists.BLL.Service
{
    public class RootCauseService:BaseService
    {
        public void Create(rca_counter_measure entity)
        {
            _unitOfWork.RootCauseRepository.Insert(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }

        public List<rca_counter_measure> GetDetail(int id)
        {
            var model = _unitOfWork.RootCauseRepository.Query().Where(x => x.rca_key_id == id).ToList();
            return model;
        }

        public rca_counter_measure GetDetailById(int id)
        {
            var model = _unitOfWork.RootCauseRepository.Find(id);
            return model;
        }

        public void DeleteRootCause(int id, bool commit = true)
        {
            _unitOfWork.RootCauseRepository.Delete(id);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }
        }

        public void Edit(rca_counter_measure entity, bool commit = true)
        {

            _unitOfWork.RootCauseRepository.Update(entity);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }

        }
        public void CreateRange(List<rca_counter_measure> entity)
        {
            _unitOfWork.RootCauseRepository.InsertRange(entity);
            _unitOfWork.Save();
            _unitOfWork.Commit();
        }
        public void EditRange(List<rca_counter_measure> entity, bool commit = true)
        {

            _unitOfWork.RootCauseRepository.UpdateRange(entity);
            if (commit)
            {
                _unitOfWork.Save();
                _unitOfWork.Commit();
            }

        }

    }
}
