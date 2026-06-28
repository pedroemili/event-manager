import api, { tokenStore } from './api';
import type { AuthTokens, LoginRequest, RegisterRequest, User } from '@/types';

interface ProfileEnvelope {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl?: string;
  phoneNumber?: string;
  emailVerified: boolean;
  roles: string[];
}

function unwrap<T>(promise: Promise<{ data: T | { data: T } }>): Promise<T> {
  return promise.then(r => {
    const body = r.data as unknown;
    if (body && typeof body === 'object' && 'data' in body) {
      return (body as { data: T }).data;
    }
    return body as T;
  });
}

export const authService = {
  login: async (data: LoginRequest): Promise<AuthTokens> =>
    unwrap(api.post<AuthTokens>('/auth/login', data)),

  register: async (data: RegisterRequest): Promise<void> => {
    await api.post('/auth/register', data);
  },

  refresh: async (refreshToken: string): Promise<AuthTokens> =>
    unwrap(api.post<AuthTokens>('/auth/refresh', { refreshToken })),

  logout: async (): Promise<void> => {
    const refreshToken = tokenStore.refreshToken;
    try {
      if (refreshToken) {
        await api.post('/auth/logout', { refreshToken });
      }
    } finally {
      tokenStore.clear();
    }
  },

  verifyEmail: async (token: string): Promise<void> => {
    await api.post('/auth/verify-email', { token });
  },

  forgotPassword: async (email: string): Promise<void> => {
    await api.post('/auth/forgot-password', { email });
  },

  resetPassword: async (token: string, newPassword: string): Promise<void> => {
    await api.post('/auth/reset-password', { token, newPassword });
  },

  changePassword: async (
    currentPassword: string,
    newPassword: string
  ): Promise<void> => {
    await api.post('/auth/change-password', {
      currentPassword,
      newPassword,
    });
  },

  getProfile: async (): Promise<User> =>
    unwrap(api.get<ProfileEnvelope>('/auth/me')).then(p => ({
      id: p.id,
      firstName: p.firstName,
      lastName: p.lastName,
      email: p.email,
      avatarUrl: p.avatarUrl,
      phoneNumber: p.phoneNumber,
      emailVerified: p.emailVerified,
      isActive: true,
      roles: p.roles,
      createdAt: '',
    })),
};
