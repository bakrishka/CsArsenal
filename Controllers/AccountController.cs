using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CsArsenal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CsArsenal.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _conn;

        public AccountController(IConfiguration config)
        {
            _conn = config.GetConnectionString("Default")!;
        }

        public static string Hash(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using (var con = new SqlConnection(_conn))
            {
                con.Open();

                using var check = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email", con);
                check.Parameters.AddWithValue("@Email", model.Email.Trim());
                if ((int)check.ExecuteScalar()! > 0)
                {
                    ModelState.AddModelError("Email", "Такой email уже зарегистрирован");
                    return View(model);
                }

                using var cmd = new SqlCommand("INSERT INTO Users (Name, Email, PasswordHash) VALUES (@Name, @Email, @Hash)", con);
                cmd.Parameters.AddWithValue("@Name", model.Name.Trim());
                cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
                cmd.Parameters.AddWithValue("@Hash", Hash(model.Password));
                cmd.ExecuteNonQuery();
            }

            TempData["Message"] = "Регистрация успешна! Теперь войдите в аккаунт.";
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            int id = 0;
            string name = "";
            string role = "";

            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using var cmd = new SqlCommand("SELECT Id, Name, Role FROM Users WHERE Email = @Email AND PasswordHash = @Hash", con);
                cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
                cmd.Parameters.AddWithValue("@Hash", Hash(model.Password));

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                    name = reader.GetString(1);
                    role = reader.GetString(2);
                }
            }

            if (id == 0)
            {
                ModelState.AddModelError("", "Неверный email или пароль");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Cookies");
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Cabinet");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
