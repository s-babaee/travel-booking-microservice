import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import AuthLayout from '../components/AuthLayout'
import ErrorNotice from '../components/ErrorNotice'
import { useAuth } from '../context/AuthContext'

const initialForm = {
  username: '',
  email: '',
  password: '',
  firstName: '',
  lastName: ''
}

export default function RegisterPage() {
  const [form, setForm] = useState(initialForm)
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isDone, setIsDone] = useState(false)
  const { register } = useAuth()
  const navigate = useNavigate()

  function updateField(event) {
    setForm((current) => ({ ...current, [event.target.name]: event.target.value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      await register(form)
      setIsDone(true)
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isDone) {
    return (
      <AuthLayout
        title="ثبت‌نام با موفقیت انجام شد"
        subtitle="حساب شما آماده‌ی استفاده است"
        footer={
          <p className="auth-footer">
            قبلاً حساب ساخته‌اید؟ <Link to="/login">وارد شوید</Link>
          </p>
        }
      >
        <div className="success-panel">
          <span className="success-mark">✓</span>
          <h3>به Travel Booking خوش آمدید</h3>
          <p>اکنون می‌توانید با اطلاعات حساب جدید خود وارد شوید.</p>
          <button type="button" className="button button-dark button-wide" onClick={() => navigate('/login')}>
            رفتن به صفحه ورود
            <span>←</span>
          </button>
        </div>
      </AuthLayout>
    )
  }

  return (
    <AuthLayout
      title="شروع سفر"
      subtitle="حساب کاربری خود را بسازید"
      footer={
        <p className="auth-footer">
          حساب دارید؟ <Link to="/login">وارد شوید</Link>
        </p>
      }
    >
      <form className="auth-form" onSubmit={handleSubmit}>
        <ErrorNotice message={error} />
        <div className="form-grid form-grid-two">
          <label className="field">
            <span>نام</span>
            <input
              name="firstName"
              value={form.firstName}
              onChange={updateField}
              placeholder="نام"
              autoComplete="given-name"
            />
          </label>
          <label className="field">
            <span>نام خانوادگی</span>
            <input
              name="lastName"
              value={form.lastName}
              onChange={updateField}
              placeholder="نام خانوادگی"
              autoComplete="family-name"
            />
          </label>
        </div>
        <label className="field">
          <span>نام کاربری</span>
          <input
            name="username"
            value={form.username}
            onChange={updateField}
            placeholder="حداقل ۳ کاراکتر"
            autoComplete="username"
            minLength={3}
            required
          />
        </label>
        <label className="field">
          <span>ایمیل</span>
          <input
            type="email"
            name="email"
            value={form.email}
            onChange={updateField}
            placeholder="you@example.com"
            dir="ltr"
            autoComplete="email"
            required
          />
        </label>
        <label className="field">
          <span>رمز عبور</span>
          <input
            type="password"
            name="password"
            value={form.password}
            onChange={updateField}
            placeholder="حداقل ۸ کاراکتر"
            autoComplete="new-password"
            minLength={8}
            required
          />
        </label>
        <button type="submit" className="button button-dark button-wide" disabled={isSubmitting}>
          {isSubmitting ? 'در حال ساخت حساب...' : 'ساخت حساب کاربری'}
          {!isSubmitting ? <span>←</span> : null}
        </button>
      </form>
    </AuthLayout>
  )
}
