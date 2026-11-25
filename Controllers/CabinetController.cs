using System.Security.Claims;
using CsArsenal.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CsArsenal.Controllers
{
    [Authorize]
    public class CabinetController : Controller
    {
        private readonly string _conn;
        private readonly ItemRepository _repo;

        public CabinetController(IConfiguration config)
        {
            _conn = config.GetConnectionString("Default")!;
            _repo = new ItemRepository(_conn);
        }

        private int UserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        public IActionResult Index()
        {
            ViewBag.Count = _repo.CountByUser(UserId());
            return View();
        }

        public IActionResult MyItems()
        {
            return View(_repo.GetByUser(UserId()));
        }

        public IActionResult Create()
        {
            return View(new ItemFormModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ItemFormModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var item = new Item
            {
                Title = model.Title.Trim(),
                Category = model.Category,
                Side = model.Side,
                Price = model.Price,
                Damage = model.Damage,
                Description = model.Description.Trim(),
                UserId = UserId()
            };
            _repo.Add(item);
            TempData["Message"] = "Запись добавлена.";
            return RedirectToAction("MyItems");
        }

        public IActionResult Edit(int id)
        {
            var item = _repo.GetById(id);
            if (item == null || (item.UserId != UserId() && !User.IsInRole("Admin")))
                return NotFound();

            var model = new ItemFormModel
            {
                Id = item.Id,
                Title = item.Title,
                Category = item.Category,
                Side = item.Side,
                Price = item.Price,
                Damage = item.Damage,
                Description = item.Description
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ItemFormModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var item = new Item
            {
                Id = model.Id,
                Title = model.Title.Trim(),
                Category = model.Category,
                Side = model.Side,
                Price = model.Price,
                Damage = model.Damage,
                Description = model.Description.Trim(),
                UserId = UserId()
            };

            bool ok = User.IsInRole("Admin") ? _repo.UpdateByAdmin(item) : _repo.Update(item);
            if (!ok) return NotFound();

            TempData["Message"] = "Запись обновлена.";
            return RedirectToAction("MyItems");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (User.IsInRole("Admin"))
                _repo.DeleteByAdmin(id);
            else
                _repo.Delete(id, UserId());

            TempData["Message"] = "Запись удалена.";
            return RedirectToAction("MyItems");
        }

        public IActionResult Profile()
        {
            var model = new ProfileModel();
            using (var con = new SqlConnection(_conn))
            {
                con.Open();
                using var cmd = new SqlCommand("SELECT Name, Email, RegisteredAt FROM Users WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", UserId());
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    model.Name = r.GetString(0);
                    model.Email = r.GetString(1);
                    model.RegisteredAt = r.GetDateTime(2);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using (var con = new SqlConnection(_conn))
            {
                con.Open();

                using var check = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @Email AND Id <> @Id", con);
                check.Parameters.AddWithValue("@Email", model.Email.Trim());
                check.Parameters.AddWithValue("@Id", UserId());
                if ((int)check.ExecuteScalar()! > 0)
                {
                    ModelState.AddModelError("Email", "Такой email уже занят");
                    return View(model);
                }

                using var cmd = new SqlCommand("UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Name", model.Name.Trim());
                cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
                cmd.Parameters.AddWithValue("@Id", UserId());
                cmd.ExecuteNonQuery();
            }

            // Обновляем имя в cookie, чтобы в личном кабинете сразу отображалось новое
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, UserId().ToString()),
                new Claim(ClaimTypes.Name, model.Name.Trim()),
                new Claim(ClaimTypes.Role, role)
            };
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));

            TempData["Message"] = "Профиль сохранён.";
            return RedirectToAction("Profile");
        }

        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using (var con = new SqlConnection(_conn))
            {
                con.Open();

                using var check = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Id = @Id AND PasswordHash = @Hash", con);
                check.Parameters.AddWithValue("@Id", UserId());
                check.Parameters.AddWithValue("@Hash", AccountController.Hash(model.OldPassword));
                if ((int)check.ExecuteScalar()! == 0)
                {
                    ModelState.AddModelError("OldPassword", "Старый пароль неверный");
                    return View(model);
                }

                using var cmd = new SqlCommand("UPDATE Users SET PasswordHash = @Hash WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Hash", AccountController.Hash(model.NewPassword));
                cmd.Parameters.AddWithValue("@Id", UserId());
                cmd.ExecuteNonQuery();
            }

            TempData["Message"] = "Пароль успешно изменён.";
            return RedirectToAction("ChangePassword");
        }
    }
}
