using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.Service
{
    public class NIHStrokeScaleService : BaseService
    {
        public IQueryable<nih_stroke_scale_question> GetAllQuestions()
        {
            return _unitOfWork.NIHStrokeScaleQuestionRepository.Query();
        }
        public IQueryable<nih_stroke_scale_answer> GetAnswers(int cas_key, int ent_key)
        {
            //return _unitOfWork.NIHStrokeScaleAnswerRepository.Query().Where(m => m.nsa_cas_key == cas_key && m.nsa_ent_key == ent_key);
            // changes for ticket - 364
            //&& (m.nsa_ent_key == null || m.nsa_ent_key == ent_key)
            return _unitOfWork.NIHStrokeScaleAnswerRepository.Query().Where(m => m.nsa_cas_key == cas_key && (m.nsa_ent_key == ent_key || m.nsa_ent_key == null));
        }
        public IQueryable<nih_stroke_scale> GetSelectedOptions(List<int> list)
        {
            return _unitOfWork.NIHStrokeScaleRepository.Query().Where(m => list.Contains(m.nss_key));
        }
        //public void SubmitAnswers(int cas_key, List<int> selectedNIHSOptions, int entityKey, string createdById, string createdByName)
        //{
          

        //    string selectedAnswers = string.Join(",", selectedNIHSOptions);

        //    var sqlParameters = new List<SqlParameter>();
        //    sqlParameters.Add(new SqlParameter("@cas_key", cas_key));
        //    sqlParameters.Add(new SqlParameter("@entityTypeKey", entityKey));
        //    sqlParameters.Add(new SqlParameter("@createdById", createdById));
        //    sqlParameters.Add(new SqlParameter("@createdByName", createdByName));
        //    sqlParameters.Add(new SqlParameter("@selectedNIHSOptions", selectedAnswers));

        //    Helpers.DBHelper.ExecuteNonQuery("usp_update_nihss_score", sqlParameters.ToArray());
        //}

        public void SubmitAnswers(int cas_key, List<int> selectedNIHSOptions, string createdById, string createdByName)
        {


            string selectedAnswers = string.Join(",", selectedNIHSOptions);

            var sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@cas_key", cas_key));
            sqlParameters.Add(new SqlParameter("@createdById", createdById));
            sqlParameters.Add(new SqlParameter("@createdByName", createdByName));
            sqlParameters.Add(new SqlParameter("@selectedNIHSOptions", selectedAnswers));

            Helpers.DBHelper.ExecuteNonQuery("usp_update_nihss_score", sqlParameters.ToArray());
        }
    }
}
