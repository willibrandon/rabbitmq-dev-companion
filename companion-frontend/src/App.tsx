import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material';
import { Layout } from './components/Layout';
import TopologyDesigner from './pages/TopologyDesigner';
import { Simulations } from './pages/Simulations';
import { DebugAnalysis } from './pages/DebugAnalysis';
import { LearningModules } from './pages/LearningModules';
import { Login } from './components/auth/Login';
import { PrivateRoute } from './components/auth/PrivateRoute';
import { AuthProvider } from './contexts/AuthContext';

const theme = createTheme({
  palette: {
    primary: {
      main: '#ff6600', // RabbitMQ orange
    },
    secondary: {
      main: '#00b5b8', // Teal accent
    },
    background: {
      default: '#f5f5f5',
    },
  },
});

const App: React.FC = () => {
  return (
    <ThemeProvider theme={theme}>
      <AuthProvider>
        <Router>
          <Layout>
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route 
                path="/" 
                element={
                  <PrivateRoute>
                    <TopologyDesigner />
                  </PrivateRoute>
                } 
              />
              <Route 
                path="/simulations" 
                element={
                  <PrivateRoute>
                    <Simulations />
                  </PrivateRoute>
                } 
              />
              <Route 
                path="/debug" 
                element={
                  <PrivateRoute>
                    <DebugAnalysis />
                  </PrivateRoute>
                } 
              />
              <Route 
                path="/learning" 
                element={
                  <PrivateRoute>
                    <LearningModules />
                  </PrivateRoute>
                } 
              />
            </Routes>
          </Layout>
        </Router>
      </AuthProvider>
    </ThemeProvider>
  );
};

export default App;
