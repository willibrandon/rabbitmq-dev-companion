export enum ExchangeType {
    Direct = 'direct',
    Fanout = 'fanout',
    Topic = 'topic',
    Headers = 'headers',
    ConsistentHash = 'x-consistent-hash',
    DeadLetter = 'x-dead-letter'
}

export interface Exchange {
    id: string;
    name: string;
    type: ExchangeType;
    durable: boolean;
    autoDelete: boolean;
    internal: boolean;
    arguments?: Record<string, any>;
}

export interface Queue {
    id: string;
    name: string;
    durable: boolean;
    autoDelete: boolean;
    exclusive: boolean;
    arguments?: Record<string, any>;
    maxLength?: number;
    maxLengthBytes?: number;
    messageTtl?: number;
    deadLetterExchange?: string;
    deadLetterRoutingKey?: string;
}

export interface Binding {
    id: string;
    sourceExchange: string;
    destinationQueue: string;
    routingKey?: string;
    arguments?: Record<string, any>;
}

export interface Topology {
    id?: string;
    name: string;
    description?: string;
    exchanges: Exchange[];
    queues: Queue[];
    bindings: Binding[];
    createdAt?: string;
    updatedAt?: string;
}

export interface ValidationResult {
    isValid: boolean;
    errors: string[];
}

// Custom node types for Reactflow
export enum NodeType {
    Exchange = 'exchange',
    Queue = 'queue'
}

export interface TopologyNode {
    id: string;
    type: NodeType;
    position: { x: number; y: number };
    data: Exchange | Queue;
}

export interface TopologyEdge {
    id: string;
    source: string;
    target: string;
    data: Binding;
} 