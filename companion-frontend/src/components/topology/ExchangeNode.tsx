import React, { memo } from 'react';
import { Handle, Position } from '@reactflow/core';
import { Card, CardContent, Typography, Chip, Box } from '@mui/material';
import { Exchange } from '../../types/topology';

interface ExchangeNodeProps {
    data: Exchange;
    selected: boolean;
}

export const ExchangeNode: React.FC<ExchangeNodeProps> = memo(({ data, selected }) => {
    return (
        <Card 
            variant="outlined"
            sx={{
                minWidth: 200,
                borderColor: selected ? 'primary.main' : 'grey.300',
                borderWidth: selected ? 2 : 1,
                bgcolor: 'background.paper',
                '&:hover': {
                    borderColor: 'primary.main',
                    boxShadow: 1
                }
            }}
        >
            <CardContent sx={{ pb: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                    <Typography variant="subtitle1" component="div" sx={{ fontWeight: 'medium' }}>
                        {data.name}
                    </Typography>
                </Box>
                <Chip 
                    label={data.type}
                    size="small"
                    color="primary"
                    variant="outlined"
                    sx={{ mb: 1 }}
                />
                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                    {data.durable ? 'Durable' : 'Non-durable'}
                    {data.autoDelete ? ' • Auto-delete' : ''}
                    {data.internal ? ' • Internal' : ''}
                </Typography>
            </CardContent>
            <Handle 
                type="target" 
                position={Position.Left}
                style={{ background: '#555' }}
            />
            <Handle 
                type="source" 
                position={Position.Right}
                style={{ background: '#555' }}
            />
        </Card>
    );
}); 