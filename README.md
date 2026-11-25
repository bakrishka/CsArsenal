# CS Arsenal

Учебный многостраничный сайт на ASP.NET Core MVC с доступом к БД через ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`).

## Запуск
1. Откройте `CsArsenal.sln` в Visual Studio 2022.
2. Нажмите F5 (или `dotnet run`). При первом запуске база данных `CsArsenalDb`, таблицы `Users` и `Items`, а также тестовые записи создадутся автоматически (при желании можно выполнить `db.sql` вручную в SSMS).
3. Если используется SQL Server Express вместо LocalDB, замените строку подключения в `appsettings.json` на `Server=localhost\SQLEXPRESS;Database=CsArsenalDb;Trusted_Connection=True;TrustServerCertificate=True;`.

## Тестовые аккаунты
- **Администратор:** `admin@csarsenal.ru` / `Admin123`
- **Пользователь:** `user@csarsenal.ru` / `User123`
