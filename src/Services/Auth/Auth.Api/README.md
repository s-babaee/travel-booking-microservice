# Auth.Api

سرویس احراز هویت مبتنی بر ASP.NET Core، Keycloak و PostgreSQL است. ساختار کد به‌صورت لایه‌ای و DDD طراحی شده است:

- `Domain`: موجودیت‌ها، enumها و قواعد اصلی دامنه
- `Application`: قراردادها، abstractionها و use caseها
- `Infrastructure`: EF Core/PostgreSQL و integration با Keycloak Admin/OpenID Connect
- `Controllers`: لایهٔ HTTP و endpointها

## اجرای کل سیستم با Docker Compose

ابتدا Docker Desktop را اجرا کنید، سپس در ریشه‌ی repository:

```powershell
Copy-Item .env.example .env
# مقادیر داخل .env را تغییر دهید
docker compose up -d --build
```

سرویس‌ها:

- PostgreSQL مشترک در `localhost:5432`
- Keycloak مشترک در `http://localhost:8081`
- Realm: `travel`
- Client: `travel-auth-api`
- کاربر اولیه: `admin` و password تعریف‌شده در `TRAVEL_ADMIN_PASSWORD`

دیتابیس Auth با نام `auth_db` و کاربر اختصاصی `AUTH_DB_USER` ساخته می‌شود. این سرویس فقط connection string دیتابیس خودش را دریافت می‌کند؛ دسترسی مستقیم به دیتابیس سرویس‌های دیگر وجود ندارد.

برای اجرای API خارج از Docker:

```powershell
dotnet run
```

در اولین اجرا migration `InitialCreate` روی دیتابیس `auth_db` اعمال می‌شود.

## نکات محیط production

مقادیر `Keycloak:ClientSecret`، `Keycloak:AdminPassword` و connection string را با environment variable یا secret manager جایگزین کنید. مقدار `Auth:ExposeResetToken` فقط برای Development فعال شده است؛ در production توکن reset باید از طریق سرویس ارسال ایمیل تحویل کاربر شود.

## نمونهٔ ورود

```http
POST http://localhost:5289/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin"
}
```

برای endpointهای مدیریتی، access token کاربر باید claim نقش `admin` داشته باشد. نقش‌ها و مجوزهای ساخته‌شده از طریق API به‌عنوان realm role در Keycloak نیز ثبت می‌شوند؛ مجوزها با پیشوند `permission:` ذخیره می‌شوند و به‌صورت composite role به نقش‌ها متصل می‌گردند.
