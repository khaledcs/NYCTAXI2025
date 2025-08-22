using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using NYC_Taxi_System.Models;
using PagedList;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Contains the actions of DriverLocations controller
    /// </summary>
    [Authorize]
    public class DriverLocationsController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: DriverLocations List
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            #region Pagination
            ViewBag.CurrentSort = sortOrder;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            #endregion

            var locations = context.DriverLocations.Include(d => d.User);

            #region Query for searching 
            if (!string.IsNullOrEmpty(searchString))
            {
                locations = locations.Where(s => s.User.Username.Contains(searchString)
                                       || s.StreetNo.Contains(searchString)
                                       || s.Route.Contains(searchString)
                                       || s.City.Contains(searchString)
                                       || s.Province.Contains(searchString)
                                       || s.ZipCode.Contains(searchString));
            }
            #endregion

            // Set pagination
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(locations.OrderBy(i => i.Driver).ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// GET: DriverLocations/Details/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverLocation driverLocation = await context.DriverLocations.FindAsync(id);
            if (driverLocation == null)
            {
                // No Location found, redirect to create
                return RedirectToAction("Create", new { Id = id });
            }
            return View(driverLocation);
        }

        /// <summary>
        /// GET: DriverLocations/Create/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Create(string id)
        {
            var location = await context.DriverLocations.FindAsync(id);
            if (location != null)
            {
                // Location found, redirect to details
                return RedirectToAction("Details", new { Id = location.Driver });
            }

            var user = await context.Users.FindAsync(id);
            if (user != null && user.UserType.Equals("Driver"))
            {
                // Driver found
                DriverLocation model = new DriverLocation
                {
                    Driver = user.Username
                };
                return View(model);
            }
            else
            {
                // Driver not found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// POST: DriverLocations/Create
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Driver,FullAddress,StreetNo,Route,City,Province,ZipCode,Latitude,Longitude")] DriverLocation driverLocation)
        {
            if (ModelState.IsValid)
            {
                context.DriverLocations.Add(driverLocation);
                await context.SaveChangesAsync();
                return RedirectToAction("Details", new { Id = driverLocation.Driver });
            }
            return View(driverLocation);
        }

        /// <summary>
        /// GET: DriverLocations/Edit/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverLocation location = await context.DriverLocations.FindAsync(id);
            if (location == null)
            {
                // No location found
                return HttpNotFound();
            }
            return View(location);
        }

        /// <summary>
        /// POST: DriverLocations/Edit/id
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Driver,FullAddress,StreetNo,Route,City,Province,ZipCode,Latitude,Longitude")] DriverLocation driverLocation)
        {
            if (ModelState.IsValid)
            {
                context.Entry(driverLocation).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Details", new { Id = driverLocation.Driver });
            }
            return View(driverLocation);
        }

        /// <summary>
        /// GET: DriverLocations/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverLocation location = await context.DriverLocations.FindAsync(id);
            if (location == null)
            {
                // No location found
                return HttpNotFound();
            }
            var reservations = context.Reservations.Where(r => r.User_Driver.Username == id);
            if (reservations.Count() != 0)
            {
                // On going reservation found
                var hasRequests = reservations.Where(r => r.Status == "Assigned" || r.Status == "Accepted");
                if (hasRequests.Count() != 0)
                {
                    // Assigned or Accepted reservation is found
                    ViewBag.Message = "Please, respond to the available reservation requests, before attempting to delete this.";
                }
            }
            return View(location);
        }

        /// <summary>
        /// POST: DriverLocations/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            // Delete the record and redirect to create
            DriverLocation location = await context.DriverLocations.FindAsync(id);
            context.DriverLocations.Remove(location);
            DriverAvailability availability = await context.DriverAvailabilities.FindAsync(id);
            if (availability != null)
            {
                // Change availability status to false
                availability.Status = false;
            }
            await context.SaveChangesAsync();
            return RedirectToAction("Create", new { Id = location.Driver });
        }

        /// <summary>
        /// GET: FindDriver
        /// </summary>
        /// <returns></returns>
        public ActionResult FindDriver()
        {
            return View();
        }

        /// <summary>
        /// POST: FindDriver
        /// </summary>
        /// <param name="findDriver"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> FindDriver(FindAccount findDriver)
        {
            string message = "";

            var account = await context.Users.SingleOrDefaultAsync(a => a.Email == findDriver.UsernameOrEmail || a.Username == findDriver.UsernameOrEmail);
            if (account != null && account.UserType.Equals("Driver"))
            {
                // Driver found
                var location = await context.DriverLocations.FindAsync(account.Username);
                if (location != null)
                {
                    // Location found, redirect to details
                    return RedirectToAction("Details", new { Id = account.Username });
                }
                else
                {
                    // Location not found, redirect to create
                    return RedirectToAction("Create", new { Id = account.Username });
                }
            }
            else
            {
                // Driver not found
                message = "Driver not found";
            }

            ViewBag.Message = message;
            return View();
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
    }
}
