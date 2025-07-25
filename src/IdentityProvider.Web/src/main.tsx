import React, { FormEvent, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { ErrorMessage } from './components/ErrorMessage';
import { LoadingSpinner, LockIcon } from './components/Icons';
import { AuthService } from './services/authService';
import './style.css';
import { LoginFormData } from './types/auth';

// Main Login Component
const LoginApp: React.FC = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [rememberMe, setRememberMe] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [showError, setShowError] = useState(false);

    const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setShowError(false);
        setIsLoading(true);

        const loginData: LoginFormData = {
            email,
            password,
            rememberMe,
        };

        try {
            const response = await AuthService.login(loginData);

            if (response.success) {
                // Store token if provided
                if (response.token) {
                    AuthService.setToken(response.token);
                }

                // Redirect or show success message
                window.location.href = '/dashboard';
            } else {
                setError(response.message || 'Login failed. Please check your credentials.');
                setShowError(true);
            }
        } catch (error) {
            setError('An unexpected error occurred. Please try again.');
            setShowError(true);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-md w-full space-y-8">
                <div>
                    <div className="mx-auto h-12 w-12 flex items-center justify-center rounded-full bg-primary-600">
                        <LockIcon />
                    </div>
                    <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
                        Sign in to your account
                    </h2>
                    <p className="mt-2 text-center text-sm text-gray-600">
                        Alex's Identity Provider Login
                    </p>
                </div>

                <form onSubmit={handleSubmit} className="mt-8 space-y-6">
                    <ErrorMessage message={error} visible={showError} />

                    <div className="space-y-4">
                        <div>
                            <label htmlFor="email" className="form-label">
                                Email address
                            </label>
                            <input
                                id="email"
                                name="email"
                                type="email"
                                autoComplete="email"
                                required
                                className="input-field"
                                placeholder="Enter your email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                disabled={isLoading}
                            />
                        </div>

                        <div>
                            <label htmlFor="password" className="form-label">
                                Password
                            </label>
                            <input
                                id="password"
                                name="password"
                                type="password"
                                autoComplete="current-password"
                                required
                                className="input-field"
                                placeholder="Enter your password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                disabled={isLoading}
                            />
                        </div>
                    </div>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center">
                            <input
                                id="remember-me"
                                name="remember-me"
                                type="checkbox"
                                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                                checked={rememberMe}
                                onChange={(e) => setRememberMe(e.target.checked)}
                                disabled={isLoading}
                            />
                            <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-900">
                                Remember me
                            </label>
                        </div>

                        <div className="text-sm">
                            <a href="#" className="font-medium text-primary-600 hover:text-primary-500">
                                Forgot your password?
                            </a>
                        </div>
                    </div>

                    <div>
                        <button
                            type="submit"
                            className="group relative w-full btn-primary"
                            disabled={isLoading}
                        >
                            {isLoading && (
                                <span className="absolute left-0 inset-y-0 flex items-center pl-3">
                                    <LoadingSpinner />
                                </span>
                            )}
                            Sign in
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};

// Initialize the app
const container = document.getElementById('app');
if (!container) throw new Error('Failed to find the app element');

const root = createRoot(container);
root.render(<LoginApp />);
