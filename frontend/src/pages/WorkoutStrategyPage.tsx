import React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Alert, Button, SkeletonCard } from '../components/ui';
import { WorkoutStrategyPage as StrategyContent } from '../components/strategy';
import { useWorkout } from '../hooks/useWorkout';
import { useWorkoutStrategy } from '../hooks/useWorkoutStrategy';
import { useAthleteContext } from '../contexts/AthleteContext';

/**
 * WorkoutStrategyPage - Route page for /workouts/:id/strategy
 * Fetches workout and strategy data, renders the strategy display
 */
export const WorkoutStrategyPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const workoutId = id ? parseInt(id, 10) : null;
  const { athleteId } = useAthleteContext();

  // Fetch workout details
  const { workout, isLoading: isWorkoutLoading, error: workoutError } = useWorkout(workoutId);

  // Check if workout has weighted movements
  const hasWeightedMovements = workout?.movements.some(m => m.loadValue !== null) ?? false;

  // Fetch all strategy data
  const strategy = useWorkoutStrategy(athleteId, workoutId, {
    enabled: !!athleteId && !!workoutId,
    hasWeightedMovements,
  });

  // Handle back navigation
  const handleBack = () => {
    navigate(`/workouts/${workoutId}`);
  };

  // Loading state for workout
  if (isWorkoutLoading) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
        <SkeletonCard />
        <div className="mt-4 space-y-4">
          <SkeletonCard />
          <SkeletonCard />
        </div>
      </div>
    );
  }

  // Error state for workout
  if (workoutError || !workout) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
        <Alert variant="error" title="Error">
          {workoutError?.getUserMessage?.() || 'Workout not found'}
        </Alert>
        <Link to="/workouts">
          <Button variant="outline" className="mt-4">Back to Workouts</Button>
        </Link>
      </div>
    );
  }

  // No athlete profile warning
  if (!athleteId) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="mb-4">
          <Button variant="outline" size="sm" onClick={handleBack}>
            &larr; Back to Workout
          </Button>
        </div>
        <Alert variant="info" title="Athlete Profile Required">
          Create an athlete profile to view personalized workout strategies.
          <div className="mt-3">
            <Link to="/profile">
              <Button variant="primary" size="sm">Create Profile</Button>
            </Link>
          </div>
        </Alert>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 lg:px-8">
      <StrategyContent
        workout={workout}
        strategy={strategy}
        onBack={handleBack}
      />
    </div>
  );
};
