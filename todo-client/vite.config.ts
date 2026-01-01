import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const uiHost = env.VITE_UI_HOST || 'localhost'
  const uiPort = Number(env.VITE_UI_PORT || 3000)

  return {
    plugins: [react()],
    server: {
      host: uiHost,
      port: uiPort,
      strictPort: true,
    },
    preview: {
      host: uiHost,
      port: uiPort,
      strictPort: true,
    },
  }
})
