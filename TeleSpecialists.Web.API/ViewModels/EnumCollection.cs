using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace TeleSpecialists.Web.API.ViewModels
{
    public class EnumCollection
    {
    }
    public enum CaseStatus
    {
        [Description("Open")]
        Open = 17,
        [Description("Waiting To Accept")]
        WaitingToAccept = 18,
        [Description("Accepted")]
        Accepted = 19,
        [Description("Complete")]
        Complete = 20,
        [Description("Cancelled")]
        Cancelled = 140
    }
}