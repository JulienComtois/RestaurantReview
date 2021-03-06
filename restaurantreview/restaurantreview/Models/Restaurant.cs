//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RestaurantReview.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class Restaurant
    {
        public Restaurant()
        {
            this.Changes = new HashSet<Change>();
            this.Reviews = new HashSet<Review>();
        }
    
        public int Restaurant_Id { get; set; }
        public string Name { get; set; }
        public string Genre { get; set; }
        public byte Price { get; set; }
        public System.Data.Entity.Spatial.DbGeography Geographic_Location { get; set; }

        [Display(Name = "Postal Code")]
        public string Postal_Code { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string City { get; set; }

        [Display(Name = "Street Name")]
        public string Street_Name { get; set; }

        [Display(Name = "Street Number")]
        public int Street_Number { get; set; }
        public System.DateTime Creation_Date { get; set; }
        public string Created_By { get; set; }
        public int Views { get; set; }

        [Display(Name = "Avg Rating")]
        public Nullable<decimal> Average_Rating { get; set; }
    
        public virtual ICollection<Change> Changes { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
