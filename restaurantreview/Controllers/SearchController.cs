using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestaurantReview.Models;

namespace RestaurantReview.Controllers
{
    public class SearchController : Controller
    {
        private RestaurantEntities db = new RestaurantEntities();

        // GET: Search
        public ActionResult Index()
        {
            var restaurants = db.Restaurants.Include(r => r.User);
            return View(restaurants.ToList());
        }
        
        // Get Search/Search
        public ActionResult Search()
        {
            return View();
        }
        
        //Post Search/SearchResults
         public ActionResult SearchResults(string restoName, string genre, string city )
        {
			if (restoName == null && genre == null && city == null || restoName.Equals("") && genre.Equals("") && city.Equals(""))
			{
				return RedirectToAction("Index", "Restaurants");
			}

            var search = db.Restaurants.Where(r => r.Name.Contains(restoName) | r.Genre.Contains(genre) | r.City.Contains(city)).ToList();

            return View("ResultList", search);
        }

		 [HttpPost]
		 [ValidateAntiForgeryToken]
		 public ActionResult SearchResults([Bind(Include = "Name,Genre,City")] Restaurant restaurant)
		 {
			 if (restaurant.Name == null && restaurant.Genre == null && city == null || restoName.Equals("") && genre.Equals("") && city.Equals(""))
			 {
				 return RedirectToAction("Index", "Restaurants");
			 }
			 //  var search = db.Restaurants.Where(r => r.Name.Contains(restoName) | r.Genre.Contains(genre) | r.City.Contains(city)).ToList();
			 var search = db.Restaurants.Where(x => x.Name.Contains(restaurant.Name) | x.Genre.Contains(restaurant.Genre) |
				x.City.Contains(restaurant.City)).ToList();
			 //var results = db.Restaurants.Where(x => x.Name.Contains(restaurant.Name) | x.Genre.Contains(restaurant.Genre) |
			 //    x.City.Contains(restaurant.City) | x.SearchTerms.Contains(restaurant.SearchTerms)).ToList();

			 return View("Index", search);
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
