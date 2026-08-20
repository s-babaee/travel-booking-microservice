# Travel Booking Microservices

پلتفرم event-driven رزرو سفر با .NET، RabbitMQ، MassTransit، PostgreSQL و Keycloak.

## اجرای کل سیستم

```powershell
Copy-Item .env.example .env
# مقادیر حساس داخل .env را تغییر دهید
docker compose up -d --build
```

`docker-compose.yml` ریشه، تمام Microserviceها و زیرساخت مشترک را اجرا می‌کند:

- یک PostgreSQL مشترک با دیتابیس‌های مستقل `auth_db`، `hotel_db`، `booking_db` و `payment_db`
- یک RabbitMQ مشترک با Exchangeهای versioned و Queueهای مستقل
- یک Keycloak مشترک با دیتابیس مستقل خودش
- یک Dockerfile مستقل برای هر سرویس

هر سرویس فقط connection string دیتابیس خودش را از environment دریافت می‌کند. Migrationهای Auth، Hotel، Booking و Payment داخل همان پروژه‌ی سرویس نگهداری می‌شوند.

در محیط production به‌جای `.env` از Docker secrets یا Secret Manager استفاده کنید.
