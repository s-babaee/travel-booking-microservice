# Booking Service

سرویس Booking با معماری Clean Architecture و DDD در یک پروژه ماژولار پیاده‌سازی شده است:

- `Domain`: Aggregate رزرو، قوانین انتقال وضعیت و Order.
- `Application`: use caseها، قراردادها و portهای ارتباط با Inventory/Payment.
- `Infrastructure`: EF Core/PostgreSQL، gatewayهای HTTP، RabbitMQ و احراز هویت.
- `Controllers`: APIهای کاربر، سفارش و مدیریت.

## Saga

برای ایجاد رزرو، orchestrator این مراحل را اجرا می‌کند:

1. ایجاد رزرو با وضعیت `PendingInventory`.
2. `POST /api/inventory/hotels/hold` یا `POST /api/inventory/flights/hold`.
3. `POST /api/payments/authorize`.
4. تأیید موجودی با `confirm`.
5. ایجاد Order و تغییر وضعیت به `Confirmed`.

در شکست پرداخت یا تأیید موجودی، ابتدا payment با `void` و سپس inventory با `release` جبران می‌شود و رزرو `Failed` می‌گردد.

## احراز هویت

تمام endpointهای کاربری به JWT Keycloak نیاز دارند. شناسه کاربر از claim `sub` خوانده می‌شود. endpointهای `/api/admin/*` و لیست کلی سفارش‌ها policy نقش `admin` دارند.

## اجرای توسعه

برای اجرای مستقیم با SDK نصب‌شده روی سیستم، ابتدا فقط dependencyهای توسعه را بالا بیاورید:

```powershell
docker compose -f docker-compose.booking.dev.yml up -d
```

این compose فقط imageهای PostgreSQL، RabbitMQ و Keycloak را اجرا می‌کند و SDK/runtime دات‌نت ندارد. سپس API را با SDK محلی اجرا کنید و connection string و تنظیمات RabbitMQ را با environment variable تنظیم کنید.

در محیط Development، `Payment:Mode=Mock` قابل استفاده است و مقدار token برابر `declined` مسیر شکست Saga را شبیه‌سازی می‌کند. در Docker مقدار `Payment:Mode=Http` است و قراردادهای زیر را از Payment Service انتظار دارد:

- `POST /api/payments/authorize`
- `POST /api/payments/{transactionId}/void`

Inventory از endpointهای موجود خودش استفاده می‌شود.
