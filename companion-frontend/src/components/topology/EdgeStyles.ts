import { Edge } from '@reactflow/core';
import { ExchangeType } from '../../types/topology';

interface HeaderArgument {
    key: string;
    value: string;
    matchType: 'all' | 'any' | 'exact';
}

export const getEdgeStyle = (edge: Edge, sourceExchangeType?: ExchangeType) => {
    const baseStyle = {
        stroke: '#555',
        strokeWidth: 2,
    };

    if (!edge.data) return baseStyle;

    // Validate based on exchange type
    let isValid = true;
    if (sourceExchangeType === ExchangeType.Direct) {
        isValid = !!edge.data.routingKey;
    } else if (sourceExchangeType === ExchangeType.Headers) {
        const headers = (edge.data.arguments?.headers || []) as HeaderArgument[];
        isValid = headers.length > 0 && headers.every(h => h.key && h.value);
    }

    return {
        ...baseStyle,
        stroke: isValid ? '#555' : '#ff0000',
        strokeWidth: isValid ? 2 : 3,
        strokeDasharray: isValid ? 'none' : '5,5',
    };
}; 