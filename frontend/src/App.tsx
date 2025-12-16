import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './lib/queryClient';
import { AuthProvider, AthleteProvider } from './contexts';
import { Router } from './Router';

/**
 * Root application component
 * Sets up providers for:
 * - TanStack Query (data fetching and caching)
 * - Auth Context (authentication state)
 * - Athlete Context (current athlete state)
 * - React Router (routing)
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <AthleteProvider>
          <Router />
        </AthleteProvider>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
