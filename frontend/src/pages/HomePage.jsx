import { Link } from 'react-router-dom'

const highlights = [
  {
    icon: '⌁',
    title: 'رزرو یکپارچه',
    text: 'هتل و پرواز را در یک جریان ساده انتخاب و رزرو کنید.'
  },
  {
    icon: '◈',
    title: 'مدیریت حرفه‌ای',
    text: 'کاتالوگ سفر، وضعیت رزرو و اعلان‌ها همیشه در دسترس شماست.'
  },
  {
    icon: '◌',
    title: 'پیگیری شفاف',
    text: 'از لحظه ثبت تا تأیید، وضعیت سفارش را لحظه‌به‌لحظه ببینید.'
  }
]

export default function HomePage() {
  return (
    <div className="landing-page">
      <header className="landing-header">
        <Link to="/" className="brand">
          <span className="brand-mark">✦</span>
          <span>
            <strong>Travel</strong>
            <small>BOOKING PLATFORM</small>
          </span>
        </Link>
        <nav className="landing-nav">
          <a href="#features">امکانات</a>
          <a href="#workflow">چطور کار می‌کند؟</a>
          <Link to="/login">ورود</Link>
          <Link to="/register" className="button button-dark button-small">
            شروع کنید
          </Link>
        </nav>
      </header>

      <main>
        <section className="hero-section">
          <div className="hero-copy">
            <span className="eyebrow">TRAVEL BOOKING PLATFORM</span>
            <h1>
              مقصد بعدی‌تان
              <br />
              <em>همین‌جا شروع می‌شود.</em>
            </h1>
            <p>
              یک تجربه‌ی کامل برای کشف، رزرو و مدیریت سفر؛ طراحی‌شده برای
              مسافرانی که جزئیات برایشان مهم است.
            </p>
            <div className="hero-actions">
              <Link to="/register" className="button button-accent">
                ساخت حساب کاربری
                <span>←</span>
              </Link>
              <Link to="/login" className="button button-outline">
                ورود به حساب
              </Link>
            </div>
            <div className="hero-note">
              <span className="note-avatars">
                <span>س</span>
                <span>م</span>
                <span>ن</span>
              </span>
              <span>همراه هزاران سفر موفق</span>
            </div>
          </div>

          <div className="hero-visual" aria-label="نمایش مسیر سفر">
            <div className="orbit orbit-large" />
            <div className="orbit orbit-small" />
            <div className="hero-sun">✦</div>
            <div className="destination-card destination-main">
              <div className="destination-image image-cappadocia">
                <span className="image-badge">پیشنهاد ویژه</span>
              </div>
              <div className="destination-content">
                <div>
                  <span className="muted-label">مقصد پیشنهادی</span>
                  <strong>کاپادوکیه، ترکیه</strong>
                </div>
                <span className="destination-arrow">↗</span>
              </div>
            </div>
            <div className="floating-card weather-card">
              <span className="weather-icon">☼</span>
              <span>
                <strong>۲۲°</strong>
                <small>هوای دلپذیر</small>
              </span>
            </div>
            <div className="floating-card route-card">
              <span className="route-dot route-dot-start" />
              <span className="route-line" />
              <span className="route-dot route-dot-end" />
              <span className="route-label">تهران ← استانبول</span>
            </div>
          </div>
        </section>

        <section id="features" className="landing-features">
          <div className="section-intro">
            <span className="eyebrow">تجربه‌ای که برای شما ساخته شده</span>
            <h2>همه‌چیز برای یک سفر خوب</h2>
          </div>
          <div className="highlight-grid">
            {highlights.map((item) => (
              <article key={item.title} className="highlight-card">
                <span className="highlight-icon">{item.icon}</span>
                <h3>{item.title}</h3>
                <p>{item.text}</p>
              </article>
            ))}
          </div>
        </section>

        <section id="workflow" className="workflow-section">
          <div>
            <span className="eyebrow eyebrow-light">سفر شما، جریان شما</span>
            <h2>از انتخاب تا رسیدن، همراهتان هستیم.</h2>
          </div>
          <div className="workflow-steps">
            <div>
              <span>۰۱</span>
              <strong>حساب بسازید</strong>
              <p>در چند ثانیه ثبت‌نام کنید و وارد فضای شخصی خود شوید.</p>
            </div>
            <div>
              <span>۰۲</span>
              <strong>سفر را انتخاب کنید</strong>
              <p>اطلاعات هتل، پرواز و کلاس سفر را با جزئیات بررسی کنید.</p>
            </div>
            <div>
              <span>۰۳</span>
              <strong>با خیال راحت رزرو کنید</strong>
              <p>رزروهای خود را ثبت کنید و وضعیت آن‌ها را پیگیری کنید.</p>
            </div>
          </div>
        </section>
      </main>

      <footer className="landing-footer">
        <span>© ۲۰۲۶ Travel Booking</span>
        <span>ساخته‌شده برای سفرهای بهتر</span>
      </footer>
    </div>
  )
}
