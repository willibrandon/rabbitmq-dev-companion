import * as signalR from '@microsoft/signalr';

const SIGNALR_URL = process.env.REACT_APP_SIGNALR_URL || 'http://localhost:5000/hubs';

class SignalRService {
    private connection: signalR.HubConnection;
    private simulationCallbacks: ((data: any) => void)[] = [];
    private debugCallbacks: ((data: any) => void)[] = [];

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${SIGNALR_URL}/simulation`)
            .withAutomaticReconnect()
            .build();

        this.connection.on('SimulationUpdate', (data: any) => {
            this.simulationCallbacks.forEach(callback => callback(data));
        });

        this.connection.on('DebugEvent', (data: any) => {
            this.debugCallbacks.forEach(callback => callback(data));
        });
    }

    public async start(): Promise<void> {
        try {
            await this.connection.start();
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
            setTimeout(() => this.start(), 5000);
        }
    }

    public onSimulationUpdate(callback: (data: any) => void): () => void {
        this.simulationCallbacks.push(callback);
        return () => {
            this.simulationCallbacks = this.simulationCallbacks.filter(cb => cb !== callback);
        };
    }

    public onDebugEvent(callback: (data: any) => void): () => void {
        this.debugCallbacks.push(callback);
        return () => {
            this.debugCallbacks = this.debugCallbacks.filter(cb => cb !== callback);
        };
    }

    public async stop(): Promise<void> {
        if (this.connection.state === signalR.HubConnectionState.Connected) {
            await this.connection.stop();
            console.log('SignalR Disconnected');
        }
    }
}

export const signalRService = new SignalRService(); 