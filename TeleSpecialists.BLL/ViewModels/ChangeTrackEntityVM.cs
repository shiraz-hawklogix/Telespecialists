using System.Collections.Generic;

namespace TeleSpecialists.BLL.ViewModels
{
    public class ChangeTrackEntityVM
    {
        public string entity { get; set; }
      
        public string field { get; set; }

        public object current { get; set; }
        public object previous { get; set; }

    }

    public class ChangeTrackEntityVMFormattedDetail
    {
        public string field { get; set; }

        public object current { get; set; }
        public object previous { get; set; }
    }

    public class ChangeTrackEntityVMFormatted
    {
        public string entity { get; set; }            
        public List<ChangeTrackEntityVMFormattedDetail>  changes { get; set; }

        public ChangeTrackEntityVMFormatted()
        {
            changes = new List<ChangeTrackEntityVMFormattedDetail>();
        }

    }
}
