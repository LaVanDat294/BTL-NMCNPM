using System.Web.Mvc;
using NMCNPM.Filters;

namespace NMCNPM.Controllers
{
    [AuthorizeRole("Q_ADMIN", "Q_MANAGER", "Q_STAFF")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}