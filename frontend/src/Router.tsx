import {
  createBrowserRouter,
  Navigate,
  RouterProvider,
} from 'react-router-dom';
import { ProfilePage } from './pages/ProfilePage';
import { BenchmarksPage } from './pages/BenchmarksPage';

/**
 * Application route definitions
 */
const router = createBrowserRouter([
  {
    path: '/',
    element: <Navigate to="/profile/new" replace />,
  },
  {
    path: '/profile/new',
    element: <ProfilePage />,
  },
  {
    path: '/profile/:id',
    element: <ProfilePage />,
  },
  {
    path: '/athletes/:athleteId/benchmarks',
    element: <BenchmarksPage />,
  },
  {
    // Catch-all route for 404
    path: '*',
    element: <Navigate to="/profile/new" replace />,
  },
]);

/**
 * Router component that provides routing context to the app
 */
export const Router: React.FC = () => {
  return <RouterProvider router={router} />;
};
