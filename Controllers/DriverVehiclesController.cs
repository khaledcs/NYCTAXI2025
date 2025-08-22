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
    /// Contains the actions of DriverVehicles controller
    /// </summary>
    [Authorize]
    public class DriverVehiclesController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: Vehicles List
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

            var vehicles = context.DriverVehicles.Include(v => v.User);

            #region Query for searching 
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicles = vehicles.Where(s => s.User.Username.Contains(searchString)
                                       || s.VehicleType.Type.Contains(searchString)
                                       || s.Brand.Contains(searchString)
                                       || s.VRN.Contains(searchString)
                                       || s.LicenseNumber.Contains(searchString)
                                       || s.InsuranceNumber.Contains(searchString)
                                       || s.NIC.Contains(searchString));
            }
            #endregion

            // Set pagination
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(vehicles.OrderBy(i => i.Driver).ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// GET: Vehicles/Details/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                // No Id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverVehicle vehicle = await context.DriverVehicles.FindAsync(id);
            if (vehicle == null)
            {
                // No vehicle record found, redirect to create
                return RedirectToAction("Create", new { Id = id });
            }
            // Return to details
            return View(vehicle);
        }

        /// <summary>
        /// GET: Vehicles/Create/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Create(string id)
        {
            var vehicle = await context.DriverVehicles.FindAsync(id);
            if (vehicle != null)
            {
                // Vehicle found, redirect to details
                return RedirectToAction("Details", new { Id = vehicle.Driver });
            }

            var user = await context.Users.FindAsync(id);
            if (user != null && user.UserType.Equals("Driver"))
            {
                // Driver found
                DriverVehicle model = new DriverVehicle
                {
                    Driver = user.Username
                };
                ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type");
                return View(model);
            }
            else
            {
                // No driver found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// POST: Vehicles/Create
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Driver,Type,Seats,Brand,VRN,LicenseNumber,InsuranceNumber,NIC")] DriverVehicle driverVehicle)
        {
            if (ModelState.IsValid)
            {
                // Save changes and redirect to the details page
                context.DriverVehicles.Add(driverVehicle);
                await context.SaveChangesAsync();
                return RedirectToAction("Details", new { Id = driverVehicle.Driver });
            }
            ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type", driverVehicle.Type);
            return View(driverVehicle);
        }

        /// <summary>
        /// GET: Vehicles/Edit/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                // No Id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverVehicle vehicle = await context.DriverVehicles.FindAsync(id);
            if (vehicle == null)
            {
                // Vehicle not found
                return HttpNotFound();
            }
            ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type", vehicle.Type);
            return View(vehicle);
        }

        /// <summary>
        /// POST: Vehicles/Edit/id
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Driver,Type,Seats,Brand,VRN,LicenseNumber,InsuranceNumber,NIC")] DriverVehicle driverVehicle)
        {
            if (ModelState.IsValid)
            {
                // Update and redirect to details
                context.Entry(driverVehicle).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Details", new { Id = driverVehicle.Driver });
            }
            ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type", driverVehicle.Type);
            return View(driverVehicle);
        }

        /// <summary>
        /// GET: Vehicles/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                // No Id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DriverVehicle vehicle = await context.DriverVehicles.FindAsync(id);
            if (vehicle == null)
            {
                // No vehicle found
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
            return View(vehicle);
        }

        /// <summary>
        /// POST: Vehicles/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            // Delete the record and redirect to create
            DriverVehicle vehicle = await context.DriverVehicles.FindAsync(id);
            context.DriverVehicles.Remove(vehicle);
            DriverAvailability availability = await context.DriverAvailabilities.FindAsync(id);
            if (availability != null)
            {
                // Change availability status to false
                availability.Status = false;
            }
            await context.SaveChangesAsync();
            return RedirectToAction("Create", new { Id = vehicle.Driver });
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
                var vehicle = await context.DriverVehicles.FindAsync(account.Username);
                if (vehicle != null)
                {
                    // Vehicle found redirect tp details
                    return RedirectToAction("Details", new { Id = account.Username });
                }
                else
                {
                    // Vehicle not found redirect to create
                    return RedirectToAction("Create", new { Id = account.Username });
                }
            }
            else
            {
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
