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
  RevenueChartData,
  DiscountCode,
} from '@/types';

export const eventService = {
  getAll: async (params?: Record<string, string>) => {
    const response = await api.get<PagedResult<Event>>('/events', { params });
    return response.data;
  },

  getBySlug: async (slug: string) => {
    const response = await api.get<Event>(`/events/${slug}`);
    return response.data;
  },

  create: async (data: CreateEventRequest) => {
    const response = await api.post<Event>('/events', data);
    return response.data;
  },

  update: async (id: string, data: Partial<CreateEventRequest>) => {
    const response = await api.put<Event>(`/events/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/events/${id}`);
  },

  publish: async (id: string) => {
    const response = await api.post<Event>(`/events/${id}/publish`);
    return response.data;
  },

  cancel: async (id: string, reason: string) => {
    const response = await api.post<Event>(`/events/${id}/cancel`, { reason });
    return response.data;
  },

  uploadImage: async (eventId: string, file: File) => {
    const formData = new FormData();
    formData.append('image', file);
    const response = await api.post(`/events/${eventId}/images`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  toggleFavorite: async (eventId: string) => {
    const response = await api.post(`/events/${eventId}/favorite`);
    return response.data;
  },
};

export const ticketService = {
  getTicketTypes: async (eventId: string) => {
    const response = await api.get<TicketType[]>(`/events/${eventId}/ticket-types`);
    return response.data;
  },

  createReservation: async (ticketTypeId: string, quantity: number) => {
    const response = await api.post<TicketReservation>('/tickets/reservations', {
      ticketTypeId,
      quantity,
    });
    return response.data;
  },

  confirmOrder: async (data: CreateOrderRequest) => {
    const response = await api.post<Order>('/orders', data);
    return response.data;
  },

  getMyTickets: async () => {
    const response = await api.get<Ticket[]>('/tickets/mine');
    return response.data;
  },

  getMyOrders: async () => {
    const response = await api.get<Order[]>('/orders/mine');
    return response.data;
  },

  cancelOrder: async (orderId: string) => {
    await api.post(`/orders/${orderId}/cancel`);
  },

  validateTicket: async (qrData: string) => {
    const response = await api.post<Ticket>('/tickets/validate', { qrData });
    return response.data;
  },
};

export const venueService = {
  getAll: async () => {
    const response = await api.get<Venue[]>('/venues');
    return response.data;
  },

  create: async (data: Partial<Venue>) => {
    const response = await api.post<Venue>('/venues', data);
    return response.data;
  },

  update: async (id: string, data: Partial<Venue>) => {
    const response = await api.put<Venue>(`/venues/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/venues/${id}`);
  },
};

export const categoryService = {
  getAll: async () => {
    const response = await api.get<Category[]>('/categories');
    return response.data;
  },
};

export const dashboardService = {
  getMetrics: async () => {
    const response = await api.get<DashboardMetrics>('/dashboard/metrics');
    return response.data;
  },

  getRevenueChart: async (months: number = 12) => {
    const response = await api.get<RevenueChartData[]>('/dashboard/revenue', {
      params: { months },
    });
    return response.data;
  },
};

export const discountService = {
  validate: async (eventId: string, code: string) => {
    const response = await api.post<DiscountCode>('/discounts/validate', {
      eventId,
      code,
    });
    return response.data;
  },
};