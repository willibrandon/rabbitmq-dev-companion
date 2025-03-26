import React, { useState, useRef } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Tabs,
  Tab,
  Box,
  TextField,
  Checkbox,
  FormControlLabel,
  Typography,
  LinearProgress,
  Alert,
  FormGroup,
  InputAdornment,
  IconButton,
} from '@mui/material';
import {
  FileUpload as FileUploadIcon,
  Download as DownloadIcon,
  ContentCopy as CopyIcon,
  Code as CodeIcon,
} from '@mui/icons-material';

import { ConfigurationOptions, ConfigurationOutput } from '../../services/api';
import { Topology } from '../../types/topology';
import { downloadTopologyAsJson, readTopologyFromJsonFile } from '../../utils/topologyConverter';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`tabpanel-${index}`}
      aria-labelledby={`tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ p: 2 }}>{children}</Box>}
    </div>
  );
}

interface ExportImportDialogProps {
  open: boolean;
  onClose: () => void;
  topology: Topology;
  onImportTopology: (topology: Topology) => void;
  onGenerateConfiguration: (options: ConfigurationOptions) => Promise<ConfigurationOutput>;
}

export const ExportImportDialog: React.FC<ExportImportDialogProps> = ({
  open,
  onClose,
  topology,
  onImportTopology,
  onGenerateConfiguration,
}) => {
  const [tabValue, setTabValue] = useState(0);
  const [topologyName, setTopologyName] = useState(topology.name || 'My Topology');
  const [topologyDescription, setTopologyDescription] = useState(
    topology.description || 'Created with RabbitMQ Developer Companion'
  );
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);
  const [isGenerating, setIsGenerating] = useState(false);
  const [configOutput, setConfigOutput] = useState<ConfigurationOutput | null>(null);
  const [configOptions, setConfigOptions] = useState<ConfigurationOptions>({
    includeDockerCompose: true,
    includeKubernetes: false,
    includeDotNetCode: true,
    includeProducerCode: true,
    includeConsumerCode: true,
  });

  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleTabChange = (event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleExportJson = () => {
    const topologyToExport = {
      ...topology,
      name: topologyName,
      description: topologyDescription,
    };
    downloadTopologyAsJson(topologyToExport);
  };

  const handleFileSelection = (event: React.ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (files && files.length > 0) {
      setSelectedFile(files[0]);
      setFileError(null);
    }
  };

  const handleImport = async () => {
    if (!selectedFile) {
      setFileError('Please select a file to import');
      return;
    }

    try {
      const importedTopology = await readTopologyFromJsonFile(selectedFile);
      onImportTopology(importedTopology);
      onClose();
    } catch (err) {
      setFileError((err as Error).message || 'Failed to import topology');
    }
  };

  const handleGenerateConfig = async () => {
    try {
      setIsGenerating(true);
      const result = await onGenerateConfiguration({
        ...configOptions,
      });
      setConfigOutput(result);
      setIsGenerating(false);
    } catch (err) {
      console.error('Failed to generate configuration:', err);
      setIsGenerating(false);
    }
  };

  const handleCopyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>Export & Import</DialogTitle>
      <DialogContent dividers>
        <Tabs value={tabValue} onChange={handleTabChange} aria-label="export-import-tabs">
          <Tab label="Export JSON" />
          <Tab label="Import JSON" />
          <Tab label="Generate Config" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <Typography variant="subtitle2" gutterBottom>
            Export current topology to a JSON file
          </Typography>
          <TextField
            label="Topology Name"
            fullWidth
            margin="normal"
            value={topologyName}
            onChange={(e) => setTopologyName(e.target.value)}
          />
          <TextField
            label="Description"
            fullWidth
            margin="normal"
            multiline
            rows={2}
            value={topologyDescription}
            onChange={(e) => setTopologyDescription(e.target.value)}
          />
          <Box mt={2} display="flex" justifyContent="flex-end">
            <Button
              variant="contained"
              startIcon={<DownloadIcon />}
              onClick={handleExportJson}
              disabled={!topologyName}
            >
              Export JSON
            </Button>
          </Box>
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Typography variant="subtitle2" gutterBottom>
            Import topology from a JSON file
          </Typography>
          <Box
            sx={{
              border: '1px dashed grey',
              borderRadius: 1,
              p: 3,
              textAlign: 'center',
              mb: 2,
              cursor: 'pointer',
              '&:hover': { bgcolor: 'rgba(0,0,0,0.05)' },
            }}
            onClick={() => fileInputRef.current?.click()}
          >
            <input
              type="file"
              accept=".json"
              style={{ display: 'none' }}
              ref={fileInputRef}
              onChange={handleFileSelection}
            />
            <FileUploadIcon sx={{ fontSize: 40, color: 'action.active', mb: 1 }} />
            <Typography>
              {selectedFile ? selectedFile.name : 'Click to select JSON file or drop it here'}
            </Typography>
          </Box>
          {fileError && <Alert severity="error">{fileError}</Alert>}
          <Box mt={2} display="flex" justifyContent="flex-end">
            <Button
              variant="contained"
              onClick={handleImport}
              disabled={!selectedFile}
            >
              Import
            </Button>
          </Box>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Typography variant="subtitle2" gutterBottom>
            Generate configuration files from your topology
          </Typography>
          <FormGroup sx={{ mb: 2 }}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={configOptions.includeDockerCompose}
                  onChange={(e) =>
                    setConfigOptions({ ...configOptions, includeDockerCompose: e.target.checked })
                  }
                />
              }
              label="Docker Compose"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={configOptions.includeKubernetes}
                  onChange={(e) =>
                    setConfigOptions({ ...configOptions, includeKubernetes: e.target.checked })
                  }
                />
              }
              label="Kubernetes YAML"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={configOptions.includeDotNetCode}
                  onChange={(e) =>
                    setConfigOptions({ ...configOptions, includeDotNetCode: e.target.checked })
                  }
                />
              }
              label=".NET Code Samples"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={configOptions.includeProducerCode}
                  onChange={(e) =>
                    setConfigOptions({ ...configOptions, includeProducerCode: e.target.checked })
                  }
                />
              }
              label="Producer Code"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={configOptions.includeConsumerCode}
                  onChange={(e) =>
                    setConfigOptions({ ...configOptions, includeConsumerCode: e.target.checked })
                  }
                />
              }
              label="Consumer Code"
            />
          </FormGroup>

          <Box mt={2} display="flex" justifyContent="flex-end">
            <Button
              variant="contained"
              startIcon={<CodeIcon />}
              onClick={handleGenerateConfig}
              disabled={isGenerating}
            >
              Generate Configuration
            </Button>
          </Box>

          {isGenerating && <LinearProgress sx={{ mt: 2 }} />}

          {configOutput && (
            <Box mt={3}>
              <Typography variant="h6" gutterBottom>
                Configuration Files
              </Typography>

              {configOutput.dockerComposeYaml && (
                <Box mt={2}>
                  <Typography variant="subtitle2" gutterBottom>
                    Docker Compose
                    <IconButton
                      size="small"
                      onClick={() => handleCopyToClipboard(configOutput.dockerComposeYaml!)}
                      sx={{ ml: 1 }}
                    >
                      <CopyIcon fontSize="small" />
                    </IconButton>
                  </Typography>
                  <TextField
                    fullWidth
                    multiline
                    rows={8}
                    value={configOutput.dockerComposeYaml}
                    InputProps={{ readOnly: true }}
                    variant="outlined"
                    size="small"
                  />
                </Box>
              )}

              {configOutput.kubernetesYaml && (
                <Box mt={2}>
                  <Typography variant="subtitle2" gutterBottom>
                    Kubernetes YAML
                    <IconButton
                      size="small"
                      onClick={() => handleCopyToClipboard(configOutput.kubernetesYaml!)}
                      sx={{ ml: 1 }}
                    >
                      <CopyIcon fontSize="small" />
                    </IconButton>
                  </Typography>
                  <TextField
                    fullWidth
                    multiline
                    rows={8}
                    value={configOutput.kubernetesYaml}
                    InputProps={{ readOnly: true }}
                    variant="outlined"
                    size="small"
                  />
                </Box>
              )}

              {configOutput.dotNetCode && (
                <Box mt={2}>
                  <Typography variant="subtitle2" gutterBottom>
                    .NET Code
                    <IconButton
                      size="small"
                      onClick={() => handleCopyToClipboard(configOutput.dotNetCode!)}
                      sx={{ ml: 1 }}
                    >
                      <CopyIcon fontSize="small" />
                    </IconButton>
                  </Typography>
                  <TextField
                    fullWidth
                    multiline
                    rows={8}
                    value={configOutput.dotNetCode}
                    InputProps={{ readOnly: true }}
                    variant="outlined"
                    size="small"
                  />
                </Box>
              )}
            </Box>
          )}
        </TabPanel>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Close</Button>
      </DialogActions>
    </Dialog>
  );
}; 