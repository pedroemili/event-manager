import axios, { type AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios';

const TOKEN_KEY = 'eventhub.accessToken';
const REFRESH_KEY = 'eventhub.refreshToken';

export const tokenStore = {
  get accessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  },
  get refreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  },
  set(accessToken: string, refreshToken: string): void {
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_KEY, refreshToken);
  },
  clear(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

/**
 * Centralized Axios instance. Base URL is `/api` because:
 *   - Vite dev server proxies `/api/*` to the .NET backend
 *   - The production nginx alongside the SPA proxies `/api/*` to the API container
 *   - The backend's controllers are all routed under `api/<controller>`
 *
 * The response unwraps `ApiResponse<T>` so callers always see T directly
 * (the backend returns `{ success, message, data, code }` envelopes).
 */
const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
  // Keep timings inside the 30s command timeout the API sometimes hits under
  // cold-start BCrypt hashing.
  timeout: 30_000,
});

// Track re-try state per-request without leaking into the AxiosRequestConfig.
type RetryFlag = InternalAxiosRequestConfig & { _refreshRetried?: boolean };

api.interceptors.request.use((config) => {
  const token = tokenStore.accessToken;
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Walk the response and:
 *   - 2xx  → unwrap `{ data, success, message }` to `data` (the typed payload)
 *           OR return the typed `data` field as-is
 *   - 4xx/5xx → throw a typed Error that includes the server's `message` + `code`
 */
api.interceptors.response.use(
  (response: AxiosResponse) => {
    const body = response.data;
    if (
      body &&
      typeof body === 'object' &&
      'success' in body &&
      'data' in body &&
      // ApiResponse<T> uses boolean success AND data property
      (body.success === true || body.success === false)
    ) {
      // success=true → unwrap to .data, success=false → throw
      if (body.success === false) {
        const err = new Error(body.message ?? 'Request failed') as Error & {
          code?: string;
          status?: number;
        };
        err.code = body.code;
        err.status = response.status;
        throw err;
      }
      return { ...response, data: body.data };
    }
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryFlag | undefined;
    const status = error.response?.status;

    if (
      status === 401 &&
      originalRequest &&
      !originalRequest._refreshRetried &&
      tokenStore.refreshToken &&
      // do not loop on the refresh endpoint itself
      !originalRequest.url?.endsWith('/auth/refresh')
    ) {
      originalRequest._refreshRetried = true;
      try {
        const response = await axios.post(
          '/api/auth/refresh',
          { refreshToken: tokenStore.refreshToken }
        );
        const body = response.data?.data ?? response.data;
        if (body?.accessToken && body?.refreshToken) {
          tokenStore.set(body.accessToken, body.refreshToken);
          originalRequest.headers =
            originalRequest.headers ?? ({} as InternalAxiosRequestConfig['headers']);
          (
            originalRequest.headers as Record<string, string>
          ).Authorization = `Bearer ${body.accessToken}`;
          return api(originalRequest);
        }
        tokenStore.clear();
      } catch {
        tokenStore.clear();
      }
      return Promise.reject(error);
    }

    // Normalize to a friendly Error with server message if present.
    const body = error.response?.data as
      | { message?: string; code?: string }
      | undefined;
    const message = body?.message ?? error.message ?? 'Network error';
    const err = new Error(message) as Error & {
      code?: string;
      status?: number;
    };
    err.code = body?.code;
    err.status = status;
    return Promise.reject(err);
  }
);

export default api;
