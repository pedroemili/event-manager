export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface DashboardMetrics {
  activeEvents: number;
  completedEvents: number;
  totalTicketsSold: number;
  totalRevenue: number;
  monthlyRevenue: number;
  attendanceRate: number;
  topSellingEvent?: {
    id: string;
    title: string;
    soldCount: number;
    revenue: number;
  };
}

export interface RevenueChartData {
  month: string;
  revenue: number;
  ticketsSold: number;
}

export interface AdminDashboardMetrics extends DashboardMetrics {
  totalUsers: number;
  newUsersThisMonth: number;
  totalEvents: number;
  eventsByCategory: { category: string; count: number }[];
  platformRevenue: number;
}