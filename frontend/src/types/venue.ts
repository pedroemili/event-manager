export interface Venue {
  id: string;
  name: string;
  description?: string;
  address: string;
  city: string;
  state?: string;
  country: string;
  zipCode?: string;
  latitude?: number;
  longitude?: number;
  capacity: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateVenueRequest {
  name: string;
  description?: string;
  address: string;
  city: string;
  state?: string;
  country: string;
  zipCode?: string;
  latitude?: number;
  longitude?: number;
  capacity: number;
}