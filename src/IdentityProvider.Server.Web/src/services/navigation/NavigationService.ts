import ApiConfig from "../config/ApiConfig";

/**
 * Service for handling application navigation and redirects
 */
export class NavigationService {
    /**
     * Navigate to the dashboard page
     */
    static navigateToDashboard(): void {
        // Check if we're running in the browser
        if (typeof window !== 'undefined') {
            window.location.href = '/dashboard';
        }
    }

    /**
     * Navigate after successful login, respecting returnUrl if provided
     * @param returnUrl - Optional return URL to redirect to instead of dashboard
     */
    static navigateAfterLogin(returnUrl?: string): void {
        if (typeof window !== 'undefined') {
            if (returnUrl && returnUrl !== '/') {
                // Check if returnUrl is an absolute URL (cross-origin)
                if (returnUrl.startsWith('http://') || returnUrl.startsWith('https://')) {
                    window.location.href = returnUrl;
                } else if (returnUrl.startsWith('/')) {
                    // For API endpoints, prepend the API base URL
                    const apiBaseUrl = ApiConfig.getBaseUrl();
                    const fullUrl = `${apiBaseUrl}${returnUrl}`;
                    window.location.href = fullUrl;
                } else {
                    // For relative URLs within the same app
                    window.location.href = returnUrl;
                }
            } else {
                // Default to dashboard
                this.navigateToDashboard();
            }
        }
    }

    /**
     * Get the returnUrl parameter from current URL query string
     * @returns returnUrl parameter value or null if not found
     */
    static getReturnUrlFromQuery(): string | null {
        if (typeof window === 'undefined') return null;

        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get('returnUrl');
    }

    /**
     * Navigate to a specific URL
     * @param url - The URL to navigate to
     */
    static navigateTo(url: string): void {
        if (typeof window !== 'undefined') {
            window.location.href = url;
        }
    }

    /**
     * Redirect to a URL with optional query parameters
     * @param baseUrl - The base URL to redirect to
     * @param params - Optional query parameters
     */
    static redirectWithParams(baseUrl: string, params?: Record<string, string>): void {
        if (typeof window === 'undefined') return;

        let url = baseUrl;

        if (params && Object.keys(params).length > 0) {
            const searchParams = new URLSearchParams(params);
            url += `?${searchParams.toString()}`;
        }

        window.location.href = url;
    }

    /**
     * Get query parameter from current URL
     * @param name - Parameter name
     * @returns Parameter value or null if not found
     */
    static getQueryParam(name: string): string | null {
        if (typeof window === 'undefined') return null;

        const urlParams = new URLSearchParams(window.location.search);
        return urlParams.get(name);
    }

    /**
     * Get the current URL path
     * @returns Current path or empty string if not in browser
     */
    static getCurrentPath(): string {
        if (typeof window === 'undefined') return '';
        return window.location.pathname;
    }

    /**
     * Check if we're currently on a specific path
     * @param path - The path to check
     * @returns true if on the specified path
     */
    static isOnPath(path: string): boolean {
        return this.getCurrentPath() === path;
    }
}

export default NavigationService;
