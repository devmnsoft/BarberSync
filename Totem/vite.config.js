import { defineConfig } from 'vite';

const kioskProxyTarget = process.env.VITE_KIOSK_PROXY_TARGET || 'http://localhost:8083';

export default defineConfig({
  root: 'app',
  server: {
    port: 5173,
    host: true,
    proxy: {
      '/KioskApi': {
        target: kioskProxyTarget,
        changeOrigin: true
      }
    }
  },
  build: { outDir: '../dist', emptyOutDir: true }
});
