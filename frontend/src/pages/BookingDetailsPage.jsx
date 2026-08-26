import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
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

export default function BookingDetailsPage() {
  const { bookingId } = useParams()
  const { request } = useAuth()
  const navigate = useNavigate()
  const [booking, setBooking] = useState(null)
  const [payments, setPayments] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [isCancelling, setIsCancelling] = useState(false)
  const [error, setError] = useState('')

  async function loadDetails() {
    setError('')
    setIsLoading(true)
    try {
      const [bookingResponse, paymentResponse] = await Promise.all([
        request(`bookings/${bookingId}`),
        request(`payments/booking/${bookingId}`).catch(() => [])
      ])
      setBooking(bookingResponse)
      setPayments(safeArray(paymentResponse))
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadDetails()
  }, [bookingId, request])

  async function cancelBooking() {
    if (!booking || !window.confirm('از لغو این رزرو مطمئن هستید؟')) {
      return
    }

    setError('')
    setIsCancelling(true)
    try {
      const updated = await request(`bookings/${booking.id}/cancel`, {
        method: 'POST',
        body: { reason: 'لغو از پنل کاربری' }
      })
      setBooking(updated)
      const paymentResponse = await request(`payments/booking/${booking.id}`).catch(() => [])
      setPayments(safeArray(paymentResponse))
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsCancelling(false)
    }
  }

  if (isLoading) {
    return <LoadingState label="در حال دریافت جزئیات رزرو..." />
  }

  if (error && !booking) {
    return (
      <div className="page-stack">
        <ErrorNotice message={error} onRetry={loadDetails} />
        <button type="button" className="button button-outline" onClick={() => navigate('/app/bookings')}>
          بازگشت به رزروها
        </button>
      </div>
    )
  }

  const isTerminal = ['Cancelled', 'Failed'].includes(booking?.status)
  const isFlight = booking?.type === 'Flight'
  const items = safeArray(booking?.items)

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="جزئیات رزرو"
        title={isFlight ? 'رزرو پرواز' : 'رزرو هتل'}
        description={`${compactId(booking?.id)} · ثبت‌شده در ${formatDateTime(booking?.createdAtUtc)}`}
        actions={
          <Link to="/app/bookings" className="button button-outline">
            ← بازگشت به رزروها
          </Link>
        }
      />

      <ErrorNotice message={error} />

      <div className="detail-layout">
        <section className="panel detail-main">
          <div className="detail-hero">
            <div className={`detail-type-icon ${isFlight ? 'flight' : 'hotel'}`}>
              {isFlight ? '✈' : '⌂'}
            </div>
            <div>
              <span className="eyebrow">{isFlight ? 'Flight booking' : 'Hotel booking'}</span>
              <h2>{isFlight ? 'سفر هوایی شما' : 'اقامت شما'}</h2>
              <p>{isFlight ? `تاریخ پرواز: ${formatDate(booking?.flightDate)}` : `ورود: ${formatDate(booking?.checkIn)}`}</p>
            </div>
            <StatusBadge value={booking?.status} />
          </div>

          <div className="detail-divider" />

          <div className="detail-info-grid">
            <div>
              <span>شناسه رزرو</span>
              <strong dir="ltr">{booking?.id}</strong>
            </div>
            <div>
              <span>{isFlight ? 'شناسه پرواز' : 'شناسه هتل'}</span>
              <strong dir="ltr">{isFlight ? booking?.flightId : booking?.hotelId}</strong>
            </div>
            <div>
              <span>نام مسافر</span>
              <strong>{booking?.passengerName || 'ثبت نشده'}</strong>
            </div>
            <div>
              <span>آخرین تغییر</span>
              <strong>{formatDateTime(booking?.updatedAtUtc)}</strong>
            </div>
          </div>

          <div className="detail-section">
            <div className="section-heading-inline">
              <h3>{isFlight ? 'کلاس‌های انتخاب‌شده' : 'اتاق‌های انتخاب‌شده'}</h3>
              <span>{items.length} مورد</span>
            </div>
            <div className="selected-items">
              {items.length ? (
                items.map((item, index) => (
                  <div className="selected-item" key={`${item.resourceTypeId}-${index}`}>
                    <span className="selected-item-index">۰{index + 1}</span>
                    <div>
                      <strong>{isFlight ? 'کلاس پروازی' : 'نوع اتاق'}</strong>
                      <small dir="ltr">{item.resourceTypeId}</small>
                    </div>
                    <span>{item.quantity} × {formatMoney(item.unitAmount, booking.currency)}</span>
                  </div>
                ))
              ) : (
                <p className="muted-copy">جزئیات آیتم‌ها در پاسخ رزرو وجود ندارد.</p>
              )}
            </div>
          </div>

          {booking?.failureReason ? (
            <div className="warning-box">
              <strong>دلیل عدم موفقیت</strong>
              <p>{booking.failureReason}</p>
            </div>
          ) : null}
        </section>

        <aside className="detail-side">
          <div className="panel summary-panel">
            <span className="eyebrow">خلاصه مالی</span>
            <h3>مبلغ نهایی</h3>
            <strong className="summary-amount">{formatMoney(booking?.totalAmount, booking?.currency)}</strong>
            <div className="summary-line">
              <span>نوع رزرو</span>
              <strong>{isFlight ? 'پرواز' : 'هتل'}</strong>
            </div>
            <div className="summary-line">
              <span>شناسه سفارش</span>
              <strong dir="ltr">{compactId(booking?.orderId)}</strong>
            </div>
            <div className="summary-line">
              <span>تاریخ تأیید</span>
              <strong>{formatDateTime(booking?.confirmedAtUtc)}</strong>
            </div>
            {!isTerminal ? (
              <button
                type="button"
                className="button button-danger button-wide"
                onClick={cancelBooking}
                disabled={isCancelling}
              >
                {isCancelling ? 'در حال لغو...' : 'لغو این رزرو'}
              </button>
            ) : null}
          </div>

          <div className="panel payment-panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">تراکنش‌ها</span>
                <h3>وضعیت پرداخت</h3>
              </div>
              <span className="payment-icon">₿</span>
            </div>
            {payments.length ? (
              <div className="payment-list">
                {payments.map((payment) => (
                  <div className="payment-row" key={payment.id}>
                    <div>
                      <strong>{formatMoney(payment.amount, payment.currency)}</strong>
                      <small>{formatDateTime(payment.createdAtUtc)}</small>
                    </div>
                    <StatusBadge value={payment.status} />
                  </div>
                ))}
              </div>
            ) : (
              <p className="muted-copy">تراکنش پرداختی برای این رزرو پیدا نشد.</p>
            )}
          </div>
        </aside>
      </div>
    </div>
  )
}
