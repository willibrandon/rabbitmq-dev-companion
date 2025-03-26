import { Edge } from '@reactflow/core';
import { ExchangeType } from '../../types/topology';
import { validateBinding } from '../../utils/validationRules';

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

    const validation = validateBinding(edge, sourceExchangeType);
    
    // Define colors
    const colors = {
        valid: '#555',
        warning: '#ff9800',
        error: '#f44336'
    };

    // Determine style based on validation
    const style = {
        ...baseStyle,
        stroke: validation.errors.length > 0 ? colors.error : 
               validation.warnings.length > 0 ? colors.warning : 
               colors.valid,
        strokeWidth: validation.errors.length > 0 ? 3 : 2,
        strokeDasharray: validation.errors.length > 0 ? '5,5' : 
                        validation.warnings.length > 0 ? '10,5' : 
                        'none',
    };

    // Add tooltip content
    const tooltipContent = [
        ...validation.errors.map(error => `❌ ${error}`),
        ...validation.warnings.map(warning => `⚠️ ${warning}`)
    ].join('\n');

    return {
        ...style,
        // Add data attributes for tooltip
        data: {
            ...edge.data,
            tooltipContent: tooltipContent || undefined,
            validationStatus: validation.errors.length > 0 ? 'error' : 
                            validation.warnings.length > 0 ? 'warning' : 
                            'valid'
        }
    };
}; 