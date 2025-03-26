import { Node, Edge } from '@reactflow/core';
import dagre from 'dagre';

// Types for layout management
export interface LayoutOptions {
  direction?: 'LR' | 'TB' | 'RL' | 'BT';
  nodeSeparation?: number;
  rankSeparation?: number;
}

export interface SavedLayout {
  positions: {
    [nodeId: string]: { x: number; y: number };
  };
  zoom?: number;
  pan?: { x: number; y: number };
}

const defaultOptions: LayoutOptions = {
  direction: 'LR',
  nodeSeparation: 100,
  rankSeparation: 200,
};

/**
 * Auto-layout nodes using the dagre library
 */
export const getAutoLayout = (
  nodes: Node[],
  edges: Edge[],
  options: LayoutOptions = defaultOptions
): Node[] => {
  if (!nodes.length) return [];

  const dagreGraph = new dagre.graphlib.Graph();
  dagreGraph.setDefaultEdgeLabel(() => ({}));

  // Direction can be 'LR' (left to right) or 'TB' (top to bottom)
  const { direction = 'LR', nodeSeparation = 100, rankSeparation = 200 } = options;
  
  dagreGraph.setGraph({
    rankdir: direction, 
    nodesep: nodeSeparation,
    ranksep: rankSeparation,
  });

  // Add nodes to the graph with their dimensions
  nodes.forEach((node) => {
    dagreGraph.setNode(node.id, {
      width: 220,  // Approximate width of our nodes
      height: 100, // Approximate height of our nodes
    });
  });

  // Add edges to the graph
  edges.forEach((edge) => {
    dagreGraph.setEdge(edge.source, edge.target);
  });

  // Calculate the layout
  dagre.layout(dagreGraph);

  // Apply the layout to the nodes
  return nodes.map((node) => {
    const nodeWithPosition = dagreGraph.node(node.id);
    
    return {
      ...node,
      position: {
        x: nodeWithPosition.x - 220 / 2, // Center the node
        y: nodeWithPosition.y - 100 / 2,
      },
    };
  });
};

/**
 * Apply snap-to-grid to node positions
 */
export const snapToGrid = (nodes: Node[], gridSize: number = 20): Node[] => {
  return nodes.map((node) => {
    const x = Math.round(node.position.x / gridSize) * gridSize;
    const y = Math.round(node.position.y / gridSize) * gridSize;
    
    return {
      ...node,
      position: { x, y },
    };
  });
};

/**
 * Save the current layout to localStorage
 */
export const saveLayout = (nodes: Node[], topologyId: string, viewportState?: any): void => {
  const positions: { [nodeId: string]: { x: number; y: number } } = {};
  
  nodes.forEach((node) => {
    positions[node.id] = { x: node.position.x, y: node.position.y };
  });

  const savedLayout: SavedLayout = {
    positions,
  };

  // Optionally save viewport state (zoom & pan)
  if (viewportState) {
    savedLayout.zoom = viewportState.zoom;
    savedLayout.pan = viewportState.position;
  }

  localStorage.setItem(`topology-layout-${topologyId}`, JSON.stringify(savedLayout));
};

/**
 * Restore layout from localStorage
 */
export const restoreLayout = (nodes: Node[], topologyId: string): { 
  nodes: Node[], 
  viewportState?: { zoom: number, position: { x: number, y: number } } 
} => {
  const savedLayoutStr = localStorage.getItem(`topology-layout-${topologyId}`);
  
  if (!savedLayoutStr) {
    return { nodes };
  }

  try {
    const savedLayout: SavedLayout = JSON.parse(savedLayoutStr);
    
    const updatedNodes = nodes.map((node) => {
      const savedPosition = savedLayout.positions[node.id];
      
      if (savedPosition) {
        return {
          ...node,
          position: savedPosition,
        };
      }
      
      return node;
    });

    // Return updated nodes and optionally viewport state
    if (savedLayout.zoom && savedLayout.pan) {
      return { 
        nodes: updatedNodes,
        viewportState: {
          zoom: savedLayout.zoom,
          position: savedLayout.pan
        }
      };
    }

    return { nodes: updatedNodes };
  } catch (error) {
    console.error('Failed to restore layout:', error);
    return { nodes };
  }
}; 