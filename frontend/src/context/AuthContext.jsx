import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react'
import { ApiError, apiRequest } from '../lib/api'

const STORAGE_KEY = 'travel-booking-session'
const AuthContext = createContext(null)

function readStoredSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => readStoredSession())
  const [isLoading, setIsLoading] = useState(true)
  const sessionRef = useRef(session)

  const commitSession = useCallback((nextSession) => {
    sessionRef.current = nextSession
    setSession(nextSession)

    if (nextSession) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(nextSession))
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  }, [])

  const refreshAccessToken = useCallback(async () => {
    const current = sessionRef.current
    if (!current?.refreshToken) {
      throw new ApiError('نشست فعالی وجود ندارد.', 401)
    }

    const tokens = await apiRequest('auth/refresh-token', {
      method: 'POST',
      body: { refreshToken: current.refreshToken }
    })

    const nextSession = {
      ...current,
      accessToken: tokens.accessToken,
      refreshToken: tokens.refreshToken,
      expiresAt: Date.now() + Number(tokens.expiresIn || 300) * 1000
    }
    commitSession(nextSession)
    return nextSession.accessToken
  }, [commitSession])

  const loadProfile = useCallback(async (accessToken) => {
    const user = await apiRequest('profile/me', { token: accessToken })
    let roles = []

    try {
      const roleResponse = await apiRequest('profile/me/roles', {
        token: accessToken
      })
      roles = Array.isArray(roleResponse)
        ? roleResponse.map((role) => role.name).filter(Boolean)
        : []
    } catch {
      // Roles are optional for the core user experience.
    }

    return { user, roles }
  }, [])

  const login = useCallback(async (credentials) => {
    const tokens = await apiRequest('auth/login', {
      method: 'POST',
      body: credentials
    })
    const profile = await loadProfile(tokens.accessToken)
    const nextSession = {
      accessToken: tokens.accessToken,
      refreshToken: tokens.refreshToken,
      userId: tokens.userId,
      expiresAt: Date.now() + Number(tokens.expiresIn || 300) * 1000,
      ...profile
    }
    commitSession(nextSession)
    return nextSession
  }, [commitSession, loadProfile])

  const register = useCallback((payload) => {
    return apiRequest('auth/register', {
      method: 'POST',
      body: payload
    })
  }, [])

  const logout = useCallback(async () => {
    const current = sessionRef.current
    try {
      if (current?.refreshToken) {
        await apiRequest('auth/logout', {
          method: 'POST',
          body: { refreshToken: current.refreshToken }
        })
      }
    } finally {
      commitSession(null)
    }
  }, [commitSession])

  const request = useCallback(async (path, options = {}) => {
    const current = sessionRef.current
    if (!current?.accessToken) {
      throw new ApiError('برای انجام این عملیات ابتدا وارد شوید.', 401)
    }

    try {
      return await apiRequest(path, {
        ...options,
        token: current.accessToken
      })
    } catch (error) {
      if (error instanceof ApiError && error.status === 401 && current.refreshToken) {
        try {
          const accessToken = await refreshAccessToken()
          return await apiRequest(path, { ...options, token: accessToken })
        } catch {
          commitSession(null)
        }
      }
      throw error
    }
  }, [commitSession, refreshAccessToken])

  const refreshProfile = useCallback(async () => {
    const current = sessionRef.current
    if (!current?.accessToken) {
      return null
    }

    const profile = await loadProfile(current.accessToken)
    const nextSession = { ...current, ...profile }
    commitSession(nextSession)
    return profile.user
  }, [commitSession, loadProfile])

  useEffect(() => {
    let cancelled = false

    async function restore() {
      const stored = sessionRef.current
      if (!stored?.accessToken) {
        if (!cancelled) {
          setIsLoading(false)
        }
        return
      }

      try {
        const profile = await loadProfile(stored.accessToken)
        if (!cancelled) {
          commitSession({ ...stored, ...profile })
        }
      } catch (error) {
        if (error instanceof ApiError && error.status === 401 && stored.refreshToken) {
          try {
            const accessToken = await refreshAccessToken()
            const profile = await loadProfile(accessToken)
            if (!cancelled) {
              commitSession({ ...sessionRef.current, ...profile })
            }
          } catch {
            if (!cancelled) {
              commitSession(null)
            }
          }
        } else if (!cancelled) {
          commitSession(null)
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    }

    restore()
    return () => {
      cancelled = true
    }
  }, [commitSession, loadProfile, refreshAccessToken])

  const value = {
    accessToken: session?.accessToken || null,
    user: session?.user || null,
    roles: session?.roles || [],
    isAdmin: (session?.roles || []).includes('admin'),
    isAuthenticated: Boolean(session?.accessToken),
    isLoading,
    login,
    register,
    logout,
    request,
    refreshProfile
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }
  return context
}
