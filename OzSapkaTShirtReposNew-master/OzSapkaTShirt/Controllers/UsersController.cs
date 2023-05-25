using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OzSapkaTShirt.Data;
using OzSapkaTShirt.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.UI.V5.Pages.Internal;

namespace OzSapkaTShirt.Controllers
{
    public class ForgetModel
    {
       
        [DisplayName("E-posta")]
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        [StringLength(256, MinimumLength = 5, ErrorMessage = "En fazla 256, en az 5 karakter")]
        [EmailAddress(ErrorMessage = "Geçersiz format")]
        public string EMail { get; set; }


    }
    public class ResetModel
    {      
        [Required]
        [StringLength(256,MinimumLength =5)]
        [EmailAddress]
        public string EMail { get; set; }


        [Required]
        public string Code { get; set; }

      
        [DisplayName("Yeni Parola")]
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "En fazla 128, en az 8 karakter")]
        [DataType(DataType.Password)]
        public string PassWord { get; set;}

     
        [DisplayName("Yeni Parola (tekrar)")]
        [Required(ErrorMessage = "Bu alan zorunludur.")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "En fazla 128, en az 8 karakter")]
        [DataType(DataType.Password)]
        [Compare("PassWord", ErrorMessage = "Parola eşleşme başarısız")]
        public string ConfirmPassWord { get; set; }
       

    }
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UsersController(UserManager<ApplicationUser> userManager, ApplicationContext context, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;

        }


        public IActionResult ResetPassword()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ViewResult ResetPassword([Bind("EMail, Code, PassWord, ConfirmPassWord")] ResetModel resetModel)
        {
            ApplicationUser applicationUser;
            IdentityResult identityResult;
            
            if (ModelState.IsValid)
            {
                applicationUser =  _userManager.FindByEmailAsync(resetModel.EMail).Result;
                applicationUser.UserName = applicationUser.UserName.Trim();
                

               identityResult   =   _userManager.ResetPasswordAsync(applicationUser, resetModel.Code, resetModel.PassWord).Result;
               
                if(identityResult.Succeeded)
                {
                    return View("Login");
                }
                else
                {
                    ModelState.AddModelError("Code", "");
                    
                }

               
             }
           
            return View();
           
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ViewResult  ForgetPassword([Bind("EMail")] ForgetModel forgetModel)
        {
            ApplicationUser? applicationUser;
            string resetTokan;

            if (ModelState.IsValid)
            {

                applicationUser =  _userManager.FindByEmailAsync(forgetModel.EMail).Result;
                if (applicationUser != null)
                {

                    resetTokan =  _userManager.GeneratePasswordResetTokenAsync(applicationUser).Result;

                    //send resetToken to ForgetModel.EMail
                    ViewData["eMail"] = forgetModel.EMail;
                    return View("ResetPassword");

                }
                else
                {
                    ModelState.AddModelError("EMail", "Böyle bir email bulunamadı");
                    
                }

            }

            return View();

        }


        // GET: Users

        // GET: Userss/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string? id)
        {
            ApplicationUser? user;
            var a = 3;

            a = a / 1000000000;

            if (id == null || _userManager.Users == null)
            {
                return NotFound();
            }

            user = await _userManager.Users.Include(u => u.GenderType).Include(u => u.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            SelectList genders = new SelectList(_context.Genders, "Id", "Name");
            SelectList cities = new SelectList(_context.Cities, "PlateCode", "Name");

            ViewData["Genders"] = genders;
            ViewData["Cities"] = cities;
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SurName,Corporate,Address,Gender,BirthDate,UserName,Email,PhoneNumber,PassWord,ConfirmPassWord,CityCode")] ApplicationUser user)
        {
            IdentityResult? identityResult;
            SelectList genders, cities;

            if (ModelState.IsValid)
            {
                identityResult = _userManager.CreateAsync(user, user.PassWord).Result;
                if (identityResult == IdentityResult.Success)
                {
                    //Add customer role to user
                    return RedirectToAction("Index", "Home");
                }
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            genders = new SelectList(_context.Genders, "Id", "Name");
            cities = new SelectList(_context.Cities, "PlateCode", "Name");
            ViewData["Genders"] = genders;
            ViewData["Cities"] = cities;
            return View(user);
        }

        // GET: Products/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string? id)
        {
            SelectList genders, cities;

            if (id == null || _userManager.Users == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            genders = new SelectList(_context.Genders, "Id", "Name", user.Gender);
            cities = new SelectList(_context.Cities, "PlateCode", "Name", user.CityCode);
            ViewData["Genders"] = genders;
            ViewData["Cities"] = cities;
            return View(user.Trim());
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,SurName,Corporate,Address,Gender,BirthDate,UserName,Email,PhoneNumber,CityCode")] ApplicationUser user)
        {
            IdentityResult? identityResult;
            SelectList genders, cities;
            ApplicationUser existingUser;

            if (id != user.Id)
            {
                return NotFound();
            }

            ModelState.Remove("PassWord");
            ModelState.Remove("ConfirmPassWord");
            if (ModelState.IsValid)
            {
                existingUser = _userManager.FindByIdAsync(id).Result;
                existingUser.Name = user.Name;
                existingUser.SurName = user.SurName;
                existingUser.Corporate = user.Corporate;
                existingUser.Address = user.Address;
                existingUser.Gender = user.Gender;
                existingUser.BirthDate = user.BirthDate;
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.CityCode = user.CityCode;
                identityResult = _userManager.UpdateAsync(existingUser).Result;
                if (identityResult == IdentityResult.Success)
                {
                    return RedirectToAction("Index", "Home");
                }
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            genders = new SelectList(_context.Genders, "Id", "Name", user.Gender);
            cities = new SelectList(_context.Cities, "PlateCode", "Name", user.CityCode);
            ViewData["Genders"] = genders;
            ViewData["Cities"] = cities;
            return View(user);
        }

        // GET: Products/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _userManager.Users == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users.Include(u => u.GenderType).Include(u => u.City)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_userManager.Users == null)
            {
                return Problem("Entity set 'ApplicationContext.Product'  is null.");
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index","Home");
        }

        private bool UserExists(string id)
        {
            return (_userManager.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName,PassWord")] ApplicationUser user)
        {
            Microsoft.AspNetCore.Identity.SignInResult signInResult;

            if (ModelState["UserName"].ValidationState == ModelValidationState.Valid)
            {
                if (ModelState["Password"].ValidationState == ModelValidationState.Valid)
                {
                    signInResult = _signInManager.PasswordSignInAsync(user.UserName, user.PassWord, false, false).Result;
                    if (signInResult.Succeeded == true)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                   
                        ModelState.AddModelError("", "Kullanıcı ya da şifre hatalı");
                    
                }
              
            }
         
            HttpContext.Session.SetInt32("basketCount", 0);
            return View(user);
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string Password)
        {
            string userIdentity = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IdentityResult identityResult;
            ApplicationUser existingUser = _userManager.FindByIdAsync(userIdentity).Result;

            existingUser.PassWord = Password;
            existingUser.ConfirmPassWord = Password;
            existingUser.UserName = existingUser.UserName.Trim();
            identityResult = _userManager.ChangePasswordAsync(existingUser, oldPassword, Password).Result;
            if (identityResult.Succeeded == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Index", "Home");
        }
      
    }
}
