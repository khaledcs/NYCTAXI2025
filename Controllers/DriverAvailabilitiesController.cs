using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NYC_Taxi_System.Models;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Contains the actions of DriverAvailabilities controller
    /// </summary>
    [Authorize]
    public class DriverAvailabilitiesController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: DriverAvailability/Toggle
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Toggle()
        {
            var currentUser = GetCurrentUser().Username;

            var vehicle = await context.DriverVehicles.SingleOrDefaultAsync(v => v.Driver == currentUser);
            if (vehicle != null)
            {
                // Vehicle found
                ViewData.Add("Vehicle", "Found");
            }
            else
            {
                // Vehicle not found
                ViewData.Add("Vehicle", "Not Found");
            }
            var location = await context.DriverLocations.SingleOrDefaultAsync(v => v.Driver == currentUser);
            if (location != null)
            {
                // Location found
                ViewData.Add("Location", "Found");
            }
            else
            {
                // Location not found
                ViewData.Add("Location", "Not Found");
            }
            var driverAvailability = await context.DriverAvailabilities.SingleOrDefaultAsync(s => s.Driver == currentUser);
            if (driverAvailability != null)
            {
                // Driver availability record found
                ViewData.Add("Driver", driverAvailability.Driver);
                if (driverAvailability.Status == true)
                {
                    ViewData.Add("Status", "true");
                }
            }
            return View();
        }

        /// <summary>
        /// POST: DriverAvailability/Toggle
        /// </summary>
        /// <param name="driverAvailability"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Toggle([Bind(Include = "Driver,Status")] DriverAvailability driverAvailability)
        {
            if (ModelState.IsValid)
            {
                var isExist = await context.DriverAvailabilities.FindAsync(GetCurrentUser().Username);
                if (isExist == null)
                {
                    // No record exists, therefore add
                    context.DriverAvailabilities.Add(driverAvailability);
                }
                else
                {
                    // Record exists, therefore update
                    isExist.Status = driverAvailability.Status ? true : false;
                }

                var isStatusExist = await context.DriverStatusCounts.SingleOrDefaultAsync(d => d.Driver == driverAvailability.Driver && d.Date.Day == DateTime.Now.Day 
                && d.Date.Month == DateTime.Now.Month && d.Date.Year == DateTime.Now.Year);
                DriverStatusCount statusCount = new DriverStatusCount();

                if (isStatusExist == null)
                {
                    // No record exists, therefore add
                    statusCount.Driver = driverAvailability.Driver;
                    if (driverAvailability.Status)
                    {
                        statusCount.OnlineCount += 1;
                    }
                    else
                    {
                        statusCount.OfflineCount += 1;
                    }
                    statusCount.Date = DateTime.Now.Date;
                    context.DriverStatusCounts.Add(statusCount);
                }
                else
                {
                    // Record exists, therefore update
                    if (driverAvailability.Status)
                    {
                        isStatusExist.OnlineCount += 1;
                    }
                    else
                    {
                        isStatusExist.OfflineCount += 1;
                    }
                }

                await context.SaveChangesAsync();
            }
            return RedirectToAction("Toggle");
        }

        /// <summary>
        /// Database Context Disposal
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Get the Current Logged in User
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public User GetCurrentUser()
        {
            var userNameOremail = User.Identity.Name;
            var user = context.Users.SingleOrDefault(u => u.Email == userNameOremail || u.Username == userNameOremail); // Getting the current logged in user
            return user;
        }
    }
}
