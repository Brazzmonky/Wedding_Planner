using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wedding_planner.Models;

namespace wedding_planner.Controllers
{
    public class HomeController : Controller
    {
        private int? UserSession
        {
            get { return HttpContext.Session.GetInt32("UserId"); }
            set { HttpContext.Session.SetInt32("UserId", (int)value); }
        }
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("createuser")]
        public IActionResult CreateUser(User newUser)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists in db
                var existingUser = dbContext.Users.FirstOrDefault(u => u.Email == newUser.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View("Index");
                }
                // Hash new user's password and save new user to db
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                dbContext.Users.Add(newUser);
                dbContext.SaveChanges();
                UserSession = newUser.UserId;
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser currUser)
        {
            if (ModelState.IsValid)
            {
                // Check if email exists in database
                var existingUser = dbContext.Users.FirstOrDefault(u => u.Email == currUser.LoginEmail);
                if (existingUser == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(currUser, existingUser.Password, currUser.LoginPassword);
                if (result == 0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                UserSession = existingUser.UserId;
                return RedirectToAction("Dashboard");
            }
            return View("Index");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


        [HttpGet("rsvp/{weddingId}")]
        public IActionResult RSVP(int weddingId)
        {
            if (UserSession == null)
                return RedirectToAction("Index");

            // Create a new response with the given weddingId and current userId
            Response newResponse = new Response()
            {
                WeddingId = weddingId,
                UserId = (int)UserSession
            };
            dbContext.Responses.Add(newResponse);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("unrsvp/{weddingId}")]
        public IActionResult UnRSVP(int weddingId)
        {
            if (UserSession == null)
                return RedirectToAction("Index");
            
            // Query to grab the appropriate response to remove
            Response toDelete = dbContext.Responses.FirstOrDefault(r => r.WeddingId == weddingId && r.UserId == UserSession);
            
            // Redirect to dashboard if no match for response in db
            if (toDelete == null)
                return RedirectToAction("Dashboard");

            dbContext.Responses.Remove(toDelete);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            // Redirect to register/login page if no user in session
            if (UserSession == null)
                return RedirectToAction("Index");

            // Get all weddings with included responses ordered by date
            var AllWeddings = dbContext.Weddings
                .Include(w => w.Responses)
                .OrderByDescending(w => w.Date)
                .ToList();
                
            ViewBag.UserId = UserSession;
            return View(AllWeddings);
        }

        [HttpGet("{weddingId}")]
        public IActionResult Show(int weddingId)
        {
            var thisWedding = dbContext.Weddings
            .Include(w => w.Responses)
            .ThenInclude(r => r.Guest)
            .FirstOrDefault(w => w.WeddingId == weddingId);
            return View(thisWedding);
        }

        [HttpGet("new")]
        public IActionResult New()
        {
            return View("New");
        }

        [HttpPost("create")]
        public IActionResult Create(Wedding newWedding)
        {
            if (UserSession == null)
                    return RedirectToAction("Index");
            if (ModelState.IsValid)
            {   
                // Crete new wedding with UserId set to current session user's id
                newWedding.UserId = (int)UserSession;
                dbContext.Weddings.Add(newWedding);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("New");
        }

        [HttpGet("delete")]
        public IActionResult Delete(int weddingId)
        {
            if (UserSession == null)
                return RedirectToAction("Index");

            Wedding toDelete = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);
            
            if (toDelete == null)
                return RedirectToAction("Dashboard");
            // Redirect to dashboard if user trying to delete isn't the wedding creator
            if (toDelete.UserId != UserSession)
                return RedirectToAction("Dashboard");
            
            dbContext.Weddings.Remove(toDelete);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }




    }
}
