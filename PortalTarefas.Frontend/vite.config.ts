import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Configuração do servidor de desenvolvimento com proxy para a API
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5057', // Porta local onde a API .NET está rodando
        changeOrigin: true,
        secure: false,
      },
    },
  },
});