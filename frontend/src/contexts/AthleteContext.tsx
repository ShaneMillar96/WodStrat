import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { useAuthContext } from './AuthContext';

interface AthleteContextValue {
  athleteId: number | null;
  hasAthlete: boolean;
  setAthleteId: (id: number | null) => void;
  clearAthlete: () => void;
}

const AthleteContext = createContext<AthleteContextValue | undefined>(undefined);

export const AthleteProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const { user, isAuthenticated, updateAthleteId } = useAuthContext();
  const [athleteId, setAthleteIdState] = useState<number | null>(
    user?.athleteId ?? null
  );

  // Sync athlete ID from auth context when user changes
  useEffect(() => {
    if (user?.athleteId !== undefined) {
      setAthleteIdState(user.athleteId);
    } else if (!isAuthenticated) {
      setAthleteIdState(null);
    }
  }, [user?.athleteId, isAuthenticated]);

  const setAthleteId = useCallback((id: number | null) => {
    setAthleteIdState(id);
    if (id !== null) {
      updateAthleteId(id);
    }
  }, [updateAthleteId]);

  const clearAthlete = useCallback(() => {
    setAthleteIdState(null);
  }, []);

  return (
    <AthleteContext.Provider
      value={{
        athleteId,
        hasAthlete: athleteId !== null,
        setAthleteId,
        clearAthlete,
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
