/**
 * Authentication-related error
 */
export class AuthError extends Error {
    constructor(
        message: string,
        public statusCode?: number,
        public details?: unknown
    ) {
        super(message);
        this.name = 'AuthError';
    }
}

/**
 * Network connectivity error
 */
export class NetworkError extends Error {
    constructor(message: string = 'Network connection failed', public originalError?: Error) {
        super(message);
        this.name = 'NetworkError';
    }
}

/**
 * Input validation error
 */
export class ValidationError extends Error {
    constructor(message: string, public field?: string) {
        super(message);
        this.name = 'ValidationError';
    }
}

export { AuthError as default };
