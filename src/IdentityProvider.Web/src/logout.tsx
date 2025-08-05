import React from 'react';
import { createRoot } from 'react-dom/client';
import './style.css';

// Simple Logout Component
const Logout: React.FC = () => {
    const handleLoginAgain = () => {
        window.location.href = '/login';
    };

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
            <div className="max-w-md w-full space-y-8">
                <div className="text-center">
                    <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
                        👋 You've been logged out
                    </h2>
                    <p className="mt-2 text-sm text-gray-600">
                        Thank you for using Alex's Identity Provider
                    </p>
                </div>

                <div className="bg-white p-6 rounded-lg shadow text-center">
                    <p className="text-gray-600 mb-4">
                        Your session has been securely terminated.
                    </p>

                    <button
                        onClick={handleLoginAgain}
                        className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
                    >
                        Login Again
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
root.render(<Logout />);
