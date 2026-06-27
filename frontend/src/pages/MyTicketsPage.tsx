import { useQuery } from '@tanstack/react-query';
import { ticketService } from '@/services';
import { Spinner, EmptyState, Card } from '@/components/ui';
import { Ticket, Calendar, MapPin } from 'lucide-react';

export default function MyTicketsPage() {
  const { data: tickets, isLoading } = useQuery({
    queryKey: ['my-tickets'],
    queryFn: ticketService.getMyTickets,
  });

  if (isLoading) return <Spinner className="py-20" />;

  const activeTickets = tickets?.filter((t) => t.status === 'Active') ?? [];

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Mis Boletos</h1>

      {activeTickets.length === 0 ? (
        <EmptyState
          icon={<Ticket className="w-12 h-12" />}
          title="No tienes boletos"
          description="Compra boletos para tus eventos favoritos"
        />
      ) : (
        <div className="space-y-4">
          {activeTickets.map((ticket) => (
            <Card key={ticket.id} className="p-6">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-semibold text-gray-900">
                    {ticket.event?.title ?? 'Evento'}
                  </h3>
                  {ticket.event && (
                    <div className="mt-1 flex items-center gap-4 text-sm text-gray-500">
                      <span className="flex items-center gap-1">
                        <Calendar className="w-4 h-4" />
                        {new Date(ticket.event.startDate).toLocaleDateString()}
                      </span>
                      {ticket.event.venue && (
                        <span className="flex items-center gap-1">
                          <MapPin className="w-4 h-4" />
                          {ticket.event.venue.name}
                        </span>
                      )}
                    </div>
                  )}
                  <p className="mt-1 text-sm text-gray-500">
                    Boleto #{ticket.ticketNumber} — {ticket.ticketTypeName}
                  </p>
                </div>
                <span className={`px-3 py-1 text-xs font-medium rounded-full ${
                  ticket.status === 'Active' ? 'bg-success-50 text-success-700' : 'bg-gray-100 text-gray-600'
                }`}>
                  {ticket.status === 'Active' ? 'Activo' : ticket.status}
                </span>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}