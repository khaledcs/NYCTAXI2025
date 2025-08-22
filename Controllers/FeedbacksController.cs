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
    /// Contains the actions of Feedbacks controller
    /// </summary>
    [Authorize]
    public class FeedbacksController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: Feedbacks List
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

            var currentUser = GetCurrentUser().Username;
            var feedbacks = context.Feedbacks.Where(f => f.Reservation.User_Passenger.Username.Equals(currentUser));

            #region Query for searching 
            if (!string.IsNullOrEmpty(searchString))
            {
                feedbacks = feedbacks.Where(s => s.Comment.Contains(searchString)
                                       || s.Reservation.PickUpRoute.Contains(searchString)
                                       || s.Reservation.PickUpCity.Contains(searchString)
                                       || s.Reservation.PickUpProvince.Contains(searchString)
                                       || s.Reservation.DropRoute.Contains(searchString)
                                       || s.Reservation.DropCity.Contains(searchString)
                                       || s.Reservation.DropProvince.Contains(searchString)
                                       || s.Reservation.PickUpDate.ToString().Contains(searchString)
                                       || s.Reservation.User_Driver.FirstName.Contains(searchString)
                                       || s.Reservation.User_Driver.LastName.Contains(searchString));
            }
            #endregion

            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(feedbacks.OrderByDescending(i => i.Reservation.PickUpDate).ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// GET: Feedbacks/Details/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = await context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        /// <summary>
        /// GET: Feedbacks/Create
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var reservation = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == id);
            if (reservation != null && reservation.Passenger != null && reservation.Feedback == null)
            {
                ViewData.Add("Passenger", reservation.User_Passenger.Username);
            }
            else
            {
                return HttpNotFound();
            }
            ViewData.Add("Driver", reservation.User_Driver.Username);
            return View();
        }

        /// <summary>
        /// POST: Feedbacks/Create
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Feedback_Id,Driver,Rating,Comment")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                context.Feedbacks.Add(feedback);
                await context.SaveChangesAsync();
                return RedirectToAction("FeedbackAdded", "Reservations", new { id = feedback.Feedback_Id });
            }
            return View(feedback);
        }

        /// <summary>
        /// GET: Feedbacks/Edit/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = await context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        /// <summary>
        /// POST: Feedbacks/Edit/id
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Feedback_Id,Driver,Rating,Comment")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                context.Entry(feedback).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(feedback);
        }

        /// <summary>
        /// GET: Feedbacks/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Feedback feedback = await context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }
            return View(feedback);
        }

        /// <summary>
        /// POST: Feedbacks/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Feedback feedback = await context.Feedbacks.FindAsync(id);
            context.Feedbacks.Remove(feedback);
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
