using System.ComponentModel.DataAnnotations;

namespace CsArsenal.Models
{
    public class ItemFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название оружия")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Выберите категорию")]
        public string Category { get; set; } = "Винтовка";

        [Required(ErrorMessage = "Выберите сторону")]
        public string Side { get; set; } = "Обе";

        [Range(0, 16000, ErrorMessage = "Цена от 0 до 16000")]
        public int Price { get; set; }

        [Range(1, 500, ErrorMessage = "Урон от 1 до 500")]
        public int Damage { get; set; } = 30;

        [Required(ErrorMessage = "Введите описание")]
        [StringLength(1000, ErrorMessage = "Максимум 1000 символов")]
        public string Description { get; set; } = "";
    }

    public class ProfileModel
    {
        [Required(ErrorMessage = "Введите имя")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Неверный email")]
        public string Email { get; set; } = "";

        public DateTime RegisteredAt { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Введите старый пароль")]
        public string OldPassword { get; set; } = "";

        [Required(ErrorMessage = "Введите новый пароль")]
        [MinLength(6, ErrorMessage = "Минимум 6 символов")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Повторите новый пароль")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = "";
    }
}
