import { useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import ErrorNotice from '../components/ErrorNotice'
import PageHeader from '../components/PageHeader'
import StatusBadge from '../components/StatusBadge'
import { useAuth } from '../context/AuthContext'
import {
  formatDate,
  formatMoney,
  makeIdempotencyKey,
  todayInputValue
} from '../lib/formatters'

function addDays(value, days) {
  const date = new Date(`${value}T00:00:00`)
  date.setDate(date.getDate() + days)
  return date.toISOString().slice(0, 10)
}

const initialHotelForm = {
  hotelId: '',
  roomTypeId: '',
  checkIn: todayInputValue(),
  checkOut: addDays(todayInputValue(), 2),
  quantity: 1,
  unitAmount: '',
  totalAmount: '',
  currency: 'USD',
  paymentMethodToken: 'demo_card_token',
  passengerName: ''
}

const initialFlightForm = {
  flightId: '',
  flightClassId: '',
  date: todayInputValue(),
  quantity: 1,
  unitAmount: '',
  totalAmount: '',
  currency: 'USD',
  paymentMethodToken: 'demo_card_token',
  passengerName: ''
}

export default function NewBookingPage() {
  const { request } = useAuth()
  const [searchParams] = useSearchParams()
  const [type, setType] = useState(searchParams.get('type') === 'flight' ? 'flight' : 'hotel')
  const [hotelForm, setHotelForm] = useState(() => ({
    ...initialHotelForm,
    hotelId: searchParams.get('hotelId') || '',
    roomTypeId: searchParams.get('roomTypeId') || ''
  }))
  const [flightForm, setFlightForm] = useState(() => ({
    ...initialFlightForm,
    flightId: searchParams.get('flightId') || '',
    flightClassId: searchParams.get('flightClassId') || ''
  }))
  const [error, setError] = useState('')
  const [success, setSuccess] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const currentForm = type === 'hotel' ? hotelForm : flightForm
  const setCurrentForm = type === 'hotel' ? setHotelForm : setFlightForm

  const calculatedAmount = useMemo(() => {
    const quantity = Number(currentForm.quantity || 0)
    const unitAmount = Number(currentForm.unitAmount || 0)
    return quantity * unitAmount
  }, [currentForm.quantity, currentForm.unitAmount])

  function updateField(event) {
    const { name, value } = event.target
    setCurrentForm((current) => ({ ...current, [name]: value }))
    setError('')
    setSuccess(null)
  }

  function selectType(nextType) {
    setType(nextType)
    setError('')
    setSuccess(null)
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setSuccess(null)
    setIsSubmitting(true)

    const totalAmount = Number(currentForm.totalAmount || calculatedAmount || 0)
    const base = {
      totalAmount,
      currency: currentForm.currency.trim().toUpperCase(),
      paymentMethodToken: currentForm.paymentMethodToken.trim(),
      passengerName: currentForm.passengerName.trim() || null,
      idempotencyKey: makeIdempotencyKey()
    }

    const body =
      type === 'hotel'
        ? {
            ...base,
            hotelId: currentForm.hotelId.trim(),
            checkIn: currentForm.checkIn,
            checkOut: currentForm.checkOut,
            rooms: [
              {
                roomTypeId: currentForm.roomTypeId.trim(),
                quantity: Number(currentForm.quantity),
                unitAmount: Number(currentForm.unitAmount)
              }
            ]
          }
        : {
            ...base,
            flightId: currentForm.flightId.trim(),
            date: currentForm.date,
            classes: [
              {
                flightClassId: currentForm.flightClassId.trim(),
                quantity: Number(currentForm.quantity),
                unitAmount: Number(currentForm.unitAmount)
              }
            ]
          }

    try {
      const booking = await request(`bookings/${type === 'hotel' ? 'hotels' : 'flights'}`, {
        method: 'POST',
        body
      })
      setSuccess(booking)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="رزرو جدید"
        title="سفر بعدی‌تان را بسازید"
        description="اطلاعات منبع سفر و پرداخت را وارد کنید تا رزرو در سیستم ثبت شود."
      />

      <div className="booking-layout">
        <section className="panel booking-form-panel">
          <div className="booking-type-switch">
            <button
              type="button"
              className={type === 'hotel' ? 'active' : ''}
              onClick={() => selectType('hotel')}
            >
              <span>⌂</span>
              رزرو هتل
            </button>
            <button
              type="button"
              className={type === 'flight' ? 'active' : ''}
              onClick={() => selectType('flight')}
            >
              <span>✈</span>
              رزرو پرواز
            </button>
          </div>

          <div className="form-section-heading">
            <span className="step-number">۰۱</span>
            <div>
              <h2>{type === 'hotel' ? 'مشخصات اقامت' : 'مشخصات پرواز'}</h2>
              <p>
                شناسه‌ها را از صفحه <Link to="/app/catalog">کاتالوگ سفر</Link> بردارید.
              </p>
            </div>
          </div>

          <form onSubmit={handleSubmit} className="app-form">
            <ErrorNotice message={error} />

            {type === 'hotel' ? (
              <>
                <div className="form-grid form-grid-two">
                  <label className="field">
                    <span>شناسه هتل</span>
                    <input
                      name="hotelId"
                      value={hotelForm.hotelId}
                      onChange={updateField}
                      placeholder="UUID هتل"
                      dir="ltr"
                      required
                    />
                  </label>
                  <label className="field">
                    <span>شناسه نوع اتاق</span>
                    <input
                      name="roomTypeId"
                      value={hotelForm.roomTypeId}
                      onChange={updateField}
                      placeholder="UUID نوع اتاق"
                      dir="ltr"
                      required
                    />
                  </label>
                </div>
                <div className="form-grid form-grid-two">
                  <label className="field">
                    <span>تاریخ ورود</span>
                    <input
                      type="date"
                      name="checkIn"
                      value={hotelForm.checkIn}
                      onChange={updateField}
                      required
                    />
                  </label>
                  <label className="field">
                    <span>تاریخ خروج</span>
                    <input
                      type="date"
                      name="checkOut"
                      value={hotelForm.checkOut}
                      onChange={updateField}
                      min={hotelForm.checkIn}
                      required
                    />
                  </label>
                </div>
              </>
            ) : (
              <>
                <div className="form-grid form-grid-two">
                  <label className="field">
                    <span>شناسه پرواز</span>
                    <input
                      name="flightId"
                      value={flightForm.flightId}
                      onChange={updateField}
                      placeholder="UUID پرواز"
                      dir="ltr"
                      required
                    />
                  </label>
                  <label className="field">
                    <span>شناسه کلاس پروازی</span>
                    <input
                      name="flightClassId"
                      value={flightForm.flightClassId}
                      onChange={updateField}
                      placeholder="UUID کلاس پروازی"
                      dir="ltr"
                      required
                    />
                  </label>
                </div>
                <label className="field">
                  <span>تاریخ پرواز</span>
                  <input
                    type="date"
                    name="date"
                    value={flightForm.date}
                    onChange={updateField}
                    required
                  />
                </label>
              </>
            )}

            <div className="form-section-heading form-section-heading-spaced">
              <span className="step-number">۰۲</span>
              <div>
                <h2>تعداد و مبلغ</h2>
                <p>مبلغ واحد و مبلغ نهایی را با واحد پولی مناسب وارد کنید.</p>
              </div>
            </div>

            <div className="form-grid form-grid-three">
              <label className="field">
                <span>{type === 'hotel' ? 'تعداد اتاق' : 'تعداد صندلی'}</span>
                <input
                  type="number"
                  min="1"
                  name="quantity"
                  value={currentForm.quantity}
                  onChange={updateField}
                  required
                />
              </label>
              <label className="field">
                <span>مبلغ واحد</span>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  name="unitAmount"
                  value={currentForm.unitAmount}
                  onChange={updateField}
                  placeholder="۰.۰۰"
                  dir="ltr"
                  required
                />
              </label>
              <label className="field">
                <span>واحد پول</span>
                <select name="currency" value={currentForm.currency} onChange={updateField}>
                  <option value="USD">USD</option>
                  <option value="EUR">EUR</option>
                  <option value="IRR">IRR</option>
                </select>
              </label>
            </div>

            <div className="calculation-strip">
              <span>مبلغ محاسبه‌شده</span>
              <strong>{formatMoney(calculatedAmount, currentForm.currency)}</strong>
              <span className="calculation-hint">در صورت نیاز مبلغ نهایی را ویرایش کنید.</span>
            </div>

            <label className="field">
              <span>مبلغ نهایی پرداخت</span>
              <input
                type="number"
                min="0"
                step="0.01"
                name="totalAmount"
                value={currentForm.totalAmount}
                onChange={updateField}
                placeholder={String(calculatedAmount || 0)}
                dir="ltr"
                required
              />
            </label>

            <div className="form-section-heading form-section-heading-spaced">
              <span className="step-number">۰۳</span>
              <div>
                <h2>اطلاعات مسافر و پرداخت</h2>
                <p>در محیط توسعه، توکن نمونه برای درگاه Mock قابل استفاده است.</p>
              </div>
            </div>

            <div className="form-grid form-grid-two">
              <label className="field">
                <span>نام مسافر</span>
                <input
                  name="passengerName"
                  value={currentForm.passengerName}
                  onChange={updateField}
                  placeholder="نام و نام خانوادگی"
                />
              </label>
              <label className="field">
                <span>توکن روش پرداخت</span>
                <input
                  name="paymentMethodToken"
                  value={currentForm.paymentMethodToken}
                  onChange={updateField}
                  placeholder="payment token"
                  dir="ltr"
                  required
                />
              </label>
            </div>

            {success ? (
              <div className="success-notice">
                <span className="success-mark success-mark-small">✓</span>
                <div>
                  <strong>رزرو شما ثبت شد</strong>
                  <p>
                    وضعیت فعلی: <StatusBadge value={success.status} />
                  </p>
                </div>
                <Link to={`/app/bookings/${success.id}`} className="button button-dark button-small">
                  مشاهده جزئیات
                </Link>
              </div>
            ) : null}

            <button type="submit" className="button button-dark button-wide" disabled={isSubmitting}>
              {isSubmitting ? 'در حال ثبت رزرو...' : 'ثبت و ادامه'}
              {!isSubmitting ? <span>←</span> : null}
            </button>
          </form>
        </section>

        <aside className="booking-aside">
          <div className="aside-card aside-card-dark">
            <span className="eyebrow eyebrow-light">نکته مهم</span>
            <h3>شناسه‌ها را از کجا پیدا کنم؟</h3>
            <p>
              در صفحه کاتالوگ می‌توانید با شناسه‌ی هتل یا پرواز، جزئیات کامل
              شامل اتاق‌ها و کلاس‌های قابل رزرو را مشاهده کنید.
            </p>
            <Link to="/app/catalog" className="button button-light button-small">
              رفتن به کاتالوگ <span>←</span>
            </Link>
          </div>
          <div className="aside-card">
            <span className="aside-card-icon">✓</span>
            <h3>رزرو امن و قابل پیگیری</h3>
            <p>با کلید یکتا، درخواست‌های تکراری دوباره ثبت نمی‌شوند و وضعیت رزرو در حساب شما قابل مشاهده است.</p>
          </div>
          <div className="mini-route-card">
            <div className="mini-route-map">
              <span className="map-pin map-pin-one">●</span>
              <span className="map-path" />
              <span className="map-pin map-pin-two">●</span>
            </div>
            <div>
              <span className="muted-label">سفر بعدی شما</span>
              <strong>{type === 'hotel' ? 'اقامتی آرام' : 'پروازی به‌یادماندنی'}</strong>
              <small>
                {type === 'hotel'
                  ? `${formatDate(hotelForm.checkIn)} تا ${formatDate(hotelForm.checkOut)}`
                  : formatDate(flightForm.date)}
              </small>
            </div>
          </div>
        </aside>
      </div>
    </div>
  )
}
