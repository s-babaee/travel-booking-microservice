import { enumLabel, statusTone } from '../lib/formatters'

export default function StatusBadge({ value, className = '' }) {
  return (
    <span className={`status-badge status-${statusTone(value)} ${className}`}>
      <span className="status-dot" />
      {enumLabel(value)}
    </span>
  )
}
