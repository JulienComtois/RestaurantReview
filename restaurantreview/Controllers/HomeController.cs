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
using System.Text.RegularExpressions;
using System.Web.Routing;
using RestaurantReview.Utility;

namespace RestaurantReview.Controllers
{
    public class HomeController : Controller
    {
        private RestaurantEntities db = new RestaurantEntities();

        // GET: Home
        public ActionResult Index()
        {
            var restaurants = db.Restaurants.Include(r => r.User);
            return View(restaurants.ToList());
        }
     
        [HttpPost]
        //Gets the 5 nearest restaurants, if user doesn't allow geolocation, will take in postlCode and 
        // find the nearest
        public ActionResult Nearest(int error, string latitude, string longitude, string postalCode)
        {

            DbGeography place = null;
            if(error == 0 && !latitude.Equals("0") && !longitude.Equals("0"))
           {
             place = DbGeography.FromText(string.Format("POINT({1} {0})", latitude, longitude), 4326);
           }
          
            else if (postalCode.Equals(""))
            {
                ModelState.AddModelError("", "Please enter a Postl Code"); //If they didn't enter anything
                   return View("Index");
            }

            else //PostalCode given
           {   
              place = Util.getGeoDude(postalCode);
              
               if (place == null) //check to see if no place was found
               {
                   ModelState.AddModelError("", "Your location was not precise enough");
                   return View("Index");
               }            
           }  
            var closest = (from r in db.Restaurants
                           orderby r.Geographic_Location.Distance(place)  //userLoc is a DbGeography that represents the user’s location
                           select r).Take(5).ToList();

            return View(closest);
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
