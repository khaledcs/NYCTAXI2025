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
    [Authorize]
    public class VehicleTypesController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: VehicleTypes List
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

            var vehicleTypes = context.VehicleTypes.Include(v => v.DriverVehicles);

            #region Query for searching 
            if (!string.IsNullOrEmpty(searchString))
            {
                vehicleTypes = vehicleTypes.Where(s => s.Type.Contains(searchString));
            }
            #endregion

            // Set pagination
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(vehicleTypes.OrderBy(i => i.Id).ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// GET: VehicleTypes/Details/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                // No Id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VehicleType vehicleType = await context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                // No vehicle types found
                return HttpNotFound();
            }
            // Return to details
            return View(vehicleType);
        }

        /// <summary>
        /// GET: VehicleTypes/Create
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: VehicleTypes/Create
        /// </summary>
        /// <param name="vehicleType"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Type,Rate")] VehicleType vehicleType)
        {
            if (ModelState.IsValid)
            {
                context.VehicleTypes.Add(vehicleType);
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(vehicleType);
        }

        /// <summary>
        /// GET: VehicleTypes/Edit/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                // No Id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VehicleType vehicleType = await context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                // Vehicle type not found
                return HttpNotFound();
            }
            return View(vehicleType);
        }

        /// <summary>
        /// POST: VehicleTypes/Edit/id
        /// </summary>
        /// <param name="vehicleType"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Type,Rate")] VehicleType vehicleType)
        {
            if (ModelState.IsValid)
            {
                // Update and redirect to index
                context.Entry(vehicleType).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(vehicleType);
        }

        /// <summary>
        /// GET: VehicleTypes/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VehicleType vehicleType = await context.VehicleTypes.FindAsync(id);
            if (vehicleType == null)
            {
                // No vehicle type found
                return HttpNotFound();
            }
            return View(vehicleType);
        }

        /// <summary>
        /// POST: VehicleTypes/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            VehicleType vehicleType = await context.VehicleTypes.FindAsync(id);
            context.VehicleTypes.Remove(vehicleType);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
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
