using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
//using project.Data;
using project.Models;
//using project.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using project.AuthData;
using Dal;
using Dal.Data;
using Dal.Models;
using Dal.ViewModels;
using Bal.Leaves;

namespace project.Controllers
{
    public class StartingController : Controller
    {
        private readonly HelperlandContextData _DbContext;
        private readonly UserManager<Users> userManager;
        private readonly SignInManager<Users> signInManager;
        private readonly ILogger<StartingController> logger;
        //private readonly RoleManager<IdentityRole> roleManager;
        private readonly RoleManager<IdentityRole<int>> roleManager;
        private readonly ILeaveServices _leaveservices;
        private readonly IDeleteTab _deleteTab;
        private readonly IUpdateLeave _updateleave;
        private readonly ILeavePageSearch _leavepagesearch;
        private readonly ILeavePageClear _leavepageclear;
        private readonly IAddSalary _addsalary;
        private readonly IGiveSalary _givesalary;
        private readonly ISeeSalary _seesalary;
        private readonly IAdminaddsEmp _adminaddemployee;
        private readonly ILeavePageHome _leavepagehome;

        public StartingController(UserManager<Users> userManager,
                                  SignInManager<Users> signInManager,
                                  ILogger<StartingController> logger,
                                  RoleManager<IdentityRole<int>> roleManager,
                                  HelperlandContextData DbContext,
                                  ILeaveServices leaveservices,
                                  IDeleteTab deleteTab,
                                  IUpdateLeave updateleave,
                                  ILeavePageSearch leavepagesearch,
                                  ILeavePageClear leavepageclear,
                                  IAddSalary addsalary,
                                  IGiveSalary givesalary,
                                  ISeeSalary seesalary,
                                  IAdminaddsEmp adminaddemployee,
                                  ILeavePageHome leavepagehome)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.roleManager = roleManager;
            _DbContext = DbContext;
            _leaveservices = leaveservices;
            _deleteTab = deleteTab;
            _updateleave = updateleave;
            _leavepagesearch = leavepagesearch;
            _leavepageclear = leavepageclear;
            _addsalary = addsalary;
            _givesalary = givesalary;
            _seesalary = seesalary;
            _adminaddemployee = adminaddemployee;
            _leavepagehome = leavepagehome;
        }

        //public StartingController(HelperlandContext DbContext)
        //{
        //    _DbContext = DbContext;
        //}
        [AuthorizeActionFilter("Read")]
        //[CustomAction]
        public IActionResult about()
        {
            return View();
        }
        [AuthorizeActionFilter("Write")]
        public IActionResult Faqs()
        {
            return View();
        }

        public IActionResult GenerateError()
        {
            throw new NotImplementedException();
        }

        public IActionResult CustomError()
        {
            return View();
        }
        [HttpGet]
        public IActionResult newaccount()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> newaccount(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users { UserName = model.Email, Email = model.Email, FirstName=model.FirstName };
                var result = await userManager.CreateAsync(user,model.Password);

                if (result.Succeeded)
                {
                    //var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    //var confirmationLink = Url.Action("confirmEmail", "Starting",
                    //                    new { userId = user.Id, token = token}, Request.Scheme);
                    //logger.Log(LogLevel.Warning,confirmationLink);

                    //ViewBag.ErrorTitle = "Registration Successful!";
                    //ViewBag.ErrorMessage = "Before you Login, please confirm your email by clicking on the link we have emailed you";
                    //return View("smallDisplay");
                    userManager.AddToRoleAsync(user, "Employee").Wait();
                    await signInManager.SignInAsync(user, isPersistent: false);
                    string code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("confirmemail", "Starting",
                                                                         new { UserId = user.Id, code = code });
                    string baseUrl ="Please confirm your account by clicking here:  " +
                        string.Format("{0}://{1}", HttpContext.Request.Scheme, HttpContext.Request.Host) + callbackUrl;

                    MailMessage ms = new MailMessage();
                    ms.To.Add(model.Email);
                    ms.From = new MailAddress("ravi.smith.1326@gmail.com");
                    ms.Subject = "Account Confirmation link";
                    ms.Body = baseUrl;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    smtp.Port = 587;


                    NetworkCredential NetworkCred = new NetworkCredential("ravi.smith.1326@gmail.com", "Sandwich#");
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Send(ms);
                    ViewBag.Message = "mail has been sent successfully ";
                    //return RedirectToAction("ForgotPasswordConfirmation");

                    return RedirectToAction("AccountConfirmation");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public IActionResult AccountConfirmation()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult>confirmemail(string code, string email, int UserId)
        {
            var x = UserId.ToString();
            var user = await userManager.FindByIdAsync(x);
            if(user == null)
            {
                return View("smallDisplay");
            }
            var result = await userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded?"confirmemail":"smallDisplay");
        }

        //[HttpGet][HttpPost]
        [AcceptVerbs("Get","Post")]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use.");
            }
        }


        [HttpPost]
        public async Task<IActionResult> logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index","Starting");
        }

        [HttpGet]
        [CustomAction]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if(user!=null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user,model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet!");
                    return View(model);
                }
                //var user = new IdentityUser { UserName = (model.FirstName), Email = model.Email };
                var result = await signInManager.PasswordSignInAsync(model.Email,model.Password,model.RememberMe,false);


                if (result.Succeeded)
                {
                    //await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("homepage");
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Credentials");
                
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult forgotpwd()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            //string resetCode = Guid.NewGuid().ToString();
            //var verifyUrl = "/Account/ResetPassword/" + resetCode;
            // var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            //var X = _DbContext.Userr.FirstOrDefault((System.Linq.Expressions.Expression<Func<Userr, bool>>)((Userr p) => (bool)p.Email.Equals((string)model.Email))).UserId;
            //string baseUrl = string.Format("{0}://{1}", HttpContext.Request.Scheme, HttpContext.Request.Host);
            //var activationUrl = $"{baseUrl}/Starting/ForgotPwd?UserId={X}";

            //var get_user = _DbContext.Userr.FirstOrDefault((System.Linq.Expressions.Expression<Func<Userr, bool>>)(p => (bool)p.Email.Equals((string)model.Email)));

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null /*|| !(await UserManager.IsEmailConfirmedAsync(user.Id))*/)
                {
                    return View("smallDisplay");
                }

                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                //    var encodedToken = Encoding.UTF8.GetBytes(code);
                //    var validToken = WebEncoders.Base64UrlEncode(encodedToken);
                var callbackUrl = Url.Action("resetpwd", "Starting",
            new { UserId = user.Id, code = code });
            //    await userManager.SendEmailAsync(user.Id, "Reset Password",
            //"Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                //string url = $"{_configuration["AppUrl"]}/ResetPassword?email={email}&token={validToken}";
                string baseUrl = "Please reset your password by clicking here: " + string.Format("{0}://{1}", HttpContext.Request.Scheme, HttpContext.Request.Host) 
                    + callbackUrl 
                    + "\n Your Reset Password link will expire in 1 minute" ;
                

                //    return View("ForgotPasswordConfirmation");
                MailMessage ms = new MailMessage();
                ms.To.Add(model.Email);
                ms.From = new MailAddress("ravi.smith.1326@gmail.com");
                ms.Subject = "Forgot Password reset link";
                ms.Body = baseUrl;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                smtp.Port = 587;


                NetworkCredential NetworkCred = new NetworkCredential("ravi.smith.1326@gmail.com", "Sandwich#");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Send(ms);
                ViewBag.Message = "mail has been sent successfully ";
                return RedirectToAction("ForgotPasswordConfirmation");
           
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        public IActionResult resetpwd(string code, string email, int UserId)
        {
            var model = new ResetPasswordModel { Token = code, Email = email, Id= UserId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> resetpwd(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
            {
                return View(resetPasswordModel);
            }
            var x = (resetPasswordModel.Id).ToString();
            var user = await userManager.FindByIdAsync(x);
            if (user == null)
            {
              return RedirectToAction("ResetPasswordConfirmation");
            }
            var resetPassResult = await userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }
            return RedirectToAction("ResetPasswordConfirmation");
        }
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        //}
        //[HttpPost]
        //public async Task<IActionResult> forgotpwd(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await userManager.FindByEmailAsync(model.Email);
        //        if (user != null)
        //        {
        //            var token = await userManager.GeneratePasswordResetTokenAsync(user);
        //        }
        //    }
        //    return View(model);
        //}

        public IActionResult homepage()
        {
            int userId = int.Parse(userManager.GetUserId(User));
            
            if (User.IsInRole("Manager"))
            {
                var query1 = (from s in _DbContext.leave
                              join y in _DbContext.Users
                              on s.userid equals y.Id
                              //let fromDate = DateTime.ParseExact(s.fromDate,"yyyyMMdd",null)
                              where
                              (y.managerid == userId) &&
                              //(DateTime.Parse(s.fromDate.ToString()).ToString("yyyy-MM-dd") == DateTime.Today.ToString("yyyy-MM-dd")) 
                              //DateTime.Parse(s.fromDate.Value.ToShortDateString()).ToString("yyyy-MM-dd") == "2022-05-18"
                              (s.fromDate.Value.Date == DateTime.Now.Date)
                              //(s.fromDate == DateTime.Now)
                              && (s.statusid == 2)
                              select s
                              ).ToList();
                ViewBag.onleave = query1.Count();
                return View(query1);
            }
            else if (User.IsInRole("Employee"))
            {
                var query2 = (from s in _DbContext.leave
                              where (s.userid == userId) && (s.statusid == 2) && (s.fromDate.Value.Date.Month == DateTime.Now.Date.Month)
                              select s
                              ).ToList();
                
                ViewBag.onleave2 = query2.Count();
                return View(query2);
            }
            else if (User.IsInRole("Admin"))
            {
                var query3 = (from s in _DbContext.leave
                              where (s.roleid == 2) && (s.statusid == 2) && (s.fromDate.Value.Date == DateTime.Now.Date)
                              select s).ToList();
                var query4 = (from s in _DbContext.leave
                              where (s.roleid == 3) && (s.statusid == 2) && (s.fromDate.Value.Date == DateTime.Now.Date)
                              select s).ToList();
                var query5 = (from h in _DbContext.holidays
                              where h.onDate.Date.Month == DateTime.Now.Date.Month
                              select h).ToList();
                ViewBag.onleave3 = query3.Count();
                ViewBag.onleave4 = query4.Count();
                ViewBag.onleave5 = query5.Count();
                return View(query4);
            }
            return View();
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public IActionResult listroles()
        {
            var roles = roleManager.Roles.ToList();
            return View(roles);
        }


        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> editrole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if(role == null)
            {
                ViewBag.ErrorMessage = $"Role with id = {id} cannot be found";
                return View("notfound");
            }
            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };
            foreach (var user in userManager.Users)
            {
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }
            return View(model);
        }

        public IActionResult addleaverequest()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> leavepage(string currentFilter, string searchString, int? pageNumber)
        {
            int userId = int.Parse(userManager.GetUserId(User));
            ViewData["CurrentFilter"] = searchString;
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            bool isManager = User.IsInRole("Manager");
            bool isAdmin = User.IsInRole("Admin");
            bool isEmployee = User.IsInRole("Employee");
            List<leave> result = _leavepagehome.leavepagehomedata(isManager,isAdmin,isEmployee,userId);
            return View(result);
            //if (User.IsInRole("Manager"))
            //{
            //    var students = ((from s in _DbContext.Users
            //                    join u in _DbContext.leave
            //                    on s.Id  equals u.userid
            //                    where s.managerid == userId
            //                     select new leave
            //                     {
            //                         leaveid = u.leaveid,
            //                         fromDate = u.fromDate,
            //                         toDate = u.toDate,
            //                         statusid = u.statusid,
            //                         userid = u.userid,
            //                         roleid = u.roleid
            //                     }).Union(
            //                        from s in _DbContext.Users
            //                        join u in _DbContext.leave
            //                        on s.Id equals u.userid
            //                        where s.Id == userId
            //                      select new leave { 
            //                        leaveid = u.leaveid,
            //                        fromDate = u.fromDate,
            //                        toDate = u.toDate,
            //                        statusid = u.statusid,
            //                          userid = u.userid,
            //                          roleid = u.roleid
            //                      })).ToList();

                
            //    return View(students);
            //}
            //else if (User.IsInRole("Admin"))
            //{
            //    //List<UserAddress> u = _DbContext.UserAddress.Where(x => x.UserId == ty).ToList();
            //    List<leave> l = _DbContext.leave.ToList();
            //    return View(l);
            //}
            //else
            //{

            //    var students = (from s in _DbContext.leave
            //               where s.userid == userId 
            //                select s);
            //    return View(students);
            //}
            //if (!String.IsNullOrEmpty(searchString))
            //{
            //    students = students.Where(s => s.reason.Contains(searchString)
            //                           /*|| s.lastname.Contains(searchString)*/);
            //}
            //List<leave> x = _DbContext.leave.Where(z => z.userid == userId).ToList();
            //return View(x);
            //int pageSize = 2;
            //return View(await PaginatedList<leave>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpPost]
        public IActionResult salary(DateTime monthyear)
        {
            
            int userId = int.Parse(userManager.GetUserId(User));
            bool isManager = User.IsInRole("Manager");
            List<salary> result = _seesalary.viewsalary(isManager,userId,monthyear,_DbContext);
            ViewBag.msg1 = monthyear.ToString("yyyy-MM");
            return View(result);
            //if (User.IsInRole("Manager"))
            //{
            //    var data = (from u in _DbContext.Users
            //                join s in _DbContext.salary
            //                on u.Id equals s.userid
            //                where u.managerid == userId
            //                select s).ToList();
            //    var result1 = data.Where(x => x.createddate == monthyear).ToList();
            //    ViewBag.msg1 = monthyear.ToString("yyyy-MM");
            //    return View(result1);
            //}
            //var query = (from s in _DbContext.salary
            //             where (s.createddate == monthyear)
            //             select s).ToList();
            //ViewBag.msg1 = monthyear.ToString("yyyy-MM");
            //return View(query);
        }

        [HttpPost]
        public IActionResult leavepage(DateTime startdate, DateTime enddate,int? statusids,Users user/*,int? id*/)
        {
            //var abc = id;
            DateTime xy = DateTime.Parse("01-01-0001 12:00:00 AM");
            DateTime uy = DateTime.Parse("01-01-9999 12:00:00 AM");
            if (enddate == xy)
            {
                enddate = DateTime.Parse("01-01-9999 12:00:00 AM");
            }
            int userId = int.Parse(userManager.GetUserId(User));
            if (statusids == null)
            {
                ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
            }
            else if(statusids != null)
            {
                ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
            }
            
            bool isManager = User.IsInRole("Manager");
            bool isAdmin = User.IsInRole("Admin");
            bool isEmployee = User.IsInRole("Employee");
            
            List<leave> result = _leavepagesearch.leavepage(startdate,enddate,statusids, userId, isManager, isAdmin, isEmployee);
            return View(result);

            //if (User.IsInRole("Manager"))
            //{
            //    var students = ((from s in _DbContext.Users
            //                     join u in _DbContext.leave
            //                     on s.Id equals u.userid
            //                     where s.managerid == userId
            //                     select new leave
            //                     {
            //                         leaveid = u.leaveid,
            //                         fromDate = u.fromDate,
            //                         toDate = u.toDate,
            //                         statusid = u.statusid,
            //                         userid = u.userid,
            //                         roleid = u.roleid
            //                     }).Union(
            //                        from s in _DbContext.Users
            //                        join u in _DbContext.leave
            //                        on s.Id equals u.userid
            //                        where s.Id == userId
            //                        select new leave
            //                        {
            //                            leaveid = u.leaveid,
            //                            fromDate = u.fromDate,
            //                            toDate = u.toDate,
            //                            statusid = u.statusid,
            //                            userid = u.userid,
            //                            roleid = u.roleid
            //                        })).ToList();
            //    if (statusids == null)
            //    {
            //        var result1 = students.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
            //        ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
            //        ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
            //        return View(result1);
            //    }
            //    else if (startdate == xy && enddate==xy)
            //    {
            //        var result2 = students.Where(x => (x.statusid == statusids)).ToList();
            //        //ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
            //        //ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
            //        return View(result2);
            //    }
            //    var result = students.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate) && (x.statusid == statusids)).ToList();
            //    ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
            //    ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
            //    return View(result);
            //}

            //else if (User.IsInRole("Admin"))
            //{
            //    List<leave> leavedates = _DbContext.leave.ToList();
            //    if (statusids == null)
            //    {
            //        var result1 = leavedates.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
            //        ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
            //        ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
            //        return View(result1);
            //    }
            //    else if (startdate == xy && enddate == uy)
            //    {
            //        var result2 = leavedates.Where(x => (x.statusid == statusids)).ToList();
            //        if(statusids == 1)
            //        {
            //            ViewBag.status = "Pending";
            //        }
            //        else if(statusids == 2)
            //        {
            //            ViewBag.status = "Accepted";
            //        }
            //        else if(statusids == 3)
            //        {
            //            ViewBag.status = "Rejected";
            //        }
            //        return View(result2);
            //    }
            //    var result = leavedates.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate) && (x.statusid == statusids)).ToList();
            //    ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
            //    ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
            //    return View(result);
            //}

            //if  (statusids == null)
            //{
            //    //var query1 = (from x in _DbContext.leave
            //    //              where (x.userid == userId)
            //    //              select x
            //    //         ).ToList();
            //    var query1 = (from x in _DbContext.leave
            //                  where ((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId))                              
            //                  select x
            //             ).ToList();
            //    return View(query1);
            //}
            //var query = (from x in _DbContext.leave
            //             where ((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId) && (x.statusid == statusids))  
            //             //((x.statusid == searchstatus) && (x.userid == userId)) ||
            //             //((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId))
            //             select x
            //             ).ToList();
            //return View(query);
        }

                
        public IActionResult salaryclear()
        {
            int userId = int.Parse(userManager.GetUserId(User));

            if (User.IsInRole("Manager"))
            {
                //var data = (from u in _DbContext.Users
                //            join s in _DbContext.salary
                //            on u.Id equals s.userid
                //            where u.managerid == userId
                //            select s).ToList();
                var data = _DbContext.salary.Where(x => x.createddate.Value.Date.Month == DateTime.Now.Date.Month && x.createddate.Value.Date.Year == DateTime.Now.Date.Year).ToList();

                return View("salary",data);
            }
            else if (User.IsInRole("Admin"))
            {
                var data2 = _DbContext.salary.Where(x => x.createddate.Value.Date.Month == DateTime.Now.Date.Month && x.createddate.Value.Date.Year == DateTime.Now.Date.Year).ToList();
                return View("salary", data2);
            }
            var query = (from s in _DbContext.salary
                         where (s.userid == userId)
                         select s).ToList();
            return View("salary", query);
        }
        public IActionResult display()
        {
            
            int userId = int.Parse(userManager.GetUserId(User));
            bool isManager = User.IsInRole("Manager");
            bool isAdmin = User.IsInRole("Admin");
            bool isEmployee = User.IsInRole("Employee");

            List<leave> result = _leavepageclear.afterclear(isManager, isAdmin, isEmployee,userId);
            return View("leavepage",result);

            //if (User.IsInRole("Manager"))
            //{
            //    var students = ((from s in _DbContext.Users
            //                     join u in _DbContext.leave
            //                     on s.Id equals u.userid
            //                     where s.managerid == userId
            //                     select new leave
            //                     {
            //                         leaveid = u.leaveid,
            //                         fromDate = u.fromDate,
            //                         toDate = u.toDate,
            //                         statusid = u.statusid,
            //                         userid = u.userid,
            //                         roleid = u.roleid
            //                     }).Union(
            //                        from s in _DbContext.Users
            //                        join u in _DbContext.leave
            //                        on s.Id equals u.userid
            //                        where s.Id == userId
            //                        select new leave
            //                        {
            //                            leaveid = u.leaveid,
            //                            fromDate = u.fromDate,
            //                            toDate = u.toDate,
            //                            statusid = u.statusid,
            //                            userid = u.userid,
            //                            roleid = u.roleid
            //                        })).ToList();
            //    return View("leavepage", students);
            //}
            //else if (User.IsInRole("Admin"))
            //{
            //    List<leave> leavedates = _DbContext.leave.ToList();
            //    return View("leavepage", leavedates);
            //}
            //var query = (from x in _DbContext.leave
            //             where (x.userid == userId)
            //             select x
            //             ).ToList();
            //return View("leavepage",query);

        }

        [HttpPost]
        public IActionResult leavesave(leave saveleavedb)
        {
            leave leavecomingdata = new leave();
            var y = User.Identity.Name;
            var abc = _DbContext.Users.Where(x => x.Email == y).FirstOrDefault();
            //saveleavedb.userid = abc.Id;
            //var z = saveleavedb.userid;
            var z = abc.Id;
            var def = _DbContext.UserRoles.Where(x => x.UserId == z).FirstOrDefault();
            //saveleavedb.roleid = def.RoleId;
            //saveleavedb.statusid = 1;
            leavecomingdata.statusid = 1;
            leavecomingdata.roleid = def.RoleId;
            leavecomingdata.userid = abc.Id;
            leavecomingdata.fromDate = saveleavedb.fromDate;
            leavecomingdata.toDate = saveleavedb.toDate;
            leavecomingdata.reason = saveleavedb.reason;

            //_DbContext.leave.Add(saveleavedb);
            //_DbContext.SaveChanges();
            
            _leaveservices.leavesave(leavecomingdata);
            return RedirectToAction("leavepage");
        }

        public string deleteTab(int i)
        {
            //leave x = _DbContext.leave.Where(z => z.leaveid == i).FirstOrDefault();
            //_DbContext.leave.Remove(x);
            //_DbContext.SaveChanges();

            _deleteTab.deleteTab(i);
            return "true";
        }

        public IActionResult getleavedata(int id)
        {
            leave z = _DbContext.leave.Where(x => x.leaveid == id).FirstOrDefault();
            return View(z);
        }

        //public IActionResult getholidaydata(int id)
        //{
        //    List<project.Models.holidays> holidayinfo = _DbContext.holidays.ToList();
        //    return View(holidayinfo);
        //}

        public bool updateleave([FromBody] leave change)
        {
            if (User.IsInRole("Manager"))
            {
                //leave z = _DbContext.leave.Where(x => x.leaveid == change.leaveid).FirstOrDefault();
                //z.fromDate = change.fromDate;
                //z.toDate = change.toDate;
                //z.reason = change.reason;
                //_DbContext.leave.Update(z);
                //_DbContext.SaveChanges();
                _updateleave.updateleave(change);
                //int userId = int.Parse(userManager.GetUserId(User));
                return true;                
            }
            //leave u = _DbContext.leave.Where(x => x.leaveid == change.leaveid).FirstOrDefault();
            //u.fromDate = change.fromDate;
            //u.toDate = change.toDate;
            //u.reason = change.reason;
            //_DbContext.leave.Update(u);
            //_DbContext.SaveChanges();
            _updateleave.updateleave(change);
            //int userIdd = int.Parse(userManager.GetUserId(User));
            //List<leave> p = _DbContext.leave.Where(z => z.userid == userIdd).ToList();
            return true;
        }

        public IActionResult activate(int id)
        {
            int userId = int.Parse(userManager.GetUserId(User));
            leave l = _DbContext.leave.Where(x => x.leaveid == id).FirstOrDefault();
            l.statusid = 2;
            _DbContext.leave.Update(l);
            _DbContext.SaveChanges();
            return RedirectToAction("leavepage");
        }

        public IActionResult deactivate(int id)
        {
            int userId = int.Parse(userManager.GetUserId(User));
            leave l = _DbContext.leave.Where(x => x.userid == id).FirstOrDefault();
            l.statusid = 3;
            _DbContext.leave.Update(l);
            _DbContext.SaveChanges();
            return RedirectToAction("leavepage");
        }

        [HttpGet]
        public async Task<IActionResult> salary(string currentFilter, string searchString, int? pageNumber)
        {
            int userId = int.Parse(userManager.GetUserId(User));
            ViewData["CurrentFilter"] = searchString;
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            
            if (User.IsInRole("Manager"))
            {
                var emp = (from u in _DbContext.Users
                           join s in _DbContext.salary
                           on u.Id equals s.userid                           
                           where (u.managerid == userId) && (s.createddate.Value.Date.Month == DateTime.Now.Date.Month) & (s.createddate.Value.Date.Year == DateTime.Now.Date.Year)
                           select s).ToList();
                //&& (s.fromDate.Value.Date.Month == DateTime.Now.Date.Month) 
                return View(emp);

            }
            else if (User.IsInRole("Admin"))
            {
                //List<project.Models.salary> salaryinfo = _DbContext.salary.Where(s => s.createddate.Value.Date.Month == DateTime.Now.Date.Month & s.createddate.Value.Date.Year == DateTime.Now.Date.Year).ToList();
                List<salary> salaryinfo = _DbContext.salary.Where(s => s.createddate.Value.Date.Month == DateTime.Now.Date.Month && s.createddate.Value.Date.Year == DateTime.Now.Date.Year).ToList();
                return View(salaryinfo);
            }
            else
            {
                var students = (from s in _DbContext.salary
                                where s.userid == userId
                                select s);
                return View(students);
            }
            
            //if (!String.IsNullOrEmpty(searchString))
            //{
            //    students = students.Where(s => s.basic.ToString().Contains(searchString)
            //                           /*|| s.lastname.Contains(searchString)*/);
            //}
            //List<salary> x = _DbContext.salary.Where(z => z.userid == userId).ToList();
            //return View(x);
            //int pageSize = 1;
            //return View(await PaginatedList<salary>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public IActionResult displayclear()
        {

            int userId = int.Parse(userManager.GetUserId(User));

            var query = (from x in _DbContext.salary
                         where (x.userid == userId)
                         select x
                         ).ToList();
            return View("salary", query);
        }

        public IActionResult unassignedroles()
        {
            var users = userManager.Users;
            return View(users);
        }

        public IActionResult assignmanager(int id)
        {
            var y = id;
            List<IdentityUserRole<int>> u = new List<IdentityUserRole<int>>();
            var query = (from t in _DbContext.Users
                 join r in _DbContext.UserRoles
                 on t.Id equals r.UserId
                 where r.RoleId == 2
                 select new mixed
                 {
                     username = t.UserName,
                     userid = t.Id
                 }
                 ).ToList();
            //u.Insert(0,new IdentityUserRole<int> {UserId=0,RoleId=0 });
            ViewBag.message = query;
            ViewBag.id = y;
            return View();
        }

        //[HttpPost]
        //public IActionResult assignmanager(IdentityUserRole<int> u)
        //{
        //    _DbContext.Add(u);
        //    _DbContext.SaveChanges();
        //    ViewBag.msg = "The selected manager" + "is assigned successfully ";
        //    return View();
        //}

        public string updatemanager([FromBody] Users x)
        {
            var user = _DbContext.Users.Where(y => y.Id == x.Id).FirstOrDefault();
            user.managerid = x.managerid;
            user.manager = x.manager;
            _DbContext.Users.Update(user);
            _DbContext.SaveChanges();
            return "true";
        }

        public IActionResult addsalary()
        {
            int userId = int.Parse(userManager.GetUserId(User));
            var y = User.Identity.Name;
            bool isManager = User.IsInRole("Manager");
            bool isAdmin = User.IsInRole("Admin");
            bool isEmployee = User.IsInRole("Employee");

            List<Users> result = _addsalary.addnewsalary(isManager, isAdmin, isEmployee, userId);
            ViewBag.msg = result;
            return View();
            // List<leave> result = _leavepageclear.afterclear(isManager, isAdmin, isEmployee,userId);
            //if (User.IsInRole("Manager"))
            //{

            ////var query = (from x in _DbContext.Users
            ////             where x.manager == y
            ////             select x).ToList();
            ////(s.fromDate.Value.Date.Month == DateTime.Now.Date.Month)
            //var query1 = (from x in _DbContext.Users
            //             where x.managerid == userId
            //              select new Users
            //              {
            //                  Id = x.Id,
            //                  UserName = x.UserName
            //              }).Distinct().ToList();
            //var query = (from x in _DbContext.Users
            //             join s in _DbContext.salary
            //             on x.Id equals s.userid
            //             where (x.managerid == userId) && (s.createddate.Value.Date.Month == DateTime.Now.Date.Month) && (s.createddate.Value.Date.Year == DateTime.Now.Date.Year)
            //             select new Users
            //             {
            //                 Id = x.Id,
            //                 UserName = x.UserName
            //             }
            //             ).ToList();
            //if(query.Count == 0)
            //{
            //    var query2 = _DbContext.Users.Where(x => x.managerid == userId).ToList();
            //    ViewBag.msg = query2;
            //    return View();
            //}
            //else if(query.Count != 0)
            //{
            //    // var result = query1.Except(query).ToList();
            //    //var result = query1.RemoveAll(x => query.Contains(x));
            //    foreach(var item in query1.ToList())
            //    {
            //        if (query.Any(x=>x.Id == item.Id))
            //        {
            //            query1.Remove(item);
            //        }
            //    }
            //    ViewBag.msg = query1;
            //    return View();
            //}
            //}
            //else if (User.IsInRole("Admin"))
            //{
            //    var query11 = (from x in _DbContext.Users
            //                  where x.managerid == userId
            //                  select new Users
            //                  {
            //                      Id = x.Id,
            //                      UserName = x.UserName
            //                  }).Distinct().ToList();
            //    var queryy = (from x in _DbContext.Users
            //                 join s in _DbContext.salary
            //                 on x.Id equals s.userid
            //                 where (x.managerid == userId) && (s.createddate.Value.Date.Month == DateTime.Now.Date.Month) && (s.createddate.Value.Date.Year == DateTime.Now.Date.Year)
            //                 select new Users
            //                 {
            //                     Id = x.Id,
            //                     UserName = x.UserName
            //                 }
            //             ).ToList();
            //    if (queryy.Count == 0)
            //    {
            //        var query2 = _DbContext.Users.Where(x => x.managerid == userId).ToList();
            //        ViewBag.msg = query2;
            //        return View();
            //    }
            //    else if (queryy.Count != 0)
            //    {
            //        // var result = query1.Except(query).ToList();
            //        //var result = query1.RemoveAll(x => query.Contains(x));
            //        foreach (var item in query11.ToList())
            //        {
            //            if (queryy.Any(x => x.Id == item.Id))
            //            {
            //                query11.Remove(item);
            //            }
            //        }
            //        ViewBag.msg = query11;
            //        return View();
            //    }
            //}
            
        }


        public string savesalary2([FromBody] salary savesalarydb)
        {
            var y = User.Identity.Name;
            //savesalarydb.username = 
            //var abc = savesalarydb.basic;
            //var def = savesalarydb.tax;
            //savesalarydb.final = abc - def;
            //savesalarydb.final = savesalarydb.basic - savesalarydb.tax;
            //_DbContext.salary.Where(x => x.Email == y).FirstOrDefault();
            salary s = new salary();
            s.createddate = DateTime.Parse(savesalarydb.date);
            s.basic = savesalarydb.basic;
            s.tax = savesalarydb.tax;
            s.final = savesalarydb.final;
            s.username = savesalarydb.username;
            s.userid = savesalarydb.userid;
            _givesalary.paysalary(s);
            //_DbContext.salary.Add(s);
            //_DbContext.SaveChanges();
            return "true";
        }

        public IActionResult addemp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> addemp(Users user)
        {
            if (ModelState.IsValid)
            {
                int userId = int.Parse(userManager.GetUserId(User));
                var yy = User.Identity.Name;
                //int i = 1;
                //bool b = Convert.ToBoolean(i);
                //var user = new User { UserName = u.UserName, EmailConfirmed = b, managerid = userId, manager = yy,Email = u.UserName,
                //                    NormalizedEmail = u.UserName.ToUpper(), NormalizedUserName = u.UserName.ToUpper(), PasswordHash = u.PasswordHash
                //};
                //var result = await userManager.CreateAsync(user, u.PasswordHash);
                var users = new Users { UserName = user.UserName, Email = user.UserName , EmailConfirmed = true, managerid = userId, manager = yy };
                var result = await userManager.CreateAsync(users, user.PasswordHash);
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(users, "Employee").Wait();
                    //await signInManager.SignInAsync(user, isPersistent: false);
                    //int i = 1;
                    //bool b = Convert.ToBoolean(i);
                    
                    //User userdata = new User();
                    
                    //uu.UserName = u.UserName;
                    //uu.PasswordHash = u.PasswordHash;
                    //uu.EmailConfirmed = b;
                    //uu.managerid = userId;
                    //uu.manager = yy;
                    //uu.Email = u.UserName;
                    //uu.NormalizedEmail = u.UserName.ToUpper();
                    //uu.NormalizedUserName = u.UserName.ToUpper();
                    //_DbContext.Users.Add(users);  I commented this bcoz CreateAsync at "result" is already adding/creating user in db so no need to write again.
                    //_DbContext.SaveChanges();      and no need to save it.
                    return RedirectToAction("homepage");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);


        }

        public IActionResult adminaddemp()
        {
            List<mixed> result = _adminaddemployee.addempadmin();
            ViewBag.message = result;
            return View();
            //var query = (from t in _DbContext.Users
            //             join r in _DbContext.UserRoles
            //             on t.Id equals r.UserId
            //             where r.RoleId == 2
            //             select new mixed
            //             {
            //                 username = t.UserName,
            //                 userid = t.Id
            //             }
            //     ).ToList();
            ////u.Insert(0, new IdentityUserRole<int> { UserId = 0, RoleId = 0 });
            //ViewBag.message = query;
            //return View();
        }

        [HttpPost]
        public async Task<IActionResult> adminaddemp([FromBody] Users x)
        {
            if (ModelState.IsValid)
            {
                int userId = int.Parse(userManager.GetUserId(User));
                var yy = User.Identity.Name;
                var users = new Users { UserName = x.UserName, Email = x.UserName, EmailConfirmed = true, managerid = userId, manager = yy };
                var result = await userManager.CreateAsync(users, x.PasswordHash);
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(users, "Employee").Wait();
                    //await signInManager.SignInAsync(user, isPersistent: false);
                    //int i = 1;
                    //bool b = Convert.ToBoolean(i);

                    //User userdata = new User();

                    //uu.UserName = u.UserName;
                    //uu.PasswordHash = u.PasswordHash;
                    //uu.EmailConfirmed = b;
                    //uu.managerid = userId;
                    //uu.manager = yy;
                    //uu.Email = u.UserName;
                    //uu.NormalizedEmail = u.UserName.ToUpper();
                    //uu.NormalizedUserName = u.UserName.ToUpper();
                    //_DbContext.Users.Add(users);  I commented this bcoz CreateAsync at "result" is already adding/creating user in db so no need to write again.
                    //_DbContext.SaveChanges();      and no need to save it.
                    return RedirectToAction("homepage");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(x);            
        }


        public IActionResult adminaddman()
        {
            List<IdentityUserRole<int>> u = new List<IdentityUserRole<int>>();
            //u = (from x in _DbContext.UserRoles
            //     where x.RoleId == 2

            //     select x
            //     //select new IdentityUserRole<int>
            //     //{
            //     //    UserId = x.UserId
            //     //}              
            //     ).ToList();
            var query = (from t in _DbContext.Users
                         join r in _DbContext.UserRoles
                         on t.Id equals r.UserId
                         where r.RoleId == 3
                         select new mixed
                         {
                             username = t.UserName,
                             userid = t.Id
                         }
                 ).ToList();
            
            //u.Insert(0,new IdentityUserRole<int> {UserId=0,RoleId=0 });
            ViewBag.message = query;
            return View();
        }

        //public IActionResult giveincrement(int id)
        //{
        //    project.Models.salary z = _DbContext.salary.Where(x => x.salaryid == id).FirstOrDefault();
        //    return View(z);
        //}


        //public string updatebyincrement([FromBody] project.Models.salary change)
        //{
        //    project.Models.salary sal = _DbContext.salary.Where(x => x.salaryid == change.salaryid).FirstOrDefault();
        //    sal.basic = change.basic + change.increment;
        //    sal.tax = change.tax;
        //    sal.final = change.basic + change.increment - change.tax;
        //    _DbContext.salary.Update(sal);
        //    _DbContext.SaveChanges();
        //    return "true";
        //}


        public string updateemptomanager([FromBody] Users userdata)
        {            
            int userId = int.Parse(userManager.GetUserId(User));
            var managerName = User.Identity.Name;
            var user = userManager.FindByIdAsync(userdata.Id.ToString()).Result;
            user.managerid = userId;
            user.manager = managerName;
            userManager.RemoveFromRoleAsync(user, "Employee").Wait();
            userManager.AddToRoleAsync(user, "Manager").Wait();
            userManager.UpdateAsync(user);            
            return "true";
        }


        public IActionResult manuallycreatemanager()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> manuallycreatemanager(Users user)
        {
            if (ModelState.IsValid)
            {
                int userId = int.Parse(userManager.GetUserId(User));
                var yy = User.Identity.Name;
                var users = new Users { UserName = user.UserName, Email = user.UserName, EmailConfirmed = true, managerid = userId, manager = yy };
                var result = await userManager.CreateAsync(users, user.PasswordHash);
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(users, "Manager").Wait();
                    return RedirectToAction("unassignedroles");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        [HttpGet]
        public IActionResult spleavepage()
        {
            int userId = int.Parse(userManager.GetUserId(User));
            SqlConnection con = new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            List<leave> leavedata = new List<leave>();
            try
            {

                if (User.IsInRole("Employee"))
                {
                    con.Open();
                    SqlCommand cmd1 = new SqlCommand("select * from leave where userid = @userId", con);
                    cmd1.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader rdr1 = cmd1.ExecuteReader();
                    if (rdr1.HasRows)
                    {
                        while (rdr1.Read())
                        {
                            leavedata.Add(new leave
                            {
                                leaveid = Convert.ToInt32(rdr1["leaveid"]),
                                fromDate = (DateTime)rdr1["fromDate"],
                                toDate = (DateTime)rdr1["toDate"],
                                statusid = Convert.ToInt32(rdr1["statusid"]),
                                reason = rdr1["reason"].ToString()
                            });
                        }
                            return View(leavedata);
                    }
                }
                else if (User.IsInRole("Manager"))
                {
                        con.Open();
                        SqlCommand cmd2 = new SqlCommand("listleaveofmanager", con);
                        cmd2.CommandType = CommandType.StoredProcedure;
                        cmd2.Parameters.AddWithValue("@userId", userId);
                        SqlDataReader rdr2 = cmd2.ExecuteReader();
                        if (rdr2.HasRows)
                        {
                            while (rdr2.Read())
                            {
                                leavedata.Add(new leave
                                {
                                    leaveid = Convert.ToInt32(rdr2["leaveid"]),
                                    fromDate = (DateTime)rdr2["fromDate"],
                                    toDate = (DateTime)rdr2["toDate"],
                                    statusid = Convert.ToInt32(rdr2["statusid"]),
                                    reason = rdr2["reason"].ToString()
                                });
                            }
                                return View(leavedata);
                            cmd2.ExecuteNonQuery();
                        }
                    }
                else if (User.IsInRole("Admin"))
                    {
                        //string action = "Admin";
                        con.Open();
                        SqlCommand cmd3 = new SqlCommand("select * from leave", con);
                        //cmd3.CommandType = CommandType.StoredProcedure;
                        //cmd3.Parameters.AddWithValue("@action", action);
                        SqlDataReader rdr3 = cmd3.ExecuteReader();
                        if (rdr3.HasRows)
                        {
                            while (rdr3.Read())
                            {
                                leavedata.Add(new leave
                                {
                                    leaveid = Convert.ToInt32(rdr3["leaveid"]),
                                    fromDate = (DateTime)rdr3["fromDate"],
                                    toDate = (DateTime)rdr3["toDate"],
                                    statusid = Convert.ToInt32(rdr3["statusid"]),
                                    reason = rdr3["reason"].ToString()
                                });
                            }
                            return View(leavedata);
                        }
                    }
            
            }

            catch (SqlException e)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
            return View();
        }


        [HttpPost]
        public IActionResult spleavepage(DateTime startdate, DateTime enddate, int? statusids, int? id)
        {
            var abc = id;
            //ViewBag.startdate = startdate;
            //ViewBag.enddate = enddate;
            DateTime xy = DateTime.Parse("01-01-0001 12:00:00 AM");
            DateTime uy = DateTime.Parse("01-01-9999 12:00:00 AM");
            if (enddate == xy)
            {
                enddate = DateTime.Parse("01-01-9999 12:00:00 AM");
            }
            int userId = int.Parse(userManager.GetUserId(User));
            SqlConnection con = new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            List<leave> leavedata = new List<leave>();
            try
            {
                if (User.IsInRole("Employee"))
                {

                    if (statusids == null)
                    {
                        con.Open();
                        SqlCommand cmd1 = new SqlCommand("select * from leave where userid = @userId and fromDate = @fromDate and toDate = @toDate", con);
                        cmd1.Parameters.AddWithValue("@userId", userId);
                        cmd1.Parameters.AddWithValue("@fromDate", startdate);
                        cmd1.Parameters.AddWithValue("@toDate", enddate);
                        SqlDataReader rdr1 = cmd1.ExecuteReader();
                            if (rdr1.HasRows)
                            {
                                while (rdr1.Read())
                                {
                                    leavedata.Add(new leave
                                    {
                                        leaveid = Convert.ToInt32(rdr1["leaveid"]),
                                        fromDate = (DateTime)rdr1["fromDate"],
                                        toDate = (DateTime)rdr1["toDate"],
                                        statusid = Convert.ToInt32(rdr1["statusid"]),
                                        reason = rdr1["reason"].ToString()
                                    });
                                }
                            ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                            ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                            return View(leavedata);
                            }
                            //var query1 = (from x in _DbContext.leave
                            //          where ((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId))
                            //          select x
                            //     ).ToList();
                
                    }
                    con.Open();
                    SqlCommand cmd2 = new SqlCommand("select * from leave where userid = @userId and fromDate = @fromDate and toDate = @toDate and statusid = @statusids", con);
                    cmd2.Parameters.AddWithValue("@userId", userId);
                    cmd2.Parameters.AddWithValue("@fromDate", startdate);
                    cmd2.Parameters.AddWithValue("@toDate", enddate);
                    cmd2.Parameters.AddWithValue("@statusids", statusids);
                    SqlDataReader rdr2 = cmd2.ExecuteReader();
                    if (rdr2.HasRows)
                    {
                        while (rdr2.Read())
                        {
                            leavedata.Add(new leave
                            {
                                leaveid = Convert.ToInt32(rdr2["leaveid"]),
                                fromDate = (DateTime)rdr2["fromDate"],
                                toDate = (DateTime)rdr2["toDate"],
                                statusid = Convert.ToInt32(rdr2["statusid"]),
                                reason = rdr2["reason"].ToString()
                            });
                        }
                        ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                        ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                        return View(leavedata);
                    }
            
                }
                else if (User.IsInRole("Admin"))
                {
                    if(statusids == null)
                    {
                        con.Open();
                        SqlCommand cmd7 = new SqlCommand("select * from leave where fromDate = @fromDate and toDate = @toDate", con);
                        cmd7.Parameters.AddWithValue("@fromDate", startdate);
                        cmd7.Parameters.AddWithValue("@toDate", enddate);
                        SqlDataReader rdr7 = cmd7.ExecuteReader();
                        if (rdr7.HasRows)
                        {
                            while (rdr7.Read())
                            {
                                leavedata.Add(new leave
                                {
                                    leaveid = Convert.ToInt32(rdr7["leaveid"]),
                                    fromDate = (DateTime)rdr7["fromDate"],
                                    toDate = (DateTime)rdr7["toDate"],
                                    statusid = Convert.ToInt32(rdr7["statusid"]),
                                    reason = rdr7["reason"].ToString(),
                                    roleid = Convert.ToInt32(rdr7["roleid"])
                                });
                            }
                            ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                            ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                            return View(leavedata);
                        }
                    }
                    con.Open();
                    SqlCommand cmd8 = new SqlCommand("select * from leave where fromDate = @fromDate and toDate = @toDate and statusid = @statusids", con);
                    //cmd8.Parameters.AddWithValue("@userId", userId);
                    cmd8.Parameters.AddWithValue("@fromDate", startdate);
                    cmd8.Parameters.AddWithValue("@toDate", enddate);
                    cmd8.Parameters.AddWithValue("@statusids", statusids);
                    SqlDataReader rdr8 = cmd8.ExecuteReader();
                    if (rdr8.HasRows)
                    {
                        while (rdr8.Read())
                        {
                            leavedata.Add(new leave
                            {
                                leaveid = Convert.ToInt32(rdr8["leaveid"]),
                                fromDate = (DateTime)rdr8["fromDate"],
                                toDate = (DateTime)rdr8["toDate"],
                                statusid = Convert.ToInt32(rdr8["statusid"]),
                                reason = rdr8["reason"].ToString(),
                                roleid = Convert.ToInt32(rdr8["roleid"])
                            });
                        }
                        ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                        ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                        return View(leavedata);
                    }
                }
                else if (User.IsInRole("Manager"))
                {
                    //con.Open();
                    //SqlCommand cmd3 = new SqlCommand("listleaveofmanager", con);
                    //cmd3.CommandType = CommandType.StoredProcedure;
                    //cmd3.Parameters.AddWithValue("@userId", userId);
                    //SqlDataReader rdr3 = cmd3.ExecuteReader();
                    ////if (rdr3.HasRows)
                    ////{
                    //    while (rdr3.Read())
                    //    {
                    //        leavedata.Add(new leave
                    //        {
                    //            leaveid = Convert.ToInt32(rdr3["leaveid"]),
                    //            fromDate = (DateTime)rdr3["fromDate"],
                    //            toDate = (DateTime)rdr3["toDate"],
                    //            statusid = Convert.ToInt32(rdr3["statusid"]),
                    //            reason = rdr3["reason"].ToString(),
                    //            roleid = Convert.ToInt32(rdr3["roleid"])
                    //        });
                    //    }
                        
                    //return View(leavedata);
                    //}
                    //List<leave> x = getleavelist();
                    if(statusids == null)
                    {
                        con.Open();
                        SqlCommand cmd4 = new SqlCommand("listofmanagerdata", con);
                        cmd4.CommandType = CommandType.StoredProcedure;
                        //cmd3.Parameters.AddWithValue("@userId", userId);
                        cmd4.Parameters.AddWithValue("@userId", userId);
                        cmd4.Parameters.AddWithValue("@fromDate", startdate);
                        cmd4.Parameters.AddWithValue("@toDate", enddate);
                        SqlDataReader rdr4 = cmd4.ExecuteReader();
                        if (rdr4.HasRows)
                        {
                            while (rdr4.Read())
                            {
                                leavedata.Add(new leave
                                {
                                    leaveid = Convert.ToInt32(rdr4["leaveid"]),
                                    fromDate = (DateTime)rdr4["fromDate"],
                                    toDate = (DateTime)rdr4["toDate"],
                                    statusid = Convert.ToInt32(rdr4["statusid"]),
                                    reason = rdr4["reason"].ToString(),
                                    roleid = Convert.ToInt32(rdr4["roleid"])
                                });
                            }
                            ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
                            ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
                            return View(leavedata);
                        }
                        
                    }
                }
                //            var result1 = students.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
            }
            catch (SqlException e)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
            return View();
        }

        public List<leave> getleavelist()
        {
            int userId = int.Parse(userManager.GetUserId(User));
            SqlConnection con = new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            List<leave> leavedata = new List<leave>();
            con.Open();
            SqlCommand cmd3 = new SqlCommand("listleaveofmanager", con);
            cmd3.CommandType = CommandType.StoredProcedure;
            cmd3.Parameters.AddWithValue("@userId", userId);
            SqlDataReader rdr3 = cmd3.ExecuteReader();
            if (rdr3.HasRows)
            {
                while (rdr3.Read())
                {
                    leavedata.Add(new leave
                    {
                        leaveid = Convert.ToInt32(rdr3["leaveid"]),
                        fromDate = (DateTime)rdr3["fromDate"],
                        toDate = (DateTime)rdr3["toDate"],
                        statusid = Convert.ToInt32(rdr3["statusid"]),
                        reason = rdr3["reason"].ToString(),
                        roleid = Convert.ToInt32(rdr3["roleid"])
                    });
                }
                var students = leavedata;
                return students.ToList();
            }
            return leavedata.ToList();
        }

        //[HttpPost]
        //public IActionResult leavepage(DateTime startdate, DateTime enddate, int? statusids, int? id)
        //{
        //    var abc = id;
        //    DateTime xy = DateTime.Parse("01-01-0001 12:00:00 AM");
        //    DateTime uy = DateTime.Parse("01-01-9999 12:00:00 AM");
        //    if (enddate == xy)
        //    {
        //        enddate = DateTime.Parse("01-01-9999 12:00:00 AM");
        //    }
        //    int userId = int.Parse(userManager.GetUserId(User));

        //    if (User.IsInRole("Manager"))
        //    {
        //        var students = ((from s in _DbContext.Users
        //                         join u in _DbContext.leave
        //                         on s.Id equals u.userid
        //                         where s.managerid == userId
        //                         select new leave
        //                         {
        //                             leaveid = u.leaveid,
        //                             fromDate = u.fromDate,
        //                             toDate = u.toDate,
        //                             statusid = u.statusid,
        //                             userid = u.userid,
        //                             roleid = u.roleid
        //                         }).Union(
        //                            from s in _DbContext.Users
        //                            join u in _DbContext.leave
        //                            on s.Id equals u.userid
        //                            where s.Id == userId
        //                            select new leave
        //                            {
        //                                leaveid = u.leaveid,
        //                                fromDate = u.fromDate,
        //                                toDate = u.toDate,
        //                                statusid = u.statusid,
        //                                userid = u.userid,
        //                                roleid = u.roleid
        //                            })).ToList();
        //        if (statusids == null)
        //        {
        //            var result1 = students.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
        //            ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
        //            ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
        //            return View(result1);
        //        }
        //        else if (startdate == xy && enddate == xy)
        //        {
        //            var result2 = students.Where(x => (x.statusid == statusids)).ToList();
        //            //ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
        //            //ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
        //            return View(result2);
        //        }
        //        var result = students.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate) && (x.statusid == statusids)).ToList();
        //        ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
        //        ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
        //        return View(result);
        //    }

        //    else if (User.IsInRole("Admin"))
        //    {
        //        List<leave> leavedates = _DbContext.leave.ToList();
        //        if (statusids == null)
        //        {
        //            var result1 = leavedates.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate)).ToList();
        //            ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
        //            ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
        //            return View(result1);
        //        }
        //        else if (startdate == xy && enddate == uy)
        //        {
        //            var result2 = leavedates.Where(x => (x.statusid == statusids)).ToList();
        //            if (statusids == 1)
        //            {
        //                ViewBag.status = "Pending";
        //            }
        //            else if (statusids == 2)
        //            {
        //                ViewBag.status = "Accepted";
        //            }
        //            else if (statusids == 3)
        //            {
        //                ViewBag.status = "Rejected";
        //            }
        //            return View(result2);
        //        }
        //        var result = leavedates.Where(x => (x.fromDate >= startdate) && (x.toDate <= enddate) && (x.statusid == statusids)).ToList();
        //        ViewBag.startdate = startdate.ToString("yyyy-MM-dd");
        //        ViewBag.enddate = enddate.ToString("yyyy-MM-dd");
        //        return View(result);
        //    }

        //    if (statusids == null)
        //    {
        //        //var query1 = (from x in _DbContext.leave
        //        //              where (x.userid == userId)
        //        //              select x
        //        //         ).ToList();
        //        var query1 = (from x in _DbContext.leave
        //                      where ((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId))
        //                      select x
        //                 ).ToList();
        //        return View(query1);
        //    }
        //    var query = (from x in _DbContext.leave
        //                 where ((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId) && (x.statusid == statusids))
        //                 //((x.statusid == searchstatus) && (x.userid == userId)) ||
        //                 //((x.fromDate >= startdate) && (x.toDate <= enddate) && (x.userid == userId))
        //                 select x
        //                 ).ToList();
        //    return View(query);
        //}

        public IActionResult spaddleave()
        {
            return View();
        }

        public IActionResult spleavesave(leave saveleavedata)
        {
            int userId = int.Parse(userManager.GetUserId(User));
            //var def = _DbContext.UserRoles.Where(x => x.UserId == z).FirstOrDefault();
            //SqlConnection con = null;
            SqlConnection con = new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            //con = new new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            con.Open();
            SqlCommand cmd = new SqlCommand("spaddleaverequest", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@userid", userId);
            //cmd.Parameters.AddWithValue("@roleid", 1);
            cmd.Parameters.AddWithValue("@fromDate", saveleavedata.fromDate);
            cmd.Parameters.AddWithValue("toDate", saveleavedata.toDate);
            cmd.Parameters.AddWithValue("@reason",saveleavedata.reason);
            cmd.Parameters.AddWithValue("@statusid", 1);
            cmd.ExecuteNonQuery();
            con.Close();
            return RedirectToAction("leavepage");
        }


        public string sppdeleteTab(int i)
        {
            SqlConnection con = new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            con.Open();
            SqlCommand cmd = new SqlCommand("spupdatedelete", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@action", "Delete");
            cmd.Parameters.AddWithValue("@leaveid", i);
            cmd.ExecuteNonQuery();
            con.Close();
            return "true";
        }


        public IActionResult sppgetleavedata(int id)
        {
            SqlConnection con = null;
            leave leavedata = new leave();
            try
            {            
            //var leavedata = new leave();
             con = new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            con.Open();
            SqlCommand cmd = new SqlCommand("select * from leave where leaveid = @leaveid", con);
            cmd.Parameters.AddWithValue("@leaveid", id);
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                //DateTime fromDate = (DateTime)rdr["fromDate"];
                //DateTime toDate = (DateTime)rdr["toDate"];
                leavedata.fromDate = (DateTime)rdr["fromDate"];
                leavedata.toDate = (DateTime)rdr["toDate"];
                leavedata.reason = rdr["reason"].ToString();
                leavedata.leaveid = Convert.ToInt32(rdr["leaveid"]);
                leavedata.statusid = Convert.ToInt32(rdr["statusid"]);
                }
            return View(leavedata);
            //con.Close();
            }
            catch(Exception e)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }

       
        public string sppupdateleave([FromBody] leave change)
        {
            SqlConnection con = new SqlConnection("Server=192.168.0.15\\SQL2019;Initial Catalog=HelperLand;Persist Security Info=False;User ID=bhavik;Password=bhavik;Connection Timeout=30;MultipleActiveResultSets=true");
            con.Open();
            SqlCommand cmd = new SqlCommand("spupdatedelete", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@action", "Update");
            cmd.Parameters.AddWithValue("@leaveid", change.leaveid);
            cmd.Parameters.AddWithValue("@fromDate", change.fromDate);
            cmd.Parameters.AddWithValue("@toDate", change.toDate);
            cmd.Parameters.AddWithValue("@reason", change.reason);
            cmd.ExecuteNonQuery();
            con.Close();
            return "true";
        }

        
    }

}
