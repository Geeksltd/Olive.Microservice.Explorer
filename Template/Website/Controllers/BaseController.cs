using Olive;

namespace Controllers
{
    public class BaseController : Olive.Mvc.Microservices.Controller
    {
        public BaseController()
        {
            ApiClient.FallBack += arg => Notify(arg.Args.FriendlyMessage, false);
        }

        protected override bool IsMicrofrontEnd => true;

        // Here you can add helper methods to all your controllers.
    }
}

namespace ViewComponents
{
    public abstract class ViewComponent : Olive.Mvc.ViewComponent
    {
        // Here you can add helper methods to all your cshtml views.
    }
}