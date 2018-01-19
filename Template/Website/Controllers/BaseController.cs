using Domain;
using Olive.Mvc;

namespace Controllers
{
    public class BaseController : Olive.Mvc.Controller
    {
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