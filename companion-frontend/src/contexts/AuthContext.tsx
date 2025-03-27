import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService, User, LoginResponse } from '../services/auth';

interface AuthContextType {
    user: User | null;
    isAuthenticated: boolean;
    login: (username: string, password: string) => Promise<void>;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    useEffect(() => {
        // Check authentication status on mount
        const checkAuth = async () => {
            if (authService.isAuthenticated()) {
                try {
                    const currentUser = await authService.getCurrentUser();
                    if (currentUser) {
                        setUser(currentUser);
                        setIsAuthenticated(true);
                    } else {
                        // If getCurrentUser returns null, clean up auth state
                        setUser(null);
                        setIsAuthenticated(false);
                        authService.logout();
                    }
                } catch {
                    setUser(null);
                    setIsAuthenticated(false);
                    authService.logout();
                }
            }
        };

        checkAuth();
    }, []);

    const login = async (username: string, password: string) => {
        const response = await authService.login(username, password);
        // After successful login, get the user details
        const currentUser = await authService.getCurrentUser();
        if (currentUser) {
            setUser(currentUser);
            setIsAuthenticated(true);
        } else {
            // If we can't get user details, clean up
            setUser(null);
            setIsAuthenticated(false);
            authService.logout();
            throw new Error('Failed to get user details');
        }
    };

    const logout = () => {
        authService.logout();
        setUser(null);
        setIsAuthenticated(false);
    };

    return (
        <AuthContext.Provider value={{ user, isAuthenticated, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
}; 