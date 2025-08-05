import React from 'react';
import { createRoot } from 'react-dom/client';
import './style.css';

// Simple Dashboard Component
const Dashboard: React.FC = () => {
    const handleLogout = async () => {
        // Clear any stored tokens
        localStorage.removeItem('auth_token');

        // Redirect to logout endpoint
        window.location.href = 'http://localhost:57481/account/logout';
    };

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-md w-full space-y-8">
                <div className="text-center">
                    <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
                        🎉 Welcome to Dashboard!
                    </h2>
                    <p className="mt-2 text-sm text-gray-600">
                        You've successfully logged in to Alex's Identity Provider
                    </p>
                </div>

                <div className="bg-white p-6 rounded-lg shadow">
                    <h3 className="text-lg font-medium text-gray-900 mb-4">User Information</h3>
                    <p className="text-sm text-gray-600">
                        Your authentication is handled by secure cookies.
                    </p>
                </div>

                <div className="text-center">
                    <button
                        onClick={handleLogout}
                        className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
                    >
                        Logout
                    </button>
                </div>
            </div>
        </div>
    );
};

// Initialize the app
const container = document.getElementById('app');
if (!container) throw new Error('Failed to find the app element');

const root = createRoot(container);
root.render(<Dashboard />);
