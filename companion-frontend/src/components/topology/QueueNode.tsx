import React, { memo } from 'react';
import { Handle, Position } from '@reactflow/core';
import { Card, CardContent, Typography, Box } from '@mui/material';
import { Queue } from '../../types/topology';

interface QueueNodeProps {
    data: Queue;
    selected: boolean;
}

export const QueueNode: React.FC<QueueNodeProps> = memo(({ data, selected }) => {
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
                <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem', mb: 1 }}>
                    {data.durable ? 'Durable' : 'Non-durable'}
                    {data.autoDelete ? ' • Auto-delete' : ''}
                    {data.exclusive ? ' • Exclusive' : ''}
                </Typography>
                {(data.maxLength || data.messageTtl || data.deadLetterExchange) && (
                    <Box sx={{ mt: 1 }}>
                        {data.maxLength && (
                            <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                Max Length: {data.maxLength}
                            </Typography>
                        )}
                        {data.messageTtl && (
                            <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                TTL: {data.messageTtl}ms
                            </Typography>
                        )}
                        {data.deadLetterExchange && (
                            <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                                DLX: {data.deadLetterExchange}
                            </Typography>
                        )}
                    </Box>
                )}
            </CardContent>
            <Handle 
                type="target" 
                position={Position.Left}
                style={{ background: '#555' }}
            />
        </Card>
    );
}); 