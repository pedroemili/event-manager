import { useQuery } from '@tanstack/react-query';
import { dashboardService } from '@/services';
import { Card, CardBody, Spinner } from '@/components/ui';
import { Calendar, Ticket, DollarSign, TrendingUp } from 'lucide-react';

export default function DashboardPage() {
  const { data: metrics, isLoading } = useQuery({
    queryKey: ['dashboard'],
    queryFn: dashboardService.getMetrics,
  });

  if (isLoading) return <Spinner className="py-20" />;

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>

      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        {[
          { label: 'Eventos Activos', value: metrics?.activeEvents ?? 0, icon: Calendar, color: 'text-primary-600 bg-primary-100' },
          { label: 'Boletos Vendidos', value: metrics?.totalTicketsSold ?? 0, icon: Ticket, color: 'text-success-600 bg-success-100' },
          { label: 'Ingresos Totales', value: `$${(metrics?.totalRevenue ?? 0).toFixed(2)}`, icon: DollarSign, color: 'text-warning-600 bg-warning-100' },
          { label: 'Tasa de Asistencia', value: `${(metrics?.attendanceRate ?? 0).toFixed(1)}%`, icon: TrendingUp, color: 'text-purple-600 bg-purple-100' },
        ].map((stat) => (
          <Card key={stat.label}>
            <CardBody>
              <div className="flex items-center gap-4">
                <div className={`p-3 rounded-lg ${stat.color}`}>
                  <stat.icon className="w-6 h-6" />
                </div>
                <div>
                  <p className="text-sm text-gray-500">{stat.label}</p>
                  <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                </div>
              </div>
            </CardBody>
          </Card>
        ))}
      </div>
    </div>
  );
}