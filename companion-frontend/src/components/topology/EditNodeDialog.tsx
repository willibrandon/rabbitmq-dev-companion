import React, { useState, useEffect } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    FormControlLabel,
    Switch,
    MenuItem,
    Box,
} from '@mui/material';
import { Exchange, Queue, NodeType, ExchangeType } from '../../types/topology';

interface EditNodeDialogProps {
    open: boolean;
    onClose: () => void;
    onSave: (type: NodeType, data: Partial<Exchange | Queue>) => void;
    nodeType: NodeType;
    initialData?: Partial<Exchange | Queue>;
}

export const EditNodeDialog: React.FC<EditNodeDialogProps> = ({
    open,
    onClose,
    onSave,
    nodeType,
    initialData,
}) => {
    const [formData, setFormData] = useState<Partial<Exchange | Queue>>(initialData || {});

    useEffect(() => {
        setFormData(initialData || {});
    }, [initialData]);

    const handleSave = () => {
        onSave(nodeType, formData);
        onClose();
    };

    const handleChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.type === 'checkbox' ? event.target.checked : event.target.value;
        setFormData((prev) => ({ ...prev, [field]: value }));
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                Edit {nodeType === NodeType.Exchange ? 'Exchange' : 'Queue'}
            </DialogTitle>
            <DialogContent>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                    <TextField
                        label="Name"
                        value={formData.name || ''}
                        onChange={handleChange('name')}
                        fullWidth
                    />

                    {nodeType === NodeType.Exchange && (
                        <>
                            <TextField
                                select
                                label="Type"
                                value={(formData as Exchange).type || ExchangeType.Direct}
                                onChange={handleChange('type')}
                                fullWidth
                            >
                                {Object.values(ExchangeType).map((type) => (
                                    <MenuItem key={type} value={type}>
                                        {type}
                                    </MenuItem>
                                ))}
                            </TextField>
                            <FormControlLabel
                                control={
                                    <Switch
                                        checked={(formData as Exchange).internal || false}
                                        onChange={handleChange('internal')}
                                    />
                                }
                                label="Internal"
                            />
                        </>
                    )}

                    {nodeType === NodeType.Queue && (
                        <>
                            <TextField
                                label="Max Length"
                                type="number"
                                value={(formData as Queue).maxLength || ''}
                                onChange={handleChange('maxLength')}
                                fullWidth
                            />
                            <TextField
                                label="Message TTL (ms)"
                                type="number"
                                value={(formData as Queue).messageTtl || ''}
                                onChange={handleChange('messageTtl')}
                                fullWidth
                            />
                            <TextField
                                label="Dead Letter Exchange"
                                value={(formData as Queue).deadLetterExchange || ''}
                                onChange={handleChange('deadLetterExchange')}
                                fullWidth
                            />
                            <TextField
                                label="Dead Letter Routing Key"
                                value={(formData as Queue).deadLetterRoutingKey || ''}
                                onChange={handleChange('deadLetterRoutingKey')}
                                fullWidth
                            />
                        </>
                    )}

                    <FormControlLabel
                        control={
                            <Switch
                                checked={formData.durable || false}
                                onChange={handleChange('durable')}
                            />
                        }
                        label="Durable"
                    />
                    <FormControlLabel
                        control={
                            <Switch
                                checked={formData.autoDelete || false}
                                onChange={handleChange('autoDelete')}
                            />
                        }
                        label="Auto Delete"
                    />
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button onClick={handleSave} variant="contained" color="primary">
                    Save
                </Button>
            </DialogActions>
        </Dialog>
    );
}; 