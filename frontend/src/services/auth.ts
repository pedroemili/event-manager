import api from './api';
import type { AuthTokens, LoginRequest, RegisterRequest, User } from '@/types';

export const authService = {
  login: async (data: LoginRequest): Promise<AuthTokens> => {
    const response = await api.post<AuthTokens>('/auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<void> => {
    await api.post('/auth/register', data);
  },

  refresh: async (refreshToken: string): Promise<AuthTokens> => {
    const response = await api.post<AuthTokens>('/auth/refresh', { refreshToken });
    return response.data;
  },

  logout: async (): Promise<void> => {
    const refreshToken = localStorage.getItem('refreshToken');
    await api.post('/auth/logout', { refreshToken });
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
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

  getProfile: async (): Promise<User> => {
    const response = await api.get<User>('/auth/me');
    return response.data;
  },
};