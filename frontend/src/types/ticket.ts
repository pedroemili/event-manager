export interface TicketType {
  id: string;
  eventId: string;
  name: string;
  description?: string;
  price: number;
  currency: string;
  totalQuantity: number;
  soldQuantity: number;
  minPerOrder: number;
  maxPerOrder: number;
  type: 'Standard' | 'VIP' | 'EarlyBird' | 'Group';
  salesStartAt?: string;
  salesEndAt?: string;
  isActive: boolean;
  displayOrder: number;
}

export interface Ticket {
  id: string;
  orderItemId: string;
  orderId: string;
  eventId: string;
  userId: string;
  ticketNumber: string;
  qrCodeData: string;
  qrCodeImageUrl?: string;
  status: 'Active' | 'Used' | 'Cancelled' | 'Refunded';
  checkedInAt?: string;
  event?: { title: string; startDate: string; venue?: { name: string } };
  ticketTypeName?: string;
}

export interface Order {
  id: string;
  userId: string;
  discountCodeId?: string;
  orderNumber: string;
  subtotalAmount: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  currency: string;
  status: 'Pending' | 'Confirmed' | 'Cancelled' | 'Expired';
  paymentMethod?: string;
  paymentStatus?: string;
  createdAt: string;
  orderItems: OrderItem[];
  discountCode?: DiscountCode;
}

export interface OrderItem {
  id: string;
  orderId: string;
  ticketTypeId: string;
  ticketTypeName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
  tickets: Ticket[];
}

export interface TicketReservation {
  id: string;
  reservationCode: string;
  ticketTypeId: string;
  quantity: number;
  expiresAt: string;
  isConfirmed: boolean;
}

export interface DiscountCode {
  id: string;
  eventId: string;
  code: string;
  description?: string;
  type: 'Percentage' | 'FixedAmount';
  value: number;
  maxTotalUses: number;
  currentUses: number;
  maxUsesPerUser: number;
  isActive: boolean;
}

export interface CreateOrderRequest {
  reservationId: string;
  discountCode?: string;
}