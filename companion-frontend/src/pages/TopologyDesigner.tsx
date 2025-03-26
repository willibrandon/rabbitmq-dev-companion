import React, { useCallback, useState, useMemo } from 'react';
import { ReactFlow, Node, Edge, Connection, useNodesState, useEdgesState, addEdge } from '@reactflow/core';
import { Background } from '@reactflow/background';
import { Controls } from '@reactflow/controls';
import { MiniMap } from '@reactflow/minimap';
import { Box, Paper, Alert, Snackbar, Fab, Tooltip } from '@mui/material';
import { Add as AddIcon, CloudDownload as CloudDownloadIcon } from '@mui/icons-material';
import { ExchangeNode } from '../components/topology/ExchangeNode';
import { QueueNode } from '../components/topology/QueueNode';
import { AddNodeDialog } from '../components/topology/AddNodeDialog';
import { EditNodeDialog } from '../components/topology/EditNodeDialog';
import { EditEdgeDialog } from '../components/topology/EditEdgeDialog';
import { getEdgeStyle } from '../components/topology/EdgeStyles';
import { topologyApi } from '../services/api';
import { NodeType, Topology, Exchange, Queue, Binding, ExchangeType } from '../types/topology';

// Import ReactFlow styles
import '@reactflow/core/dist/style.css';
import '@reactflow/controls/dist/style.css';
import '@reactflow/minimap/dist/style.css';

// Define nodeTypes outside of component to prevent recreation on each render
const NODE_TYPES = {
    [NodeType.Exchange]: ExchangeNode,
    [NodeType.Queue]: QueueNode,
};

const INITIAL_VIEWPORT = { x: 0, y: 0, zoom: 1.5 };

// Custom edge with tooltip
const CustomEdge = ({
    id,
    sourceX,
    sourceY,
    targetX,
    targetY,
    sourcePosition,
    targetPosition,
    style = {},
    data,
    markerEnd,
}: any) => {
    const edgePath = `M${sourceX},${sourceY}L${targetX},${targetY}`;
    const midX = (sourceX + targetX) / 2;
    const midY = (sourceY + targetY) / 2;

    return (
        <>
            {data?.tooltipContent && (
                <Tooltip 
                    title={<pre style={{ margin: 0 }}>{data.tooltipContent}</pre>}
                    placement="top"
                >
                    <g>
                        <path
                            id={id}
                            style={style}
                            className="react-flow__edge-path"
                            d={edgePath}
                            markerEnd={markerEnd}
                        />
                        {/* Invisible wider path for better hover area */}
                        <path
                            style={{ 
                                stroke: 'transparent',
                                strokeWidth: 20,
                                fill: 'none',
                                cursor: 'pointer'
                            }}
                            d={edgePath}
                        />
                    </g>
                </Tooltip>
            )}
            {!data?.tooltipContent && (
                <path
                    id={id}
                    style={style}
                    className="react-flow__edge-path"
                    d={edgePath}
                    markerEnd={markerEnd}
                />
            )}
            {data?.label && (
                <text
                    style={{ 
                        fill: data.validationStatus === 'error' ? '#f44336' :
                              data.validationStatus === 'warning' ? '#ff9800' :
                              '#555',
                        fontSize: '12px'
                    }}
                    x={midX}
                    y={midY}
                    dy={-5}
                    textAnchor="middle"
                    alignmentBaseline="middle"
                >
                    {data.label}
                </text>
            )}
        </>
    );
};

const EDGE_TYPES = {
    default: CustomEdge,
};

export const TopologyDesigner: React.FC = () => {
    const [nodes, setNodes, onNodesChange] = useNodesState([]);
    const [edges, setEdges, onEdgesChange] = useEdgesState([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [isAddNodeDialogOpen, setIsAddNodeDialogOpen] = useState(false);
    const [editNode, setEditNode] = useState<{ node: Node | null, isOpen: boolean }>({
        node: null,
        isOpen: false
    });
    const [editEdge, setEditEdge] = useState<{ 
        edge: Edge | null, 
        sourceExchangeType: ExchangeType | undefined,
        isOpen: boolean 
    }>({
        edge: null,
        sourceExchangeType: undefined,
        isOpen: false
    });

    // Get source exchange type for an edge
    const getSourceExchangeType = useCallback((sourceId: string): ExchangeType | undefined => {
        const sourceNode = nodes.find(n => n.id === sourceId);
        return sourceNode?.data?.type as ExchangeType;
    }, [nodes]);

    // Style edges based on validation
    const styledEdges = useMemo(() => {
        return edges.map(edge => ({
            ...edge,
            type: 'default', // Use our custom edge
            ...getEdgeStyle(edge, getSourceExchangeType(edge.source)),
        }));
    }, [edges, getSourceExchangeType]);

    const onConnect = useCallback(
        (params: Connection) => {
            const sourceExchangeType = getSourceExchangeType(params.source!);
            
            // Create a new edge with default binding data
            const edge = {
                ...params,
                id: `e${params.source}-${params.target}`,
                data: {
                    routingKey: sourceExchangeType === ExchangeType.Direct ? 'route-key' : '',
                    arguments: sourceExchangeType === ExchangeType.Headers ? { headers: [] } : undefined,
                },
                label: sourceExchangeType === ExchangeType.Direct ? 'route-key' : '',
                style: getEdgeStyle({ ...params, data: {} } as Edge, sourceExchangeType),
            };

            setEdges((eds) => addEdge(edge, eds));

            // Open the edit dialog immediately for the new edge
            setEditEdge({
                edge: edge as Edge,
                sourceExchangeType,
                isOpen: true
            });
        },
        [getSourceExchangeType, setEdges]
    );

    const handleAddNode = useCallback((type: NodeType, data: Partial<Exchange | Queue>) => {
        const id = `${type}-${Date.now()}`;
        const newNode: Node = {
            id,
            type,
            position: { x: 100, y: 100 },
            data: {
                id,
                ...data
            },
        };

        setNodes((nds) => [...nds, newNode]);
    }, [setNodes]);

    const loadTopologyFromBroker = useCallback(async () => {
        try {
            setIsLoading(true);
            setError(null);

            const topology = await topologyApi.getFromBroker();
            const { nodes, edges } = convertTopologyToFlow(topology);
            
            setNodes(nodes);
            setEdges(edges);
        } catch (err) {
            setError('Failed to load topology from broker');
            console.error('Error loading topology:', err);
        } finally {
            setIsLoading(false);
        }
    }, [setNodes, setEdges]);

    const convertTopologyToFlow = useCallback((topology: Topology) => {
        const nodes: Node[] = [];
        const edges: Edge[] = [];
        
        let exchangeY = 50;
        let queueY = 50;
        
        topology.exchanges.forEach((exchange) => {
            nodes.push({
                id: exchange.id,
                type: NodeType.Exchange,
                position: { x: 100, y: exchangeY },
                data: exchange,
            });
            exchangeY += 150;
        });

        topology.queues.forEach((queue) => {
            nodes.push({
                id: queue.id,
                type: NodeType.Queue,
                position: { x: 500, y: queueY },
                data: queue,
            });
            queueY += 150;
        });

        topology.bindings.forEach((binding) => {
            edges.push({
                id: binding.id,
                source: binding.sourceExchange,
                target: binding.destinationQueue,
                data: binding,
                label: binding.routingKey,
                type: 'smoothstep',
            });
        });

        return { nodes, edges };
    }, []);

    const handleNodeDoubleClick = useCallback((event: React.MouseEvent, node: Node) => {
        setEditNode({ node, isOpen: true });
    }, []);

    const handleUpdateNode = useCallback((type: NodeType, data: Partial<Exchange | Queue>) => {
        if (!editNode.node) return;

        setNodes((nds) =>
            nds.map((node) =>
                node.id === editNode.node?.id
                    ? { ...node, data: { ...node.data, ...data } }
                    : node
            )
        );
    }, [editNode.node, setNodes]);

    const handleEdgeDoubleClick = useCallback((event: React.MouseEvent, edge: Edge) => {
        const sourceExchangeType = getSourceExchangeType(edge.source);
        setEditEdge({ 
            edge, 
            sourceExchangeType,
            isOpen: true 
        });
    }, [getSourceExchangeType]);

    const handleUpdateEdge = useCallback((edge: Edge, data: Partial<Binding>) => {
        setEdges((eds) =>
            eds.map((e) =>
                e.id === edge.id
                    ? { 
                        ...e, 
                        data: { ...e.data, ...data },
                        label: data.routingKey || e.label 
                    }
                    : e
            )
        );
    }, [setEdges]);

    return (
        <Box sx={{ height: 'calc(100vh - 128px)', position: 'relative' }}>
            <Paper 
                elevation={3} 
                sx={{ 
                    height: '100%',
                    bgcolor: 'grey.50',
                    '& .react-flow__node': {
                        width: 'auto',
                        height: 'auto'
                    }
                }}
            >
                <ReactFlow
                    nodes={nodes}
                    edges={styledEdges}
                    onNodesChange={onNodesChange}
                    onEdgesChange={onEdgesChange}
                    onConnect={onConnect}
                    onNodeDoubleClick={handleNodeDoubleClick}
                    onEdgeDoubleClick={handleEdgeDoubleClick}
                    nodeTypes={NODE_TYPES}
                    edgeTypes={EDGE_TYPES}
                    defaultViewport={INITIAL_VIEWPORT}
                    minZoom={0.1}
                    maxZoom={4}
                    fitView
                    attributionPosition="bottom-left"
                >
                    <Background color="#aaa" gap={16} />
                    <Controls />
                    <MiniMap />
                </ReactFlow>
            </Paper>

            <Box sx={{ position: 'absolute', bottom: 16, right: 16 }}>
                <Tooltip title="Load from Broker">
                    <Fab
                        color="primary"
                        onClick={loadTopologyFromBroker}
                        disabled={isLoading}
                        sx={{ mr: 1 }}
                    >
                        <CloudDownloadIcon />
                    </Fab>
                </Tooltip>
                <Tooltip title="Add Node">
                    <Fab 
                        color="secondary"
                        onClick={() => setIsAddNodeDialogOpen(true)}
                    >
                        <AddIcon />
                    </Fab>
                </Tooltip>
            </Box>

            <AddNodeDialog
                open={isAddNodeDialogOpen}
                onClose={() => setIsAddNodeDialogOpen(false)}
                onAdd={handleAddNode}
            />

            <EditNodeDialog
                open={editNode.isOpen}
                onClose={() => setEditNode({ node: null, isOpen: false })}
                onSave={handleUpdateNode}
                nodeType={editNode.node?.type as NodeType}
                initialData={editNode.node?.data}
            />

            <EditEdgeDialog
                open={editEdge.isOpen}
                onClose={() => setEditEdge({ edge: null, sourceExchangeType: undefined, isOpen: false })}
                onSave={handleUpdateEdge}
                edge={editEdge.edge}
                sourceExchangeType={editEdge.sourceExchangeType}
            />

            <Snackbar
                open={!!error}
                autoHideDuration={6000}
                onClose={() => setError(null)}
            >
                <Alert severity="error" onClose={() => setError(null)}>
                    {error}
                </Alert>
            </Snackbar>
        </Box>
    );
}; 