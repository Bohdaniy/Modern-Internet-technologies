using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationData.Data
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorEmail { get; set; }
        public int Year { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }
    }
}