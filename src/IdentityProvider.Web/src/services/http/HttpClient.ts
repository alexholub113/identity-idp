import { AuthError, NetworkError, ValidationError } from '../errors/AuthError';

// Configuration constants
const REQUEST_TIMEOUT = 10000; // 10 seconds
const MAX_RETRY_ATTEMPTS = 2;
const RETRY_DELAY_BASE = 1000; // 1 second

/**
 * HTTP client with retry logic and error handling
 */
export class HttpClient {
    /**
     * Make a POST request with retry logic
     * @param url - The URL to make the request to
     * @param body - The request body
     * @param options - Additional request options
     * @returns Promise with the response data
     */
    static async postWithRetry<T>(
        url: string,
        body: unknown,
        options: RequestInit = {}
    ): Promise<T> {
        let lastError: Error | null = null;

        for (let attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++) {
            try {
                return await this.makeRequest<T>(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        ...options.headers,
                    },
                    credentials: 'include',
                    body: JSON.stringify(body),
                    ...options,
                });
            } catch (error) {
                lastError = error as Error;

                // Don't retry on client errors (4xx) or validation errors
                if (error instanceof AuthError && error.statusCode && error.statusCode < 500) {
                    throw error;
                }

                if (error instanceof ValidationError) {
                    throw error;
                }

                // Only retry on network errors or server errors (5xx)
                if (attempt < MAX_RETRY_ATTEMPTS) {
                    console.warn(`Request attempt ${attempt} failed, retrying...`, error);
                    await this.delay(RETRY_DELAY_BASE * Math.pow(2, attempt - 1));
                }
            }
        }

        throw new NetworkError(
            `Request failed after ${MAX_RETRY_ATTEMPTS} attempts`,
            lastError || undefined
        );
    }

    /**
     * Make a GET request for health checks
     * @param url - The URL to check
     * @param timeout - Request timeout in milliseconds
     * @returns Promise with boolean indicating if request was successful
     */
    static async healthCheck(url: string, timeout: number = 5000): Promise<boolean> {
        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), timeout);

            const response = await fetch(url, {
                method: 'GET',
                signal: controller.signal,
                credentials: 'include'
            });

            clearTimeout(timeoutId);
            return response.ok;
        } catch {
            return false;
        }
    }

    /**
     * Make a request with timeout and error handling
     * @param url - The URL to make the request to
     * @param options - Request options
     * @returns Promise with the response data
     */
    private static async makeRequest<T>(url: string, options: RequestInit): Promise<T> {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT);

        try {
            const response = await fetch(url, {
                ...options,
                signal: controller.signal,
            });

            clearTimeout(timeoutId);

            if (!response.ok) {
                await this.handleHttpError(response);
            }

            const data = await response.json();
            return data as T;
        } catch (error) {
            clearTimeout(timeoutId);

            if (error instanceof DOMException && error.name === 'AbortError') {
                throw new NetworkError('Request timed out. Please check your connection and try again.');
            }

            if (error instanceof TypeError && error.message.includes('fetch')) {
                throw new NetworkError('Unable to connect to server. Please check your internet connection.');
            }

            if (error instanceof SyntaxError) {
                throw new AuthError('Server returned invalid response');
            }

            throw error;
        }
    }

    /**
     * Handle HTTP error responses
     * @param response - The failed response
     */
    private static async handleHttpError(response: Response): Promise<never> {
        let errorMessage = 'Request failed';
        let errorDetails: unknown;

        try {
            const errorData = await response.json();
            errorMessage = errorData.message || errorData.detail || errorData.title || errorMessage;
            errorDetails = errorData;
        } catch {
            // If we can't parse the error response, use generic messages based on status
            switch (response.status) {
                case 400:
                    errorMessage = 'Bad request. Please check your input.';
                    break;
                case 401:
                    errorMessage = 'Invalid email or password';
                    break;
                case 403:
                    errorMessage = 'Access denied';
                    break;
                case 404:
                    errorMessage = 'Service not found';
                    break;
                case 429:
                    errorMessage = 'Too many requests. Please try again later.';
                    break;
                case 500:
                    errorMessage = 'Server error. Please try again later.';
                    break;
                case 503:
                    errorMessage = 'Service temporarily unavailable. Please try again later.';
                    break;
                default:
                    errorMessage = `Request failed (${response.status})`;
            }
        }

        throw new AuthError(errorMessage, response.status, errorDetails);
    }

    /**
     * Delay execution for specified milliseconds
     * @param ms - Milliseconds to delay
     */
    private static delay(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}

export default HttpClient;
