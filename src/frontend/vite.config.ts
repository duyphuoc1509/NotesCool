import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import basicSsl from '@vitejs/plugin-basic-ssl'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
    basicSsl(),
  ],
  server: {
    host: 'localhost',
    port: 10001,
    strictPort: true,
    https: {},
    proxy: {
      '/api': {
        target: process.env.VITE_DEV_API_PROXY_TARGET || 'https://localhost:10002',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
