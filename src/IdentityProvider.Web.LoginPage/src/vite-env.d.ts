/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly API_BASE_URL?: string
    // Add other environment variables here as needed
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}
