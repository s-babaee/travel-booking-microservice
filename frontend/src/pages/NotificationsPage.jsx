import { useEffect, useState } from 'react'
import EmptyState from '../components/EmptyState'
import ErrorNotice from '../components/ErrorNotice'
import LoadingState from '../components/LoadingState'
import PageHeader from '../components/PageHeader'
import { useAuth } from '../context/AuthContext'
import { formatDateTime, safeArray } from '../lib/formatters'

export default function NotificationsPage() {
  const { request } = useAuth()
  const [notifications, setNotifications] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [isMarkingAll, setIsMarkingAll] = useState(false)

  async function loadNotifications() {
    setIsLoading(true)
    setError('')
    try {
      const response = await request('notifications/me?page=1&pageSize=50')
      setNotifications(safeArray(response?.items || response?.Items))
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadNotifications()
  }, [request])

  async function markRead(id) {
    try {
      const updated = await request(`notifications/${id}/read`, { method: 'POST' })
      setNotifications((current) =>
        current.map((notification) => (notification.id === id ? updated : notification))
      )
    } catch (requestError) {
      setError(requestError.message)
    }
  }

  async function markAllRead() {
    setIsMarkingAll(true)
    setError('')
    try {
      await request('notifications/read-all', { method: 'POST' })
      setNotifications((current) =>
        current.map((notification) => ({ ...notification, isRead: true }))
      )
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsMarkingAll(false)
    }
  }

  const unreadCount = notifications.filter((notification) => !notification.isRead).length

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="مرکز پیام"
        title="اعلان‌های شما"
        description="خبرهای مربوط به رزرو و وضعیت سفر را اینجا دنبال کنید."
        actions={
          unreadCount ? (
            <button
              type="button"
              className="button button-outline"
              onClick={markAllRead}
              disabled={isMarkingAll}
            >
              {isMarkingAll ? 'در حال به‌روزرسانی...' : 'خواندن همه'}
            </button>
          ) : null
        }
      />

      <ErrorNotice message={error} onRetry={loadNotifications} />

      {isLoading ? (
        <LoadingState />
      ) : notifications.length ? (
        <div className="panel notification-panel">
          <div className="notification-summary">
            <span className="summary-dot" />
            <span>{unreadCount ? `${unreadCount} اعلان خوانده‌نشده` : 'همه اعلان‌ها خوانده شده‌اند'}</span>
          </div>
          <div className="notification-page-list">
            {notifications.map((notification) => (
              <article
                className={`notification-card ${notification.isRead ? 'is-read' : 'is-unread'}`}
                key={notification.id}
              >
                <div className="notification-card-icon">
                  {notification.eventType?.toLowerCase().includes('booking') ? '▣' : '◌'}
                </div>
                <div className="notification-card-content">
                  <div className="notification-card-top">
                    <h3>{notification.subject || 'اعلان سفر'}</h3>
                    <time>{formatDateTime(notification.createdAtUtc)}</time>
                  </div>
                  <p>{notification.body}</p>
                  <span className="notification-event">{notification.eventType || 'Travel event'}</span>
                </div>
                {!notification.isRead ? (
                  <button
                    type="button"
                    className="button button-ghost button-small"
                    onClick={() => markRead(notification.id)}
                  >
                    خواندم
                  </button>
                ) : (
                  <span className="read-label">خوانده شده ✓</span>
                )}
              </article>
            ))}
          </div>
        </div>
      ) : (
        <div className="panel">
          <EmptyState
            icon="◌"
            title="اعلانی برای شما نیست"
            description="با ثبت رزرو، وضعیت‌های مهم در این بخش نمایش داده می‌شوند."
          />
        </div>
      )}
    </div>
  )
}
