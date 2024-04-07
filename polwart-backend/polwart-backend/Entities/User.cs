using System.ComponentModel.DataAnnotations;

namespace polwart_backend.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Login { get; set; }
        public required string Password { get; set; }
    }
}
