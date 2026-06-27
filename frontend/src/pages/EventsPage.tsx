import { Link } from 'react-router-dom';
import { Spinner, EmptyState, Card } from '@/components/ui';
import { Calendar, MapPin } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { eventService } from '@/services';
import type { Event } from '@/types';

export default function EventsPage() {
  const { data, isLoading } = useQuery({
    queryKey: ['events'],
    queryFn: () => eventService.getAll(),
  });

  const events = data?.items ?? [];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Eventos</h1>
        <p className="mt-2 text-gray-500">Descubre eventos increíbles cerca de ti</p>
      </div>

      {isLoading ? (
        <Spinner className="py-20" />
      ) : events.length === 0 ? (
        <EmptyState
          icon={<Calendar className="w-12 h-12" />}
          title="No hay eventos disponibles"
          description="Aún no se han publicado eventos. ¡Sé el primero en crear uno!"
        />
      ) : (
        <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {events.map((event: Event) => (
            <Link key={event.id} to={`/events/${event.slug}`}>
              <Card className="hover:shadow-md transition-shadow h-full">
                {event.cardImageUrl || event.mainImageUrl ? (
                  <img
                    src={event.cardImageUrl || event.mainImageUrl}
                    alt={event.title}
                    className="w-full h-48 object-cover rounded-t-xl"
                  />
                ) : (
                  <div className="w-full h-48 bg-gradient-to-br from-primary-400 to-primary-600 rounded-t-xl flex items-center justify-center">
                    <Calendar className="w-12 h-12 text-white/70" />
                  </div>
                )}
                <div className="p-4">
                  <span className="text-xs font-medium text-primary-600 uppercase">
                    {event.category?.name}
                  </span>
                  <h3 className="mt-1 text-lg font-semibold text-gray-900 line-clamp-2">
                    {event.title}
                  </h3>
                  <div className="mt-3 flex items-center gap-4 text-sm text-gray-500">
                    <span className="flex items-center gap-1">
                      <Calendar className="w-4 h-4" />
                      {new Date(event.startDate).toLocaleDateString()}
                    </span>
                    {event.venue && (
                      <span className="flex items-center gap-1">
                        <MapPin className="w-4 h-4" />
                        {event.venue.city}
                      </span>
                    )}
                  </div>
                  {event.ticketTypes && event.ticketTypes.length > 0 && (
                    <div className="mt-3 text-sm font-semibold text-gray-900">
                      Desde ${Math.min(...event.ticketTypes.map((t) => t.price)).toFixed(2)}
                    </div>
                  )}
                </div>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}