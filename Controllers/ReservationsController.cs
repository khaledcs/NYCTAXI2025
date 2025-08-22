using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using NYC_Taxi_System.Models;
using PagedList;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using System.Web.Configuration;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Contains the actions of Reservations controller
    /// </summary>
    [Authorize]
    public class ReservationsController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();
        private int radius = 0;

        /// <summary>
        /// GET: Reservations List
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<ActionResult> Index(string id, string state, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData.Add("ReservationUser", id);
            #region Get a list of years
            List<string> states = new List<string>();
            states.Add("Not Assigned");
            states.Add("Assigned");
            states.Add("Accepted");
            states.Add("Rejected");
            states.Add("Ended");
            states.Add("Ended & Feedback Left");
            ViewBag.States = states;
            #endregion

            var user = await context.Users.SingleOrDefaultAsync(p => p.Username == id);
            if (user != null)
            {
                #region Get the relavent records for each user
                IQueryable<Reservation> reservations = null;
                if (user.UserType == "Operator")
                {
                    reservations = context.Reservations.Where(r => r.Status == state);
                }
                else if (user.UserType == "Driver")
                {
                    if (user.DriverVehicle == null || user.DriverLocation == null)
                    {
                        ViewData.Add("IsValid", "false");
                    }
                    reservations = context.Reservations.Where(r => r.Driver == id && r.Status == state);
                }
                else if (user.UserType == "Passenger")
                {
                    reservations = context.Reservations.Where(r => r.Passenger == id && r.Status == state);
                }
                #endregion

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

                ViewBag.CurrentState = state;
                ViewBag.CurrentFilter = searchString;
                #endregion

                #region Query for searching 
                if (!string.IsNullOrEmpty(searchString))
                {
                    reservations = reservations.Where(s => s.User_Passenger.FirstName.Contains(searchString)
                                           || s.User_Passenger.LastName.Contains(searchString)
                                           || s.User_Passenger.Phone.Contains(searchString)
                                           || s.FirstName.Contains(searchString)
                                           || s.LastName.Contains(searchString)
                                           || s.Phone.Contains(searchString)
                                           || s.PickUpRoute.Contains(searchString)
                                           || s.PickUpCity.Contains(searchString)
                                           || s.PickUpProvince.Contains(searchString)
                                           || s.DropRoute.Contains(searchString)
                                           || s.DropCity.Contains(searchString)
                                           || s.DropProvince.Contains(searchString)
                                           || s.User_Driver.FirstName.Contains(searchString)
                                           || s.User_Driver.LastName.Contains(searchString)
                                           || s.User_Driver.Phone.Contains(searchString)
                                           || s.VehicleType.Type.Contains(searchString));
                }
                #endregion

                // Set pagination
                int pageSize = 4;
                int pageNumber = (page ?? 1);
                return View(reservations.OrderByDescending(i => i.PickUpDate).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// GET: Reservations/Details/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = await context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                // No reservation found
                return HttpNotFound();
            }
            return View(reservation);
        }

        /// <summary>
        /// GET: Reservations/Create
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type");
            return View();
        }

        /// <summary>
        /// POST: Reservations/Create
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Exclude = "Driver")] Reservation reservation)
        {
            #region User Details Validation
            if (GetCurrentUser().UserType == "Operator")
            {
                // Validations for operator
                bool isValid = true;
                if (reservation.FirstName == null)
                {
                    ModelState.AddModelError("FNameRequired", "The First Name field is required.");
                    isValid = false;
                }
                if (reservation.LastName == null)
                {
                    ModelState.AddModelError("LNameRequired", "The Last Name field is required.");
                    isValid = false;
                }
                if (reservation.Phone == null)
                {
                    ModelState.AddModelError("PhoneRequired", "The Phone Number field is required.");
                    isValid = false;
                }
                if (isValid == false)
                {
                    return View(reservation);
                }
            }
            #endregion

            if (ModelState.IsValid)
            {
                // Save the record and redirect to SelectDriver
                reservation.Status = "Not Assigned";
                context.Reservations.Add(reservation);
                await context.SaveChangesAsync();
                return RedirectToAction("SelectDriver", new { id = reservation.Reservation_Id });
            }
            ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type");
            return View(reservation);
        }

        /// <summary>
        /// GET: Reservations/Edit/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = await context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                // Reservation not found
                return HttpNotFound();
            }
            string pickUpLatLng = $"{ reservation.PickUpLat }, { reservation.PickUpLng }";
            ViewData.Add("PickUp", pickUpLatLng);

            ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type");
            return View(reservation);
        }

        /// <summary>
        /// POST: Reservations/Edit/id
        /// </summary>
        /// <param name="reservation"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Exclude = "Driver")] Reservation reservation)
        {
            #region User Details Validation
            if (GetCurrentUser().UserType == "Operator" && reservation.User_Passenger == null)
            {
                // Validations for operators
                bool isValid = true;
                if (reservation.FirstName == null)
                {
                    ModelState.AddModelError("FNameRequired", "The First Name field is required.");
                    isValid = false;
                }
                if (reservation.LastName == null)
                {
                    ModelState.AddModelError("LNameRequired", "The Last Name field is required.");
                    isValid = false;
                }
                if (reservation.Phone == null)
                {
                    ModelState.AddModelError("PhoneRequired", "The Phone Number field is required.");
                    isValid = false;
                }
                if (isValid == false)
                {
                    return View(reservation);
                }
            }
            #endregion

            if (ModelState.IsValid)
            {
                // Update and save changes, then redirect to SelectDriver
                context.Entry(reservation).State = EntityState.Modified;
                reservation.Status = "Not Assigned";
                await context.SaveChangesAsync();
                return RedirectToAction("SelectDriver", new { id = reservation.Reservation_Id });
            }
            ViewBag.Types = new SelectList(context.VehicleTypes, "Id", "Type");
            return View(reservation);
        }

        /// <summary>
        /// GET: Reservations/Delete/id
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
            Reservation reservation = await context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                // No reservation found
                return HttpNotFound();
            }
            return View(reservation);
        }

        /// <summary>
        /// POST: Reservations/Delete/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            // Delete the reservation record
            Reservation reservation = await context.Reservations.FindAsync(id);
            context.Reservations.Remove(reservation);
            await context.SaveChangesAsync();
            return RedirectToAction("Index", new { Id = GetCurrentUser().Username });
        }

        /// <summary>
        /// GET: Nearest Drivers List
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<ActionResult> SelectDriver(int? id, string sortOrder, string currentFilter, string searchString, int? page)
        {
            string message = "";
            var reservation = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == id);
            if (reservation != null)
            {
                // Reservation found
                if (reservation.Status == "Not Assigned" || reservation.Status == "Assigned" || reservation.Status == "Rejected")
                {
                    // Reservation found with status "Not Assigned" or "Assigned" or "Rejected"
                    #region Find the nearest drivers with their ratings

                    int searchCount = 1;

                    // Getting the latitude and longitude values for the 2km range circle for the first search
                    decimal minLat2km = reservation.PickUpLat - (decimal)0.015; // 0.015 is approx. 2km
                    decimal maxLat2km = reservation.PickUpLat + (decimal)0.015;
                    decimal minLng2km = reservation.PickUpLng - (decimal)0.015;
                    decimal maxLng2km = reservation.PickUpLng + (decimal)0.015;

                    // Available drivers within 2km radius
                    var drivers = context.Users.Where(r => r.DriverAvailability.Status == true &&
                    r.DriverVehicle.VehicleType.Type == reservation.VehicleType.Type &&
                    (r.DriverLocation.Latitude <= maxLat2km && r.DriverLocation.Latitude >= minLat2km) &&
                    (r.DriverLocation.Longitude <= maxLng2km && r.DriverLocation.Longitude >= minLng2km));

                    if (drivers.Count() == 0)
                    {
                        // Getting the latitude and longitude values for the 4km range circle for the second search
                        decimal minLat4km = minLat2km - (decimal)0.015; // 0.015 is approx. 2km
                        decimal maxLat4km = maxLat2km + (decimal)0.015;
                        decimal minLng4km = minLng2km - (decimal)0.015;
                        decimal maxLng4km = maxLng2km + (decimal)0.015;

                        // Available drivers within 4km radius
                        drivers = context.Users.Where(r => r.DriverAvailability.Status == true &&
                        r.DriverVehicle.VehicleType.Type == reservation.VehicleType.Type &&
                        (r.DriverLocation.Latitude <= maxLat4km && r.DriverLocation.Latitude >= minLat4km) &&
                        (r.DriverLocation.Longitude <= maxLng4km && r.DriverLocation.Longitude >= minLng4km));

                        searchCount = 2;

                        if (drivers.Count() == 0)
                        {
                            // Getting the latitude and longitude values for the 6km range circle for the third search
                            decimal minLat6km = minLat4km - (decimal)0.015; // 0.015 is approx. 2km
                            decimal maxLat6km = maxLat4km + (decimal)0.015;
                            decimal minLng6km = minLng4km - (decimal)0.015;
                            decimal maxLng6km = maxLng4km + (decimal)0.015;

                            // Available drivers within 6km radius
                            drivers = context.Users.Where(r => r.DriverAvailability.Status == true &&
                            r.DriverVehicle.VehicleType.Type == reservation.VehicleType.Type &&
                            (r.DriverLocation.Latitude <= maxLat6km && r.DriverLocation.Latitude >= minLat6km) &&
                            (r.DriverLocation.Longitude <= maxLng6km && r.DriverLocation.Longitude >= minLng6km));

                            searchCount = 3;
                        }
                    }

                    if (drivers.Count() != 0)
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

                        #region Query for searching 
                        if (!string.IsNullOrEmpty(searchString))
                        {
                            drivers = drivers.Where(s => s.Username.Contains(searchString)
                                                   || s.FirstName.Contains(searchString)
                                                   || s.LastName.Contains(searchString)
                                                   || s.Phone.Contains(searchString)
                                                   || s.DriverLocation.Route.Contains(searchString)
                                                   || s.DriverVehicle.VehicleType.Type.Contains(searchString)
                                                   || s.DriverVehicle.Brand.Contains(searchString));
                        }
                        #endregion

                        List<string> ratings = new List<string>();
                        foreach (var driver in drivers)
                        {
                            // Loop through all nearest drivers to calculate their average ratings
                            double rating = 0;
                            double avgRating = 0;
                            var feedbacks = context.Feedbacks.Where(f => f.Driver == driver.Username);
                            int count = feedbacks.Count();
                            if (count != 0)
                            {
                                foreach (var feedback in feedbacks)
                                {
                                    rating += Convert.ToDouble(feedback.Rating);
                                }
                                avgRating = rating / count;
                            }
                            ratings.Add(avgRating.ToString("0.0"));
                        }
                        ViewData.Add("Feedback", ratings);

                        if (searchCount == 2)
                        {
                            radius = 4000;
                        }
                        else if (searchCount == 3)
                        {
                            radius = 6000;
                        }
                        else
                        {
                            radius = 2000;
                        }
                        ViewData.Add("Radius", radius);
                    }
                    else
                    {
                        // Fulfilment failed
                        message = "Sorry, there are no nearby drivers found for your location";
                        ViewBag.Message = message;
                    }
                    #endregion

                    // Set pagination
                    int pageSize = 5;
                    int pageNumber = (page ?? 1);
                    return View(drivers.OrderBy(i => i.Username).ToPagedList(pageNumber, pageSize));
                }
                else
                {
                    // Invalid reservation state
                    ViewData.Add("Status", "Invalid");
                    return View();
                }
            }
            else
            {
                // No reservation found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// POST: Assign the Selected Driver
        /// </summary>
        /// <param name="driverId"></param>
        /// <param name="resId"></param>
        /// <returns></returns>
        public async Task<ActionResult> DriverSelected(string driverId, int? resId)
        {
            string message = "";
            bool status = false;
            var res = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == resId);
            if (res != null)
            {
                // Reservation found
                if (res.Status == "Not Assigned" || res.Status == "Assigned" || res.Status == "Rejected")
                {
                    // Reservation found with status "Not Assigned" or "Assigned" or "Rejected"
                    var driver = await context.Users.SingleOrDefaultAsync(d => d.Username == driverId /*&& d.DriverAvailability.Status == true*/);
                    if (driver != null)
                    {
                        // Driver found, assign the driver and save 
                        res.Driver = driverId;
                        res.Status = "Assigned";
                        context.Configuration.ValidateOnSaveEnabled = false;
                        await context.SaveChangesAsync();
                        message = "Reservation has been created successfully. Request has been sent to the selected driver.";
                        status = true;

                        #region Send SMS to the Driver
                        string name = "", phone = "";

                        if (res.Passenger != null)
                        {
                            // Reservation done by a passenger
                            name = $"{ res.User_Passenger.FirstName } { res.User_Passenger.LastName }";
                            phone = res.User_Passenger.Phone;
                        }
                        else
                        {
                            // Reservation done by an operator
                            name = $"{ res.FirstName } { res.LastName }";
                            phone = res.Phone;
                        }

                        // Send SMS to driver regarding the reservation request
                        SendSMS(name, phone, $"{ res.PickUpStreetNo } { res.PickUpRoute } { res.PickUpCity } { res.PickUpProvince } { res.PickUpZipCode }",
                                $"{ res.DropStreetNo } { res.DropRoute } { res.DropCity } { res.DropProvince } { res.DropZipCode }", res.PickUpDate.ToShortDateString(), res.PickUpTime.ToString(),
                                res.User_Driver.Phone, "", "", "", 0);
                        #endregion
                    }
                    else
                    {
                        // Driver couldn't be found
                        message = "Something went wrong.";
                        ViewData.Add("Reservation_Id", resId);
                    }
                    ViewBag.Message = message;
                    ViewBag.Status = status;
                    return View();
                }
                else
                {
                    // Invalid reservation
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                // No reservation found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// GET: DistanceDetails
        /// </summary>
        /// <param name="driverId"></param>
        /// <param name="resId"></param>
        /// <returns></returns>
        public async Task<ActionResult> DistanceDetails(string driverId, int? resId, int? radius)
        {
            var reservation = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == resId);
            if (reservation != null)
            {
                // Reservation found
                if (reservation.Status == "Not Assigned" || reservation.Status == "Assigned" || reservation.Status == "Rejected")
                {
                    // Reservation found with status "Not Assigned" or "Assigned" or "Rejected"
                    string pickUpLatLng = $"{ reservation.PickUpLat }, { reservation.PickUpLng }";
                    ViewData.Add("pickup", pickUpLatLng);
                    ViewData.Add("pickupLat", reservation.PickUpLat);
                    ViewData.Add("pickupLng", reservation.PickUpLng);

                    var driver = await context.Users.SingleOrDefaultAsync(d => d.Username == driverId);
                    if (driver != null)
                    {
                        // Driver found
                        string driverLocation = $"{ driver.DriverLocation.Latitude }, { driver.DriverLocation.Longitude }";
                        ViewData.Add("driverLocation", driverLocation);
                        ViewData.Add("resId", resId);
                        ViewData.Add("Radius", radius);
                        return View();
                    }
                    else
                    {
                        // No driver found
                        return HttpNotFound();
                    }
                }
                else
                {
                    // Invalid reservation state
                    ViewData.Add("Status", "Invalid");
                    return View();
                }
            }
            else
            {
                // Reservation not found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// GET: Directions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Directions(int? id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var res = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == id);
            if (res != null)
            {
                // Reservation found
                string pickUpLatLng = $"{ res.PickUpLat }, { res.PickUpLng }";
                ViewData.Add("PickUp", pickUpLatLng);
                return View(res);
            }
            else
            {
                // No reservation found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Driver Accepted the Reservation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> ReservationAccepted(int id)
        {
            var res = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == id);
            if (res != null)
            {
                // Reservation found, therefore change the status to "Accepted" and save
                res.Status = "Accepted";
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();

                #region Send SMS to the Passenger
                var phone = "";

                if (res.Passenger != null)
                {
                    // Reservation done by a passenger
                    phone = res.User_Passenger.Phone;
                }
                else
                {
                    // Reservation done by an operator
                    phone = res.Phone;
                }

                SendSMS($"{ res.User_Driver.FirstName } { res.User_Driver.LastName }", phone, $"{ res.PickUpStreetNo } { res.PickUpRoute } { res.PickUpCity } { res.PickUpProvince } { res.PickUpZipCode }",
                       $"{ res.DropStreetNo } { res.DropRoute } { res.DropCity } { res.DropProvince } { res.DropZipCode }", res.PickUpDate.ToShortDateString(), res.PickUpTime.ToString(),
                       res.User_Driver.Phone, res.User_Driver.DriverVehicle.Brand, res.User_Driver.DriverVehicle.VehicleType.Type, res.User_Driver.DriverVehicle.Seats.ToString(), 0, "Accepted");
                #endregion

                return RedirectToAction("Index", new { Id = GetCurrentUser().Username });
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Driver Rejected the Reservation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> ReservationRejected(int id)
        {
            var res = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == id);
            if (res != null)
            {
                #region Send SMS to the Passenger
                var phone = "";

                if (res.Passenger != null)
                {
                    // Reservation done by a passenger
                    phone = res.User_Passenger.Phone;
                }
                else
                {
                    // Reservation done by an operator
                    phone = res.Phone;
                }

                SendSMS($"{ res.User_Driver.FirstName } { res.User_Driver.LastName }", phone, $"{ res.PickUpStreetNo } { res.PickUpRoute } { res.PickUpCity } { res.PickUpProvince } { res.PickUpZipCode }",
                       $"{ res.DropStreetNo } { res.DropRoute } { res.DropCity } { res.DropProvince } { res.DropZipCode }", res.PickUpDate.ToShortDateString(), res.PickUpTime.ToString(),
                       "", "", "", "", 0, "Rejected");
                #endregion

                res.Driver = null;
                res.Status = "Rejected";
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
                return RedirectToAction("Index", new { Id = GetCurrentUser().Username });
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Driver Ended the Trip
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> EndTrip(int id)
        {
            var res = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == id);
            if (res != null)
            {
                // Reservation found
                #region Calculate wait time
                decimal finalCharge = res.Charge;
                var waitingTime = await context.WaitingTimes.SingleOrDefaultAsync(w => w.Id == id);
                if (waitingTime != null && waitingTime.Duration != null)
                {
                    // Waiting time record found
                    decimal waitingTimeCharge = (decimal)waitingTime.Duration * res.VehicleType.Rate * 5 / 100;
                    finalCharge = waitingTimeCharge + res.Charge;
                    res.Charge = finalCharge;
                }
                #endregion

                res.Status = "Ended"; // Change status to ended
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();

                #region Send SMS to the Passenger
                var phone = "";

                if (res.Passenger != null)
                {
                    // Reservation done by a passenger
                    phone = res.User_Passenger.Phone;
                }
                else
                {
                    // Reservation done by an operator
                    phone = res.Phone;
                }

                SendSMS("", phone, "", $"{ res.DropStreetNo } { res.DropRoute } { res.DropCity } { res.DropProvince } { res.DropZipCode }", "", "", "", "", "", "", finalCharge, "Ended");
                #endregion

                return RedirectToAction("Index", new { Id = GetCurrentUser().Username });
            }
            else
            {
                // Reservation not found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Customer Added the Feedback
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> FeedbackAdded(int id)
        {
            var res = await context.Reservations.SingleOrDefaultAsync(r => r.Reservation_Id == id);
            if (res != null)
            {
                // Reservation found, therefore change the status to "Ended & Feedback Left" and save
                res.Status = "Ended & Feedback Left";
                context.Configuration.ValidateOnSaveEnabled = false;
                await context.SaveChangesAsync();
                return RedirectToAction("Index", new { Id = GetCurrentUser().Username });
            }
            else
            {
                // Reservation not found
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Return a list of vehicle types
        /// </summary>
        /// <returns></returns>
        public async Task<decimal> GetVehicleTypes(int? id)
        {
            VehicleType type = await context.VehicleTypes.FindAsync(id);
            return type.Rate;
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
        /// Get Current Logged in User
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public User GetCurrentUser()
        {
            var userNameOremail = User.Identity.Name;
            var user = context.Users.SingleOrDefault(u => u.Email == userNameOremail || u.Username == userNameOremail); // Getting the current logged in user
            return user;
        }

        /// <summary>
        /// Send SMS to the Passengers and Drivers
        /// </summary>
        /// <param name="name"></param>
        /// <param name="passengerPhone"></param>
        /// <param name="pickup"></param>
        /// <param name="drop"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <param name="driverPhone"></param>
        /// <param name="brand"></param>
        /// <param name="vehicleType"></param>
        /// <param name="seats"></param>
        /// <param name="charge"></param>
        /// <param name="smsFor"></param>
        [NonAction]
        public void SendSMS(string name, string passengerPhone, string pickup, string drop, string date, string time, string driverPhone,
            string brand, string vehicleType, string seats, decimal charge, string smsFor = "DriverSelected")
        {
            var accountSid = WebConfigurationManager.AppSettings["Twilio_Acc_SID"];
            var authToken = WebConfigurationManager.AppSettings["Twilio_Auth_Token"];
            TwilioClient.Init(accountSid, authToken);

            var from = new PhoneNumber(WebConfigurationManager.AppSettings["Twilio_Phone_Number"]);
            PhoneNumber to = "";
            var body = "";

            if (smsFor == "DriverSelected")
            {
                to = new PhoneNumber("+1" + driverPhone);
                body = $"(Free) { Environment.NewLine } { Environment.NewLine }" +
                    $"Congratulations! You have recieved a reservation request from, { Environment.NewLine } { Environment.NewLine }" +
                    $"Passenger - { name } { Environment.NewLine }" +
                    $"Phone - { passengerPhone } { Environment.NewLine } { Environment.NewLine }" +
                    $"Pick-Up - { pickup } { Environment.NewLine }" +
                    $"Destination - { drop } { Environment.NewLine }" +
                    $"At - { time } { date }";
            }
            else if (smsFor == "Accepted")
            {
                to = new PhoneNumber("+1" + passengerPhone);
                body = $"(Free) { Environment.NewLine } { Environment.NewLine }" +
                    $"Congratulations! The driver has accepted your reservation request for, { Environment.NewLine } { Environment.NewLine }" +
                    $"Pick-Up - { pickup } { Environment.NewLine }" +
                    $"Destination - { drop } { Environment.NewLine }" +
                    $"At - { time } { date } { Environment.NewLine } { Environment.NewLine }" +
                    $"Driver - { name } { Environment.NewLine }" +
                    $"Phone - { driverPhone } { Environment.NewLine }" +
                    $"Vehicle - { brand } { vehicleType } with { seats } seats";
            }
            else if (smsFor == "Rejected")
            {
                to = new PhoneNumber("+1" + passengerPhone);
                body = $"(Free) { Environment.NewLine } { Environment.NewLine }" +
                    $"Sorry, the driver has rejected your reservation request for, { Environment.NewLine } { Environment.NewLine }" +
                    $"Pick-Up - { pickup } { Environment.NewLine }" +
                    $"Destination - { drop } { Environment.NewLine }" +
                    $"At - { time } { date } { Environment.NewLine } { Environment.NewLine }" +
                    $"Driver - { name } { Environment.NewLine } { Environment.NewLine }" +
                    $"Please select another driver from the system.";
            }
            else if (smsFor == "Ended")
            {
                to = new PhoneNumber("+1" + passengerPhone);
                body = $"(Free) { Environment.NewLine } { Environment.NewLine }" +
                    $"Congratulations! You have successfully ended your trip to, { Environment.NewLine } { Environment.NewLine }" +
                    $"Destination - { drop } { Environment.NewLine }" +
                    $"At - { DateTime.Now } { Environment.NewLine } { Environment.NewLine }" +
                    $"Your trip charge is (CAD) - { charge.ToString("F") } { Environment.NewLine } { Environment.NewLine }" +
                    $"Thank you for using NYC Taxi.";
            }

            try
            {
                var message = MessageResource.Create(
                    to: to,
                    from: from,
                    body: body);
            }
            catch { }
        }
    }
}
