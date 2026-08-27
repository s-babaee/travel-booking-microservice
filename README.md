
---
# Travel Booking Microservices

پلتفرم event-driven رزرو سفر با .NET، RabbitMQ، MassTransit، PostgreSQL و Keycloak.

## اجرای کل سیستم

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

`docker-compose.yml` ریشه، تمام Microserviceها و زیرساخت مشترک را اجرا می‌کند:

- یک PostgreSQL مشترک با دیتابیس‌های مستقل `auth_db`، `hotel_db`، `booking_db` و `payment_db`
- یک RabbitMQ مشترک با Exchangeهای versioned و Queueهای مستقل
- یک Keycloak مشترک با دیتابیس مستقل خودش
- یک Dockerfile مستقل برای هر سرویس

هر سرویس فقط connection string دیتابیس خودش را از environment دریافت می‌کند. Migrationهای Auth، Hotel، Booking و Payment داخل همان پروژه‌ی سرویس نگهداری می‌شوند.

در محیط production به‌جای `.env` از Docker secrets یا Secret Manager استفاده کنید.

## فرانت React

فرانت در مسیر `frontend` با React و Vite قرار دارد و از API Gateway استفاده می‌کند.

اجرای توسعه:

```powershell
cd frontend
Copy-Item .env.example .env
npm install
npm run dev
```

سپس فرانت روی `http://localhost:5173` و API Gateway روی `http://localhost:5000` در دسترس است. Vite مسیرهای `/api` و `/uploads` را به Gateway پروکسی می‌کند.

اجرای کامل با Docker:

```powershell
docker compose up -d --build
```

در این حالت فرانت روی `http://localhost:3000` ارائه می‌شود. حساب اولیه‌ی مدیر از تنظیمات Keycloak در `.env` ساخته می‌شود.
