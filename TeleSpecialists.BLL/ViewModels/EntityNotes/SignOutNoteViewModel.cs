using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.BLL.ViewModels.EntityNotes
{
   public class SignOutNoteViewModel
    {
        public int Id { get; set; }

        public string CreatedBy { get; set; }

        public string Name { get; set; }

        public string Date { get; set; }

        public string Notes { get; set; } 
        public DateTime CreatedOrModified { get; set; }

        public bool IsModified { get; set; }
    }
}
