import { LoginFormData, LoginResponse } from '../types/auth';

const API_BASE_URL = 'http://localhost:5043';

export class AuthService {
    static async login(formData: LoginFormData): Promise<LoginResponse> {
        try {
            const response = await fetch(`${API_BASE_URL}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData),
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            return data;
        } catch (error) {
            console.error('Login error:', error);
            return {
                success: false,
                message: 'Network error. Please try again.',
            };
        }
    }

    static setToken(token: string): void {
        localStorage.setItem('auth_token', token);
    }

    static getToken(): string | null {
        return localStorage.getItem('auth_token');
    }

    static removeToken(): void {
        localStorage.removeItem('auth_token');
    }

    static isAuthenticated(): boolean {
        return this.getToken() !== null;
    }
}
