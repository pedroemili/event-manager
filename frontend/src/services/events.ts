import api from './api';
import type {
  Event,
  CreateEventRequest,
  PagedResult,
  TicketType,
  Ticket,
  Order,
  TicketReservation,
  CreateOrderRequest,
  Category,
  Venue,
  DashboardMetrics,
} from '@/types';

async function unwrap<T>(promise: Promise<{ data: T | { data: T } }>): Promise<T> {
  const response = await promise;
  const body = response.data as unknown;
  if (body && typeof body === 'object' && 'data' in body) {
    return (body as { data: T }).data;
  }
  return body as T;
}

interface EventsListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: string;
  fromDate?: string;
  featured?: boolean;
  upcoming?: boolean;
}

export const eventService = {
  listPublished: async (params: EventsListParams = {}): Promise<PagedResult<Event>> =>
    unwrap(api.get<PagedResult<Event>>('/events', { params: { ...params } })),

  getBySlug: async (slug: string): Promise<Event | null> => {
    try {
      return await unwrap(api.get<Event>(`/events/slug/${encodeURIComponent(slug)}`));
    } catch {
      return null;
    }
  },

  getById: async (id: string): Promise<Event | null> => {
    try {
      return await unwrap(api.get<Event>(`/events/${id}`));
    } catch {
      return null;
    }
  },

  getMine: async (): Promise<Event[]> => unwrap(api.get<Event[]>('/events/mine')),

  create: async (data: CreateEventRequest): Promise<Event> =>
    unwrap(api.post<Event>('/events', data)),

  update: async (id: string, data: CreateEventRequest): Promise<Event> =>
    unwrap(api.put<Event>(`/events/${id}`, data)),

  publish: async (id: string): Promise<Event> =>
    unwrap(api.post<Event>(`/events/${id}/publish`)),

  cancel: async (id: string, reason: string): Promise<Event> =>
    unwrap(api.post<Event>(`/events/${id}/cancel`, { reason })),

  toggleFavorite: async (id: string): Promise<boolean> =>
    unwrap(api.post<boolean>(`/events/${id}/favorite`)),
};

export const ticketService = {
  createReservation: async (
    ticketTypeId: string,
    quantity: number
  ): Promise<TicketReservation> =>
    unwrap(
      api.post<TicketReservation>('/tickets/reservations', {
        ticketTypeId,
        quantity,
      })
    ),

  confirmOrder: async (data: CreateOrderRequest): Promise<Order> =>
    unwrap(api.post<Order>('/tickets/orders/confirm', data)),

  cancelOrder: async (orderId: string): Promise<void> => {
    await api.post(`/tickets/orders/${orderId}/cancel`);
  },

  validateTicket: async (qrData: string, eventId: string): Promise<Ticket> =>
    unwrap(
      api.post<Ticket>('/tickets/validate', { qrData, eventId })
    ),

  getMyTickets: async (): Promise<Ticket[]> =>
    unwrap(api.get<Ticket[]>('/tickets/mine')),

  getMyOrders: async (): Promise<Order[]> =>
    unwrap(api.get<Order[]>('/tickets/orders')),
};

export const venueService = {
  getAll: async (): Promise<Venue[]> =>
    unwrap(api.get<Venue[]>('/venues')),
};

export const categoryService = {
  getAll: async (): Promise<Category[]> =>
    unwrap(api.get<Category[]>('/categories')),
};

export const dashboardService = {
  getOrganizerMetrics: async (): Promise<DashboardMetrics> =>
    unwrap(api.get<DashboardMetrics>('/dashboard/organizer/metrics')),
};
