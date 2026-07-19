import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./useAuth";

/**
 * Route guard component that protects routes requiring authentication.
 *
 * Props:
 *   - children: The protected content to render when authorized.
 *   - allowedRoles: Optional array of role strings (e.g., ["hse-manager", "admin"]).
 *                   If provided, user must have one of these roles to access the route.
 */
export function ProtectedRoute({ children, allowedRoles }) {
  const { isAuthenticated, isLoading, user } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <div role="status" aria-label="Loading authentication status">
        <p>Loading...</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles && allowedRoles.length > 0) {
    const userRole = user?.role;
    if (!userRole || !allowedRoles.includes(userRole)) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  return children;
}
