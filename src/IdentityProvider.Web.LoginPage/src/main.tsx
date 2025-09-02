import React, { FormEvent, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { ErrorMessage } from './components/ErrorMessage';
import { LoadingSpinner, LockIcon } from './components/Icons';
import { AuthService } from './services/authService';
import './style.css';
import { LoginFormData } from './types/auth';

// Main Login Component
const LoginApp: React.FC = () => {
    const [email, setEmail] = useState('user@test.com');
    const [password, setPassword] = useState('user@test.com');
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
                // AuthService automatically navigates to dashboard on successful login
                // No need for manual redirect here since we're focusing on login step
                console.log('Login successful! Redirecting to dashboard...');
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
        <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-md w-full space-y-8">
                {/* Header */}
                <div className="text-center">
                    <div className="mx-auto h-16 w-16 bg-gradient-to-r from-blue-500 to-purple-600 rounded-xl flex items-center justify-center shadow-lg">
                        <LockIcon />
                    </div>
                    <h2 className="mt-6 text-3xl font-bold text-white">
                        Identity Provider
                    </h2>
                    <p className="mt-2 text-gray-300">
                        Sign in to continue to your application
                    </p>
                    <div className="mt-4 flex items-center justify-center space-x-2">
                        <div className="h-1 w-8 bg-gradient-to-r from-blue-500 to-purple-600 rounded"></div>
                        <div className="h-1 w-8 bg-gray-600 rounded"></div>
                        <div className="h-1 w-8 bg-gray-600 rounded"></div>
                    </div>
                </div>

                {/* Login Form */}
                <div className="bg-gray-800 rounded-xl shadow-xl p-8 border border-gray-700">
                    <form onSubmit={handleSubmit} className="space-y-6">
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
                                    className="h-4 w-4 text-blue-600 bg-gray-700 border-gray-600 rounded focus:ring-blue-500 focus:ring-offset-gray-800"
                                    checked={rememberMe}
                                    onChange={(e) => setRememberMe(e.target.checked)}
                                    disabled={isLoading}
                                />
                                <label htmlFor="remember-me" className="ml-2 block text-sm text-gray-300">
                                    Remember me
                                </label>
                            </div>

                            <div className="text-sm">
                                <a href="#" className="font-medium text-blue-400 hover:text-blue-300 transition-colors">
                                    Forgot password?
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
                                {isLoading ? 'Signing in...' : 'Sign In'}
                            </button>
                        </div>
                    </form>

                    {/* Demo Info */}
                    <div className="mt-6 pt-6 border-t border-gray-700">
                        <div className="bg-gradient-to-r from-blue-900/30 to-purple-900/30 rounded-lg p-4 border border-blue-700/30">
                            <h4 className="text-blue-300 font-medium mb-2 flex items-center">
                                <span className="mr-2">🔑</span>
                                Demo Credentials
                            </h4>
                            <div className="text-sm text-blue-200 space-y-1">
                                <p><strong>Email:</strong> user@test.com</p>
                                <p><strong>Password:</strong> user@test.com</p>
                                <p className="text-xs text-blue-300 mt-2">
                                    These credentials are pre-filled for demonstration purposes
                                </p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Technical Info */}
                <div className="text-center text-sm text-gray-400">
                    <p>OAuth2 + OpenID Connect Identity Provider</p>
                    <p className="mt-1">Built with ASP.NET Core & React</p>
                </div>
            </div>
        </div>
    );
};

// Initialize the app
const container = document.getElementById('app');
if (!container) throw new Error('Failed to find the app element');

const root = createRoot(container);
root.render(<LoginApp />);
