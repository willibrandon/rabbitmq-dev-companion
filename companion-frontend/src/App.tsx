import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material';
import { Layout } from './components/Layout';
import { TopologyDesigner } from './pages/TopologyDesigner';
import { Simulations } from './pages/Simulations';
import { DebugAnalysis } from './pages/DebugAnalysis';
import { LearningModules } from './pages/LearningModules';

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
      <Router>
        <Layout>
          <Routes>
            <Route path="/" element={<TopologyDesigner />} />
            <Route path="/simulations" element={<Simulations />} />
            <Route path="/debug" element={<DebugAnalysis />} />
            <Route path="/learning" element={<LearningModules />} />
          </Routes>
        </Layout>
      </Router>
    </ThemeProvider>
  );
};

export default App;
