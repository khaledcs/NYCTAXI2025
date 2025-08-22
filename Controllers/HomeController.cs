using System.Web.Mvc;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Contains the actions of Home controller
    /// </summary>
    public class HomeController : ApplicationBaseController
    {
        /// <summary>
        /// Home page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// About page
        /// </summary>
        /// <returns></returns>
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Contact page
        /// </summary>
        /// <returns></returns>
        public ActionResult Contact()
        {
            return View();
        }
    }
}