export enum ExchangeType {
    Direct = 'direct',
    Fanout = 'fanout',
    Topic = 'topic',
    Headers = 'headers',
    ConsistentHash = 'x-consistent-hash',
    DeadLetter = 'x-dead-letter'
}

export interface Exchange {
    name: string;
    type: ExchangeType;
    durable: boolean;
    autoDelete: boolean;
    internal: boolean;
    arguments?: Record<string, any>;
}

export interface Queue {
    name: string;
    durable: boolean;
    autoDelete: boolean;
    exclusive: boolean;
    arguments?: Record<string, any>;
    maxLength?: number;
    messageTtl?: number;
    deadLetterExchange?: string;
    deadLetterRoutingKey?: string;
}

export interface Binding {
    sourceExchange: string;
    destinationQueue: string;
    routingKey: string;
    arguments?: Record<string, any>;
}

export interface Topology {
    id?: string;
    name: string;
    description: string;
    exchanges: Exchange[];
    queues: Queue[];
    bindings: Binding[];
    createdAt: string;
    updatedAt: string;
} 