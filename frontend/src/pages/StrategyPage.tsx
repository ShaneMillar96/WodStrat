import React from 'react';
import { Badge } from '../components/ui';

export const StrategyPage: React.FC = () => {
  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="text-center">
        <div className="mx-auto mb-6 flex h-24 w-24 items-center justify-center rounded-full bg-primary-100">
          <svg
            className="h-12 w-12 text-primary-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"
            />
          </svg>
        </div>

        <h1 className="text-2xl font-bold text-gray-900 sm:text-3xl">
          Strategy Generator
        </h1>

        <div className="mt-3">
          <Badge variant="primary" size="lg" rounded>
            Coming Soon
          </Badge>
        </div>

        <p className="mt-6 text-gray-600">
          Paste your daily WOD and get a personalized strategy based on your
          benchmarks and athlete profile. Our AI-powered system will help you
          optimize your pacing, break patterns, and movement approach.
        </p>

        <div className="mt-8 text-left">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
            What to expect
          </h2>
          <ul className="mt-4 space-y-3">
            {[
              "Paste any WOD format - we'll parse it automatically",
              'Get personalized pacing recommendations',
              'Movement-specific break strategies',
              'Target time predictions based on your benchmarks',
            ].map((feature, index) => (
              <li key={index} className="flex items-start">
                <span className="mr-3 mt-1 h-1.5 w-1.5 flex-shrink-0 rounded-full bg-primary-500" />
                <span className="text-gray-600">{feature}</span>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
};
