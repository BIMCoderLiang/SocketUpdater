using System.Web.Http;

namespace UpdaterWebServer.ApiController
{
    public class ConnectionController : System.Web.Http.ApiController
    {
        [HttpGet]
        [ActionName("ConnectionTest")]
        public string ConnectionTest()
        {
            return "connected_success";
        }
    }
}
