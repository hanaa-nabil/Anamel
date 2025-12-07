using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Anamel.Core.Entities
{
   
    public class Product : BaseEntity
    {
        private static readonly Random _random = new Random();
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        [JsonIgnore]
        public virtual Category Category { get; set; }

        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, 5)]
        public int Rate { get; set; } = _random.Next(3, 6); 
    }
}
