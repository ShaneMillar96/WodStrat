import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './lib/queryClient';
import { AthleteProvider } from './contexts';
import { Router } from './Router';

/**
 * Root application component
 * Sets up providers for:
 * - TanStack Query (data fetching and caching)
 * - Athlete Context (current athlete state)
 * - React Router (routing)
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AthleteProvider>
        <Router />
      </AthleteProvider>
    </QueryClientProvider>
  );
}

export default App;
