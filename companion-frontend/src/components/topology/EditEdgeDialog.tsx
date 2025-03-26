import React, { useState, useEffect } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Box,
    Typography,
    Alert,
} from '@mui/material';
import { Edge } from '@reactflow/core';
import { Binding, ExchangeType } from '../../types/topology';

interface EditEdgeDialogProps {
    open: boolean;
    onClose: () => void;
    onSave: (edge: Edge, data: Partial<Binding>) => void;
    edge: Edge | null;
    sourceExchangeType?: ExchangeType;
}

export const EditEdgeDialog: React.FC<EditEdgeDialogProps> = ({
    open,
    onClose,
    onSave,
    edge,
    sourceExchangeType,
}) => {
    const [formData, setFormData] = useState<Partial<Binding>>({});
    const [validationError, setValidationError] = useState<string | null>(null);

    useEffect(() => {
        if (edge?.data) {
            setFormData(edge.data as Partial<Binding>);
        } else {
            setFormData({});
        }
        setValidationError(null);
    }, [edge]);

    const validateRoutingKey = (routingKey: string): boolean => {
        if (!sourceExchangeType) return true;

        switch (sourceExchangeType) {
            case ExchangeType.Direct:
                // Direct exchanges require an exact routing key
                return routingKey.length > 0;
            case ExchangeType.Topic:
                // Topic exchanges use dot-separated words with * and # wildcards
                const validTopicPattern = /^([a-zA-Z0-9_-]+|\*|#)(\.[a-zA-Z0-9_-]+|\*|#)*$/;
                return validTopicPattern.test(routingKey);
            case ExchangeType.Headers:
                // Headers exchange doesn't use routing keys
                return true;
            case ExchangeType.Fanout:
                // Fanout exchanges ignore routing keys
                return true;
            default:
                return true;
        }
    };

    const handleChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;
        setFormData((prev) => ({ ...prev, [field]: value }));

        if (field === 'routingKey') {
            if (!validateRoutingKey(value)) {
                setValidationError('Invalid routing key format for this exchange type');
            } else {
                setValidationError(null);
            }
        }
    };

    const handleSave = () => {
        if (edge && !validationError) {
            onSave(edge, formData);
            onClose();
        }
    };

    const getRoutingKeyHelperText = (): string => {
        if (!sourceExchangeType) return '';

        switch (sourceExchangeType) {
            case ExchangeType.Direct:
                return 'Enter an exact routing key';
            case ExchangeType.Topic:
                return 'Use dot-separated words. * matches one word, # matches zero or more words';
            case ExchangeType.Headers:
                return 'Headers exchange uses header values instead of routing keys';
            case ExchangeType.Fanout:
                return 'Fanout exchange broadcasts to all queues, routing key is ignored';
            default:
                return '';
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>Edit Binding</DialogTitle>
            <DialogContent>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                    {sourceExchangeType && (
                        <Typography variant="body2" color="textSecondary">
                            Exchange Type: {sourceExchangeType}
                        </Typography>
                    )}

                    <TextField
                        label="Routing Key"
                        value={formData.routingKey || ''}
                        onChange={handleChange('routingKey')}
                        helperText={getRoutingKeyHelperText()}
                        error={!!validationError}
                        disabled={sourceExchangeType === ExchangeType.Fanout}
                        fullWidth
                    />

                    {validationError && (
                        <Alert severity="error">
                            {validationError}
                        </Alert>
                    )}

                    {/* TODO: Add header arguments editor for Headers exchange type */}
                    {sourceExchangeType === ExchangeType.Headers && (
                        <Alert severity="info">
                            Header arguments editor coming soon
                        </Alert>
                    )}
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button 
                    onClick={handleSave}
                    variant="contained"
                    color="primary"
                    disabled={!!validationError}
                >
                    Save
                </Button>
            </DialogActions>
        </Dialog>
    );
}; 