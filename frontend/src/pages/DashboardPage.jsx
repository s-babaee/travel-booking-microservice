import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import EmptyState from '../components/EmptyState'
import ErrorNotice from '../components/ErrorNotice'
import LoadingState from '../components/LoadingState'
import PageHeader from '../components/PageHeader'
import StatusBadge from '../components/StatusBadge'
import { useAuth } from '../context/AuthContext'
import {
  compactId,
  formatDate,
  formatDateTime,
  formatMoney,
  safeArray
} from '../lib/formatters'

function responseItems(response) {
  return safeArray(response?.items || response?.Items)
}

function countConfirmed(bookings) {
  return bookings.filter((booking) => String(booking.status).toLowerCase() === 'confirmed').length
}

export default function DashboardPage() {
  const { user, request } = useAuth()
  const [data, setData] = useState({ bookings: [], notifications: [] })
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  async function loadDashboard() {
    setError('')
    setIsLoading(true)

    try {
      const [bookingResponse, notificationResponse] = await Promise.all([
        request('bookings/user/me?page=1&pageSize=8'),
        request('notifications/me?page=1&pageSize=5')
      ])

      setData({
        bookings: responseItems(bookingResponse),
        notifications: responseItems(notificationResponse)
      })
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadDashboard()
  }, [request])

  const confirmed = countConfirmed(data.bookings)
  const totalSpend = data.bookings
    .filter((booking) => String(booking.status).toLowerCase() === 'confirmed')
    .reduce((total, booking) => total + Number(booking.totalAmount || 0), 0)
  const unreadCount = data.notifications.filter((notification) => !notification.isRead).length
  const displayName = user?.firstName || user?.username || 'مسافر'

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="نمای کلی"
        title={`سلام ${displayName}، آماده‌ی سفر هستید؟`}
        description="وضعیت رزروها و آخرین فعالیت‌های حساب شما را در یک نگاه ببینید."
        actions={
          <Link to="/app/bookings/new" className="button button-accent">
            <span>＋</span>
            رزرو جدید
          </Link>
        }
      />

      <ErrorNotice message={error} onRetry={loadDashboard} />

      {isLoading ? (
        <LoadingState />
      ) : (
        <>
          <section className="stat-grid">
            <article className="stat-card stat-card-dark">
              <div className="stat-card-top">
                <span className="stat-icon">▣</span>
                <span className="stat-trend">این حساب</span>
              </div>
              <strong>{data.bookings.length}</strong>
              <span>کل رزروها</span>
            </article>
            <article className="stat-card">
              <div className="stat-card-top">
                <span className="stat-icon stat-icon-accent">✓</span>
                <span className="stat-trend trend-up">موفق</span>
              </div>
              <strong>{confirmed}</strong>
              <span>رزروهای تأییدشده</span>
            </article>
            <article className="stat-card">
              <div className="stat-card-top">
                <span className="stat-icon stat-icon-lilac">◌</span>
                <span className="stat-trend">پیام جدید</span>
              </div>
              <strong>{unreadCount}</strong>
              <span>اعلان خوانده‌نشده</span>
            </article>
            <article className="stat-card">
              <div className="stat-card-top">
                <span className="stat-icon stat-icon-blue">↗</span>
                <span className="stat-trend">تأییدشده</span>
              </div>
              <strong>{formatMoney(totalSpend, data.bookings[0]?.currency || 'USD')}</strong>
              <span>مجموع ارزش رزروها</span>
            </article>
          </section>

          <section className="dashboard-grid">
            <div className="panel panel-main">
              <div className="panel-header">
                <div>
                  <span className="eyebrow">آخرین فعالیت</span>
                  <h2>رزروهای اخیر</h2>
                </div>
                <Link to="/app/bookings" className="text-link">
                  مشاهده همه <span>←</span>
                </Link>
              </div>
              {data.bookings.length ? (
                <div className="booking-list">
                  {data.bookings.slice(0, 5).map((booking) => (
                    <Link
                      to={`/app/bookings/${booking.id}`}
                      className="booking-row"
                      key={booking.id}
                    >
                      <span className={`booking-type-icon ${String(booking.type).toLowerCase()}`}>
                        {String(booking.type).toLowerCase() === 'flight' ? '✈' : '⌂'}
                      </span>
                      <span className="booking-row-main">
                        <strong>
                          {booking.type === 'Flight' ? 'رزرو پرواز' : 'رزرو هتل'}
                        </strong>
                        <small>
                          {formatDate(booking.flightDate || booking.checkIn)} · {compactId(booking.id)}
                        </small>
                      </span>
                      <span className="booking-row-amount">
                        <strong>{formatMoney(booking.totalAmount, booking.currency)}</strong>
                        <StatusBadge value={booking.status} />
                      </span>
                      <span className="row-arrow">←</span>
                    </Link>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon="✈"
                  title="هنوز رزروی ثبت نشده"
                  description="اولین سفر خود را همین حالا برنامه‌ریزی کنید."
                  action={
                    <Link to="/app/bookings/new" className="button button-dark button-small">
                      ثبت اولین رزرو
                    </Link>
                  }
                />
              )}
            </div>

            <div className="panel panel-side">
              <div className="panel-header">
                <div>
                  <span className="eyebrow">پیام‌ها</span>
                  <h2>اعلان‌های اخیر</h2>
                </div>
                <Link to="/app/notifications" className="icon-link" aria-label="همه اعلان‌ها">
                  ↗
                </Link>
              </div>
              {data.notifications.length ? (
                <div className="notification-list">
                  {data.notifications.slice(0, 4).map((notification) => (
                    <Link
                      to="/app/notifications"
                      className={`notification-row ${notification.isRead ? '' : 'unread'}`}
                      key={notification.id}
                    >
                      <span className="notification-dot" />
                      <span>
                        <strong>{notification.title || 'اعلان جدید'}</strong>
                        <small>{notification.body || notification.message || 'برای مشاهده جزئیات باز کنید.'}</small>
                        <time>{formatDateTime(notification.createdAtUtc || notification.createdAt)}</time>
                      </span>
                    </Link>
                  ))}
                </div>
              ) : (
                <EmptyState
                  icon="◌"
                  title="اعلان جدیدی ندارید"
                  description="خبرهای مهم سفر در این بخش نمایش داده می‌شوند."
                />
              )}
            </div>
          </section>

          <section className="inspiration-banner">
            <div>
              <span className="eyebrow eyebrow-light">لحظه‌ی برنامه‌ریزی است</span>
              <h2>سفر بعدی‌تان را از همین امروز بسازید.</h2>
              <p>هتل یا پرواز موردنظر را انتخاب کنید و بقیه را به ما بسپارید.</p>
            </div>
            <Link to="/app/bookings/new" className="button button-light">
              شروع برنامه‌ریزی <span>←</span>
            </Link>
            <div className="banner-sparkle">✦</div>
          </section>
        </>
      )}
    </div>
  )
}
