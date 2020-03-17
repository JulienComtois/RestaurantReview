using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestaurantReview.Models;
using RestaurantReview.Validators;
using RestaurantReview.Utility;

namespace RestaurantReview.Controllers
{
    public class LoginController : Controller  
    {
        private RestaurantEntities db = new RestaurantEntities();

        // GET: Login
        public ActionResult Login(String returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

		[AdminAuthorize(Users = "admin")] 
		public ActionResult List()
		{
			var users = db.Users.ToList();
			return View(users);
		}

        // GET: Login/Create
        public ActionResult Create()
        {
            return View();
        }

        public ActionResult LogOut()
        {
            System.Web.Security.FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }


        // POST: Login/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password,Email")] User user)
        {
            if (ModelState.IsValid)
            {
                var username = db.Users.Where(u => u.Username == user.Username).FirstOrDefault(); //Checking to see if username is already in use
				if(username != null)
				 {
					 ModelState.AddModelError("", "Username already taken");
					 return View("Create");
				 }
                user.Username = user.Username.ToLower();
				user.Email = user.Email.ToLower();
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Username, string Password, String returnUrl)
        {          
            if (ModelState.IsValid)
            {
               User user = (from us in db.Users
                        where us.Username.Equals(Username)
                           && us.Password.Equals(Password)
                        select us).FirstOrDefault();

                if (user != null)
                    System.Web.Security.FormsAuthentication.RedirectFromLoginPage(Username, false);

                else
                {
                    ModelState.AddModelError("", "Invalid Username or passowrd");
                }
            }
            
            return View();
        }

		[AuthorizeRedirect] 
        public ActionResult Edit(string id)
        {
			bool isAdmin = Util.IsAdmin(User.Identity.Name);
			if(!isAdmin)
			{
				if (id == null || User.Identity.Name != id)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
				}
			}
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Password,Email")] User user)
        {
			bool isAdmin = Util.IsAdmin(User.Identity.Name);
			if (!isAdmin && !user.Username.Equals(User.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Restaurants");
            }
            return View(user);
        }

        // GET: Login/Delete/5
		[AdminAuthorize(Users = "admin")] 
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
			
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Login/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[AdminAuthorize(Users = "admin")] 
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
			var query = from x in db.Restaurants where x.Created_By == id select x;
			foreach (var p in query)
			{

				p.Created_By = "default";
			}
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
