using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TeleSpecialists.BLL.Service;
using TeleSpecialists.Controllers;

namespace TeleSpecialists.Web.Controllers
{
    [Authorize]
    public class PatientDetailsController : BaseController
    {
        private readonly PatientDetailsService _patientDetailsService;

        public PatientDetailsController() : base()
        {
            _patientDetailsService = new PatientDetailsService();
        }


        [HttpGet]
        public JsonResult GetAll(string fName, string lName, string facility,/* DateTime dob,*/ string mrn)
        {
            try
            {
                var res = _patientDetailsService.GetPatients(fName, lName, facility, mrn)
                    .Select( p => new
                {
                    patientNumber = p.pat_Account_Number,
                    patientName = p.pat_firstname + " " + p.pat_lastname
                }); 

                var jsonResult = Json(res, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
               
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return Json(new { success = false, message = "An error has been occurred while processing your request, please try later." });
            }
        }


        #region ----- Disposable -----
        private bool disposed = false; // to detect redundant calls
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    this._patientDetailsService?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}