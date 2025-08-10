using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Text.RegularExpressions;
using CoffeeManagement.ViewModels;
using CoffeeManagement.Models;

namespace CoffeeManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly Gol82750Ecoffee1Context _context;

        public UserController(Gol82750Ecoffee1Context context)
        {
            _context = context;
        }

        private string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 3)
            {
                return email;
            }
            var emailName = email.Substring(0, atIndex);
            var domain = email.Substring(atIndex);
            var maskedEmailName = emailName.Substring(0, 3) + new string('*', emailName.Length - 3);
            return maskedEmailName + domain;
        }

        [Route("login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            TempData.Clear();
            return View();
        }

        [Route("login")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(User model, string ReturnUrl)
        {
            TempData.Clear();
            var user = _context.Users.Include(x => x.UserGroup)
                .FirstOrDefault(x => x.Username == model.Username && x.Password == model.Password);

            if (user == null || user.UserGroup == null)
            {
                ViewBag.Alert = "Username hoặc Password không đúng!";
                return View(model);
            }
            else
            {

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.UserGroup.Code),
                    new Claim(ClaimTypes.GivenName, user.FullName ?? "No Name"),
                    new Claim(ClaimTypes.Surname, user.UserGroup.Name)
                };

                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);


                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(3)
                };

                await HttpContext.SignInAsync(principal, authProperties);

                TempData["UserLogin"] = true;

                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }


        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            TempData.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User");
        }


        //[AllowAnonymous]
        //public IActionResult ForgetPassword()
        //{
        //    return View();
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<IActionResult> ForgetPassword(string userIndent)
        //{
        //    var model = new User();
        //    if (userIndent.Contains('@'))
        //    {
        //        model = await _context.Users.FirstOrDefaultAsync(x => x.Email == userIndent);
        //    }
        //    else
        //    {
        //        model = await _context.Users.FirstOrDefaultAsync(x => x.Username == userIndent);
        //    }
        //    if (model == null)
        //    {
        //        ViewBag.Alert = "User không tồn tại!!!";
        //        return View();
        //    }
        //    if (model.IsBlock)
        //    {
        //        ViewBag.Alert = "User bị khóa!!!";
        //        return View();
        //    }
        //    Random random = new Random();
        //    model.Otp = random.Next(100000, 999999).ToString();
        //    model.OtplastestSend = DateTime.Now;

        //    HttpContext.Session.SetString("VerifyAccount", JsonConvert.SerializeObject(new UserViewModel(model)));
        //    bool emailSent = await _emailSendService.SendOtpToEmail(model.Email ?? "", model.Otp);
        //    if (!emailSent)
        //    {
        //        ViewBag.Alert = "Không thể gửi email, vui lòng thử lại!";
        //        return View();
        //    }

        //    return RedirectToAction("VerifyAccount");
        //}

        //[AllowAnonymous]
        //[HttpGet]
        //public IActionResult VerifyAccount()
        //{
        //    var userJson = HttpContext.Session.GetString("VerifyAccount");
        //    if (string.IsNullOrEmpty(userJson))
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    var user = JsonConvert.DeserializeObject<UserViewModel>(userJson);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    ViewBag.Email = MaskEmail(user.Email);
        //    return View();
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public IActionResult VerifyAccount(string otp)
        //{
        //    var userJson = HttpContext.Session.GetString("VerifyAccount");
        //    if (string.IsNullOrEmpty(userJson))
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    var user = JsonConvert.DeserializeObject<UserViewModel>(userJson);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    ViewBag.Email = MaskEmail(user.Email);
        //    if (user.OtplastestSend.HasValue && (DateTime.Now - user.OtplastestSend.Value).TotalMinutes > 10)
        //    {
        //        ViewBag.Alert = "Mã OTP đã hết hạn!!!";
        //        TempData.Remove("VerifyAccount");
        //        return View();
        //    }
        //    if (user.Otp != otp)
        //    {
        //        ViewBag.Alert = "OTP không chính xác!!!";
        //        return View();
        //    }

        //    user.Otp = null;
        //    user.OtplastestSend = null;
        //    HttpContext.Session.Remove("VerifyAccount");

        //    // Chuyển long sang string trước khi lưu vào TempData
        //    TempData["ChangePassword"] = user.Id.ToString();

        //    return RedirectToAction("ChangePassword", "User");
        //}
        //[AllowAnonymous]
        //public IActionResult ChangePassword()
        //{
        //    if (TempData["ChangePassword"] == null)
        //    {
        //        TempData.Clear();
        //        return RedirectToAction("Login");
        //    }

        //    TempData.Keep("ChangePassword");
        //    return View();
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<IActionResult> ChangePassword(string password, string password2)
        //{
        //    if (password != password2)
        //    {
        //        TempData.Keep("ChangePassword");
        //        ViewBag.Alert = "Password nhập lại không khớp!";
        //        return View();
        //    }
        //    if (TempData["ChangePassword"] == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    if (string.IsNullOrEmpty(password))
        //    {
        //        TempData.Keep("ChangePassword");
        //        ViewBag.Alert = "Password không thể rỗng";
        //        return View();
        //    }
        //    if (password.Length < 8)
        //    {
        //        TempData.Keep("ChangePassword");
        //        ViewBag.Alert = "Password phải ít nhất có 8 ký tự";
        //        return View();
        //    }

        //    // Chuyển đổi từ string sang long
        //    if (!int.TryParse(TempData["ChangePassword"]?.ToString(), out int id))
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    TempData.Clear();
        //    var model = await _context.Users.FindAsync(id);
        //    if (model == null)
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    model.Password = password;
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction("Login");
        //}


    }
}
