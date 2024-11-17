using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime LastModified { get; set; } = DateTime.UtcNow;
    }
}
