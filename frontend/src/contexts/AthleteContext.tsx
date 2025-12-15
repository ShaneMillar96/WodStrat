import React, { createContext, useContext, useState, useCallback } from 'react';

interface AthleteContextValue {
  athleteId: string | null;
  setAthleteId: (id: string | null) => void;
  clearAthlete: () => void;
  hasAthlete: boolean;
}

const STORAGE_KEY = 'wodstrat_current_athlete_id';

const AthleteContext = createContext<AthleteContextValue | undefined>(undefined);

export const AthleteProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [athleteId, setAthleteIdState] = useState<string | null>(() => {
    return localStorage.getItem(STORAGE_KEY);
  });

  const setAthleteId = useCallback((id: string | null) => {
    setAthleteIdState(id);
    if (id !== null) {
      localStorage.setItem(STORAGE_KEY, id);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, []);

  const clearAthlete = useCallback(() => {
    setAthleteIdState(null);
    localStorage.removeItem(STORAGE_KEY);
  }, []);

  return (
    <AthleteContext.Provider
      value={{
        athleteId,
        setAthleteId,
        clearAthlete,
        hasAthlete: athleteId !== null,
      }}
    >
      {children}
    </AthleteContext.Provider>
  );
};

export const useAthleteContext = () => {
  const context = useContext(AthleteContext);
  if (!context) {
    throw new Error('useAthleteContext must be used within AthleteProvider');
  }
  return context;
};
