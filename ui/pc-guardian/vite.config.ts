import { readFileSync } from 'node:fs'
import { join } from 'node:path'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

const apiUrl = 'http://127.0.0.1:5788'

const tokenFile =
  process.env.PC_GUARDIAN_TOKEN_FILE ??
  join(process.env.ProgramData ?? 'C:\\ProgramData', 'SecurityMonitor', 'Api', 'api-token')

function readApiToken(): string | undefined {
  try {
    return readFileSync(tokenFile, 'utf8').trim() || undefined
  } catch {
    return undefined
  }
}

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: apiUrl,
        changeOrigin: true,
        configure: (proxy) => {
          proxy.on('proxyReq', (proxyRequest) => {
            const token = readApiToken()
            if (token)
              proxyRequest.setHeader('Authorization', `Bearer ${token}`)
            else
              console.warn(`[api] Could not read the API token from ${tokenFile}`)
          })
        },
      },
    },
  },
})