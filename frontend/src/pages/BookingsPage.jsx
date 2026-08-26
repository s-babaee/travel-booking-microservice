import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import EmptyState from '../components/EmptyState'
import ErrorNotice from '../components/ErrorNotice'
import LoadingState from '../components/LoadingState'
import PageHeader from '../components/PageHeader'
import StatusBadge from '../components/StatusBadge'
import { useAuth } from '../context/AuthContext'
import { compactId, formatDate, formatDateTime, formatMoney, safeArray } from '../lib/formatters'

function responseItems(response) {
  return safeArray(response?.items || response?.Items)
}

export default function BookingsPage() {
  const { request } = useAuth()
  const [bookings, setBookings] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState('all')
  const [cancellingId, setCancellingId] = useState('')

  async function loadBookings() {
    setIsLoading(true)
    setError('')
    try {
      const response = await request('bookings/user/me?page=1&pageSize=50')
      setBookings(responseItems(response))
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadBookings()
  }, [request])

  async function cancelBooking(bookingId) {
    if (!window.confirm('از لغو این رزرو مطمئن هستید؟')) {
      return
    }

    setCancellingId(bookingId)
    setError('')
    try {
      const updated = await request(`bookings/${bookingId}/cancel`, {
        method: 'POST',
        body: { reason: 'لغو از پنل کاربری' }
      })
      setBookings((current) =>
        current.map((booking) => (booking.id === bookingId ? updated : booking))
      )
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setCancellingId('')
    }
  }

  const filteredBookings = bookings.filter((booking) => {
    if (filter === 'all') {
      return true
    }
    if (filter === 'active') {
      return !['Cancelled', 'Failed'].includes(booking.status)
    }
    return booking.type === filter
  })

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="سفرهای شما"
        title="رزروهای من"
        description="همه‌ی رزروهای هتل و پرواز شما در یک فهرست قابل پیگیری."
        actions={
          <Link to="/app/bookings/new" className="button button-accent">
            <span>＋</span>
            رزرو جدید
          </Link>
        }
      />

      <ErrorNotice message={error} onRetry={loadBookings} />

      <div className="filter-tabs">
        {[
          ['all', 'همه رزروها'],
          ['active', 'در جریان'],
          ['Flight', 'پروازها'],
          ['Hotel', 'هتل‌ها']
        ].map(([value, label]) => (
          <button
            type="button"
            key={value}
            className={filter === value ? 'active' : ''}
            onClick={() => setFilter(value)}
          >
            {label}
            {value === 'all' ? <span>{bookings.length}</span> : null}
          </button>
        ))}
      </div>

      {isLoading ? (
        <LoadingState />
      ) : filteredBookings.length ? (
        <div className="panel table-panel">
          <div className="table-wrap">
            <table className="data-table">
              <thead>
                <tr>
                  <th>رزرو</th>
                  <th>نوع</th>
                  <th>تاریخ</th>
                  <th>مبلغ</th>
                  <th>وضعیت</th>
                  <th aria-label="عملیات" />
                </tr>
              </thead>
              <tbody>
                {filteredBookings.map((booking) => {
                  const isCancelled = ['Cancelled', 'Failed'].includes(booking.status)
                  return (
                    <tr key={booking.id}>
                      <td>
                        <Link to={`/app/bookings/${booking.id}`} className="table-primary">
                          {compactId(booking.id)}
                        </Link>
                        <span className="table-secondary">
                          {formatDateTime(booking.createdAtUtc)}
                        </span>
                      </td>
                      <td>
                        <span className="type-pill">
                          <span>{booking.type === 'Flight' ? '✈' : '⌂'}</span>
                          {booking.type === 'Flight' ? 'پرواز' : 'هتل'}
                        </span>
                      </td>
                      <td>
                        <span className="table-primary">
                          {formatDate(booking.flightDate || booking.checkIn)}
                        </span>
                        <span className="table-secondary">
                          {booking.checkOut ? `تا ${formatDate(booking.checkOut)}` : 'یک سفر'}
                        </span>
                      </td>
                      <td className="amount-cell">{formatMoney(booking.totalAmount, booking.currency)}</td>
                      <td><StatusBadge value={booking.status} /></td>
                      <td>
                        <div className="table-actions">
                          <Link
                            to={`/app/bookings/${booking.id}`}
                            className="icon-button icon-button-small"
                            aria-label="جزئیات"
                          >
                            ↗
                          </Link>
                          {!isCancelled ? (
                            <button
                              type="button"
                              className="icon-button icon-button-small icon-button-danger"
                              aria-label="لغو رزرو"
                              disabled={cancellingId === booking.id}
                              onClick={() => cancelBooking(booking.id)}
                            >
                              {cancellingId === booking.id ? '…' : '×'}
                            </button>
                          ) : null}
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>
      ) : (
        <div className="panel">
          <EmptyState
            icon="✈"
            title={bookings.length ? 'رزروی با این فیلتر پیدا نشد' : 'هنوز سفری ثبت نکرده‌اید'}
            description="رزرو بعدی‌تان را با چند قدم ساده ثبت کنید."
            action={
              <Link to="/app/bookings/new" className="button button-dark button-small">
                ایجاد رزرو
              </Link>
            }
          />
        </div>
      )}
    </div>
  )
}
