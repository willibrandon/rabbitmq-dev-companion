import axios from 'axios';
import { Topology, ValidationResult } from '../types/topology';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5052/api';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

export interface ConfigurationOptions {
    includeDotNetCode?: boolean;
    includeDockerCompose?: boolean;
    includeKubernetes?: boolean;
    includeProducerCode?: boolean;
    includeConsumerCode?: boolean;
}

export interface ConfigurationOutput {
    dotNetCode?: string;
    dockerComposeYaml?: string;
    kubernetesYaml?: string;
    producerCode?: string;
    consumerCode?: string;
}

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
    },

    saveTopology: async (topology: Topology): Promise<Topology> => {
        const response = await api.post<Topology>('/topologies', topology);
        return response.data;
    },

    generateConfiguration: async (topology: Topology, options: ConfigurationOptions): Promise<ConfigurationOutput> => {
        const response = await api.post<ConfigurationOutput>('/config-generator', { 
            topology, 
            options 
        });
        return response.data;
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