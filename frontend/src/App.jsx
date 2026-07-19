import { BrowserRouter, Routes, Route, NavLink, Navigate } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { BarrierCockpit } from "./pages/BarrierCockpit/BarrierCockpit";
import { RiskDashboard } from "./pages/RiskDashboard/RiskDashboard";
import { ExecutiveCockpit } from "./pages/ExecutiveCockpit/ExecutiveCockpit";
import { AICopilot } from "./pages/AICopilot/AICopilot";
import { Login } from "./pages/Login";
import { useAuth } from "./auth/useAuth";
import { Shield, BarChart3, Gauge, MessageSquare, LogOut } from "lucide-react";

function Sidebar() {
  const { user, logout, isAuthenticated } = useAuth();

  if (!isAuthenticated) return null;

  return (
    <nav className="flex w-64 flex-col border-r border-gray-200 bg-white" aria-label="Main navigation">
      <div className="flex h-16 items-center border-b border-gray-200 px-6">
        <h1 className="text-lg font-bold text-gray-900">HSE Cockpit</h1>
      </div>
      <ul className="flex-1 space-y-1 px-3 py-4">
        <li>
          <NavLink to="/barriers" className={({ isActive }) => `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors ${isActive ? "bg-blue-50 text-blue-700" : "text-gray-700 hover:bg-gray-100"}`}>
            <Shield className="h-5 w-5" aria-hidden="true" />
            Barrier Cockpit
          </NavLink>
        </li>
        <li>
          <NavLink to="/risk" className={({ isActive }) => `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors ${isActive ? "bg-blue-50 text-blue-700" : "text-gray-700 hover:bg-gray-100"}`}>
            <BarChart3 className="h-5 w-5" aria-hidden="true" />
            Risk Dashboard
          </NavLink>
        </li>
        <li>
          <NavLink to="/executive" className={({ isActive }) => `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors ${isActive ? "bg-blue-50 text-blue-700" : "text-gray-700 hover:bg-gray-100"}`}>
            <Gauge className="h-5 w-5" aria-hidden="true" />
            Executive Cockpit
          </NavLink>
        </li>
        <li>
          <NavLink to="/copilot" className={({ isActive }) => `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors ${isActive ? "bg-blue-50 text-blue-700" : "text-gray-700 hover:bg-gray-100"}`}>
            <MessageSquare className="h-5 w-5" aria-hidden="true" />
            AI Copilot
          </NavLink>
        </li>
      </ul>
      {user && (
        <div className="border-t border-gray-200 px-3 py-4">
          <p className="text-xs text-gray-500 truncate">{user.email}</p>
          <p className="text-xs text-gray-400">{user.role}</p>
          <button onClick={logout} className="mt-2 flex items-center gap-2 text-xs text-gray-600 hover:text-gray-900">
            <LogOut className="h-3 w-3" /> Sign out
          </button>
        </div>
      )}
    </nav>
  );
}

function AppShell() {
  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <main className="flex-1 overflow-auto">
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/barriers" element={<ProtectedRoute><BarrierCockpit /></ProtectedRoute>} />
          <Route path="/risk" element={<ProtectedRoute><RiskDashboard /></ProtectedRoute>} />
          <Route path="/executive" element={<ProtectedRoute><ExecutiveCockpit /></ProtectedRoute>} />
          <Route path="/copilot" element={<ProtectedRoute><AICopilot /></ProtectedRoute>} />
          <Route path="*" element={<Navigate to="/barriers" replace />} />
        </Routes>
      </main>
    </div>
  );
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppShell />
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
