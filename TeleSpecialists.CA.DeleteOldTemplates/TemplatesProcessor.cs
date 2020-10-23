using System;
using TeleSpecialists.BLL.Common.Helpers;
using TeleSpecialists.BLL.Common.Process;

namespace TeleSpecialists.CA.DeleteOldTemplates
{
    public class TemplatesProcessor : ProcessorBase
    {
        public TemplatesProcessor()
        {
            _serviceName = "Delete Old Templates";
        }
        public void ProcessTemplates()
        {
            try
            {
                // create log
                _logger.AddLogEntry(_serviceName, "INPROGRESS", _serviceName + " Started", "eAlertResend");
                DBHelper.ExecuteNonQuery("usp_template_old_delete", null);
            }
            catch (Exception exception)
            {
                _logger.AddLogEntry(_serviceName, "ERROR", exception.ToString(), "eAlertResend");
            }
            finally
            {
                _logger.AddLogEntry(_serviceName, "COMPLETED", "", "eAlertResend");
            }

            _logger.AddLogEntry(_serviceName, "COMPLETED", "Exiting function", "eAlertResend");

        }
    }
}
