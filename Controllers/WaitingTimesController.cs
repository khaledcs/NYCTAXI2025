using NYC_Taxi_System.Models;
using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Contains the actions of WaitingTimes controller
    /// </summary>
    [Authorize]
    public class WaitingTimesController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: WaitingTime/Toggle
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Toggle(int? id)
        {
            var waitingTime = await context.WaitingTimes.SingleOrDefaultAsync(s => s.Id == id);
            ViewData.Add("ResId", id);
            if (waitingTime != null)
            {
                // WaitingTime record found 
                if (waitingTime.Status == true)
                {
                    ViewData.Add("Status", "true");
                }
            }
            var reservation = await context.Reservations.FindAsync(id);
            ViewData.Add("Driver", reservation.Driver);
            if (reservation.Status == "Accepted")
            {
                ViewData.Add("OnHire", true);
            }
            return View();
        }

        /// <summary>
        /// POST: WaitingTime/Toggle
        /// </summary>
        /// <param name="driverAvailability"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Toggle([Bind(Include = "Id,Status")] WaitingTime waitingTime)
        {
            if (ModelState.IsValid)
            {
                var isExist = await context.WaitingTimes.FindAsync(waitingTime.Id);
                if (isExist == null)
                {
                    // No record exists, therefore add
                    if (waitingTime.Status)
                    {
                        waitingTime.StartTime = DateTime.Now;
                    }
                    context.WaitingTimes.Add(waitingTime);
                }
                else
                {
                    // Record exists, therefore update
                    if (waitingTime.Status)
                    {
                        // Status true
                        isExist.StartTime = DateTime.Now;
                    }
                    else
                    {
                        // Status false
                        if (isExist.Duration == null)
                        {
                            isExist.Duration = 0;
                        }
                        isExist.Duration += (Int32)(DateTime.Now - isExist.StartTime.Value).TotalMinutes;
                        isExist.StartTime = null;
                    }
                    isExist.Status = waitingTime.Status;
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
    }
}