const labels = {
  Active: 'فعال',
  Inactive: 'غیرفعال',
  PendingInventory: 'در انتظار موجودی',
  PendingPayment: 'در انتظار پرداخت',
  ConfirmingInventory: 'در حال تأیید',
  Confirmed: 'تأیید شده',
  Cancelling: 'در حال لغو',
  Cancelled: 'لغو شده',
  Failed: 'ناموفق',
  Authorized: 'تأیید پرداخت',
  Voided: 'باطل شده',
  Refunded: 'مسترد شده',
  Hotel: 'هتل',
  Flight: 'پرواز',
  Economy: 'اکونومی',
  PremiumEconomy: 'پریمیوم اکونومی',
  Business: 'بیزنس',
  First: 'فرست'
}

const numericLabels = {
  0: 'غیرفعال',
  1: 'فعال',
  2: 'غیرفعال',
  3: 'بیزنس',
  4: 'فرست',
  5: 'لغو شده',
  6: 'ناموفق',
  7: 'در انتظار پرداخت'
}

export function enumLabel(value) {
  if (value === null || value === undefined || value === '') {
    return '—'
  }
  return labels[value] || numericLabels[value] || String(value)
}

export function statusLabel(value) {
  return enumLabel(value)
}

export function statusTone(value) {
  const normalized = String(value || '').toLowerCase()

  if (['confirmed', 'active', 'authorized'].includes(normalized)) {
    return 'success'
  }
  if (['failed', 'cancelled', 'inactive', 'voided'].includes(normalized)) {
    return 'danger'
  }
  if (['refunded'].includes(normalized)) {
    return 'info'
  }
  return 'warning'
}

export function formatMoney(value, currency = 'USD') {
  const amount = Number(value || 0)
  return `${new Intl.NumberFormat('fa-IR', {
    maximumFractionDigits: 2
  }).format(amount)} ${currency}`
}

export function formatDate(value) {
  if (!value) {
    return '—'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return String(value)
  }

  return date.toLocaleDateString('fa-IR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

export function formatDateTime(value) {
  if (!value) {
    return '—'
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return String(value)
  }

  return date.toLocaleDateString('fa-IR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

export function initials(user) {
  const value = [user?.firstName, user?.lastName]
    .filter(Boolean)
    .join(' ')
    .trim()

  if (!value) {
    return (user?.username || 'کاربر').slice(0, 2).toUpperCase()
  }

  return value
    .split(/\s+/)
    .map((part) => part[0])
    .join('')
    .slice(0, 2)
}

export function todayInputValue() {
  const now = new Date()
  const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60_000)
  return localDate.toISOString().slice(0, 10)
}

export function makeIdempotencyKey() {
  if (globalThis.crypto?.randomUUID) {
    return globalThis.crypto.randomUUID()
  }
  return `web-${Date.now()}-${Math.random().toString(36).slice(2)}`
}

export function compactId(value) {
  if (!value) {
    return '—'
  }
  const text = String(value)
  return text.length > 16 ? `${text.slice(0, 8)}…${text.slice(-5)}` : text
}

export function safeArray(value) {
  return Array.isArray(value) ? value : []
}
