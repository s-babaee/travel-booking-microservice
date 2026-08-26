export default function ErrorNotice({ message, onRetry }) {
  if (!message) {
    return null
  }

  return (
    <div className="error-notice" role="alert">
      <span className="notice-icon">!</span>
      <div>
        <strong>عملیات انجام نشد</strong>
        <p>{message}</p>
      </div>
      {onRetry ? (
        <button type="button" className="button button-ghost button-small" onClick={onRetry}>
          تلاش دوباره
        </button>
      ) : null}
    </div>
  )
}
