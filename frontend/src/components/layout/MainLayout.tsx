import { Outlet, Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Calendar, Ticket, LogOut, Menu, X, LayoutDashboard } from 'lucide-react';
import { useState } from 'react';

export default function MainLayout() {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 sticky top-0 z-50">
        <nav className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
          <div className="flex items-center gap-8">
            <Link to="/" className="text-2xl font-bold text-primary-600">
              EventHub
            </Link>
            <div className="hidden md:flex items-center gap-6">
              <Link to="/events" className="text-sm font-medium text-gray-600 hover:text-primary-600 flex items-center gap-1">
                <Calendar className="w-4 h-4" /> Eventos
              </Link>
            </div>
          </div>

          <div className="hidden md:flex items-center gap-4">
            {isAuthenticated ? (
              <>
                <Link to="/dashboard" className="text-sm font-medium text-gray-600 hover:text-primary-600 flex items-center gap-1">
                  <LayoutDashboard className="w-4 h-4" /> Dashboard
                </Link>
                <Link to="/my-tickets" className="text-sm font-medium text-gray-600 hover:text-primary-600 flex items-center gap-1">
                  <Ticket className="w-4 h-4" /> Mis Boletos
                </Link>
                <button
                  onClick={handleLogout}
                  className="text-sm font-medium text-gray-600 hover:text-danger-600 flex items-center gap-1 cursor-pointer"
                >
                  <LogOut className="w-4 h-4" /> Salir
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-sm font-medium text-gray-600 hover:text-primary-600">
                  Iniciar Sesión
                </Link>
                <Link
                  to="/register"
                  className="text-sm font-medium bg-primary-600 text-white px-4 py-2 rounded-lg hover:bg-primary-700 transition-colors"
                >
                  Registrarse
                </Link>
              </>
            )}
          </div>

          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="md:hidden p-2 text-gray-600 cursor-pointer"
          >
            {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
          </button>
        </nav>

        {mobileMenuOpen && (
          <div className="md:hidden border-t border-gray-200 bg-white px-4 py-3 space-y-2">
            <Link to="/events" className="block py-2 text-sm text-gray-600" onClick={() => setMobileMenuOpen(false)}>
              Eventos
            </Link>
            {isAuthenticated ? (
              <>
                <Link to="/dashboard" className="block py-2 text-sm text-gray-600" onClick={() => setMobileMenuOpen(false)}>
                  Dashboard
                </Link>
                <Link to="/my-tickets" className="block py-2 text-sm text-gray-600" onClick={() => setMobileMenuOpen(false)}>
                  Mis Boletos
                </Link>
                <button onClick={handleLogout} className="block py-2 text-sm text-danger-600 cursor-pointer w-full text-left">
                  Cerrar Sesión
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="block py-2 text-sm text-gray-600" onClick={() => setMobileMenuOpen(false)}>
                  Iniciar Sesión
                </Link>
                <Link to="/register" className="block py-2 text-sm text-primary-600 font-medium" onClick={() => setMobileMenuOpen(false)}>
                  Registrarse
                </Link>
              </>
            )}
          </div>
        )}
      </header>

      <main>
        <Outlet />
      </main>

      <footer className="bg-white border-t border-gray-200 mt-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="text-center text-sm text-gray-500">
            &copy; {new Date().getFullYear()} EventHub. Todos los derechos reservados.
          </div>
        </div>
      </footer>
    </div>
  );
}