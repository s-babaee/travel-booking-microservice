import { useState } from 'react'
import { NavLink, Outlet, Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { initials } from '../lib/formatters'

const navItems = [
  { to: '/app', label: 'نمای کلی', icon: '⌂', end: true },
  { to: '/app/bookings', label: 'رزروهای من', icon: '▣' },
  { to: '/app/bookings/new', label: 'رزرو جدید', icon: '＋' },
  { to: '/app/catalog', label: 'کاتالوگ سفر', icon: '◈' },
  { to: '/app/notifications', label: 'اعلان‌ها', icon: '◌' },
  { to: '/app/profile', label: 'پروفایل من', icon: '◎' }
]

export default function AppShell() {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const { user, roles, isAdmin, logout } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/login')
  }

  return (
    <div className="app-shell">
      <div
        className={`sidebar-backdrop ${sidebarOpen ? 'is-visible' : ''}`}
        onClick={() => setSidebarOpen(false)}
      />

      <aside className={`sidebar ${sidebarOpen ? 'is-open' : ''}`}>
        <div className="sidebar-top">
          <Link to="/app" className="brand" onClick={() => setSidebarOpen(false)}>
            <span className="brand-mark">✦</span>
            <span>
              <strong>Travel</strong>
              <small>BOOKING PLATFORM</small>
            </span>
          </Link>
          <button
            type="button"
            className="sidebar-close"
            aria-label="بستن منو"
            onClick={() => setSidebarOpen(false)}
          >
            ×
          </button>
        </div>

        <div className="sidebar-label">فضای کاری</div>
        <nav className="sidebar-nav">
          {navItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              onClick={() => setSidebarOpen(false)}
              className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
            >
              <span className="nav-icon">{item.icon}</span>
              <span>{item.label}</span>
            </NavLink>
          ))}

          {isAdmin ? (
            <>
              <div className="sidebar-label sidebar-label-spaced">مدیریت</div>
              <NavLink
                to="/app/admin"
                onClick={() => setSidebarOpen(false)}
                className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
              >
                <span className="nav-icon">▤</span>
                <span>داشبورد مدیر</span>
              </NavLink>
            </>
          ) : null}
        </nav>

        <div className="sidebar-footer">
          <div className="support-card">
            <span className="support-orb">?</span>
            <div>
              <strong>نیاز به راهنمایی دارید؟</strong>
              <small>مستندات API و راهنمای رزرو</small>
            </div>
          </div>
          <button type="button" className="logout-link" onClick={handleLogout}>
            <span>↪</span>
            خروج از حساب
          </button>
        </div>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <button
            type="button"
            className="menu-toggle"
            aria-label="باز کردن منو"
            onClick={() => setSidebarOpen(true)}
          >
            ☰
          </button>
          <div className="topbar-context">
            <span className="topbar-kicker">Travel Booking</span>
            <span className="topbar-divider">/</span>
            <span>پنل کاربری</span>
          </div>
          <div className="topbar-actions">
            <Link to="/app/notifications" className="icon-button" aria-label="اعلان‌ها">
              ◌
            </Link>
            <Link to="/app/profile" className="user-chip">
              <span className="avatar avatar-small">{initials(user)}</span>
              <span className="user-chip-copy">
                <strong>{user?.firstName || user?.username || 'کاربر'}</strong>
                <small>{roles.includes('admin') ? 'مدیر سامانه' : 'مسافر'}</small>
              </span>
              <span className="chevron">⌄</span>
            </Link>
          </div>
        </header>

        <main className="app-content">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
