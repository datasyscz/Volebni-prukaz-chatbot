using System.Web;
using System.Web.Http;
using VolebniPrukaz.Dialogs;
using VolebniPrukaz.Scorables;

namespace VolebniPrukaz
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ScorableHandlers.RegisterModule(RootDialog.StartWithHelloChain());

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}