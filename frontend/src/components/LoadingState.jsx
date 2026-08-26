export default function LoadingState({ label = 'در حال بارگذاری...' }) {
  return (
    <div className="loading-state" role="status">
      <span className="spinner" />
      <span>{label}</span>
    </div>
  )
}
