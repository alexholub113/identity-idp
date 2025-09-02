/**
 * Configuration service for API endpoints and settings
 */
export class ApiConfig {
    /**
     * Get the base API URL from environment or fallback logic
     */
    static getBaseUrl(): string {
        const baseUrl = import.meta.env.VITE_API_BASE_URL;
        console.log({baseUrl})
        if (!baseUrl) {
            throw new Error("VITE_API_BASE_URL is not defined");
        }
        
        return baseUrl;
    }

    /**
     * Get the full login endpoint URL
     */
    static getLoginUrl(): string {
        return `${this.getBaseUrl()}/login`;
    }

    /**
     * Get the OpenID configuration endpoint URL
     */
    static getOpenIdConfigUrl(): string {
        return `${this.getBaseUrl()}/.well-known/openid-configuration`;
    }

    /**
     * Get API timeout configuration
     */
    static getTimeout(): number {
        return 10000; // 10 seconds
    }

    /**
     * Get retry configuration
     */
    static getRetryConfig(): { maxAttempts: number; baseDelay: number } {
        return {
            maxAttempts: 2,
            baseDelay: 1000 // 1 second
        };
    }

    /**
     * Check if we're in development mode
     */
    static isDevelopment(): boolean {
        return import.meta.env.DEV || false;
    }

    /**
     * Get environment name
     */
    static getEnvironment(): string {
        return import.meta.env.MODE || 'production';
    }
}

export default ApiConfig;
