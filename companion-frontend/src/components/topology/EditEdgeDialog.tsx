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
    IconButton,
    Paper,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    SelectChangeEvent,
} from '@mui/material';
import { Add as AddIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { Edge } from '@reactflow/core';
import { Binding, ExchangeType } from '../../types/topology';

interface HeaderArgument {
    key: string;
    value: string;
    matchType: 'all' | 'any' | 'exact';
}

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
    const [headerArgs, setHeaderArgs] = useState<HeaderArgument[]>([]);

    useEffect(() => {
        if (edge?.data) {
            setFormData(edge.data as Partial<Binding>);
            setHeaderArgs(
                ((edge.data as Partial<Binding>).arguments?.headers as HeaderArgument[]) || []
            );
        } else {
            setFormData({});
            setHeaderArgs([]);
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

    const validateHeaders = (): boolean => {
        if (sourceExchangeType !== ExchangeType.Headers) return true;
        if (headerArgs.length === 0) {
            setValidationError('Headers exchange requires at least one header argument');
            return false;
        }
        if (headerArgs.some(h => !h.key || !h.value)) {
            setValidationError('All header arguments must have both key and value');
            return false;
        }
        return true;
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

    const handleAddHeader = () => {
        setHeaderArgs([...headerArgs, { key: '', value: '', matchType: 'all' }]);
    };

    const handleRemoveHeader = (index: number) => {
        setHeaderArgs(headerArgs.filter((_, i) => i !== index));
    };

    const handleHeaderTextChange = (index: number, field: 'key' | 'value') => (
        event: React.ChangeEvent<HTMLInputElement>
    ) => {
        const newHeaders = [...headerArgs];
        newHeaders[index] = {
            ...newHeaders[index],
            [field]: event.target.value,
        };
        setHeaderArgs(newHeaders);
    };

    const handleHeaderMatchTypeChange = (index: number) => (
        event: SelectChangeEvent
    ) => {
        const newHeaders = [...headerArgs];
        newHeaders[index] = {
            ...newHeaders[index],
            matchType: event.target.value as 'all' | 'any' | 'exact',
        };
        setHeaderArgs(newHeaders);
    };

    const handleSave = () => {
        if (edge && !validationError && validateHeaders()) {
            const data = {
                ...formData,
                arguments: {
                    ...formData.arguments,
                    headers: headerArgs,
                    'x-match': headerArgs.some(h => h.matchType === 'any') ? 'any' : 'all',
                },
            };
            onSave(edge, data);
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
                        disabled={sourceExchangeType === ExchangeType.Fanout || sourceExchangeType === ExchangeType.Headers}
                        fullWidth
                    />

                    {sourceExchangeType === ExchangeType.Headers && (
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Typography variant="subtitle2">Header Arguments</Typography>
                                <IconButton 
                                    size="small" 
                                    onClick={handleAddHeader}
                                    color="primary"
                                >
                                    <AddIcon />
                                </IconButton>
                            </Box>
                            
                            {headerArgs.map((header, index) => (
                                <Paper key={index} sx={{ p: 2 }} variant="outlined">
                                    <Box sx={{ display: 'flex', gap: 1, alignItems: 'flex-start' }}>
                                        <TextField
                                            label="Key"
                                            value={header.key}
                                            onChange={handleHeaderTextChange(index, 'key')}
                                            size="small"
                                            fullWidth
                                        />
                                        <TextField
                                            label="Value"
                                            value={header.value}
                                            onChange={handleHeaderTextChange(index, 'value')}
                                            size="small"
                                            fullWidth
                                        />
                                        <FormControl size="small" sx={{ minWidth: 120 }}>
                                            <InputLabel>Match</InputLabel>
                                            <Select
                                                value={header.matchType}
                                                onChange={handleHeaderMatchTypeChange(index)}
                                                label="Match"
                                            >
                                                <MenuItem value="all">All</MenuItem>
                                                <MenuItem value="any">Any</MenuItem>
                                                <MenuItem value="exact">Exact</MenuItem>
                                            </Select>
                                        </FormControl>
                                        <IconButton
                                            size="small"
                                            onClick={() => handleRemoveHeader(index)}
                                            color="error"
                                        >
                                            <DeleteIcon />
                                        </IconButton>
                                    </Box>
                                </Paper>
                            ))}
                        </Box>
                    )}

                    {validationError && (
                        <Alert severity="error">
                            {validationError}
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