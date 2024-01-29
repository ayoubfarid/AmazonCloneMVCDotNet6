using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AmazonCloneMVC.Models
{
    
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        public ICollection<Product>? Products { get; set; } = new List<Product>();

        public int? UserId { get; set; }
        //public string CartProductIds { get; set; }  
        [ForeignKey("UserId")]
        public User User { get; set; }
        public void AddProductById(int productId)
        {

        }
        public void ClearCart()
        {
            //CartProductIds = "[]";
            Products = new List<Product>();
        }
    
}
}
