import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import AuthLayout from '../components/AuthLayout'
import ErrorNotice from '../components/ErrorNotice'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
  const [form, setForm] = useState({ username: '', password: '' })
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  function updateField(event) {
    setForm((current) => ({ ...current, [event.target.name]: event.target.value }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      await login(form)
      navigate(location.state?.from || '/app', { replace: true })
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout
      title="خوش آمدید"
      subtitle="وارد حساب خود شوید"
      footer={
        <p className="auth-footer">
          حساب ندارید؟ <Link to="/register">ثبت‌نام کنید</Link>
        </p>
      }
    >
      <form className="auth-form" onSubmit={handleSubmit}>
        <ErrorNotice message={error} />
        <label className="field">
          <span>نام کاربری</span>
          <input
            name="username"
            value={form.username}
            onChange={updateField}
            placeholder="نام کاربری خود را وارد کنید"
            autoComplete="username"
            required
          />
        </label>
        <label className="field">
          <span className="field-label-row">
            <span>رمز عبور</span>
            <Link to="/forgot-password" className="field-link">
              رمز عبور را فراموش کرده‌اید؟
            </Link>
          </span>
          <input
            type="password"
            name="password"
            value={form.password}
            onChange={updateField}
            placeholder="رمز عبور خود را وارد کنید"
            autoComplete="current-password"
            required
          />
        </label>
        <button type="submit" className="button button-dark button-wide" disabled={isSubmitting}>
          {isSubmitting ? 'در حال ورود...' : 'ورود به حساب'}
          {!isSubmitting ? <span>←</span> : null}
        </button>
      </form>
    </AuthLayout>
  )
}
