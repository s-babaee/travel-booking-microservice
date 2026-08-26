import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import ErrorNotice from '../components/ErrorNotice'
import LoadingState from '../components/LoadingState'
import PageHeader from '../components/PageHeader'
import StatusBadge from '../components/StatusBadge'
import { useAuth } from '../context/AuthContext'
import { compactId, formatDateTime, formatMoney, safeArray } from '../lib/formatters'

const statusOptions = [
  ['', 'همه وضعیت‌ها'],
  ['PendingInventory', 'در انتظار موجودی'],
  ['PendingPayment', 'در انتظار پرداخت'],
  ['Confirmed', 'تأیید شده'],
  ['Cancelled', 'لغو شده'],
  ['Failed', 'ناموفق']
]

export default function AdminDashboardPage() {
  const { request } = useAuth()
  const [stats, setStats] = useState(null)
  const [bookings, setBookings] = useState([])
  const [statusFilter, setStatusFilter] = useState('')
  const [typeFilter, setTypeFilter] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [updatingId, setUpdatingId] = useState('')

  async function loadAdminData() {
    setIsLoading(true)
    setError('')
    try {
      const params = new URLSearchParams({ page: '1', pageSize: '50' })
      if (statusFilter) params.set('status', statusFilter)
      if (typeFilter) params.set('type', typeFilter)

      const [statsResponse, bookingResponse] = await Promise.all([
        request('admin/bookings/stats'),
        request(`admin/bookings/search?${params.toString()}`)
      ])

      setStats(statsResponse)
      setBookings(safeArray(bookingResponse?.items || bookingResponse?.Items))
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadAdminData()
  }, [request, statusFilter, typeFilter])

  async function updateStatus(bookingId, status) {
    if (!status) {
      return
    }
    setUpdatingId(bookingId)
    setError('')
    try {
      const updated = await request(`admin/bookings/${bookingId}/status`, {
        method: 'PATCH',
        body: { status, reason: 'به‌روزرسانی از پنل مدیر' }
      })
      setBookings((current) => current.map((booking) => (booking.id === bookingId ? updated : booking)))
      setStats((current) => {
        if (!current) return current
        return current
      })
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setUpdatingId('')
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="مدیریت سامانه"
        title="داشبورد مدیر"
        description="نمایش آماری و کنترل وضعیت رزروهای ثبت‌شده در سیستم."
        actions={
          <Link to="/app/catalog" className="button button-outline">
            مشاهده کاتالوگ
          </Link>
        }
      />

      <ErrorNotice message={error} onRetry={loadAdminData} />

      {isLoading ? (
        <LoadingState label="در حال دریافت گزارش مدیریتی..." />
      ) : (
        <>
          <section className="admin-stat-grid">
            <article className="stat-card stat-card-dark">
              <span className="stat-icon">▣</span>
              <strong>{stats?.total ?? 0}</strong>
              <span>کل رزروها</span>
            </article>
            <article className="stat-card">
              <span className="stat-icon stat-icon-accent">✓</span>
              <strong>{stats?.confirmed ?? 0}</strong>
              <span>تأییدشده</span>
            </article>
            <article className="stat-card">
              <span className="stat-icon stat-icon-lilac">◌</span>
              <strong>{(stats?.pending || 0)}</strong>
              <span>در انتظار اقدام</span>
            </article>
            <article className="stat-card">
              <span className="stat-icon stat-icon-blue">↗</span>
              <strong>{formatMoney(stats?.confirmedAmount || 0, 'USD')}</strong>
              <span>مبلغ تأییدشده</span>
            </article>
          </section>

          <section className="panel table-panel">
            <div className="panel-header admin-table-header">
              <div>
                <span className="eyebrow">عملیات</span>
                <h2>مدیریت رزروها</h2>
              </div>
              <div className="inline-filters">
                <select value={typeFilter} onChange={(event) => setTypeFilter(event.target.value)}>
                  <option value="">همه انواع</option>
                  <option value="Hotel">هتل</option>
                  <option value="Flight">پرواز</option>
                </select>
                <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)}>
                  {statusOptions.map(([value, label]) => (
                    <option value={value} key={value}>{label}</option>
                  ))}
                </select>
              </div>
            </div>
            <div className="table-wrap">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>رزرو</th>
                    <th>کاربر</th>
                    <th>نوع</th>
                    <th>مبلغ</th>
                    <th>وضعیت</th>
                    <th>تغییر وضعیت</th>
                  </tr>
                </thead>
                <tbody>
                  {bookings.length ? bookings.map((booking) => (
                    <tr key={booking.id}>
                      <td>
                        <Link to={`/app/bookings/${booking.id}`} className="table-primary">
                          {compactId(booking.id)}
                        </Link>
                        <span className="table-secondary">{formatDateTime(booking.createdAtUtc)}</span>
                      </td>
                      <td>
                        <span className="table-primary" dir="ltr">{compactId(booking.userId)}</span>
                        <span className="table-secondary">{booking.passengerName || 'بدون نام'}</span>
                      </td>
                      <td>{booking.type === 'Flight' ? 'پرواز' : 'هتل'}</td>
                      <td className="amount-cell">{formatMoney(booking.totalAmount, booking.currency)}</td>
                      <td><StatusBadge value={booking.status} /></td>
                      <td>
                        <select
                          className="table-select"
                          value=""
                          disabled={updatingId === booking.id}
                          onChange={(event) => updateStatus(booking.id, event.target.value)}
                        >
                          <option value="">انتخاب...</option>
                          <option value="PendingPayment">در انتظار پرداخت</option>
                          <option value="Confirmed">تأیید شده</option>
                          <option value="Cancelled">لغو شده</option>
                          <option value="Failed">ناموفق</option>
                        </select>
                      </td>
                    </tr>
                  )) : (
                    <tr>
                      <td colSpan="6" className="table-empty">رزروی با این فیلتر پیدا نشد.</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </section>
        </>
      )}
    </div>
  )
}
