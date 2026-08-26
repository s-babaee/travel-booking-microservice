import { Link } from 'react-router-dom'

export default function AuthLayout({ children, title, subtitle, footer }) {
  return (
    <div className="auth-page">
      <section className="auth-showcase">
        <Link to="/" className="brand brand-light">
          <span className="brand-mark">✦</span>
          <span>
            <strong>Travel</strong>
            <small>BOOKING PLATFORM</small>
          </span>
        </Link>

        <div className="showcase-copy">
          <span className="eyebrow eyebrow-light">سفر، ساده‌تر از همیشه</span>
          <h1>
            هر مقصدی،
            <br />
            <em>یک تجربه‌ی ماندگار.</em>
          </h1>
          <p>
            از مدیریت کاتالوگ تا رزرو و پیگیری سفارش، همه‌چیز در یک فضای
            یکپارچه و قابل اعتماد.
          </p>
        </div>

        <div className="showcase-stats">
          <div>
            <strong>۴</strong>
            <span>ماژول اصلی</span>
          </div>
          <div>
            <strong>۲۴/۷</strong>
            <span>همراه سفر شما</span>
          </div>
        </div>
      </section>

      <section className="auth-panel">
        <div className="auth-card">
          <div className="auth-heading">
            <span className="eyebrow">{title}</span>
            <h2>{subtitle}</h2>
          </div>
          {children}
          {footer}
        </div>
      </section>
    </div>
  )
}
