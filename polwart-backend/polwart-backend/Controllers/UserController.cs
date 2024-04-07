using Microsoft.AspNetCore.Mvc;
using polwart_backend.Entities;

namespace polwart_backend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private const string method_route = "[action]";
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route(method_route)]
        public void Register(RegisterRequest request)
        {
            string secret = PasswordHash.PasswordHash.CreateHash(request.Password);

            try
            {
                _context.Users.Add(new() { Login = request.Login, Password = secret, Name = request.Name });
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Response.StatusCode = 500;
            }
        }

        [HttpPost]
        [Route(method_route)]
        public AuthResponse Auth(AuthRequest request)
        {
            try
            {
                List<User> users =
                [
                    .. _context.Users.Where(p => p.Login == request.Login),
                ];

                if (users.Count < 1)
                {
                    Response.StatusCode = 404;
                    return new();
                }

                if (!PasswordHash.PasswordHash.ValidatePassword(request.Password, users[0].Password))
                {
                    Response.StatusCode = 403;
                    return new();
                }

                return new() { User = users[0] };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Response.StatusCode = 500;

                return new();
            }
        }



        public class RegisterRequest
        {
            public required string Login { get; set; }
            public required string Password { get; set; }
            public required string Name { get; set; }
        }

        public class AuthRequest
        {
            public required string Login { get; set; }
            public required string Password { get; set; }
        }
        public class AuthResponse
        {
            public User? User { get; set; } = null;
        }
    }
}
