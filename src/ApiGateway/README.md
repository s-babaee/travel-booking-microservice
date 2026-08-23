# API Gateway

Gateway ورودی HTTP پروژه‌ی Travel Booking است و درخواست‌ها را با YARP به
Microserviceهای داخلی ارسال می‌کند.

## اجرای مستقیم در محیط توسعه

ابتدا سرویس‌های زیرساخت و سرویس‌های API موردنیاز را اجرا کنید، سپس:

```powershell
dotnet run --project src/ApiGateway/ApiGateway.csproj
```

Gateway روی `http://localhost:5000` در دسترس است. برای مثال:

- `POST /api/auth/login` → Auth
- `/api/hotels/*` و `/uploads/*` → Hotel
- `/api/flights/*`، `/api/routes/*` و `/api/airlines/*` → Flight
- `/api/bookings/*` و `/api/orders/*` → Booking
- `/api/payments/*` و `/api/refunds/*` → Payment
- `/api/inventory/*` → Inventory
- `/api/notifications/*` → Notification
- `/api/search/*` → Search (پیشوند `/api/search` قبل از ارسال حذف می‌شود)

به‌جز endpointهای عمومی Auth، reset password و `/health`، تمام مسیرها به JWT
معتبر Keycloak نیاز دارند. مسیرهای کاربران، نقش‌ها و مجوزها علاوه بر JWT به
نقش `admin` نیز نیاز دارند.

آدرس سرویس‌ها در `appsettings.Development.json` برای اجرای مستقیم و در
`appsettings.json` برای شبکه‌ی Docker تعریف شده‌اند.
