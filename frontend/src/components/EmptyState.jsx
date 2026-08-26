export default function EmptyState({
  icon = '◌',
  title = 'داده‌ای برای نمایش نیست',
  description = 'با تغییر فیلترها یا ایجاد یک مورد جدید دوباره تلاش کنید.',
  action
}) {
  return (
    <div className="empty-state">
      <div className="empty-icon">{icon}</div>
      <h3>{title}</h3>
      <p>{description}</p>
      {action}
    </div>
  )
}
