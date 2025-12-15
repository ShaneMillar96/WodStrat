import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './lib/queryClient';
import { Router } from './Router';

/**
 * Root application component
 * Sets up providers for:
 * - TanStack Query (data fetching and caching)
 * - React Router (routing)
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-gray-50">
        <Router />
      </div>
    </QueryClientProvider>
  );
}

export default App;
