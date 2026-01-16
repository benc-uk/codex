import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'

const __dirname = dirname(fileURLToPath(import.meta.url))

export default defineConfig({
  assetsInclude: ['**.yaml'],
  build: {
    rollupOptions: {
      input: {
        main: resolve(__dirname, 'index.html'),
        ff: resolve(__dirname, 'sys/fighting-fantasy/index.html'),
        scifi: resolve(__dirname, 'sys/sci-fi/index.html'),
      },
    },
  },
})
