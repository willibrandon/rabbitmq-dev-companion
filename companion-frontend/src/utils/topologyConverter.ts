import { Node, Edge } from '@reactflow/core';
import { NodeType, Topology, Exchange, Queue, Binding, ExchangeType } from '../types/topology';

/**
 * Converts ReactFlow nodes and edges to a Topology object
 */
export const convertFlowToTopology = (
  nodes: Node[],
  edges: Edge[],
  name: string = 'My Topology',
  description: string = 'Created with RabbitMQ Developer Companion'
): Topology => {
  const exchanges: Exchange[] = [];
  const queues: Queue[] = [];
  const bindings: Binding[] = [];

  // Process nodes
  nodes.forEach((node) => {
    if (node.type === NodeType.Exchange) {
      exchanges.push(node.data as Exchange);
    } else if (node.type === NodeType.Queue) {
      queues.push(node.data as Queue);
    }
  });

  // Process edges
  edges.forEach((edge) => {
    const sourceNode = nodes.find((n) => n.id === edge.source);
    const targetNode = nodes.find((n) => n.id === edge.target);
    
    if (sourceNode && targetNode && 
        sourceNode.type === NodeType.Exchange && 
        targetNode.type === NodeType.Queue) {
      
      const binding: Binding = {
        id: edge.id,
        sourceExchange: edge.source,
        destinationQueue: edge.target,
        ...edge.data,
      };
      
      bindings.push(binding);
    }
  });

  return {
    name,
    description,
    exchanges,
    queues,
    bindings,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
};

/**
 * Converts a Topology object to ReactFlow nodes and edges
 */
export const convertTopologyToFlow = (topology: Topology): { nodes: Node[], edges: Edge[] } => {
  const nodes: Node[] = [];
  const edges: Edge[] = [];
  
  let exchangeY = 50;
  let queueY = 50;
  
  // Process exchanges
  topology.exchanges.forEach((exchange) => {
    nodes.push({
      id: exchange.id,
      type: NodeType.Exchange,
      position: { x: 100, y: exchangeY },
      data: exchange,
    });
    exchangeY += 150;
  });

  // Process queues
  topology.queues.forEach((queue) => {
    nodes.push({
      id: queue.id,
      type: NodeType.Queue,
      position: { x: 500, y: queueY },
      data: queue,
    });
    queueY += 150;
  });

  // Process bindings
  topology.bindings.forEach((binding) => {
    edges.push({
      id: binding.id,
      source: binding.sourceExchange,
      target: binding.destinationQueue,
      data: binding,
      label: binding.routingKey,
      type: 'default',
    });
  });

  return { nodes, edges };
};

/**
 * Downloads a JSON file with the topology
 */
export const downloadTopologyAsJson = (topology: Topology) => {
  const jsonString = JSON.stringify(topology, null, 2);
  const blob = new Blob([jsonString], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  
  const link = document.createElement('a');
  link.href = url;
  link.download = `${topology.name.replace(/\s+/g, '_').toLowerCase()}.json`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};

/**
 * Reads a topology from a JSON file
 */
export const readTopologyFromJsonFile = (file: File): Promise<Topology> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    
    reader.onload = (event) => {
      try {
        const content = event.target?.result as string;
        const topology = JSON.parse(content) as Topology;
        resolve(topology);
      } catch (error) {
        reject(new Error('Invalid topology JSON file'));
      }
    };
    
    reader.onerror = () => {
      reject(new Error('Failed to read file'));
    };
    
    reader.readAsText(file);
  });
}; 