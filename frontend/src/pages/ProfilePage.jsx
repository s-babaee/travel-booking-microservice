import { useEffect, useState } from 'react'
import ErrorNotice from '../components/ErrorNotice'
import PageHeader from '../components/PageHeader'
import StatusBadge from '../components/StatusBadge'
import { useAuth } from '../context/AuthContext'
import { enumLabel, initials } from '../lib/formatters'

export default function ProfilePage() {
  const { user, roles, request, refreshProfile } = useAuth()
  const [profileForm, setProfileForm] = useState({
    email: user?.email || '',
    firstName: user?.firstName || '',
    lastName: user?.lastName || ''
  })
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '',
    newPassword: ''
  })
  const [profileMessage, setProfileMessage] = useState('')
  const [passwordMessage, setPasswordMessage] = useState('')
  const [error, setError] = useState('')
  const [isSavingProfile, setIsSavingProfile] = useState(false)
  const [isSavingPassword, setIsSavingPassword] = useState(false)

  useEffect(() => {
    setProfileForm({
      email: user?.email || '',
      firstName: user?.firstName || '',
      lastName: user?.lastName || ''
    })
  }, [user])

  async function saveProfile(event) {
    event.preventDefault()
    setError('')
    setProfileMessage('')
    setIsSavingProfile(true)
    try {
      await request('profile/me', {
        method: 'PUT',
        body: profileForm
      })
      await refreshProfile()
      setProfileMessage('اطلاعات پروفایل با موفقیت ذخیره شد.')
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsSavingProfile(false)
    }
  }

  async function changePassword(event) {
    event.preventDefault()
    setError('')
    setPasswordMessage('')
    setIsSavingPassword(true)
    try {
      await request('profile/me/password', {
        method: 'PATCH',
        body: passwordForm
      })
      setPasswordForm({ currentPassword: '', newPassword: '' })
      setPasswordMessage('رمز عبور با موفقیت تغییر کرد.')
    } catch (requestError) {
      setError(requestError.message)
    } finally {
      setIsSavingPassword(false)
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="حساب کاربری"
        title="پروفایل من"
        description="اطلاعات حساب و امنیت ورود خود را مدیریت کنید."
      />

      <ErrorNotice message={error} />

      <div className="profile-layout">
        <aside className="panel profile-card">
          <div className="profile-avatar">{initials(user)}</div>
          <h2>{[user?.firstName, user?.lastName].filter(Boolean).join(' ') || user?.username}</h2>
          <p>{user?.email}</p>
          <StatusBadge value={user?.status} />
          <div className="profile-meta">
            <div>
              <span>نام کاربری</span>
              <strong>{user?.username}</strong>
            </div>
            <div>
              <span>شناسه کاربر</span>
              <strong dir="ltr">{user?.userId}</strong>
            </div>
            <div>
              <span>نقش‌ها</span>
              <strong>{roles.length ? roles.join('، ') : 'کاربر'}</strong>
            </div>
          </div>
        </aside>

        <div className="profile-forms">
          <section className="panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">اطلاعات پایه</span>
                <h2>ویرایش پروفایل</h2>
              </div>
              <span className="panel-header-icon">◎</span>
            </div>
            <form className="app-form" onSubmit={saveProfile}>
              <div className="form-grid form-grid-two">
                <label className="field">
                  <span>نام</span>
                  <input
                    value={profileForm.firstName}
                    onChange={(event) =>
                      setProfileForm((current) => ({ ...current, firstName: event.target.value }))
                    }
                    placeholder="نام"
                  />
                </label>
                <label className="field">
                  <span>نام خانوادگی</span>
                  <input
                    value={profileForm.lastName}
                    onChange={(event) =>
                      setProfileForm((current) => ({ ...current, lastName: event.target.value }))
                    }
                    placeholder="نام خانوادگی"
                  />
                </label>
              </div>
              <label className="field">
                <span>ایمیل</span>
                <input
                  type="email"
                  value={profileForm.email}
                  onChange={(event) =>
                    setProfileForm((current) => ({ ...current, email: event.target.value }))
                  }
                  dir="ltr"
                  required
                />
              </label>
              {profileMessage ? <div className="inline-success">{profileMessage}</div> : null}
              <button type="submit" className="button button-dark" disabled={isSavingProfile}>
                {isSavingProfile ? 'در حال ذخیره...' : 'ذخیره تغییرات'}
              </button>
            </form>
          </section>

          <section className="panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">امنیت حساب</span>
                <h2>تغییر رمز عبور</h2>
              </div>
              <span className="panel-header-icon">⌁</span>
            </div>
            <form className="app-form" onSubmit={changePassword}>
              <label className="field">
                <span>رمز عبور فعلی</span>
                <input
                  type="password"
                  value={passwordForm.currentPassword}
                  onChange={(event) =>
                    setPasswordForm((current) => ({
                      ...current,
                      currentPassword: event.target.value
                    }))
                  }
                  autoComplete="current-password"
                  required
                />
              </label>
              <label className="field">
                <span>رمز عبور جدید</span>
                <input
                  type="password"
                  value={passwordForm.newPassword}
                  onChange={(event) =>
                    setPasswordForm((current) => ({
                      ...current,
                      newPassword: event.target.value
                    }))
                  }
                  minLength={8}
                  autoComplete="new-password"
                  required
                />
              </label>
              {passwordMessage ? <div className="inline-success">{passwordMessage}</div> : null}
              <button type="submit" className="button button-outline" disabled={isSavingPassword}>
                {isSavingPassword ? 'در حال تغییر...' : 'تغییر رمز عبور'}
              </button>
            </form>
          </section>
        </div>
      </div>
    </div>
  )
}
