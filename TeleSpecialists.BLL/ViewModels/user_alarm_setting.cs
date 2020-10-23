using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TeleSpecialists.BLL.Model;

namespace TeleSpecialists.BLL.ViewModels
{
  public  class user_alarm_setting
    {
        public IEnumerable<alarm_tunes> alarm_list { get; set; }
        public alarm_setting obj_alarm_Setting { get; set; }
        public default_notification_tune obj_default_tune { get; set; }
    }
}
