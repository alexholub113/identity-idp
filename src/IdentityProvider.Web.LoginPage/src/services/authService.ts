import { LoginFormData, LoginResponse } from '../types/auth';
import { ApiConfig } from './config/ApiConfig';
import { AuthError, NetworkError, ValidationError } from './errors/AuthError';
import { HttpClient } from './http/HttpClient';
import { NavigationService } from './navigation/NavigationService';
import { LoginValidator } from './validation/LoginValidator';

/**
 * Main authentication service providing login functionality
 */
export class AuthService {
    /**
     * Authenticate user with email and password
     * @param formData - Login form data containing email, password, and optional rememberMe
     * @returns Promise with login response containing success status and optional redirect
     */
    static async login(formData: LoginFormData): Promise<LoginResponse> {
        try {
            // Validate input
            const validationError = LoginValidator.validate(formData);
            if (validationError) {
                return {
                    success: false,
                    message: validationError.message
                };
            }

            // Get returnUrl from current page query parameters
            const returnUrl = NavigationService.getReturnUrlFromQuery();

            // Prepare request payload
            const payload = {
                email: formData.email.trim(),
                password: formData.password,
                rememberMe: formData.rememberMe || false,
                returnUrl: returnUrl // Include returnUrl in the request
            };

            // Make login request
            const response = await HttpClient.post<{
                success: boolean;
                message: string;
                redirectUrl?: string;
            }>(ApiConfig.getLoginUrl(), payload);

            // Handle successful response
            if (response.success) {
                // Use the redirectUrl from the API response, or fallback to returnUrl, or dashboard
                const finalRedirectUrl = response.redirectUrl || returnUrl || '/dashboard';

                // Navigate to the appropriate URL
                NavigationService.navigateAfterLogin(finalRedirectUrl);

                return {
                    success: true,
                    message: response.message,
                    redirectUrl: finalRedirectUrl
                };
            } else {
                return {
                    success: false,
                    message: response.message || 'Login failed'
                };
            }
        } catch (error) {
            return this.handleLoginError(error);
        }
    }

    /**
     * Navigate to dashboard page
     */
    static navigateToDashboard(): void {
        NavigationService.navigateToDashboard();
    }

    /**
     * Handle login errors and convert them to user-friendly responses
     * @param error - The error that occurred during login
     * @returns LoginResponse with error details
     */
    private static handleLoginError(error: unknown): LoginResponse {
        console.error('Login error:', error);

        if (error instanceof ValidationError) {
            return {
                success: false,
                message: error.message
            };
        }

        if (error instanceof AuthError) {
            return {
                success: false,
                message: error.message
            };
        }

        if (error instanceof NetworkError) {
            return {
                success: false,
                message: error.message
            };
        }

        // Unknown error
        return {
            success: false,
            message: 'An unexpected error occurred. Please try again.'
        };
    }
}

// Re-export error types for convenience
export { AuthError, NetworkError, ValidationError };

// Default export
export default AuthService;
