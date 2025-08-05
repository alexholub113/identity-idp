import React from 'react';

interface ErrorMessageProps {
    message: string;
    visible: boolean;
}

export const ErrorMessage: React.FC<ErrorMessageProps> = ({ message, visible }) => {
    if (!visible) return null;

    return (
        <div className="bg-red-50 border border-red-300 text-red-700 px-4 py-3 rounded-lg">
            {message}
        </div>
    );
};
