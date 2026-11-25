using Microsoft.Data.SqlClient;

namespace CsArsenal.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Side { get; set; } = "";
        public int Price { get; set; }
        public int Damage { get; set; }
        public string Description { get; set; } = "";
        public int UserId { get; set; }
    }

    public class ItemRepository
    {
        private readonly string _conn;

        public ItemRepository(string conn)
        {
            _conn = conn;
        }

        private Item Read(SqlDataReader r)
        {
            return new Item
            {
                Id = r.GetInt32(0),
                Title = r.GetString(1),
                Category = r.GetString(2),
                Side = r.GetString(3),
                Price = r.GetInt32(4),
                Damage = r.GetInt32(5),
                Description = r.GetString(6),
                UserId = r.GetInt32(7)
            };
        }

        public List<Item> GetAll()
        {
            var list = new List<Item>();
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, Title, Category, Side, Price, Damage, Description, UserId FROM Items ORDER BY Id", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(Read(r));
            }
            return list;
        }

        public Item? GetById(int id)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, Title, Category, Side, Price, Damage, Description, UserId FROM Items WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            using var r = cmd.ExecuteReader();
            if (r.Read()) return Read(r);
            return null;
        }

        public List<Item> GetByUser(int userId)
        {
            var list = new List<Item>();
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, Title, Category, Side, Price, Damage, Description, UserId FROM Items WHERE UserId = @UserId ORDER BY Id DESC", con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(Read(r));
            }
            return list;
        }

        public int CountByUser(int userId)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM Items WHERE UserId = @UserId", con);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return (int)cmd.ExecuteScalar()!;
        }

        public void Add(Item item)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO Items (Title, Category, Side, Price, Damage, Description, UserId) VALUES (@Title, @Category, @Side, @Price, @Damage, @Description, @UserId)", con);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@Category", item.Category);
            cmd.Parameters.AddWithValue("@Side", item.Side);
            cmd.Parameters.AddWithValue("@Price", item.Price);
            cmd.Parameters.AddWithValue("@Damage", item.Damage);
            cmd.Parameters.AddWithValue("@Description", item.Description);
            cmd.Parameters.AddWithValue("@UserId", item.UserId);
            cmd.ExecuteNonQuery();
        }

        public bool Update(Item item)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand(
                "UPDATE Items SET Title = @Title, Category = @Category, Side = @Side, Price = @Price, Damage = @Damage, Description = @Description WHERE Id = @Id AND UserId = @UserId", con);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@Category", item.Category);
            cmd.Parameters.AddWithValue("@Side", item.Side);
            cmd.Parameters.AddWithValue("@Price", item.Price);
            cmd.Parameters.AddWithValue("@Damage", item.Damage);
            cmd.Parameters.AddWithValue("@Description", item.Description);
            cmd.Parameters.AddWithValue("@Id", item.Id);
            cmd.Parameters.AddWithValue("@UserId", item.UserId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateByAdmin(Item item)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand(
                "UPDATE Items SET Title = @Title, Category = @Category, Side = @Side, Price = @Price, Damage = @Damage, Description = @Description WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Title", item.Title);
            cmd.Parameters.AddWithValue("@Category", item.Category);
            cmd.Parameters.AddWithValue("@Side", item.Side);
            cmd.Parameters.AddWithValue("@Price", item.Price);
            cmd.Parameters.AddWithValue("@Damage", item.Damage);
            cmd.Parameters.AddWithValue("@Description", item.Description);
            cmd.Parameters.AddWithValue("@Id", item.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id, int userId)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand("DELETE FROM Items WHERE Id = @Id AND UserId = @UserId", con);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteByAdmin(int id)
        {
            using var con = new SqlConnection(_conn);
            con.Open();
            using var cmd = new SqlCommand("DELETE FROM Items WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }

    public static class DbInit
    {
        public static void EnsureCreated(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using (var masterCon = new SqlConnection(builder.ConnectionString))
            {
                masterCon.Open();
                using var createDb = new SqlCommand(
                    $"IF DB_ID(N'{dbName}') IS NULL CREATE DATABASE [{dbName}];", masterCon);
                createDb.ExecuteNonQuery();
            }

            using var con = new SqlConnection(connectionString);
            con.Open();

            using (var cmdUsers = new SqlCommand(@"
                IF OBJECT_ID(N'Users', N'U') IS NULL
                CREATE TABLE Users (
                    Id INT IDENTITY PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    Email NVARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash NVARCHAR(200) NOT NULL,
                    Role NVARCHAR(20) NOT NULL DEFAULT 'User',
                    RegisteredAt DATETIME NOT NULL DEFAULT GETDATE()
                );", con))
            {
                cmdUsers.ExecuteNonQuery();
            }

            using (var cmdItems = new SqlCommand(@"
                IF OBJECT_ID(N'Items', N'U') IS NULL
                CREATE TABLE Items (
                    Id INT IDENTITY PRIMARY KEY,
                    Title NVARCHAR(100) NOT NULL,
                    Category NVARCHAR(50) NOT NULL,
                    Side NVARCHAR(20) NOT NULL,
                    Price INT NOT NULL,
                    Damage INT NOT NULL,
                    Description NVARCHAR(1000) NOT NULL,
                    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id)
                );", con))
            {
                cmdItems.ExecuteNonQuery();
            }

            using var checkUsers = new SqlCommand("SELECT COUNT(*) FROM Users", con);
            if ((int)checkUsers.ExecuteScalar()! == 0)
            {
                using var seedUsers = new SqlCommand(@"
                    INSERT INTO Users (Name, Email, PasswordHash, Role) VALUES
                    (N'Администратор', N'admin@csarsenal.ru', N'3B612C75A7B5048A435FB6EC81E52FF92D6D795A8B5A9C17070F6A63C97A53B2', N'Admin'),
                    (N'Игрок', N'user@csarsenal.ru', N'A61A8ADF60038792A2CB88E670B20540A9D6C2CA204AB754FC768950E79E7D36', N'User');", con);
                seedUsers.ExecuteNonQuery();
            }

            using var checkItems = new SqlCommand("SELECT COUNT(*) FROM Items", con);
            if ((int)checkItems.ExecuteScalar()! == 0)
            {
                using var seedItems = new SqlCommand(@"
                    INSERT INTO Items (Title, Category, Side, Price, Damage, Description, UserId) VALUES
                    (N'AK-47', N'Винтовка', N'T', 2700, 36, N'Основная штурмовая винтовка террористов. Высокий урон и убийство в голову с одного выстрела даже в шлеме.', 1),
                    (N'M4A4', N'Винтовка', N'CT', 3000, 33, N'Классическая автоматическая винтовка спецназа с магазином на 30 патронов и высоким темпом стрельбы.', 1),
                    (N'AWP', N'Снайперская', N'Обе', 4750, 115, N'Мощная снайперская винтовка, способная устранить противника одним выстрелом в корпус или голову.', 1),
                    (N'Desert Eagle', N'Пистолет', N'Обе', 700, 53, N'Крупнокалиберный пистолет с огромным уроном. Убивает в голову с одного выстрела на ближней и средней дистанции.', 1),
                    (N'USP-S', N'Пистолет', N'CT', 200, 35, N'Стартовый пистолет спецназа со съёмным глушителем и высокой точностью первого выстрела.', 1),
                    (N'Glock-18', N'Пистолет', N'T', 200, 30, N'Стартовый пистолет террористов с большим магазином на 20 патронов и режимом стрельбы очередями.', 1);", con);
                seedItems.ExecuteNonQuery();
            }
        }
    }
}
