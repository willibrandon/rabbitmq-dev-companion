import { Edge } from '@reactflow/core';
import { ExchangeType, Binding } from '../types/topology';

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
            }
            break;

        case ExchangeType.Fanout:
            if (data.routingKey) {
                warnings.push('Routing key is ignored for fanout exchanges');
            }
            break;
    }

    // Common validations
    if (data.routingKey && data.routingKey.length > 255) {
        errors.push('Routing key exceeds maximum length of 255 characters');
    }

    // Performance warnings
    if (sourceExchangeType === ExchangeType.Topic && data.routingKey?.startsWith('#')) {
        warnings.push('Starting a topic pattern with # will match all messages, consider a more specific pattern');
    }

    return {
        isValid: errors.length === 0,
        errors,
        warnings
    };
}; 