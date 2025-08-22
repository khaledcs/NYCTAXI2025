using NYC_Taxi_System.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Base controller with user infromation
    /// </summary>
    public class ApplicationBaseController : Controller
    {
        /// <summary>
        /// Get the data of the logged in user
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (User != null)
            {
                var usernameOremail = User.Identity.Name;

                if (!String.IsNullOrEmpty(usernameOremail))
                {
                    using (ABC_DBEntities context = new ABC_DBEntities())
                    {
                        var user = context.Users.SingleOrDefault(u => u.Email == usernameOremail || u.Username == usernameOremail); // Getting the current logged in user

                        try
                        {
                            #region Getting the Full Name of the current logged in user
                            string fullName = string.Concat(new string[] { user.FirstName, " ", user.LastName });
                            ViewData.Add("FullName", fullName);
                            #endregion

                            #region Getting the User Type of the current logged in user
                            string userType = user.UserType;
                            ViewData.Add("UserType", userType);
                            #endregion

                            #region Getting the Username of the current logged in user
                            string username = user.Username;
                            ViewData.Add("Username", username);
                            #endregion
                        }
                        catch
                        {
                            FormsAuthentication.SignOut();
                            Response.Redirect("/Home/Index");
                        }
                    }
                }

                base.OnActionExecuted(filterContext);
            }
        }
    }
}