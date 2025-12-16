import React, { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { FormField, Input, Button, Alert } from '../components/ui';
import { useAuth } from '../hooks';
import { registerSchema, type RegisterSchemaType } from '../schemas/authSchemas';

/**
 * Password requirements display component
 */
const PasswordRequirements: React.FC = () => (
  <div className="rounded-md bg-gray-50 p-3 text-sm text-gray-600">
    <p className="font-medium">Password requirements:</p>
    <ul className="mt-1 list-inside list-disc space-y-1">
      <li>Minimum 8 characters</li>
      <li>At least 1 capital letter</li>
      <li>At least 1 special character</li>
    </ul>
  </div>
);

/**
 * Registration page component
 * Handles new user registration with email and password
 */
export const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const { register: registerUser, isLoading, error, clearError, isAuthenticated } = useAuth();

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/profile', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterSchemaType>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
    },
    mode: 'onBlur',
  });

  const onSubmit = async (data: RegisterSchemaType) => {
    try {
      await registerUser(data);
    } catch {
      // Error is handled by useAuth hook
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-12 sm:px-6 lg:px-8">
      <div className="w-full max-w-md space-y-8">
        {/* Header */}
        <div className="text-center">
          <h1 className="text-3xl font-bold text-primary-600">WodStrat</h1>
          <h2 className="mt-6 text-2xl font-bold text-gray-900">
            Create your account
          </h2>
          <p className="mt-2 text-sm text-gray-600">
            Already have an account?{' '}
            <Link
              to="/login"
              className="font-medium text-primary-600 hover:text-primary-500"
            >
              Sign in
            </Link>
          </p>
        </div>

        {/* Error Alert */}
        {error && (
          <Alert variant="error" dismissible onDismiss={clearError}>
            {error.getUserMessage()}
          </Alert>
        )}

        {/* Registration Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="mt-8 space-y-6" noValidate>
          <div className="space-y-4 rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
            {/* Email Field */}
            <FormField
              label="Email address"
              htmlFor="email"
              required
              error={errors.email?.message}
            >
              <Input
                {...register('email')}
                id="email"
                type="email"
                autoComplete="email"
                placeholder="you@example.com"
                error={!!errors.email}
                disabled={isLoading}
              />
            </FormField>

            {/* Password Field */}
            <FormField
              label="Password"
              htmlFor="password"
              required
              error={errors.password?.message}
            >
              <Input
                {...register('password')}
                id="password"
                type="password"
                autoComplete="new-password"
                placeholder="Create a password"
                error={!!errors.password}
                disabled={isLoading}
              />
            </FormField>

            {/* Confirm Password Field */}
            <FormField
              label="Confirm password"
              htmlFor="confirmPassword"
              required
              error={errors.confirmPassword?.message}
            >
              <Input
                {...register('confirmPassword')}
                id="confirmPassword"
                type="password"
                autoComplete="new-password"
                placeholder="Confirm your password"
                error={!!errors.confirmPassword}
                disabled={isLoading}
              />
            </FormField>

            {/* Password Requirements */}
            <PasswordRequirements />
          </div>

          {/* Submit Button */}
          <Button
            type="submit"
            variant="primary"
            className="w-full"
            size="lg"
            loading={isLoading}
          >
            Create account
          </Button>
        </form>
      </div>
    </div>
  );
};
