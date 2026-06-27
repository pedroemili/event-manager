import { Link } from 'react-router-dom';
import { Button } from '@/components/ui';
import { Calendar, Ticket, Users, TrendingUp } from 'lucide-react';

export default function HomePage() {
  return (
    <div>
      <section className="bg-gradient-to-br from-primary-600 to-primary-800 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 text-center">
          <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold tracking-tight">
            Gestiona tus eventos
            <span className="block text-primary-200">como un profesional</span>
          </h1>
          <p className="mt-6 text-lg text-primary-100 max-w-2xl mx-auto">
            Crea eventos, vende boletos con QR, gestiona tu staff y analiza tus resultados.
            Todo en una sola plataforma.
          </p>
          <div className="mt-10 flex items-center justify-center gap-4">
            <Link to="/register">
              <Button size="lg" className="bg-white text-primary-700 hover:bg-gray-100">
                Comenzar Gratis
              </Button>
            </Link>
            <Link to="/events">
              <Button size="lg" variant="ghost" className="text-white border border-white/30 hover:bg-white/10">
                Explorar Eventos
              </Button>
            </Link>
          </div>
        </div>
      </section>

      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20">
        <div className="grid md:grid-cols-4 gap-8">
          {[
            { icon: Calendar, title: 'Creación Simple', description: 'Crea y publica eventos en minutos con nuestro formulario intuitivo.' },
            { icon: Ticket, title: 'Boletos con QR', description: 'Genera boletos digitales con QR único para validación segura.' },
            { icon: Users, title: 'Gestión de Staff', description: 'Invita a tu equipo y asigna roles para el día del evento.' },
            { icon: TrendingUp, title: 'Analíticas', description: 'Dashboard con métricas en tiempo real de ventas y asistencia.' },
          ].map((feature) => (
            <div key={feature.title} className="text-center p-6">
              <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-primary-100 text-primary-600 mb-4">
                <feature.icon className="w-6 h-6" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">{feature.title}</h3>
              <p className="text-sm text-gray-500">{feature.description}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}