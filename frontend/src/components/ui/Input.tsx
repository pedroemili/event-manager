import { type ComponentPropsWithoutRef, forwardRef } from 'react';

interface InputProps extends ComponentPropsWithoutRef<'input'> {
  label?: string;
  error?: string;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, className = '', id, ...props }, ref) => {
    const inputId = id || label?.toLowerCase().replace(/\s+/g, '-');
    return (
      <div className="space-y-1">
        {label && (
          <label htmlFor={inputId} className="block text-sm font-medium text-gray-700">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={`block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-gray-400 focus:border-primary-500 focus:ring-1 focus:ring-primary-500 disabled:bg-gray-50 disabled:text-gray-500 ${
            error ? 'border-danger-500 focus:border-danger-500 focus:ring-danger-500' : ''
          } ${className}`}
          {...props}
        />
        {error && <p className="text-sm text-danger-600">{error}</p>}
      </div>
    );
  }
);
Input.displayName = 'Input';