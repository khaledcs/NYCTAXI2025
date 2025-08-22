using NYC_Taxi_System.Models;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Contains the actions of Dashboard controller
    /// </summary>
    [Authorize]
    public class DashboardsController : ApplicationBaseController
    {
        ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: Daily Revenue List
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult DriverRevenue(string driver, DateTime? fromDate, DateTime? toDate, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData.Add("Driver", driver);
            IQueryable<Reservation> revs = context.Reservations.Where(r => r.Driver == driver && (r.Status == "Ended" || r.Status == "Ended & Feedback Left")
            && r.PickUpDate >= fromDate && r.PickUpDate <= toDate);
            if (revs != null)
            {
                #region Get the days
                List<DriverDailyRev> dailyRevs = new List<DriverDailyRev>();
                List<DateTime> days = new List<DateTime>();

                foreach (var rev in revs)
                {
                    if (!days.Contains(rev.PickUpDate.Date))
                    {
                        days.Add(rev.PickUpDate.Date);
                        dailyRevs.Add(new DriverDailyRev() { Date = rev.PickUpDate.Date });
                    }
                }
                #endregion

                #region Get the daily revenue for the driver
                foreach (var rev in revs)
                {
                    foreach (var day in days)
                    {
                        if (rev.PickUpDate.Date == day)
                        {
                            foreach (var revM in dailyRevs)
                            {
                                if (revM.Date.Date == day)
                                {
                                    // Add the amount to the daily revenue
                                    revM.Amount += rev.Charge;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Get the total revenue for the period
                decimal totalRev = 0;
                foreach (var rev in dailyRevs)
                {
                    totalRev += rev.Amount;
                }
                ViewData.Add("TotalRev", totalRev.ToString("F"));
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

                ViewBag.CurrentDriver = driver;
                if (fromDate != null && toDate != null)
                {
                    ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
                    ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
                }
                ViewBag.CurrentFilter = searchString;
                #endregion

                #region Query for searching 
                if (!string.IsNullOrEmpty(searchString))
                {
                    dailyRevs = dailyRevs.Where(s => s.Amount.ToString("F").Contains(searchString) ||
                                                s.Date.ToLongDateString().Contains(searchString)).ToList();
                }
                #endregion

                ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");

                // Set pagination
                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(dailyRevs.OrderByDescending(i => i.Date).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// POST: DriverRevenueCharts
        /// </summary>
        /// <returns></returns>
        public ActionResult DriverRevenueCharts(string driver, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");

            #region Chart instantiation
            Highcharts columnChart = new Highcharts("columnchart");
            Highcharts barChart = new Highcharts("barchart");
            Highcharts areaChart = new Highcharts("areaChart");
            Highcharts lineChart = new Highcharts("lineChart");
            Highcharts areasplineChart = new Highcharts("areasplineChart");
            Highcharts splineChart = new Highcharts("splineChart");
            Highcharts pieChart = new Highcharts("pieChart");
            Highcharts scatterChart = new Highcharts("scatterChart");
            #endregion

            #region Charts List
            var charts = new ChartsModel
            {
                ColumnChart = columnChart,
                BarChart = barChart,
                AreaChart = areaChart,
                LineChart = lineChart,
                AreasplineChart = areasplineChart,
                SplineChart = splineChart,
                PieChart = pieChart,
                ScatterChart = scatterChart
            };
            #endregion

            IQueryable<Reservation> revs = context.Reservations.Where(r => r.Driver == driver && (r.Status == "Ended" || r.Status == "Ended & Feedback Left")
            && r.PickUpDate >= fromDate && r.PickUpDate <= toDate);
            if (revs != null && revs.Count() != 0)
            {
                #region Get the dates
                List<DriverDailyRev> dailyRev = new List<DriverDailyRev>();
                List<string> dates = new List<string>();

                foreach (var rev in revs)
                {
                    if (!dates.Contains(rev.PickUpDate.Date.ToShortDateString()))
                    {
                        dates.Add(rev.PickUpDate.Date.ToShortDateString());
                        dailyRev.Add(new DriverDailyRev() { Date = rev.PickUpDate.Date });
                    }
                }
                #endregion

                #region Get the daily revenue for the driver
                foreach (var rev in revs)
                {
                    foreach (var day in dates)
                    {
                        if (rev.PickUpDate.Date.ToShortDateString() == day)
                        {
                            foreach (var revM in dailyRev)
                            {
                                if (revM.Date.Date.ToShortDateString() == day)
                                {
                                    // Add the amount to the daily revenue
                                    revM.Amount += rev.Charge;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Get daily revenue list
                List<object> amount = new List<object>();
                foreach (var rev in dailyRev)
                {
                    amount.Add(rev.Amount);
                }
                #endregion

                #region Initialization
                columnChart = InitializeChart(columnChart, ChartTypes.Column, driver, dates, amount, null, null);
                barChart = InitializeChart(barChart, ChartTypes.Bar, driver, dates, amount, null, null);
                areaChart = InitializeChart(areaChart, ChartTypes.Area, driver, dates, amount, null, null);
                lineChart = InitializeChart(lineChart, ChartTypes.Line, driver, dates, amount, null, null);
                areasplineChart = InitializeChart(areasplineChart, ChartTypes.Areaspline, driver, dates, amount, null, null);
                splineChart = InitializeChart(splineChart, ChartTypes.Spline, driver, dates, amount, null, null);
                pieChart = InitializeChart(pieChart, ChartTypes.Pie, driver, dates, amount, null, null);
                scatterChart = InitializeChart(scatterChart, ChartTypes.Scatter, driver, dates, amount, null, null);
                ViewBag.Charts = true;
                #endregion
            }

            return View(charts);
        }

        /// <summary>
        /// GET: Monthly Income and Commission List
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="year"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public ActionResult DriverMonthlyRevenue(string driver, string year, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData.Add("Driver", driver);
            #region Get a list of years
            List<int> years = new List<int>();
            int startYear = DateTime.Now.Year;
            while (startYear >= DateTime.Now.AddYears(-5).Year)
            {
                years.Add(startYear);
                startYear -= 1;
            }
            ViewBag.Years = years;
            #endregion

            IQueryable<Reservation> revs = context.Reservations.Where(r => r.Driver == driver && (r.Status == "Ended" || r.Status == "Ended & Feedback Left")
            && r.PickUpDate.Year.ToString() == year);
            if (revs != null)
            {
                #region Get the months
                List<DriverMonthlyRev> monthlyRevs = new List<DriverMonthlyRev>();
                List<int> months = new List<int>();
                foreach (var rev in revs)
                {
                    if (!months.Contains(rev.PickUpDate.Month))
                    {
                        months.Add(rev.PickUpDate.Month);
                        monthlyRevs.Add(new DriverMonthlyRev() { Month = rev.PickUpDate.Month, MonthInText = GetMonth(rev.PickUpDate.Month) });
                    }
                }
                #endregion

                #region Get the monthly income and commission for the driver
                foreach (var rev in revs)
                {
                    foreach (var month in months)
                    {
                        if (rev.PickUpDate.Month == month)
                        {
                            foreach (var revM in monthlyRevs)
                            {
                                if (revM.Month == month)
                                {
                                    // Add the charge to the monthly income and commission
                                    revM.Income += rev.Charge;
                                    revM.Commission += rev.Charge * 90 / 100;
                                }
                            }
                        }
                    }
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

                ViewBag.CurrentDriver = driver;
                ViewBag.CurrentYear = year;
                ViewBag.CurrentFilter = searchString;
                #endregion

                #region Query for searching 
                if (!string.IsNullOrEmpty(searchString))
                {
                    monthlyRevs = monthlyRevs.Where(s => s.MonthInText.Contains(searchString) ||
                                                    s.Income.ToString().Contains(searchString) ||
                                                    s.Commission.ToString().Contains(searchString)).ToList();
                }
                #endregion

                ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");

                // Set pagination
                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(monthlyRevs.OrderByDescending(i => i.Month).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// POST: DriverRevenueCharts
        /// </summary>
        /// <returns></returns>
        public ActionResult DriverMonthlyRevenueCharts(string driver, string year)
        {
            ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");
            #region Get a list of years
            List<int> years = new List<int>();
            int startYear = DateTime.Now.Year;
            while (startYear >= DateTime.Now.AddYears(-5).Year)
            {
                years.Add(startYear);
                startYear -= 1;
            }
            ViewBag.Years = years;
            #endregion

            #region Chart instantiation
            Highcharts columnChart = new Highcharts("columnchart");
            Highcharts barChart = new Highcharts("barchart");
            Highcharts areaChart = new Highcharts("areaChart");
            Highcharts lineChart = new Highcharts("lineChart");
            Highcharts areasplineChart = new Highcharts("areasplineChart");
            Highcharts splineChart = new Highcharts("splineChart");
            Highcharts pieChart = new Highcharts("pieChart");
            Highcharts scatterChart = new Highcharts("scatterChart");
            #endregion

            #region Charts List
            var charts = new ChartsModel
            {
                ColumnChart = columnChart,
                BarChart = barChart,
                AreaChart = areaChart,
                LineChart = lineChart,
                AreasplineChart = areasplineChart,
                SplineChart = splineChart,
                PieChart = pieChart,
                ScatterChart = scatterChart
            };
            #endregion

            IQueryable<Reservation> revs = context.Reservations.Where(r => r.Driver == driver && (r.Status == "Ended" || r.Status == "Ended & Feedback Left")
            && r.PickUpDate.Year.ToString() == year);
            if (revs != null && revs.Count() != 0)
            {
                #region Get the months
                List<DriverMonthlyRev> monthlyRevs = new List<DriverMonthlyRev>();
                List<string> months = new List<string>();

                foreach (var rev in revs)
                {
                    if (!months.Contains(GetMonth(rev.PickUpDate.Month)))
                    {
                        months.Add(GetMonth(rev.PickUpDate.Month));
                        monthlyRevs.Add(new DriverMonthlyRev() { Month = rev.PickUpDate.Month, MonthInText = GetMonth(rev.PickUpDate.Month) });
                    }
                }
                #endregion

                #region Get the daily revenue for the driver
                foreach (var rev in revs)
                {
                    foreach (var month in months)
                    {
                        if (GetMonth(rev.PickUpDate.Month) == month)
                        {
                            foreach (var revM in monthlyRevs)
                            {
                                if (GetMonth(revM.Month) == month)
                                {
                                    // Add the charge to the monthly income and commission
                                    revM.Income += rev.Charge;
                                    revM.Commission += rev.Charge * 90 / 100;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Get monthly income and commission list
                List<object> income = new List<object>();
                List<object> commision = new List<object>();
                foreach (var rev in monthlyRevs)
                {
                    income.Add(rev.Income);
                    commision.Add(rev.Commission);
                }
                #endregion

                #region Initialization
                columnChart = InitializeChart(columnChart, ChartTypes.Column, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                barChart = InitializeChart(barChart, ChartTypes.Bar, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                areaChart = InitializeChart(areaChart, ChartTypes.Area, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                lineChart = InitializeChart(lineChart, ChartTypes.Line, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                areasplineChart = InitializeChart(areasplineChart, ChartTypes.Areaspline, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                splineChart = InitializeChart(splineChart, ChartTypes.Spline, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                pieChart = InitializeChart(pieChart, ChartTypes.Pie, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                scatterChart = InitializeChart(scatterChart, ChartTypes.Scatter, driver, months, commision, income, "Income", "Monthly Income & Commission Analysis", "Month", "Income & Commission", "Commission");
                ViewBag.Charts = true;
                #endregion
            }

            return View(charts);
        }

        /// <summary>
        /// GET: Daily Distance Coverage List
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult DriverDistance(string driver, DateTime? fromDate, DateTime? toDate, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData.Add("Driver", driver);
            IQueryable<Reservation> revs = context.Reservations.Where(r => r.Driver == driver && (r.Status == "Ended" || r.Status == "Ended & Feedback Left")
            && r.PickUpDate >= fromDate && r.PickUpDate <= toDate);
            if (revs != null)
            {
                #region Get the days
                List<DriverDailyDist> dailyDists = new List<DriverDailyDist>();
                List<DateTime> days = new List<DateTime>();

                foreach (var rev in revs)
                {
                    if (!days.Contains(rev.PickUpDate.Date))
                    {
                        days.Add(rev.PickUpDate.Date);
                        dailyDists.Add(new DriverDailyDist() { Date = rev.PickUpDate.Date });
                    }
                }
                #endregion

                #region Get the daily distance for the driver
                foreach (var rev in revs)
                {
                    foreach (var day in days)
                    {
                        if (rev.PickUpDate.Date == day)
                        {
                            foreach (var distM in dailyDists)
                            {
                                if (distM.Date.Date == day)
                                {
                                    // Add the OnHire and Full distance to the daily distance coverage
                                    distM.OnHire += rev.OnHireDistance;
                                    if (rev.OverallDistance == null)
                                    {
                                        rev.OverallDistance = 0;
                                    }
                                    distM.Full += (decimal)rev.OverallDistance;
                                }
                            }
                        }
                    }
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

                ViewBag.CurrentDriver = driver;
                if (fromDate != null && toDate != null)
                {
                    ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
                    ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
                }
                ViewBag.CurrentFilter = searchString;
                #endregion

                #region Query for searching 
                if (!string.IsNullOrEmpty(searchString))
                {
                    dailyDists = dailyDists.Where(s => s.OnHire.ToString().Contains(searchString) ||
                                                       s.Full.ToString().Contains(searchString) ||
                                                       s.Date.ToLongDateString().Contains(searchString)).ToList();
                }
                #endregion

                ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");

                // Set pagination
                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(dailyDists.OrderByDescending(i => i.Date).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// POST: Daily Distance Coverage Charts
        /// </summary>
        /// <returns></returns>
        public ActionResult DriverDistanceCharts(string driver, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");

            #region Chart instantiation
            Highcharts columnChart = new Highcharts("columnchart");
            Highcharts barChart = new Highcharts("barchart");
            Highcharts areaChart = new Highcharts("areaChart");
            Highcharts lineChart = new Highcharts("lineChart");
            Highcharts areasplineChart = new Highcharts("areasplineChart");
            Highcharts splineChart = new Highcharts("splineChart");
            Highcharts pieChart = new Highcharts("pieChart");
            Highcharts scatterChart = new Highcharts("scatterChart");
            #endregion

            #region Charts List
            var charts = new ChartsModel
            {
                ColumnChart = columnChart,
                BarChart = barChart,
                AreaChart = areaChart,
                LineChart = lineChart,
                AreasplineChart = areasplineChart,
                SplineChart = splineChart,
                PieChart = pieChart,
                ScatterChart = scatterChart
            };
            #endregion

            IQueryable<Reservation> revs = context.Reservations.Where(r => r.Driver == driver && (r.Status == "Ended" || r.Status == "Ended & Feedback Left")
            && r.PickUpDate >= fromDate && r.PickUpDate <= toDate);
            if (revs != null && revs.Count() != 0)
            {
                #region Get the dates
                List<DriverDailyDist> dailyDist = new List<DriverDailyDist>();
                List<string> dates = new List<string>();

                foreach (var rev in revs)
                {
                    if (!dates.Contains(rev.PickUpDate.Date.ToShortDateString()))
                    {
                        dates.Add(rev.PickUpDate.Date.ToShortDateString());
                        dailyDist.Add(new DriverDailyDist() { Date = rev.PickUpDate.Date });
                    }
                }
                #endregion

                #region Get the daily distance for the driver
                foreach (var rev in revs)
                {
                    foreach (var day in dates)
                    {
                        if (rev.PickUpDate.Date.ToShortDateString() == day)
                        {
                            foreach (var distM in dailyDist)
                            {
                                if (distM.Date.Date.ToShortDateString() == day)
                                {
                                    // Add the OnHire and Full distance to the daily distance coverage
                                    distM.OnHire += rev.OnHireDistance;
                                    if (rev.OverallDistance == null)
                                    {
                                        rev.OverallDistance = 0;
                                    }
                                    distM.Full += (decimal)rev.OverallDistance;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Get daily revenue list
                List<object> onHireDist = new List<object>();
                List<object> fullDist = new List<object>();
                foreach (var rev in dailyDist)
                {
                    onHireDist.Add(rev.OnHire);
                    fullDist.Add(rev.Full);
                }
                #endregion

                #region Initialization
                columnChart = InitializeChart(columnChart, ChartTypes.Column, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                barChart = InitializeChart(barChart, ChartTypes.Bar, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                areaChart = InitializeChart(areaChart, ChartTypes.Area, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                lineChart = InitializeChart(lineChart, ChartTypes.Line, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                areasplineChart = InitializeChart(areasplineChart, ChartTypes.Areaspline, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                splineChart = InitializeChart(splineChart, ChartTypes.Spline, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                pieChart = InitializeChart(pieChart, ChartTypes.Pie, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                scatterChart = InitializeChart(scatterChart, ChartTypes.Scatter, driver, dates, onHireDist, fullDist, "Full Distance", "Daily Distance Coverage Analysis", "Date", "On Hire & Full Distance (KM)", "On Hire Distance");
                ViewBag.Charts = true;
                #endregion
            }

            return View(charts);
        }

        /// <summary>
        /// GET: Driver Status for a time period
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult DriverStatus(string driver, DateTime? fromDate, DateTime? toDate, string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewData.Add("Driver", driver);
            IQueryable<DriverStatusCount> statusCounts = context.DriverStatusCounts.Where(d => d.Driver == driver && d.Date >= fromDate && d.Date <= toDate);
            if (statusCounts != null)
            {
                #region Get the days
                List<DriverStatusPercentage> statuspercentages = new List<DriverStatusPercentage>();
                List<DateTime> days = new List<DateTime>();

                foreach (var status in statusCounts)
                {
                    if (!days.Contains(status.Date))
                    {
                        days.Add(status.Date);
                        statuspercentages.Add(new DriverStatusPercentage() { Date = status.Date });
                    }
                }
                #endregion

                #region Get the status percentage for the driver
                foreach (var status in statusCounts)
                {
                    foreach (var day in days)
                    {
                        if (status.Date == day)
                        {
                            foreach (var percentM in statuspercentages)
                            {
                                if (percentM.Date.Date == day)
                                {
                                    // Get status percentage
                                    int value = status.OnlineCount + status.OfflineCount;
                                    percentM.OnlinePercentage = status.OnlineCount * 100 / value;
                                    percentM.OfflinePercentage = status.OfflineCount * 100 / value;
                                }
                            }
                        }
                    }
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

                ViewBag.CurrentDriver = driver;
                if (fromDate != null && toDate != null)
                {
                    ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
                    ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
                }
                ViewBag.CurrentFilter = searchString;
                #endregion

                #region Query for searching 
                if (!string.IsNullOrEmpty(searchString))
                {
                    statuspercentages = statuspercentages.Where(s => s.OnlinePercentage.ToString().Contains(searchString) ||
                                                       s.OfflinePercentage.ToString().Contains(searchString) ||
                                                       s.Date.ToLongDateString().Contains(searchString)).ToList();
                }
                #endregion

                ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");

                // Set pagination
                int pageSize = 5;
                int pageNumber = (page ?? 1);
                return View(statuspercentages.OrderByDescending(i => i.Date).ToPagedList(pageNumber, pageSize));
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// POST: Driver Status percentage Charts
        /// </summary>
        /// <returns></returns>
        public ActionResult DriverStatusCharts(string driver, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.Drivers = new SelectList(context.Users.Where(u => u.UserType == "Driver"), "Username", "Username");

            #region Chart instantiation
            Highcharts columnChart = new Highcharts("columnchart");
            Highcharts barChart = new Highcharts("barchart");
            Highcharts areaChart = new Highcharts("areaChart");
            Highcharts lineChart = new Highcharts("lineChart");
            Highcharts areasplineChart = new Highcharts("areasplineChart");
            Highcharts splineChart = new Highcharts("splineChart");
            Highcharts pieChart = new Highcharts("pieChart");
            Highcharts scatterChart = new Highcharts("scatterChart");
            #endregion

            #region Charts List
            var charts = new ChartsModel
            {
                ColumnChart = columnChart,
                BarChart = barChart,
                AreaChart = areaChart,
                LineChart = lineChart,
                AreasplineChart = areasplineChart,
                SplineChart = splineChart,
                PieChart = pieChart,
                ScatterChart = scatterChart
            };
            #endregion

            IQueryable<DriverStatusCount> statusCounts = context.DriverStatusCounts.Where(d => d.Driver == driver && d.Date >= fromDate && d.Date <= toDate);
            if (statusCounts != null)
            {
                #region Get the dates
                List<DriverStatusPercentage> statuspercentages = new List<DriverStatusPercentage>();
                List<string> dates = new List<string>();

                foreach (var status in statusCounts)
                {
                    if (status.Date == null)
                    {
                        status.Date = DateTime.Now.Date;
                    }
                    if (!dates.Contains(status.Date.ToShortDateString()))
                    {
                        dates.Add(status.Date.ToShortDateString());
                        statuspercentages.Add(new DriverStatusPercentage() { Date = status.Date });
                    }
                }
                #endregion

                #region Get the daily status percentage for the driver
                foreach (var status in statusCounts)
                {
                    foreach (var day in dates)
                    {
                        if (status.Date.ToShortDateString() == day)
                        {
                            foreach (var percentM in statuspercentages)
                            {
                                if (percentM.Date.ToShortDateString() == day)
                                {
                                    // Get status percentage
                                    int value = status.OnlineCount + status.OfflineCount;
                                    percentM.OnlinePercentage = status.OnlineCount * 100 / value;
                                    percentM.OfflinePercentage = status.OfflineCount * 100 / value;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Get daily revenue list
                List<object> onlineState = new List<object>();
                List<object> offlineState = new List<object>();
                foreach (var percentage in statuspercentages)
                {
                    onlineState.Add(percentage.OnlinePercentage);
                    offlineState.Add(percentage.OfflinePercentage);
                }
                #endregion

                #region Initialization
                columnChart = InitializeChart(columnChart, ChartTypes.Column, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                barChart = InitializeChart(barChart, ChartTypes.Bar, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                areaChart = InitializeChart(areaChart, ChartTypes.Area, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                lineChart = InitializeChart(lineChart, ChartTypes.Line, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                areasplineChart = InitializeChart(areasplineChart, ChartTypes.Areaspline, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                splineChart = InitializeChart(splineChart, ChartTypes.Spline, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                pieChart = InitializeChart(pieChart, ChartTypes.Pie, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                scatterChart = InitializeChart(scatterChart, ChartTypes.Scatter, driver, dates, onlineState, offlineState, "Offline percentage", "Driver Status Analysis", "Date", "percentage", "Online percentage");
                ViewBag.Charts = true;
                #endregion
            }

            return View(charts);
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
        /// Get the months in text
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        [NonAction]
        public string GetMonth(int month)
        {
            string monthInText = String.Empty;

            #region Get months in text form
            if (month == 1) monthInText = "January";
            else if (month == 2) monthInText = "February";
            else if (month == 3) monthInText = "March";
            else if (month == 4) monthInText = "April";
            else if (month == 5) monthInText = "May";
            else if (month == 6) monthInText = "June";
            else if (month == 7) monthInText = "July";
            else if (month == 8) monthInText = "Auguest";
            else if (month == 9) monthInText = "September";
            else if (month == 10) monthInText = "October";
            else if (month == 11) monthInText = "November";
            else if (month == 12) monthInText = "December";
            #endregion

            return monthInText;
        }

        /// <summary>
        /// Chart initialization
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="driver"></param>
        /// <param name="xVal"></param>
        /// <param name="yVal"></param>
        /// <returns></returns>
        [NonAction]
        public Highcharts InitializeChart(Highcharts chart, ChartTypes type, string driver, List<string> xVal, List<object> yVal, List<object> yVal2, string series2,
            string title = "Daily Revenue Analysis", string xAxis = "Date", string yAxis = "Revenue", string series1 = "Revenue")
        {
            #region Get names and values into one array for the first series
            object[] names = xVal.ToArray();
            object[] vals = yVal.ToArray();
            object[,] namesAndVals = new object[names.Count(), 2];
            for (int i = 0; i < names.Count(); i++)
            {
                namesAndVals[i, 0] = names[i];
                namesAndVals[i, 1] = vals[i];
            }
            #endregion

            #region Get names and values into one array for the second series
            object[] sNames = xVal.ToArray();
            object[,] sNamesAndVals = new object[sNames.Count(), 2];
            if (yVal2 != null)
            {
                object[] sVals = yVal2.ToArray();
                for (int i = 0; i < sNames.Count(); i++)
                {
                    sNamesAndVals[i, 0] = sNames[i];
                    sNamesAndVals[i, 1] = sVals[i];
                }
            }
            #endregion

            // Chart basic customizations
            chart.InitChart(new Chart()
            {
                Type = type,
                BackgroundColor = new BackColorOrGradient(System.Drawing.Color.AntiqueWhite),
                Style = "fontWeight: 'bold', fontSize: '17px'",
                BorderColor = System.Drawing.Color.Orange,
                BorderRadius = 0,
                BorderWidth = 2,
            });

            // Chart title
            chart.SetTitle(new Title()
            {
                Text = title
            });

            // Chart subtitle
            chart.SetSubtitle(new Subtitle()
            {
                Text = $"For | { driver }"
            });

            // Set chart XAxis
            chart.SetXAxis(new XAxis()
            {
                Type = AxisTypes.Category,
                Title = new XAxisTitle()
                {
                    Text = xAxis,
                    Style = "fontWeight: 'bold', fontSize: '17px'"
                },
                Categories = xVal.ToArray()
            });

            // Set chart YAxis
            chart.SetYAxis(new YAxis()
            {
                Title = new YAxisTitle()
                {
                    Text = yAxis,
                    Style = "fontWeight: 'bold', fontSize: '17px'"
                },
                ShowFirstLabel = true,
                ShowLastLabel = true,
                Min = 0
            });

            // Set Plot Options for Pie chart
            chart.SetPlotOptions(new PlotOptions()
            {
                Pie = new PlotOptionsPie()
                {
                    ShowInLegend = true,
                    AllowPointSelect = true,
                    Cursor = Cursors.Default,
                    DataLabels = new PlotOptionsPieDataLabels
                    {
                        // return both name and percentage
                        Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.percentage.toFixed(2) +' %'; }"
                    }
                }
            });

            // Set chart series values
            if (type == ChartTypes.Pie || series2 == null)
            {
                chart.SetSeries(new Series[]
                {
                new Series{

                    Name = series1,
                    Data = new Data(namesAndVals),
                    Color = System.Drawing.Color.Orange
                },
                });
            }
            else if (series2 != null)
            {
                chart.SetSeries(new Series[]
                {
                    new Series{

                        Name = series2,
                        Data = new Data(sNamesAndVals),
                        Color = System.Drawing.Color.DodgerBlue
                    },
                    new Series{

                        Name = series1,
                        Data = new Data(namesAndVals),
                        Color = System.Drawing.Color.Orange
                    },
                });
            }

            return chart;
        }
    }
}