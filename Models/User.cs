
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AmazonCloneMVC.Models
{
    [Serializable]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userId { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int? CartId { get; set; }
        public Cart? Cart { get; set; }
    }
}
