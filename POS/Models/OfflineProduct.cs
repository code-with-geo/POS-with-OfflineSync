using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Models
{
    public class OfflineProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public bool IsSynced { get; set; } = false; 
    }
}
