IF DB_ID(N'CsArsenalDb') IS NULL
    CREATE DATABASE CsArsenalDb;
GO

USE CsArsenalDb;
GO

IF OBJECT_ID(N'Users', N'U') IS NULL
CREATE TABLE Users (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',
    RegisteredAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

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
);
GO

-- Тестовые пользователи:
-- Админ: admin@csarsenal.ru / Admin123
-- Пользователь: user@csarsenal.ru / User123
IF NOT EXISTS (SELECT 1 FROM Users)
BEGIN
    INSERT INTO Users (Name, Email, PasswordHash, Role) VALUES
    (N'Администратор', N'admin@csarsenal.ru', N'3B612C75A7B5048A435FB6EC81E52FF92D6D795A8B5A9C17070F6A63C97A53B2', N'Admin'),
    (N'Игрок', N'user@csarsenal.ru', N'A61A8ADF60038792A2CB88E670B20540A9D6C2CA204AB754FC768950E79E7D36', N'User');
END
GO

IF NOT EXISTS (SELECT 1 FROM Items)
BEGIN
    INSERT INTO Items (Title, Category, Side, Price, Damage, Description, UserId) VALUES
    (N'AK-47', N'Винтовка', N'T', 2700, 36, N'Основная штурмовая винтовка террористов. Высокий урон и убийство в голову с одного выстрела даже в шлеме.', 1),
    (N'M4A4', N'Винтовка', N'CT', 3000, 33, N'Классическая автоматическая винтовка спецназа с магазином на 30 патронов и высоким темпом стрельбы.', 1),
    (N'AWP', N'Снайперская', N'Обе', 4750, 115, N'Мощная снайперская винтовка, способная устранить противника одним выстрелом в корпус или голову.', 1),
    (N'Desert Eagle', N'Пистолет', N'Обе', 700, 53, N'Крупнокалиберный пистолет с огромным уроном. Убивает в голову с одного выстрела на ближней и средней дистанции.', 1),
    (N'USP-S', N'Пистолет', N'CT', 200, 35, N'Стартовый пистолет спецназа со съёмным глушителем и высокой точностью первого выстрела.', 1),
    (N'Glock-18', N'Пистолет', N'T', 200, 30, N'Стартовый пистолет террористов с большим магазином на 20 патронов и режимом стрельбы очередями.', 1);
END
GO
