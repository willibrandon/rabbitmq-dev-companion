import { Node, Edge } from '@reactflow/core';

// Define a topology state snapshot
export interface TopologySnapshot {
  nodes: Node[];
  edges: Edge[];
  timestamp: number; // For tracking when the snapshot was created
}

// Draft interfaces
export interface TopologyDraft {
  id: string;
  name: string;
  snapshot: TopologySnapshot;
  createdAt: number;
  updatedAt: number;
}

// History Manager Class
export class HistoryManager {
  private history: TopologySnapshot[] = [];
  private currentIndex: number = -1;
  private maxHistorySize: number = 50; // Limit history size
  private draftKey: string = 'topology-drafts';

  constructor() {
    // Initialize empty history
    this.clear();
  }

  // Create a snapshot of the current topology state
  createSnapshot(nodes: Node[], edges: Edge[]): TopologySnapshot {
    return {
      nodes: JSON.parse(JSON.stringify(nodes)), // Deep clone
      edges: JSON.parse(JSON.stringify(edges)), // Deep clone
      timestamp: Date.now(),
    };
  }

  // Add a new state to history
  addToHistory(snapshot: TopologySnapshot): void {
    // If we're not at the end of history, truncate the future states
    if (this.currentIndex < this.history.length - 1) {
      this.history = this.history.slice(0, this.currentIndex + 1);
    }

    // Add new state
    this.history.push(snapshot);
    this.currentIndex++;

    // Limit history size
    if (this.history.length > this.maxHistorySize) {
      this.history.shift();
      this.currentIndex--;
    }
  }

  // Can undo?
  canUndo(): boolean {
    return this.currentIndex > 0;
  }

  // Can redo?
  canRedo(): boolean {
    return this.currentIndex < this.history.length - 1;
  }

  // Undo operation
  undo(): TopologySnapshot | null {
    if (!this.canUndo()) {
      return null;
    }

    this.currentIndex--;
    return this.getCurrentSnapshot();
  }

  // Redo operation
  redo(): TopologySnapshot | null {
    if (!this.canRedo()) {
      return null;
    }

    this.currentIndex++;
    return this.getCurrentSnapshot();
  }

  // Get current state
  getCurrentSnapshot(): TopologySnapshot {
    return this.history[this.currentIndex];
  }

  // Get the entire history
  getHistory(): TopologySnapshot[] {
    return this.history;
  }

  // Get history at specific index
  getHistoryAt(index: number): TopologySnapshot | null {
    if (index >= 0 && index < this.history.length) {
      return this.history[index];
    }
    return null;
  }

  // Get current index
  getCurrentIndex(): number {
    return this.currentIndex;
  }

  // Set current index directly
  setCurrentIndex(index: number): void {
    if (index >= 0 && index < this.history.length) {
      this.currentIndex = index;
    }
  }

  // Clear history
  clear(): void {
    this.history = [];
    this.currentIndex = -1;
  }

  // Save a draft
  saveDraft(id: string, name: string, snapshot: TopologySnapshot): void {
    // Get existing drafts
    const drafts = this.getDrafts();
    
    // Check if draft with this ID already exists
    const existingDraftIndex = drafts.findIndex(draft => draft.id === id);
    const now = Date.now();
    
    if (existingDraftIndex >= 0) {
      // Update existing draft
      drafts[existingDraftIndex] = {
        ...drafts[existingDraftIndex],
        name,
        snapshot,
        updatedAt: now,
      };
    } else {
      // Add new draft
      drafts.push({
        id,
        name,
        snapshot,
        createdAt: now,
        updatedAt: now,
      });
    }
    
    // Save to localStorage
    localStorage.setItem(this.draftKey, JSON.stringify(drafts));
  }

  // Get all drafts
  getDrafts(): TopologyDraft[] {
    const draftsJson = localStorage.getItem(this.draftKey);
    if (!draftsJson) {
      return [];
    }
    
    try {
      return JSON.parse(draftsJson);
    } catch (e) {
      console.error('Failed to parse topology drafts', e);
      return [];
    }
  }

  // Get a specific draft
  getDraft(id: string): TopologyDraft | null {
    const drafts = this.getDrafts();
    return drafts.find(draft => draft.id === id) || null;
  }

  // Delete a draft
  deleteDraft(id: string): void {
    const drafts = this.getDrafts();
    const filteredDrafts = drafts.filter(draft => draft.id !== id);
    localStorage.setItem(this.draftKey, JSON.stringify(filteredDrafts));
  }
}

// Create and export a singleton instance
export const historyManager = new HistoryManager(); 