using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace AmazonCloneMVC.Models
{

    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    public static class SessionExtensions 
    {
        public static void SetCartService(this ISession session, CartProvider cartService)
        {
            string cartServiceJson = JsonConvert.SerializeObject(cartService);
            session.SetString("CartProvider", cartServiceJson);
        }

        public static CartProvider GetCartService(this ISession session)
        {
            string cartServiceJson = session.GetString("CartProvider");
            if (cartServiceJson != null)
            {
                return JsonConvert.DeserializeObject<CartProvider>(cartServiceJson);
            }
            return new CartProvider(); // Return  if the object is not found in session
        }


    }
   

}
