import React, { useState, useRef, useEffect } from 'react';
import {
  Box,
  Button,
  ButtonGroup,
  Tooltip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  TextField,
  Typography,
  Badge,
} from '@mui/material';
import {
  Undo as UndoIcon,
  Redo as RedoIcon,
  Save as SaveIcon,
  Drafts as DraftsIcon,
  Delete as DeleteIcon,
  History as HistoryIcon,
} from '@mui/icons-material';
import { TopologyDraft, TopologySnapshot } from '../../utils/historyManager';
import { ChangeHistoryDialog } from './ChangeHistoryDialog';

interface HistoryControlsProps {
  canUndo: boolean;
  canRedo: boolean;
  onUndo: () => void;
  onRedo: () => void;
  onSaveDraft: (name: string) => void;
  onLoadDraft: (draftId: string) => void;
  onDeleteDraft: (draftId: string) => void;
  onJumpToState?: (index: number) => void;
  drafts: TopologyDraft[];
  history?: TopologySnapshot[];
  currentHistoryIndex?: number;
}

export const HistoryControls: React.FC<HistoryControlsProps> = ({
  canUndo,
  canRedo,
  onUndo,
  onRedo,
  onSaveDraft,
  onLoadDraft,
  onDeleteDraft,
  onJumpToState,
  drafts,
  history = [],
  currentHistoryIndex = -1,
}) => {
  const [saveDraftDialogOpen, setSaveDraftDialogOpen] = useState(false);
  const [draftDialogOpen, setDraftDialogOpen] = useState(false);
  const [historyDialogOpen, setHistoryDialogOpen] = useState(false);
  const [draftName, setDraftName] = useState('');
  
  // Refs for handling click and hold
  const undoTimerRef = useRef<number | null>(null);
  const redoTimerRef = useRef<number | null>(null);
  const holdDelayRef = useRef<number>(500); // Initial delay before rapid actions
  const holdIntervalRef = useRef<number>(100); // Interval between actions when holding
  
  // Counters for undo/redo operations
  const [undoCount, setUndoCount] = useState(0);
  const [redoCount, setRedoCount] = useState(0);
  const [isHoldingUndo, setIsHoldingUndo] = useState(false);
  const [isHoldingRedo, setIsHoldingRedo] = useState(false);

  // Reset counters after a short delay when operation stops
  const resetCountTimerRef = useRef<number | null>(null);
  
  const handleSaveDraft = () => {
    if (draftName.trim()) {
      onSaveDraft(draftName);
      setSaveDraftDialogOpen(false);
      setDraftName('');
    }
  };

  const formatDate = (timestamp: number) => {
    return new Date(timestamp).toLocaleString();
  };

  // Clean up any active timers when component unmounts
  useEffect(() => {
    return () => {
      if (undoTimerRef.current) {
        window.clearTimeout(undoTimerRef.current);
      }
      if (redoTimerRef.current) {
        window.clearTimeout(redoTimerRef.current);
      }
      if (resetCountTimerRef.current) {
        window.clearTimeout(resetCountTimerRef.current);
      }
    };
  }, []);

  // Reset counters after a delay
  const scheduleCounterReset = () => {
    if (resetCountTimerRef.current) {
      window.clearTimeout(resetCountTimerRef.current);
    }
    resetCountTimerRef.current = window.setTimeout(() => {
      setUndoCount(0);
      setRedoCount(0);
      setIsHoldingUndo(false);
      setIsHoldingRedo(false);
    }, 1000);
  };

  // Handle mouse down for undo button
  const handleUndoMouseDown = () => {
    if (!canUndo) return;
    
    // Trigger the first undo immediately
    onUndo();
    setUndoCount(1);
    setIsHoldingUndo(true);
    
    // Set up timer for continued undos if button is held
    undoTimerRef.current = window.setTimeout(() => {
      // Start rapid undos after initial delay
      const rapidUndo = () => {
        if (canUndo) {
          onUndo();
          setUndoCount(count => count + 1);
          undoTimerRef.current = window.setTimeout(rapidUndo, holdIntervalRef.current);
        } else {
          handleUndoMouseUp();
        }
      };
      rapidUndo();
    }, holdDelayRef.current);
  };

  // Handle mouse down for redo button
  const handleRedoMouseDown = () => {
    if (!canRedo) return;
    
    // Trigger the first redo immediately
    onRedo();
    setRedoCount(1);
    setIsHoldingRedo(true);
    
    // Set up timer for continued redos if button is held
    redoTimerRef.current = window.setTimeout(() => {
      // Start rapid redos after initial delay
      const rapidRedo = () => {
        if (canRedo) {
          onRedo();
          setRedoCount(count => count + 1);
          redoTimerRef.current = window.setTimeout(rapidRedo, holdIntervalRef.current);
        } else {
          handleRedoMouseUp();
        }
      };
      rapidRedo();
    }, holdDelayRef.current);
  };

  // Handle mouse up/leave for undo button
  const handleUndoMouseUp = () => {
    if (undoTimerRef.current) {
      window.clearTimeout(undoTimerRef.current);
      undoTimerRef.current = null;
    }
    scheduleCounterReset();
  };

  // Handle mouse up/leave for redo button
  const handleRedoMouseUp = () => {
    if (redoTimerRef.current) {
      window.clearTimeout(redoTimerRef.current);
      redoTimerRef.current = null;
    }
    scheduleCounterReset();
  };

  return (
    <Box 
      sx={{ 
        position: 'absolute', 
        top: 16, 
        left: 16, 
        zIndex: 10, 
        display: 'flex',
        flexDirection: 'row',
        gap: 1,
        backgroundColor: 'rgba(255, 255, 255, 0.8)',
        padding: 0.5,
        borderRadius: 1
      }}
    >
      <Tooltip title="Undo (Click and hold for multiple)">
        <span>
          <Badge 
            badgeContent={undoCount > 1 ? undoCount : 0} 
            color="primary"
            sx={{ 
              '.MuiBadge-badge': { 
                opacity: isHoldingUndo ? 1 : 0,
                transition: 'opacity 0.2s'
              } 
            }}
          >
            <IconButton 
              size="small"
              onMouseDown={handleUndoMouseDown}
              onMouseUp={handleUndoMouseUp}
              onMouseLeave={handleUndoMouseUp}
              disabled={!canUndo}
              color="primary"
            >
              <UndoIcon />
            </IconButton>
          </Badge>
        </span>
      </Tooltip>
      
      <Tooltip title="Redo (Click and hold for multiple)">
        <span>
          <Badge 
            badgeContent={redoCount > 1 ? redoCount : 0} 
            color="primary"
            sx={{ 
              '.MuiBadge-badge': { 
                opacity: isHoldingRedo ? 1 : 0,
                transition: 'opacity 0.2s'
              } 
            }}
          >
            <IconButton 
              size="small"
              onMouseDown={handleRedoMouseDown}
              onMouseUp={handleRedoMouseUp}
              onMouseLeave={handleRedoMouseUp}
              disabled={!canRedo}
              color="primary"
            >
              <RedoIcon />
            </IconButton>
          </Badge>
        </span>
      </Tooltip>
      
      <Tooltip title="History">
        <span>
          <IconButton 
            size="small"
            onClick={() => setHistoryDialogOpen(true)}
            color="primary"
            disabled={history.length === 0}
          >
            <HistoryIcon />
          </IconButton>
        </span>
      </Tooltip>
      
      <Tooltip title="Save Draft">
        <IconButton 
          size="small"
          onClick={() => setSaveDraftDialogOpen(true)}
          color="primary"
        >
          <SaveIcon />
        </IconButton>
      </Tooltip>
      
      <Tooltip title="Manage Drafts">
        <IconButton 
          size="small"
          onClick={() => setDraftDialogOpen(true)}
          color="primary"
        >
          <DraftsIcon />
        </IconButton>
      </Tooltip>

      {/* Save Draft Dialog */}
      <Dialog 
        open={saveDraftDialogOpen} 
        onClose={() => setSaveDraftDialogOpen(false)}
        maxWidth="xs"
        fullWidth
      >
        <DialogTitle>Save Draft</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Draft Name"
            fullWidth
            value={draftName}
            onChange={(e) => setDraftName(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSaveDraftDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleSaveDraft} variant="contained" color="primary">
            Save
          </Button>
        </DialogActions>
      </Dialog>

      {/* Drafts Dialog */}
      <Dialog 
        open={draftDialogOpen} 
        onClose={() => setDraftDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Saved Drafts</DialogTitle>
        <DialogContent>
          {drafts.length === 0 ? (
            <Typography variant="body1" sx={{ py: 2 }}>
              No drafts saved yet.
            </Typography>
          ) : (
            <List>
              {drafts.map((draft) => (
                <ListItem 
                  key={draft.id}
                  onClick={() => {
                    onLoadDraft(draft.id);
                    setDraftDialogOpen(false);
                  }}
                  sx={{ cursor: 'pointer' }}
                >
                  <ListItemText 
                    primary={draft.name} 
                    secondary={`Last updated: ${formatDate(draft.updatedAt)}`} 
                  />
                  <ListItemSecondaryAction>
                    <IconButton 
                      edge="end" 
                      onClick={(e) => {
                        e.stopPropagation();
                        onDeleteDraft(draft.id);
                      }}
                    >
                      <DeleteIcon />
                    </IconButton>
                  </ListItemSecondaryAction>
                </ListItem>
              ))}
            </List>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDraftDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Change History Dialog */}
      {onJumpToState && (
        <ChangeHistoryDialog 
          open={historyDialogOpen}
          onClose={() => setHistoryDialogOpen(false)}
          history={history}
          currentIndex={currentHistoryIndex}
          onJumpToState={onJumpToState}
        />
      )}
    </Box>
  );
}; 