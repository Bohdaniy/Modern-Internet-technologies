using System.ComponentModel.DataAnnotations.Schema; // Додайте цей using

namespace WebApplicationData.Data
{
    public class AppResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorId { get; set; }


        [ForeignKey("AuthorId")] 
        public WebApplicationUser Author { get; set; }
    }
}