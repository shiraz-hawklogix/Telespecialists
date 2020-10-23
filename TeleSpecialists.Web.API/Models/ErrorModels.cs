using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace TeleSpecialists.Web.API.Models
{
    public class APIErrorResponse
    {
        public HttpStatusCode ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}