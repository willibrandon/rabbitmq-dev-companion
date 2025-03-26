import { Edge, Node } from '@reactflow/core';
import { ExchangeType, Binding, Exchange, Queue } from '../types/topology';

export interface ValidationResult {
    isValid: boolean;
    errors: string[];
    warnings: string[];
}

interface HeaderArgument {
    key: string;
    value: string;
    matchType: 'all' | 'any' | 'exact';
}

interface TopologyValidationContext {
    exchanges: Node<Exchange>[];
    queues: Node<Queue>[];
    edges: Edge<Binding>[];
}

export const validateTopology = (context: TopologyValidationContext): ValidationResult => {
    const errors: string[] = [];
    const warnings: string[] = [];

    // Check for orphaned exchanges (no bindings)
    context.exchanges.forEach(exchange => {
        const hasOutgoingBindings = context.edges.some(edge => edge.source === exchange.id);
        const hasIncomingBindings = context.edges.some(edge => edge.target === exchange.id);
        
        if (!hasOutgoingBindings && !hasIncomingBindings) {
            warnings.push(`Exchange "${exchange.data.name}" has no bindings`);
        }
    });

    // Check for orphaned queues (no bindings)
    context.queues.forEach(queue => {
        const hasBindings = context.edges.some(edge => edge.target === queue.id);
        if (!hasBindings) {
            warnings.push(`Queue "${queue.data.name}" has no bindings`);
        }
    });

    // Check for potential message loss scenarios
    context.queues.forEach(queue => {
        if (!queue.data.durable && !queue.data.deadLetterExchange) {
            warnings.push(`Non-durable queue "${queue.data.name}" has no dead letter exchange configured`);
        }
        
        if (queue.data.autoDelete) {
            warnings.push(`Auto-delete queue "${queue.data.name}" may lead to message loss if consumers disconnect`);
        }

        if (!queue.data.maxLength && !queue.data.maxLengthBytes) {
            warnings.push(`Queue "${queue.data.name}" has no size limits configured`);
        }
    });

    // Check for potential bottlenecks
    const queueBindingCounts = new Map<string, number>();
    context.edges.forEach(edge => {
        const count = queueBindingCounts.get(edge.target) || 0;
        queueBindingCounts.set(edge.target, count + 1);
    });

    queueBindingCounts.forEach((bindingCount, queueId) => {
        if (bindingCount > 5) {
            const queue = context.queues.find(q => q.id === queueId);
            if (queue) {
                warnings.push(`Queue "${queue.data.name}" has ${bindingCount} bindings which may impact routing performance`);
            }
        }
    });

    // Check for competing consumer patterns
    context.exchanges.forEach(exchange => {
        if (exchange.data.type === ExchangeType.Direct) {
            const bindings = context.edges.filter(edge => edge.source === exchange.id);
            const routingKeys = new Map<string, string[]>();
            
            bindings.forEach(binding => {
                const key = binding.data?.routingKey || '';
                const targets = routingKeys.get(key) || [];
                targets.push(binding.target);
                routingKeys.set(key, targets);
            });

            routingKeys.forEach((targets, key) => {
                if (targets.length > 1) {
                    warnings.push(`Multiple queues bound with routing key "${key}" to direct exchange "${exchange.data.name}" creates competing consumers`);
                }
            });
        }
    });

    // Check for potential message loops
    const findCycles = (startId: string, visited = new Set<string>(), path = new Set<string>()): boolean => {
        if (path.has(startId)) return true;
        if (visited.has(startId)) return false;

        visited.add(startId);
        path.add(startId);

        const outgoingEdges = context.edges.filter(edge => edge.source === startId);
        for (const edge of outgoingEdges) {
            if (findCycles(edge.target, visited, path)) {
                return true;
            }
        }

        path.delete(startId);
        return false;
    };

    context.exchanges.forEach(exchange => {
        if (findCycles(exchange.id)) {
            errors.push(`Detected potential message loop involving exchange "${exchange.data.name}"`);
        }
    });

    return {
        isValid: errors.length === 0,
        errors,
        warnings
    };
};

export const validateBinding = (
    edge: Edge,
    sourceExchangeType?: ExchangeType
): ValidationResult => {
    const errors: string[] = [];
    const warnings: string[] = [];
    const data = edge.data as Partial<Binding>;

    // Basic validation
    if (!data) {
        errors.push('Binding has no configuration');
        return { isValid: false, errors, warnings };
    }

    // Exchange-specific validation
    switch (sourceExchangeType) {
        case ExchangeType.Direct:
            if (!data.routingKey) {
                errors.push('Direct exchange requires a routing key');
            } else if (data.routingKey.includes('*') || data.routingKey.includes('#')) {
                errors.push('Direct exchange routing key cannot contain wildcards (* or #)');
            }
            break;

        case ExchangeType.Topic:
            if (data.routingKey) {
                const validTopicPattern = /^([a-zA-Z0-9_-]+|\*|#)(\.[a-zA-Z0-9_-]+|\*|#)*$/;
                if (!validTopicPattern.test(data.routingKey)) {
                    errors.push('Invalid topic pattern. Use dot-separated words with * or # wildcards');
                }
                if (data.routingKey.includes('##')) {
                    errors.push('Invalid use of multiple # wildcards together');
                }
                if (data.routingKey.split('.').filter(part => part === '#').length > 1) {
                    warnings.push('Multiple # wildcards in a topic pattern may lead to unexpected behavior');
                }
                if (data.routingKey.startsWith('#')) {
                    warnings.push('Starting a topic pattern with # will match all messages, consider a more specific pattern');
                }
                if (data.routingKey.split('.').length > 5) {
                    warnings.push('Complex topic patterns with many segments may impact routing performance');
                }
            }
            break;

        case ExchangeType.Headers:
            const headers = (data.arguments?.headers || []) as HeaderArgument[];
            if (headers.length === 0) {
                errors.push('Headers exchange requires at least one header argument');
            } else {
                if (headers.some(h => !h.key || !h.value)) {
                    errors.push('All header arguments must have both key and value');
                }
                if (headers.some(h => h.key.includes('.'))) {
                    warnings.push('Using dots in header keys may cause issues with some AMQP clients');
                }
                if (headers.length > 10) {
                    warnings.push('Large number of headers may impact performance');
                }
                if (!headers.some(h => h.matchType)) {
                    errors.push('Each header argument must specify a match type (all, any, or exact)');
                }
            }
            break;

        case ExchangeType.Fanout:
            if (data.routingKey) {
                warnings.push('Routing key is ignored for fanout exchanges');
            }
            if (data.arguments && Object.keys(data.arguments).length > 0) {
                warnings.push('Arguments are ignored for fanout exchanges');
            }
            break;
    }

    // Common validations
    if (data.routingKey && data.routingKey.length > 255) {
        errors.push('Routing key exceeds maximum length of 255 characters');
    }

    return {
        isValid: errors.length === 0,
        errors,
        warnings
    };
}; 