import axios from 'axios';
import { Topology, ValidationResult } from '../types/topology';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5052/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

export const topologyApi = {
    getFromBroker: async (): Promise<Topology> => {
        const response = await api.get<Topology>('/topologies/from-broker');
        return response.data;
    },

    validate: async (topology: Topology): Promise<ValidationResult> => {
        const response = await api.post<ValidationResult>('/designer/validate', topology);
        return response.data;
    },

    normalize: async (topology: Topology): Promise<Topology> => {
        const response = await api.post<Topology>('/designer/normalize', topology);
        return response.data;
    },

    getBrokerHealth: async (): Promise<boolean> => {
        try {
            await api.get('/topologies/health');
            return true;
        } catch {
            return false;
        }
    }
};

export const simulationApi = {
    startSimulation: async (config: any): Promise<string> => {
        const response = await api.post<string>('/simulations/start', config);
        return response.data;
    },

    stopSimulation: async (simulationId: string): Promise<void> => {
        await api.post(`/simulations/${simulationId}/stop`);
    },

    getSimulationStatus: async (simulationId: string): Promise<any> => {
        const response = await api.get(`/simulations/${simulationId}/status`);
        return response.data;
    }
};

export const learningApi = {
    getModules: async (): Promise<any[]> => {
        const response = await api.get('/modules');
        return response.data;
    },

    getModule: async (moduleId: string): Promise<any> => {
        const response = await api.get(`/modules/${moduleId}`);
        return response.data;
    },

    updateProgress: async (moduleId: string, stepIndex: number): Promise<void> => {
        await api.post(`/modules/${moduleId}/progress`, { stepIndex });
    }
}; 