using Domain;
using Olive.Mvc;

namespace Controllers
{
    public class BaseController : Olive.Mvc.Controller
    {
		public BaseController()
        {
            ApiClient.FallBack.Handle(arg => Notify(arg.FriendlyMessage, false));
        }
		
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