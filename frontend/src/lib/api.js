const configuredApiUrl = import.meta.env.VITE_API_URL?.trim()
export const API_BASE_URL = (configuredApiUrl || '/api').replace(/\/+$/, '')

export class ApiError extends Error {
  constructor(message, status, payload) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.payload = payload
  }
}

function getErrorMessage(payload, status) {
  if (typeof payload === 'string' && payload.trim()) {
    return payload
  }

  if (payload?.detail) {
    return payload.detail
  }

  if (payload?.title) {
    return payload.title
  }

  if (payload?.errors && typeof payload.errors === 'object') {
    const validationMessages = Object.values(payload.errors)
      .flat()
      .filter(Boolean)
    if (validationMessages.length) {
      return validationMessages.join(' ')
    }
  }

  const messages = {
    401: 'نشست شما منقضی شده است. دوباره وارد شوید.',
    403: 'شما اجازه انجام این عملیات را ندارید.',
    404: 'منبع موردنظر پیدا نشد.',
    409: 'این عملیات با وضعیت فعلی داده‌ها سازگار نیست.',
    500: 'خطایی در سرور رخ داد.'
  }

  return messages[status] || 'درخواست انجام نشد. دوباره تلاش کنید.'
}

export async function apiRequest(path, options = {}) {
  const {
    method = 'GET',
    body,
    token,
    headers: customHeaders = {},
    signal
  } = options

  const normalizedPath = String(path).replace(/^\/+/, '')
  const url = `${API_BASE_URL}/${normalizedPath}`
  const headers = new Headers(customHeaders)
  let requestBody = body

  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  if (body !== undefined && body !== null && !(body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
    requestBody = JSON.stringify(body)
  }

  const response = await fetch(url, {
    method,
    headers,
    body: requestBody,
    signal
  })

  const contentType = response.headers.get('content-type') || ''
  let payload = null

  if (response.status !== 204) {
    const responseText = await response.text()
    if (responseText) {
      payload = contentType.includes('json')
        ? parseJson(responseText)
        : responseText
    }
  }

  if (!response.ok) {
    throw new ApiError(
      getErrorMessage(payload, response.status),
      response.status,
      payload
    )
  }

  return payload
}

function parseJson(value) {
  try {
    return JSON.parse(value)
  } catch {
    return value
  }
}

export function mediaUrl(value) {
  if (!value) {
    return ''
  }

  if (/^https?:\/\//i.test(value)) {
    return value
  }

  if (API_BASE_URL.startsWith('/')) {
    return `${window.location.origin}${value.startsWith('/') ? value : `/${value}`}`
  }

  const origin = API_BASE_URL.replace(/\/api\/?$/, '')
  return `${origin}${value.startsWith('/') ? value : `/${value}`}`
}
