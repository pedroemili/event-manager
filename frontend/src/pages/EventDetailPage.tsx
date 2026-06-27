import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { eventService } from '@/services';
import { Spinner, Card, Button } from '@/components/ui';
import { Calendar, MapPin, Clock, Users } from 'lucide-react';

export default function EventDetailPage() {
  const { slug } = useParams<{ slug: string }>();

  const { data: event, isLoading } = useQuery({
    queryKey: ['event', slug],
    queryFn: () => eventService.getBySlug(slug!),
    enabled: !!slug,
  });

  if (isLoading) return <Spinner className="py-20" />;
  if (!event) return <div className="text-center py-20">Evento no encontrado</div>;

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="grid lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">
          {event.mainImageUrl && (
            <img
              src={event.mainImageUrl}
              alt={event.title}
              className="w-full h-96 object-cover rounded-xl"
            />
          )}

          <div>
            <span className="text-sm font-medium text-primary-600">{event.category?.name}</span>
            <h1 className="text-3xl font-bold text-gray-900 mt-1">{event.title}</h1>

            {event.shortDescription && (
              <p className="mt-3 text-lg text-gray-600">{event.shortDescription}</p>
            )}
          </div>

          <Card className="p-6">
            <div className="grid sm:grid-cols-2 gap-4">
              <div className="flex items-center gap-3">
                <Calendar className="w-5 h-5 text-primary-600" />
                <div>
                  <p className="text-sm text-gray-500">Fecha</p>
                  <p className="font-medium">{new Date(event.startDate).toLocaleDateString('es-ES', {
                    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
                  })}</p>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <Clock className="w-5 h-5 text-primary-600" />
                <div>
                  <p className="text-sm text-gray-500">Hora</p>
                  <p className="font-medium">{new Date(event.startDate).toLocaleTimeString('es-ES', {
                    hour: '2-digit', minute: '2-digit',
                  })}</p>
                </div>
              </div>
              {event.venue && (
                <div className="flex items-center gap-3 sm:col-span-2">
                  <MapPin className="w-5 h-5 text-primary-600" />
                  <div>
                    <p className="text-sm text-gray-500">Ubicación</p>
                    <p className="font-medium">
                      {event.venue.name} — {event.venue.address}, {event.venue.city}, {event.venue.country}
                    </p>
                  </div>
                </div>
              )}
              {event.maxAttendees && (
                <div className="flex items-center gap-3">
                  <Users className="w-5 h-5 text-primary-600" />
                  <div>
                    <p className="text-sm text-gray-500">Capacidad</p>
                    <p className="font-medium">{event.maxAttendees} asistentes</p>
                  </div>
                </div>
              )}
            </div>
          </Card>

          {event.description && (
            <div>
              <h2 className="text-xl font-semibold text-gray-900 mb-3">Acerca del Evento</h2>
              <p className="text-gray-600 whitespace-pre-wrap">{event.description}</p>
            </div>
          )}
        </div>

        <div>
          <Card className="p-6 sticky top-20">
            <h2 className="text-lg font-semibold text-gray-900 mb-4">Boletos</h2>

            {event.ticketTypes && event.ticketTypes.length > 0 ? (
              <div className="space-y-3">
                {event.ticketTypes.map((tt) => (
                  <div key={tt.id} className="flex items-center justify-between p-3 border border-gray-200 rounded-lg">
                    <div>
                      <p className="font-medium text-gray-900">{tt.name}</p>
                      <p className="text-sm text-gray-500">
                        {tt.soldQuantity}/{tt.totalQuantity} vendidos
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="font-bold text-primary-600">${tt.price.toFixed(2)}</p>
                      <Link to="/login">
                        <Button size="sm" className="mt-1">Comprar</Button>
                      </Link>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-gray-500 text-sm">No hay boletos disponibles</p>
            )}
          </Card>
        </div>
      </div>
    </div>
  );
}