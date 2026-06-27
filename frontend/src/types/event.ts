import type { Venue } from './venue';
import type { TicketType } from './ticket';

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  iconName?: string;
  isActive: boolean;
}

export interface Tag {
  id: string;
  name: string;
  slug: string;
}

export interface Event {
  id: string;
  organizerId: string;
  categoryId?: string;
  venueId?: string;
  title: string;
  slug: string;
  shortDescription?: string;
  description?: string;
  visibility: 'Public' | 'Private' | 'Unlisted';
  startDate: string;
  endDate: string;
  timezone: string;
  status: 'Draft' | 'Published' | 'Cancelled' | 'Completed';
  mainImageUrl?: string;
  thumbnailUrl?: string;
  cardImageUrl?: string;
  heroImageUrl?: string;
  maxAttendees?: number;
  ageRestriction?: number;
  isFeatured: boolean;
  viewCount: number;
  externalUrl?: string;
  publishedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
  rejectionReason?: string;
  completedAt?: string;
  createdAt: string;

  organizer?: { id: string; firstName: string; lastName: string };
  category?: Category;
  venue?: Venue;
  ticketTypes?: TicketType[];
  images?: EventImage[];
  tags?: Tag[];
  isFavorited?: boolean;
  favoriteCount?: number;
}

export interface EventImage {
  id: string;
  eventId: string;
  imageUrl: string;
  orderIndex: number;
  altText?: string;
}

export interface CreateEventRequest {
  title: string;
  description?: string;
  categoryId?: string;
  venueId?: string;
  startDate: string;
  endDate: string;
  timezone?: string;
  maxAttendees?: number;
  ageRestriction?: number;
  visibility?: 'Public' | 'Private' | 'Unlisted';
}