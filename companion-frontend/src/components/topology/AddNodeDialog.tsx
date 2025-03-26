import React, { useState } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    FormControlLabel,
    Checkbox,
    Box,
} from '@mui/material';
import { ExchangeType, NodeType } from '../../types/topology';

interface AddNodeDialogProps {
    open: boolean;
    onClose: () => void;
    onAdd: (type: NodeType, data: any) => void;
}

export const AddNodeDialog: React.FC<AddNodeDialogProps> = ({ open, onClose, onAdd }) => {
    const [nodeType, setNodeType] = useState<NodeType>(NodeType.Exchange);
    const [name, setName] = useState('');
    const [exchangeType, setExchangeType] = useState<ExchangeType>(ExchangeType.Direct);
    const [durable, setDurable] = useState(true);
    const [autoDelete, setAutoDelete] = useState(false);
    const [internal, setInternal] = useState(false);
    const [exclusive, setExclusive] = useState(false);

    const handleSubmit = () => {
        if (!name) return;

        if (nodeType === NodeType.Exchange) {
            onAdd(NodeType.Exchange, {
                name,
                type: exchangeType,
                durable,
                autoDelete,
                internal,
            });
        } else {
            onAdd(NodeType.Queue, {
                name,
                durable,
                autoDelete,
                exclusive,
            });
        }

        handleClose();
    };

    const handleClose = () => {
        setName('');
        setExchangeType(ExchangeType.Direct);
        setDurable(true);
        setAutoDelete(false);
        setInternal(false);
        setExclusive(false);
        onClose();
    };

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>Add {nodeType === NodeType.Exchange ? 'Exchange' : 'Queue'}</DialogTitle>
            <DialogContent>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                    <FormControl fullWidth>
                        <InputLabel>Node Type</InputLabel>
                        <Select
                            value={nodeType}
                            label="Node Type"
                            onChange={(e) => setNodeType(e.target.value as NodeType)}
                        >
                            <MenuItem value={NodeType.Exchange}>Exchange</MenuItem>
                            <MenuItem value={NodeType.Queue}>Queue</MenuItem>
                        </Select>
                    </FormControl>

                    <TextField
                        label="Name"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        fullWidth
                        required
                    />

                    {nodeType === NodeType.Exchange && (
                        <FormControl fullWidth>
                            <InputLabel>Exchange Type</InputLabel>
                            <Select
                                value={exchangeType}
                                label="Exchange Type"
                                onChange={(e) => setExchangeType(e.target.value as ExchangeType)}
                            >
                                <MenuItem value={ExchangeType.Direct}>Direct</MenuItem>
                                <MenuItem value={ExchangeType.Fanout}>Fanout</MenuItem>
                                <MenuItem value={ExchangeType.Topic}>Topic</MenuItem>
                                <MenuItem value={ExchangeType.Headers}>Headers</MenuItem>
                            </Select>
                        </FormControl>
                    )}

                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={durable}
                                onChange={(e) => setDurable(e.target.checked)}
                            />
                        }
                        label="Durable"
                    />

                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={autoDelete}
                                onChange={(e) => setAutoDelete(e.target.checked)}
                            />
                        }
                        label="Auto Delete"
                    />

                    {nodeType === NodeType.Exchange && (
                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={internal}
                                    onChange={(e) => setInternal(e.target.checked)}
                                />
                            }
                            label="Internal"
                        />
                    )}

                    {nodeType === NodeType.Queue && (
                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={exclusive}
                                    onChange={(e) => setExclusive(e.target.checked)}
                                />
                            }
                            label="Exclusive"
                        />
                    )}
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose}>Cancel</Button>
                <Button onClick={handleSubmit} variant="contained" disabled={!name}>
                    Add
                </Button>
            </DialogActions>
        </Dialog>
    );
}; 