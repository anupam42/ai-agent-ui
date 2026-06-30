import { WireframeSchema } from './wireframe.model';

export interface HistorySession {
  id: string;
  schemaName: string;
  prompt: string;
  schema: WireframeSchema;
  createdAt: string; // ISO string for JSON serialization
}

export interface GroupedSessions {
  label: string;
  sessions: HistorySession[];
}
