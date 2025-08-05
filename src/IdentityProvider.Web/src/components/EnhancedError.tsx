import React from 'react';

export interface ErrorDisplayProps {
    message: string;
    visible: boolean;
    type?: 'error' | 'warning' | 'info';
    onRetry?: () => void;
    showRetry?: boolean;
}

export const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
    message,
    visible,
    type = 'error',
    onRetry,
    showRetry = false
}) => {
    if (!visible) return null;

    const getErrorStyles = () => {
        switch (type) {
            case 'warning':
                return 'bg-yellow-50 border-yellow-200 text-yellow-800';
            case 'info':
                return 'bg-blue-50 border-blue-200 text-blue-800';
            default:
                return 'bg-red-50 border-red-200 text-red-800';
        }
    };

    const getIcon = () => {
        switch (type) {
            case 'warning':
                return '⚠️';
            case 'info':
                return 'ℹ️';
            default:
                return '❌';
        }
    };

    return (
        <div className={`border rounded-md p-4 ${getErrorStyles()}`}>
            <div className="flex items-start">
                <div className="flex-shrink-0">
                    <span className="text-lg">{getIcon()}</span>
                </div>
                <div className="ml-3 flex-1">
                    <p className="text-sm font-medium">{message}</p>
                    {showRetry && onRetry && (
                        <button
                            onClick={onRetry}
                            className="mt-2 text-sm underline hover:no-underline focus:outline-none"
                        >
                            Try again
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
};

export interface LoadingStateProps {
    isLoading: boolean;
    message?: string;
}

export const LoadingState: React.FC<LoadingStateProps> = ({
    isLoading,
    message = 'Processing...'
}) => {
    if (!isLoading) return null;

    return (
        <div className="flex items-center justify-center p-4 bg-blue-50 border border-blue-200 rounded-md">
            <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600 mr-3"></div>
            <span className="text-blue-800 text-sm">{message}</span>
        </div>
    );
};
