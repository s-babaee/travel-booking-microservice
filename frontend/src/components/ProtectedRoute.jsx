import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import LoadingState from './LoadingState'

export function ProtectedRoute({ children }) {
  const { isAuthenticated, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return <LoadingState label="در حال بررسی نشست..." />
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return children || <Outlet />
}

export function AdminRoute({ children }) {
  const { isAdmin, isLoading } = useAuth()

  if (isLoading) {
    return <LoadingState label="در حال بررسی دسترسی..." />
  }

  if (!isAdmin) {
    return <Navigate to="/app" replace />
  }

  return children || <Outlet />
}
