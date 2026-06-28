import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

export default defineConfig(({ mode }) => {
  // Pick up .env and .env.<mode> for build-time vars (VITE_API_BASE etc.)
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src'),
      },
    },
    define: {
      'import.meta.env.VITE_API_BASE': JSON.stringify(env.VITE_API_BASE ?? '/api'),
    },
    server: {
      port: 5173,
      host: '0.0.0.0',
      strictPort: false,
      // Vite dev server proxies /api/* to the ASP.NET Core backend. The proxy
      // changeOrigin=true is required so the API sees the original Host.
      proxy: {
        '/api': {
          target: env.VITE_BACKEND_URL ?? 'http://localhost:5277',
          changeOrigin: true,
          secure: false,
        },
      },
    },
    build: {
      outDir: 'dist',
      sourcemap: mode !== 'production',
      chunkSizeWarningLimit: 1024,
    },
  };
});
