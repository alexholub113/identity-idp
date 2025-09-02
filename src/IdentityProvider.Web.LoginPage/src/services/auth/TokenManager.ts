/**
 * Service for managing authentication tokens in local storage
 */
export class TokenManager {
    private static readonly TOKEN_KEY = 'auth_token';

    /**
     * Store authentication token in local storage
     * @param token - The token to store
     */
    static setToken(token: string): void {
        try {
            localStorage.setItem(this.TOKEN_KEY, token);
        } catch (error) {
            console.warn('Failed to store auth token:', error);
        }
    }

    /**
     * Retrieve authentication token from local storage
     * @returns The stored token or null if not found
     */
    static getToken(): string | null {
        try {
            return localStorage.getItem(this.TOKEN_KEY);
        } catch (error) {
            console.warn('Failed to retrieve auth token:', error);
            return null;
        }
    }

    /**
     * Remove authentication token from local storage
     */
    static removeToken(): void {
        try {
            localStorage.removeItem(this.TOKEN_KEY);
        } catch (error) {
            console.warn('Failed to remove auth token:', error);
        }
    }

    /**
     * Check if user is authenticated (has a stored token)
     * @returns true if authenticated, false otherwise
     */
    static isAuthenticated(): boolean {
        return this.getToken() !== null;
    }

    /**
     * Clear all authentication data
     */
    static clearAuthData(): void {
        this.removeToken();
    }
}

export default TokenManager;
