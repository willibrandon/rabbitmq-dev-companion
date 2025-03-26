import React, { useState } from 'react';
import {
  Box,
  Button,
  ButtonGroup,
  Tooltip,
  Switch,
  FormControlLabel,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  Typography,
} from '@mui/material';
import {
  AutoFixHigh as AutoLayoutIcon,
  GridOn as GridIcon,
  Save as SaveIcon,
  Restore as RestoreIcon,
  Settings as SettingsIcon,
} from '@mui/icons-material';
import { LayoutOptions } from '../../utils/layoutManagement';

interface LayoutControlsProps {
  onAutoLayout: () => void;
  onToggleSnapToGrid: (enabled: boolean) => void;
  onSaveLayout: () => void;
  onRestoreLayout: () => void;
  onUpdateLayoutOptions: (options: LayoutOptions) => void;
  isSnapToGridEnabled: boolean;
  layoutOptions: LayoutOptions;
}

export const LayoutControls: React.FC<LayoutControlsProps> = ({
  onAutoLayout,
  onToggleSnapToGrid,
  onSaveLayout,
  onRestoreLayout,
  onUpdateLayoutOptions,
  isSnapToGridEnabled,
  layoutOptions,
}) => {
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [tempLayoutOptions, setTempLayoutOptions] = useState<LayoutOptions>(layoutOptions);

  const handleToggleSnapToGrid = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    onToggleSnapToGrid(event.target.checked);
  };

  const handleOpenSettings = () => {
    setTempLayoutOptions(layoutOptions);
    setSettingsOpen(true);
  };

  const handleCloseSettings = () => {
    setSettingsOpen(false);
  };

  const handleSaveSettings = () => {
    onUpdateLayoutOptions(tempLayoutOptions);
    setSettingsOpen(false);
  };

  const handleLayoutOptionChange = (
    field: keyof LayoutOptions,
    value: any
  ) => {
    setTempLayoutOptions((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  return (
    <>
      <Box
        sx={{
          position: 'absolute',
          top: 10,
          right: 10,
          zIndex: 10,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'flex-end',
          gap: 1,
        }}
      >
        <ButtonGroup variant="contained" size="small">
          <Tooltip title="Auto Layout">
            <Button onClick={onAutoLayout} startIcon={<AutoLayoutIcon />}>
              Auto Layout
            </Button>
          </Tooltip>
          <Tooltip title="Layout Settings">
            <Button onClick={handleOpenSettings}>
              <SettingsIcon />
            </Button>
          </Tooltip>
        </ButtonGroup>

        <ButtonGroup variant="contained" size="small">
          <Tooltip title="Save Layout">
            <Button onClick={onSaveLayout} startIcon={<SaveIcon />}>
              Save
            </Button>
          </Tooltip>
          <Tooltip title="Restore Layout">
            <Button onClick={onRestoreLayout} startIcon={<RestoreIcon />}>
              Restore
            </Button>
          </Tooltip>
        </ButtonGroup>

        <FormControlLabel
          control={
            <Switch
              checked={isSnapToGridEnabled}
              onChange={handleToggleSnapToGrid}
              color="primary"
              size="small"
            />
          }
          label={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <GridIcon fontSize="small" />
              <Typography variant="body2">Snap to Grid</Typography>
            </Box>
          }
          sx={{ bgcolor: 'background.paper', borderRadius: 1, px: 1 }}
        />
      </Box>

      <Dialog open={settingsOpen} onClose={handleCloseSettings}>
        <DialogTitle>Layout Settings</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1 }}>
            <FormControl fullWidth>
              <InputLabel>Direction</InputLabel>
              <Select
                value={tempLayoutOptions.direction || 'LR'}
                label="Direction"
                onChange={(e) =>
                  handleLayoutOptionChange('direction', e.target.value)
                }
              >
                <MenuItem value="LR">Left to Right</MenuItem>
                <MenuItem value="RL">Right to Left</MenuItem>
                <MenuItem value="TB">Top to Bottom</MenuItem>
                <MenuItem value="BT">Bottom to Top</MenuItem>
              </Select>
            </FormControl>

            <TextField
              label="Node Separation"
              type="number"
              value={tempLayoutOptions.nodeSeparation || 100}
              onChange={(e) =>
                handleLayoutOptionChange(
                  'nodeSeparation',
                  parseInt(e.target.value, 10)
                )
              }
              fullWidth
            />

            <TextField
              label="Rank Separation"
              type="number"
              value={tempLayoutOptions.rankSeparation || 200}
              onChange={(e) =>
                handleLayoutOptionChange(
                  'rankSeparation',
                  parseInt(e.target.value, 10)
                )
              }
              fullWidth
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseSettings}>Cancel</Button>
          <Button onClick={handleSaveSettings} variant="contained">
            Apply
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}; 