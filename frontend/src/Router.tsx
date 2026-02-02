import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import { MainLayout } from './components/layout';
import { ProtectedRoute } from './components/auth';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ProfilePage } from './pages/ProfilePage';
import { BenchmarksPage } from './pages/BenchmarksPage';
import { StrategyPage } from './pages/StrategyPage';
import { WorkoutsPage } from './pages/WorkoutsPage';
import { WorkoutInputPage } from './pages/WorkoutInputPage';
import { WorkoutDetailPage } from './pages/WorkoutDetailPage';

/**
 * Application route definitions
 * Public routes: /login, /register
 * Protected routes: /profile, /profile/new, /benchmarks, /strategy, /workouts, /workouts/new
 */
const router = createBrowserRouter([
  // Public routes (no layout)
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/register',
    element: <RegisterPage />,
  },

  // Protected routes (with layout)
  {
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: '/',
        element: <Navigate to="/profile" replace />,
      },
      {
        path: '/profile',
        element: <ProfilePage />,
      },
      {
        path: '/profile/new',
        element: <ProfilePage />,
      },
      {
        path: '/benchmarks',
        element: <BenchmarksPage />,
      },
      {
        path: '/strategy',
        element: <StrategyPage />,
      },
      {
        path: '/workouts',
        element: <WorkoutsPage />,
      },
      {
        path: '/workouts/new',
        element: <WorkoutInputPage />,
      },
      {
        path: '/workouts/:id',
        element: <WorkoutDetailPage />,
      },
      {
        path: '/workouts/:id/edit',
        element: <WorkoutInputPage />,
      },
      {
        // Catch-all route for 404
        path: '*',
        element: <Navigate to="/profile" replace />,
      },
    ],
  },
]);

/**
 * Router component that provides routing context to the app
 */
export const Router: React.FC = () => {
  return <RouterProvider router={router} />;
};
