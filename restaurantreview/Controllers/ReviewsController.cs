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

namespace RestaurantReview.Controllers
{
    public class ReviewsController : Controller
    {
        private RestaurantEntities db = new RestaurantEntities();

        // GET: Reviews
		[AdminAuthorize(Users = "admin")] 
        public ActionResult Index()
        {
            var reviews = db.Reviews.Include(r => r.Restaurant).Include(r => r.User);
            return View(reviews.ToList());
        }

        // GET: Reviews/Details/5
		[AdminAuthorize(Users = "admin")] 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.reviewId = (int)id;
            return View(review);
        }

        // GET: Reviews/Create
		[AuthorizeRedirect]
        public ActionResult Create(int? id)
        {            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
         
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            ViewBag.restoId = id;
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[AuthorizeRedirect]
        public ActionResult Create([Bind(Include = "Content,Title,Rating")] Review review, int restoId)
        {
            if (ModelState.IsValid)
            {
                review.Restaurant_Id = restoId;
                review.Date = DateTime.Today.Date;
                review.Created_By = User.Identity.Name;
                             
                db.Reviews.Add(review);
                db.SaveChanges();

                setAverageRating(restoId);
                return RedirectToAction("Details", "Restaurants", new { id = restoId });
            }

            ViewBag.Restaurant_Id = new SelectList(db.Restaurants, "Restaurant_Id", "Name", review.Restaurant_Id);
            return View(review);
        }
  
      
        private void setAverageRating(int restoId) //Recalculates the average rating when a review is created/edited/deleted
        {
             Restaurant restaurant = db.Restaurants.Find(restoId);
                var allRatings = db.Reviews.Where(x => x.Restaurant_Id == restaurant.Restaurant_Id).Average(x => (x.Rating));
             restaurant.Average_Rating = (decimal)allRatings;
                db.Entry(restaurant).State = EntityState.Modified;
                db.SaveChanges();
        }
        // GET: Reviews/Edit/5
		[AdminAuthorize(Users = "admin")] 
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.reviewId = (int)id;
            ViewBag.Restaurant_Id = new SelectList(db.Restaurants, "Restaurant_Id", "Name", review.Restaurant_Id);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[AdminAuthorize(Users = "admin")] 
		public ActionResult Edit([Bind(Include = "Content,Title,Rating")] Review newReview, int reviewId)
        {
            if (ModelState.IsValid)
            {
				var review = (from x in db.Reviews where x.Review_Id == reviewId select x).FirstOrDefault();
				review.Title = newReview.Title;
				review.Content = newReview.Content;
				review.Rating = newReview.Rating;
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                setAverageRating(review.Restaurant_Id);

                return RedirectToAction("Details", "Restaurants", new { id = review.Restaurant_Id });
            }
			ViewBag.Review_Id = new SelectList(db.Reviews, "Review_Id", "Name", newReview.Review_Id);
            return View(newReview);
        }

        // GET: Reviews/Delete/5
		[AdminAuthorize(Users = "admin")] 
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[AdminAuthorize(Users = "admin")] 
        public ActionResult DeleteConfirmed(int id, int restoId)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
            db.SaveChanges();
            setAverageRating(restoId);
            return RedirectToAction("Index");
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
