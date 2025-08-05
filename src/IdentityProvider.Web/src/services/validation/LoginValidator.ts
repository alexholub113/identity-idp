import { LoginFormData } from '../../types/auth';
import { ValidationError } from '../errors/AuthError';

/**
 * Service for validating login form data
 */
export class LoginValidator {
    /**
     * Validate login form data
     * @param formData - The form data to validate
     * @returns ValidationError if validation fails, null if valid
     */
    static validate(formData: LoginFormData): ValidationError | null {
        if (!formData.email?.trim()) {
            return new ValidationError('Email is required', 'email');
        }

        if (!this.isValidEmail(formData.email)) {
            return new ValidationError('Please enter a valid email address', 'email');
        }

        if (!formData.password?.trim()) {
            return new ValidationError('Password is required', 'password');
        }

        if (formData.password.length < 3) {
            return new ValidationError('Password must be at least 3 characters long', 'password');
        }

        return null;
    }

    /**
     * Validate email format using regex
     * @param email - Email address to validate
     * @returns true if valid, false otherwise
     */
    private static isValidEmail(email: string): boolean {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email.trim());
    }
}

export default LoginValidator;
