using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using NYC_Taxi_System.Models;
using PagedList;
using System.Net.Mail;
using System.Web.Security;
using System.Web;
using NYC_Taxi_System.Helpers;

namespace NYC_Taxi_System.Controllers
{
    /// <summary>
    /// Contains the actions of User controller
    /// </summary>
    public class UsersController : ApplicationBaseController
    {
        private ABC_DBEntities context = new ABC_DBEntities();

        /// <summary>
        /// GET: Operators List
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
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

            var operators = context.Users.Where(c => c.UserType == "Operator");

            #region Query for searching 
            if (!string.IsNullOrEmpty(searchString))
            {
                operators = operators.Where(s => s.Username.Contains(searchString)
                                       || s.UserType.Contains(searchString)
                                       || s.FirstName.Contains(searchString)
                                       || s.LastName.Contains(searchString)
                                       || s.Phone.Contains(searchString)
                                       || s.Email.Contains(searchString));
            }
            #endregion

            // Set pagination
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(operators.OrderBy(i => i.Username).ToPagedList(pageNumber, pageSize));
        }

        /// <summary>
        /// GET: User/Details/id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await context.Users.FindAsync(id);
            if (user == null)
            {
                // No User found
                return HttpNotFound();
            }
            return View(user);
        }

        /// <summary>
        /// GET: Users/Create
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Users/Create
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Exclude = "IsEmailVerified,ActivationCode,ResetPasswordCode")] User user)
        {
            string password = user.Password; // To get the unencrypted Password before hashing
            bool status = false;
            string message = "";

            // Model Validation
            if (ModelState.IsValid)
            {
                #region UserName Validation
                var isUserExist = IsUserNameExist(user.Username);
                if (await isUserExist)
                {
                    // User exists
                    ModelState.AddModelError("UserExist", "The 'User Name' entered already exist");
                    return View(user);
                }
                #endregion

                #region Email Validation
                var isEmailExist = IsEmailExist(user.Email);
                if (await isEmailExist)
                {
                    // Email exists
                    ModelState.AddModelError("EmailExist", "The 'Email' entered already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); //To match the Password and Confirm Password
                #endregion
                user.IsEmailVerified = false;

                #region Save User to the DataBase
                context.Users.Add(user);
                await context.SaveChangesAsync();
                #endregion

                #region Send Email to the User
                SendEmail(user.Email, user.ActivationCode.ToString(), user.FirstName, user.Username, password);
                message = $"Registration completed successfully. Account activation link has been sent to your email address:";
                ViewData.Add("Email", user.Email);
                status = true;
                #endregion                
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(user);
        }

        /// <summary>
        /// GET: User/Edit/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await context.Users.FindAsync(id);
            if (user == null)
            {
                // No User found
                return HttpNotFound();
            }
            return View(user);
        }

        /// <summary>
        /// POST: User/Edit/5
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Exclude = "IsEmailVerified,ActivationCode,ResetPasswordCode")] User user)
        {
            string password = user.Password; // To get the unencrypted Password before hashing
            bool status = false;
            string message = "";

            // Model Validation
            if (ModelState.IsValid)
            {
                #region Email Validation
                var userEmail = await context.Users.SingleOrDefaultAsync(a => a.Username == user.Username);
                var isEmailExist = IsEmailExist(user.Email);
                if (userEmail.Email != user.Email && await isEmailExist)
                {
                    ModelState.AddModelError("EmailExist", "The 'Email' entered already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); //To match the Password and Confirm Password
                #endregion
                user.IsEmailVerified = false;

                #region Save data to the DataBase
                using (ABC_DBEntities context = new ABC_DBEntities())
                {
                    context.Entry(user).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                }
                #endregion

                #region Send Email to the User
                SendEmail(user.Email, user.ActivationCode.ToString(), user.FirstName, user.Username, password, "VerifyUpdate");
                message = $"Account updated successfully. Account verification link has been sent to your email address:";
                ViewData.Add("Email", user.Email);
                status = true;
                #endregion                
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = status;
            FormsAuthentication.SignOut();
            return View(user);
        }

        /// <summary>
        /// GET: User/Delete/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                // No id
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await context.Users.FindAsync(id);
            if (user == null)
            {
                // No User found
                return HttpNotFound();
            }
            return View(user);
        }

        /// <summary>
        /// POST: User/Delete/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var reservations = await context.Reservations.Where(r => r.User_Driver.Username == id).ToListAsync();
            if (reservations.Count() != 0)
            {
                // Reservation record(s) found for this account
                for (int i = 0; i < reservations.Count(); i++)
                {
                    // Loop through each reservation record and remove the driver
                    reservations[i].Driver = null;
                }
            }

            User user = await context.Users.FindAsync(id);
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return null;
        }

        /// <summary>
        /// Verify Account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> VerifyAccount(string id)
        {
            bool status = false;
            context.Configuration.ValidateOnSaveEnabled = false; // To avoid Confirm Password does not match issue on save changes
            var user = await context.Users.FirstOrDefaultAsync(a => a.ActivationCode == new Guid(id));
            if (user != null)
            {
                user.IsEmailVerified = true;
                user.ActivationCode = null;
                await context.SaveChangesAsync();
                status = true;
            }
            else
            {
                ViewBag.Message = "Invalid Request";
            }
            ViewBag.Status = status;
            return View();
        }

        /// <summary>
        /// Verify Account Update
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> VerifyUpdate(string id)
        {
            bool status = false;
            context.Configuration.ValidateOnSaveEnabled = false; // To avoid Confirm Password does not match issue on save changes
            var user = await context.Users.FirstOrDefaultAsync(a => a.ActivationCode == new Guid(id));
            if (user != null)
            {
                user.IsEmailVerified = true;
                user.ActivationCode = null;
                await context.SaveChangesAsync();
                status = true;
            }
            else
            {
                ViewBag.Message = "Invalid Request";
            }
            ViewBag.Status = status;
            return View();
        }

        /// <summary>
        /// Login Action
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Login Post Action
        /// </summary>
        /// <param name="login"></param>
        /// <param name="ReturnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserLogin login, string ReturnUrl = "")
        {
            string message = "";
            var v = await context.Users.SingleOrDefaultAsync(a => a.Email == login.UsernameOrEmail || a.Username == login.UsernameOrEmail);
            if (v != null)
            {
                if (!v.IsEmailVerified)
                {
                    ViewBag.Message = "Please verify your email before attempt to log in";
                    return View();
                }
                if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                {
                    int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                    var ticket = new FormsAuthenticationTicket(login.UsernameOrEmail, login.RememberMe, timeout);
                    string encrypted = FormsAuthentication.Encrypt(ticket);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
                    {
                        Expires = DateTime.Now.AddMinutes(timeout),
                        HttpOnly = true
                    };
                    Response.Cookies.Add(cookie);

                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    message = "Invalid credentials";
                }
            }
            else
            {
                message = "Invalid credentials";
            }
            ViewBag.Message = message;
            return View();
        }

        /// <summary>
        /// Logout Action
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Forgot Password Action
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Forgot Password Post Action
        /// </summary>
        /// <param name="forgotPassword"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(FindAccount forgotPassword)
        {
            string message = "";
            bool status = false;

            var account = await context.Users.SingleOrDefaultAsync(a => a.Email == forgotPassword.UsernameOrEmail || a.Username == forgotPassword.UsernameOrEmail);
            if (account != null)
            {
                #region Generate Reset Password Code
                account.ResetPasswordCode = Guid.NewGuid();
                #endregion

                #region Send Verification Email
                SendEmail(account.Email, account.ResetPasswordCode.ToString(), account.FirstName, "", "", "ResetPassword");
                #endregion

                context.Configuration.ValidateOnSaveEnabled = false; // To avoid Confirm Password does not match issue on save changes
                await context.SaveChangesAsync();
                status = true;
                message = $"Reset Password link has been sent to your email address:";
                ViewData.Add("Email", account.Email);
            }
            else
            {
                message = "Account not found";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
        }

        /// <summary>
        /// Reset Password Action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> ResetPassword(string id)
        {
            // Verify the reset password link
            // Find the account which is associated with the link
            // Redirect to reset password page
            if (id != null)
            {
                var user = await context.Users.FirstOrDefaultAsync(a => a.ResetPasswordCode == new Guid(id));
                if (user != null)
                {
                    ResetPassword model = new ResetPassword
                    {
                        ResetCode = new Guid(id)
                    };
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Reset Password Post Action
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPassword model)
        {
            string password = model.NewPassword; // To get the New Password before hashing
            var message = "";
            var status = false;
            if (ModelState.IsValid)
            {
                var user = await context.Users.FirstOrDefaultAsync(a => a.ResetPasswordCode == model.ResetCode);
                if (user != null)
                {
                    user.Password = Crypto.Hash(model.NewPassword);
                    user.ResetPasswordCode = null;

                    context.Configuration.ValidateOnSaveEnabled = false;
                    await context.SaveChangesAsync();

                    #region Send Email to the User
                    SendEmail(user.Email, "", user.FirstName, user.Username, password, "ResetPasswordSuccess");
                    message = "Password has been updated successfully!";
                    status = true;
                    #endregion
                }
            }
            else
            {
                message = "Invalid Request";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View(model);
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
        /// Check if the Username already exist in the database
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [NonAction]
        public async Task<bool> IsUserNameExist(string userName)
        {
            var v = await context.Users.FindAsync(userName);
            return v != null;
        }

        /// <summary>
        /// Check if the Email already exist in the database
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [NonAction]
        public async Task<bool> IsEmailExist(string email)
        {
            var v = await context.Users.FirstOrDefaultAsync(a => a.Email == email);
            return v != null;
        }

        /// <summary>
        /// Send Verification Emails
        /// </summary>
        /// <param name="email"></param>
        /// <param name="activationCode"></param>
        /// <param name="fName"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        [NonAction]
        public void SendEmail(string email, string activationCode, string fName, string username, string password, string emailFor = "VerifyAccount")
        {
            // Link for account activation
            var verifyUrl = $"/Users/{ emailFor }/{ activationCode }";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            // Link for login
            var loginUrl = "/Users/Login";
            var loginLink = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, loginUrl);

            var fromEmail = new MailAddress("noreply.nyc.taxi@gmail.com", "NYC Taxi");
            var fromEmailPassword = "ddczdxsdymtzzyps";
            var toEmail = new MailAddress(email);

            string subject = "";
            string body = "";
            if (emailFor == "VerifyAccount")
            {
                subject = "Your account has been created successfully!";
                body = $"<br/>Dear { fName }," +
                    "<br/><br/>We are excited to inform you that your NYC Taxi account has been created successfully. " +
                    "Please click the link below to activate your account," +
                    $"<br/><a href = '{ link }'> Click here to activate your account</a>" +
                    $"<br/><br/><font size='{ 4 }'>Login details:</font>" +
                    $"<br/><b>User Name:</b> { username }" +
                    $"<br/><b>Password:</b> { password }" +
                    $"<br/><br/>Copyright © { DateTime.Now.Year } - NYC Taxi. All rights reserved.";
            }
            if (emailFor == "VerifyUpdate")
            {
                subject = "Your account details have been updated successfully!";
                body = $"<br/>Dear { fName }," +
                    "<br/><br/>Your NYC Taxi account details have been updated successfully. " +
                    "Please click the link below to verify your account," +
                    $"<br/><a href = '{ link }'> Click here to verify your account</a>" +
                    $"<br/><br/><font size='{ 4 }'>Login details:</font>" +
                    $"<br/><b>User Name:</b> { username }" +
                    $"<br/><b>Password:</b> { password }" +
                    "<br/><br/>If you did not update your account details recently, your NYC Taxi account might be at risk, please contact customer support." +
                    $"<br/><br/>Copyright © { DateTime.Now.Year } - NYC Taxi Company. All rights reserved.";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = $"<br/>Dear { fName }," +
                    "<br/><br/>We got a request to reset your NYC Taxi account password. " +
                    "Please click the link below to reset your password," +
                    $"<br/><a href = '{ link }'> Click here to reset your password</a>" +
                    "<br/><br/>If you did not request a password reset, please ignore this email." +
                    $"<br/><br/>Copyright © { DateTime.Now.Year } - NYC Taxi Company. All rights reserved.";
            }
            else if (emailFor == "ResetPasswordSuccess")
            {
                subject = "Your Password has been updated successfully!";
                body = $"<br/>Dear { fName }," +
                    "<br/><br/>We have successfully updated your NYC Taxi account password. " +
                    $"<br/><a href = '{ loginLink }'> Click here to Login</a>" +
                    $"<br/><br/><font size='{ 4 }'>Login details:</font>" +
                    $"<br/><b>User Name:</b> { username }" +
                    $"<br/><b>New Password:</b> { password }" +
                    "<br/><br/>If you did not reset your password recently, your NYC Taxi account might be at risk, please contact customer support." +
                    $"<br/><br/>Copyright © { DateTime.Now.Year } - NYC Taxi Company. All rights reserved.";
            }

            // Create the smtp client
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            // Create the message
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message); // Send email
        }
    }
}