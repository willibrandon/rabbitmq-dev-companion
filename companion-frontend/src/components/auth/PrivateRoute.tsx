import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

interface PrivateRouteProps {
    children: React.ReactNode;
    requiredRole?: string;
}

export const PrivateRoute: React.FC<PrivateRouteProps> = ({ 
    children, 
    requiredRole 
}) => {
    const { isAuthenticated, user } = useAuth();
    const location = useLocation();

    if (!isAuthenticated) {
        // Redirect to login page with return url
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (requiredRole && user?.role !== requiredRole) {
        // Redirect to home page if user doesn't have required role
        return <Navigate to="/" replace />;
    }

    return <>{children}</>;
}; 