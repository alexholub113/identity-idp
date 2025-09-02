import React from 'react';

interface ErrorMessageProps {
    message: string;
    visible: boolean;
}

export const ErrorMessage: React.FC<ErrorMessageProps> = ({ message, visible }) => {
    if (!visible) return null;

    return (
        <div className="bg-red-900/50 border border-red-600/50 text-red-200 px-4 py-3 rounded-lg">
            <div className="flex items-center">
                <span className="mr-2">⚠️</span>
                <span>{message}</span>
            </div>
        </div>
    );
};
