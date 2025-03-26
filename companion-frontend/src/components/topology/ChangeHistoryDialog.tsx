import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  List,
  ListItem,
  ListItemText,
  Typography,
  Divider,
  Box,
} from '@mui/material';
import { TopologySnapshot } from '../../utils/historyManager';

interface ChangeHistoryDialogProps {
  open: boolean;
  onClose: () => void;
  history: TopologySnapshot[];
  currentIndex: number;
  onJumpToState: (index: number) => void;
}

export const ChangeHistoryDialog: React.FC<ChangeHistoryDialogProps> = ({
  open,
  onClose,
  history,
  currentIndex,
  onJumpToState,
}) => {
  // Generate change description for a snapshot
  const getChangeDescription = (snapshot: TopologySnapshot, index: number): string => {
    if (index === 0) {
      return 'Initial state';
    }
    
    const prevSnapshot = history[index - 1];
    
    // Compare node counts
    const prevNodeCount = prevSnapshot.nodes.length;
    const currentNodeCount = snapshot.nodes.length;
    
    // Compare edge counts
    const prevEdgeCount = prevSnapshot.edges.length;
    const currentEdgeCount = snapshot.edges.length;
    
    if (currentNodeCount > prevNodeCount) {
      return `Added node(s): ${currentNodeCount - prevNodeCount}`;
    } else if (currentNodeCount < prevNodeCount) {
      return `Removed node(s): ${prevNodeCount - currentNodeCount}`;
    } else if (currentEdgeCount > prevEdgeCount) {
      return `Added binding(s): ${currentEdgeCount - prevEdgeCount}`;
    } else if (currentEdgeCount < prevEdgeCount) {
      return `Removed binding(s): ${prevEdgeCount - currentEdgeCount}`;
    } else {
      // Node/edge counts unchanged, could be a modification or layout change
      return 'Modified properties or layout';
    }
  };

  // Format timestamp
  const formatTime = (timestamp: number): string => {
    return new Date(timestamp).toLocaleTimeString();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Change History</DialogTitle>
      <DialogContent>
        {history.length === 0 ? (
          <Typography variant="body1">No changes recorded yet.</Typography>
        ) : (
          <List>
            {history.map((snapshot, index) => (
              <React.Fragment key={index}>
                <ListItem 
                  onClick={() => onJumpToState(index)} 
                  sx={{ 
                    cursor: 'pointer',
                    backgroundColor: index === currentIndex ? 'rgba(0, 0, 0, 0.04)' : 'transparent'
                  }}
                >
                  <ListItemText
                    primary={
                      <Box display="flex" justifyContent="space-between">
                        <Typography variant="body1">
                          {getChangeDescription(snapshot, index)}
                        </Typography>
                        <Typography variant="body2" color="textSecondary">
                          {formatTime(snapshot.timestamp)}
                        </Typography>
                      </Box>
                    }
                    secondary={
                      <Typography variant="body2" color="textSecondary">
                        Nodes: {snapshot.nodes.length}, Bindings: {snapshot.edges.length}
                      </Typography>
                    }
                  />
                </ListItem>
                {index < history.length - 1 && <Divider />}
              </React.Fragment>
            ))}
          </List>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}; 