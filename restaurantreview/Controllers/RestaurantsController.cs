using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestaurantReview.Models;
using System.Xml.Linq;
using System.Data.Entity.Spatial;
using System.Dynamic;
using RestaurantReview.Validators;
using RestaurantReview.Utility;

namespace RestaurantReview.Controllers
{
    public class RestaurantsController : Controller
    {
        private RestaurantEntities db = new RestaurantEntities();

        // GET: Restaurants
        public ActionResult Index()
        {
            var restaurants = db.Restaurants.Include(r => r.User);
            return View(restaurants.ToList());
        }

        // GET: Restaurants/Details/5
        public ActionResult Details(int? id)
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
           
            restaurant.Views++;
            db.Entry(restaurant).State = EntityState.Modified;
            db.SaveChanges();

           
           return View(restaurant);
        }

		public ActionResult Search()
		{
			return View();
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SearchResults([Bind(Include = "Name,Genre,City")] Restaurant restaurant) // Searching for a restaurant on 3 fields" Name, Genre and City
        {
            if (String.IsNullOrEmpty(restaurant.Name) && String.IsNullOrEmpty(restaurant.Genre) && String.IsNullOrEmpty(restaurant.City)) //Shows all if nothing entered
			{
				return RedirectToAction("Index");
			}
            var search = db.Restaurants.Where(x => x.Name.Contains(restaurant.Name) | 
				x.Genre.Contains(restaurant.Genre) |
                x.City.Contains(restaurant.City) ).ToList(); //Checking to see if we can find a match for name, genre or city. 

            return View("Index", search);
        }

        // GET: Restaurants/Create
		[AuthorizeRedirect]
        public ActionResult Create()
        {         
            return View();
        }

        // GET: Restaurants/Create
		[AdminAuthorize(Users = "admin")] 
        public ActionResult EditLog(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           // var changes = db.Changes.Where(r => r.Restaurant_Id == id).Select(a => a).ToList();
            var changes = (from x in db.Changes
                           where x.Restaurant_Id == id
                           select x).ToList();          
          
            return View(changes);
        }

        // POST: Restaurants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[AuthorizeRedirect]//So only authorized users can create
        //We Decided not to ask for postal code, since we figure a customer wouldn't really know what postal code the restaurant is
        public ActionResult Create([Bind(Include = "Name,Genre,Price,Province,City,Street_Name,Street_Number")] Restaurant restaurant)
		{
            if (ModelState.IsValid)
            {
                restaurant.Creation_Date = DateTime.Today.Date;
				restaurant.Created_By = User.Identity.Name;
                restaurant.Views = 0;
                var address = restaurant.Street_Number + restaurant.Street_Name + restaurant.City + restaurant.Province;
                var xElement = Util.GetGeocodingSearchResults(address);

                DbGeography place = Util.getGeoDude(restaurant.Street_Number + restaurant.Street_Name + restaurant.City + restaurant.Province);
              
                if(place == null)
                {
                    ModelState.AddModelError("", "Address was not precise enough");
                   return View("Create");
                }
                            
				var postalCode = (from x in xElement.Descendants("address_component") where x.Element("type").Value == "postal_code" select x.Element("long_name").Value).FirstOrDefault();
				var country = (from x in xElement.Descendants("address_component") where x.Element("type").Value == "country" select x.Element("long_name").Value).FirstOrDefault();

                if (postalCode == null || country == null)
                {
                    ModelState.AddModelError("", "Address was not precise enough");
                    return View("Create");
                }
                
                restaurant.Geographic_Location = place;
				restaurant.Postal_Code = postalCode;
				restaurant.Country = country;           
             
                db.Restaurants.Add(restaurant);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(restaurant);
        }

        // GET: Restaurants/Edit/5
		[AdminAuthorize (Users="admin")] 
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
			ViewBag.restaurantId = id;
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurant);
        }
        
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Revert(int? id) //Allows an admin to rever changes back on a restaurant to a prevoious point
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var query = (from x in db.Changes where x.Edit_id == id select x).FirstOrDefault();
            Restaurant restaurant = new Restaurant();
			restaurant.Name = query.Name;
			restaurant.Price = query.Price;
			restaurant.Genre = query.Genre;
			int restoId = query.Restaurant_Id;
			editRestaurant(restaurant, restoId);
            return RedirectToAction("EditLog", new { id = restoId } );
        }
        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[AdminAuthorize(Users = "admin")] 
		public ActionResult Edit([Bind(Include = "Name,Genre,Price")] Restaurant newRestaurant, int restaurantId)
        {
            if (ModelState.IsValid)
			{
                editRestaurant(newRestaurant, restaurantId);
                return RedirectToAction("Index");
            }
            return View(newRestaurant);
        }

        private void editRestaurant(Restaurant newResto, int restoId)
        {
            var restaurant = (from x in db.Restaurants where x.Restaurant_Id == restoId select x).FirstOrDefault();

            Change changes = new Change();
            changes.Restaurant_Id = restoId;
            changes.Name = restaurant.Name;
            changes.Changed_By = User.Identity.Name;
            changes.Change_Date = DateTime.Today.Date;
            changes.Price = restaurant.Price;
            changes.Genre = restaurant.Genre;
            db.Changes.Add(changes);
            db.SaveChanges();

            restaurant.Name = newResto.Name;
            restaurant.Genre = newResto.Genre;
            restaurant.Price = newResto.Price;

            db.Entry(restaurant).State = EntityState.Modified;
            db.SaveChanges();
        }

        // GET: Restaurants/Delete/5
		[AdminAuthorize(Users = "admin")] 
        public ActionResult Delete(int? id)
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
            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[AdminAuthorize(Users = "admin")] 
        public ActionResult DeleteConfirmed(int id)
        {
            Restaurant restaurant = db.Restaurants.Find(id);
            db.Restaurants.Remove(restaurant);
            db.SaveChanges();
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
