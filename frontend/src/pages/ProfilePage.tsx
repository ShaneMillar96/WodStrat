import React, { useState, useEffect, useRef } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { ProfileForm } from '../components/forms/ProfileForm';
import { Alert, Button } from '../components/ui';
import { useAthleteProfile } from '../hooks';
import { useAthleteContext } from '../contexts';
import { ApiException } from '../services';
import type {
  AthleteFormData,
  CreateAthleteRequest,
  UpdateAthleteRequest,
  Gender,
  ExperienceLevel,
  AthleteGoal,
} from '../types';

/**
 * Convert form data to API request format
 */
function formDataToRequest(
  data: AthleteFormData
): CreateAthleteRequest | UpdateAthleteRequest {
  return {
    name: data.name,
    dateOfBirth: data.dateOfBirth || null,
    gender: (data.gender as Gender) || null,
    heightCm: data.heightCm ? parseFloat(data.heightCm) : null,
    weightKg: data.weightKg ? parseFloat(data.weightKg) : null,
    experienceLevel: data.experienceLevel as ExperienceLevel,
    primaryGoal: data.primaryGoal as AthleteGoal,
  };
}

/**
 * Loading skeleton for the profile page
 */
const LoadingSkeleton: React.FC = () => (
  <div className="animate-pulse space-y-6">
    <div className="h-8 w-48 rounded bg-gray-200" />
    <div className="space-y-4">
      {[...Array(5)].map((_, i) => (
        <div key={i} className="space-y-2">
          <div className="h-4 w-24 rounded bg-gray-200" />
          <div className="h-10 w-full rounded bg-gray-200" />
        </div>
      ))}
    </div>
  </div>
);

/**
 * Error display component
 */
const ErrorDisplay: React.FC<{
  error: ApiException | Error;
  onRetry?: () => void;
}> = ({ error, onRetry }) => {
  const isNotFound = error instanceof ApiException && error.isNotFound();
  const message =
    error instanceof ApiException
      ? error.getUserMessage()
      : error.message || 'An unexpected error occurred';

  return (
    <div className="space-y-4">
      <Alert variant="error" title={isNotFound ? 'Profile Not Found' : 'Error'}>
        {message}
      </Alert>
      {onRetry && !isNotFound && (
        <Button variant="outline" onClick={onRetry}>
          Try Again
        </Button>
      )}
      {isNotFound && (
        <Button variant="primary" onClick={() => window.location.href = '/profile/new'}>
          Create New Profile
        </Button>
      )}
    </div>
  );
};

/**
 * Profile page container component
 * Handles both create and edit modes
 * Uses session-based identification (no ID in URL)
 */
export const ProfilePage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();

  // Determine mode from URL path
  const isNewProfilePage = location.pathname === '/profile/new';

  const {
    athlete,
    isLoading,
    isFetching,
    error,
    createAthlete,
    updateAthlete,
    isCreating,
    isUpdating,
    createSuccess,
    updateSuccess,
    hasAthlete,
    createdAthleteId,
    resetMutationState,
  } = useAthleteProfile();

  const { setAthleteId } = useAthleteContext();

  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // Navigation guard to prevent infinite redirect loops
  const hasNavigated = useRef(false);

  // Determine if we're in edit mode (existing athlete) or create mode
  const isEditMode = hasAthlete && !isNewProfilePage;

  // Handle redirects between /profile and /profile/new based on athlete existence
  useEffect(() => {
    // Skip if still loading or if we've already navigated
    if (isLoading || hasNavigated.current) {
      return;
    }

    // Redirect to /profile if on /profile/new but already has athlete
    if (hasAthlete && isNewProfilePage) {
      hasNavigated.current = true;
      navigate('/profile', { replace: true });
      return;
    }

    // Redirect to /profile/new if on /profile but no athlete
    if (!hasAthlete && !isNewProfilePage && athlete === null) {
      hasNavigated.current = true;
      navigate('/profile/new', { replace: true });
    }
  }, [isLoading, hasAthlete, isNewProfilePage, athlete, navigate]);

  // Navigate to profile page after successful creation
  useEffect(() => {
    if (createSuccess && createdAthleteId) {
      setSuccessMessage('Profile created successfully!');
      setAthleteId(createdAthleteId);
      // Navigate to the profile page
      setTimeout(() => {
        navigate('/profile', { replace: true });
      }, 1500);
    }
  }, [createSuccess, createdAthleteId, navigate, setAthleteId]);

  // Show success message after update
  useEffect(() => {
    if (updateSuccess) {
      setSuccessMessage('Profile updated successfully!');
      // Clear success message after a few seconds
      const timer = setTimeout(() => {
        setSuccessMessage(null);
        resetMutationState();
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [updateSuccess, resetMutationState]);

  const handleSubmit = async (formData: AthleteFormData) => {
    setSubmitError(null);
    setSuccessMessage(null);

    try {
      const requestData = formDataToRequest(formData);

      if (isEditMode) {
        await updateAthlete(requestData as UpdateAthleteRequest);
      } else {
        await createAthlete(requestData as CreateAthleteRequest);
      }
    } catch (err) {
      if (err instanceof ApiException) {
        setSubmitError(err.getUserMessage());
      } else if (err instanceof Error) {
        setSubmitError(err.message);
      } else {
        setSubmitError('An unexpected error occurred. Please try again.');
      }
    }
  };

  const handleDismissSuccess = () => {
    setSuccessMessage(null);
  };

  const handleDismissError = () => {
    setSubmitError(null);
  };

  // Show loading skeleton while fetching
  if (isLoading) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
        <LoadingSkeleton />
      </div>
    );
  }

  // Show error if fetch failed (but not for 404 which just means no profile yet)
  if (error && !(error instanceof ApiException && error.isNotFound())) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
        <ErrorDisplay
          error={error}
          onRetry={() => window.location.reload()}
        />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
      {/* Page Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl">
          {isEditMode ? 'Edit Profile' : 'Create Profile'}
        </h1>
        <p className="mt-2 text-sm text-gray-600">
          {isEditMode
            ? 'Update your athlete profile information.'
            : 'Set up your athlete profile to get personalized workout strategies.'}
        </p>
      </div>

      {/* Success Message */}
      {successMessage && (
        <div className="mb-6">
          <Alert
            variant="success"
            dismissible
            onDismiss={handleDismissSuccess}
          >
            {successMessage}
          </Alert>
        </div>
      )}

      {/* Submit Error Message */}
      {submitError && (
        <div className="mb-6">
          <Alert
            variant="error"
            dismissible
            onDismiss={handleDismissError}
          >
            {submitError}
          </Alert>
        </div>
      )}

      {/* Refetching indicator */}
      {isFetching && !isLoading && (
        <div className="mb-4">
          <Alert variant="info">Refreshing profile data...</Alert>
        </div>
      )}

      {/* Profile Form */}
      <div className="rounded-lg border border-gray-200 bg-white p-6 shadow-sm">
        <ProfileForm
          athlete={athlete}
          onSubmit={handleSubmit}
          isSubmitting={isCreating || isUpdating}
        />
      </div>
    </div>
  );
};
