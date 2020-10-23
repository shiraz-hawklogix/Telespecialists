using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebSockets;

namespace TeleSpecialists.Web.Controllers
{
    public class TCSocketController : Controller
    {
        // GET: TCSocket
        public ActionResult Get()
        {
            var currentContext = System.Web.HttpContext.Current;
            if (currentContext.IsWebSocketRequest ||
                currentContext.IsWebSocketRequestUpgrading)
            {
                  currentContext.AcceptWebSocketRequest(ProcessWebsocketSession);
            }

            return new HttpStatusCodeResult(HttpStatusCode.SwitchingProtocols);
        }

        private  Task ProcessWebsocketSession(AspNetWebSocketContext context)
        {
            var handler = new Hubs.WebSocketEventHandler();            
            var processTask = handler.ProcessWebSocketRequestAsync(context);
            return processTask;
        }

       

       
    }
}