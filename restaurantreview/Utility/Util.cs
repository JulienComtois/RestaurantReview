using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace RestaurantReview.Utility
{
    public class Util
    {
        public static bool IsAdmin(string username)
        {
            if (username.Equals("admin"))
            {  
                return true;
            }
            return false;
        }

        public static XElement GetGeocodingSearchResults(string address) //XElement holds the XML result, withing the System.Xml.Linq namespace
        {
            var url = String.Format("http://maps.google.com/maps/api/geocode/xml?address={0}&sensor=false", HttpUtility.UrlEncode(address));  //Url encode since it was provided by user

            // Load the XML into an XElement object
            var results = XElement.Load(url);

            return results;
        }

        public static DbGeography  getGeoDude (String address)
        {
             var xElement = GetGeocodingSearchResults(address);
             var numbResults = (from x in xElement.Descendants("result") select x).ToList();
           
            if(xElement.Value == "ZERO_RESULTS" || numbResults.Count > 1 ) //Checking to see if there was no results found or if there wre more than 1 result found
            {              
                return null;
            }

            var latitude = (xElement.Descendants("geometry").Descendants("location").First().Element("lat").Value); //Getting lat
            var longitude = (xElement.Descendants("geometry").Descendants("location").First().Element("lng").Value); //Getting long
           
            return DbGeography.FromText(string.Format("POINT({1} {0})", latitude, longitude), 4326);
        }
         
    }
} 