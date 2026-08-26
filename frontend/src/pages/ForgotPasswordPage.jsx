import { useState } from 'react'
import { Link } from 'react-router-dom'
import AuthLayout from '../components/AuthLayout'
import ErrorNotice from '../components/ErrorNotice'
import { apiRequest } from '../lib/api'

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setMessage('')
    setIsSubmitting(true)
    try {
      const response = await apiRequest('password/forgot', {
        method: 'POST',
        body: { email }
      })
      setMessage(response?.message || 'اگر این ایمیل در سامانه وجود داشته باشد، راهنمای بازیابی ارسال می‌شود.')
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout
      title="بازیابی دسترسی"
      subtitle="رمز عبور خود را بازیابی کنید"
      footer={
        <p className="auth-footer">
          رمز را به خاطر آوردید؟ <Link to="/login">بازگشت به ورود</Link>
        </p>
      }
    >
      <form className="auth-form" onSubmit={handleSubmit}>
        <ErrorNotice message={error} />
        {message ? <div className="inline-success">{message}</div> : null}
        <label className="field">
          <span>ایمیل حساب</span>
          <input
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="you@example.com"
            dir="ltr"
            required
          />
        </label>
        <button type="submit" className="button button-dark button-wide" disabled={isSubmitting}>
          {isSubmitting ? 'در حال ارسال...' : 'ارسال راهنمای بازیابی'}
          {!isSubmitting ? <span>←</span> : null}
        </button>
      </form>
    </AuthLayout>
  )
}
