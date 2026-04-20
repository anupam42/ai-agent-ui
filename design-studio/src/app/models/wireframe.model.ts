export type WireframeBlockType =
  | 'header' | 'sidebar' | 'main' | 'footer'
  | 'row' | 'column' | 'card' | 'stat-card'
  | 'data-table' | 'form' | 'hero' | 'nav'
  | 'button' | 'image' | 'text' | 'input'
  | 'tabs' | 'grid' | 'icon-button' | 'logo'
  | 'badge' | 'divider' | 'app-shell' | 'toolbar'
  | 'search' | 'list' | 'stepper';

export interface WireframeNode {
  id?: string;
  type: WireframeBlockType;
  label?: string;
  children?: WireframeNode[];
  span?: number;
  width?: string;
  height?: string;
  sticky?: boolean;
  items?: string[];
  columns?: string[];
  rows?: number;
  title?: string;
  value?: string;
  mappedComponent?: string;
  placeholder?: string;
}

export interface WireframeSchema {
  id: string;
  name: string;
  prompt: string;
  root: WireframeNode;
  createdAt: Date;
  mappings?: ComponentMapping[];
}

export interface ComponentMapping {
  blockLabel: string;
  blockType: WireframeBlockType;
  componentName: string;
  matchConfidence: 'exact' | 'partial' | 'fallback';
}

export interface GeneratedCode {
  html: string;
  ts: string;
  scss: string;
}
