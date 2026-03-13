export interface Solicitor {
  id: number;
  name: string;
  location: string;
  siteId: string;
  phone?: string;
  email?: string;
  address?: string;
  rating?: number;
  reviewCount?: number;
  website?: string;
  isNew: boolean;
  parseHealth: 'healthy' | 'degraded' | 'structure_changed' | 'empty';
}

export interface SearchRun {
  id: number;
  startedAt: string;
  completedAt?: string;
  locations: string[];
  status: 'pending' | 'running' | 'completed' | 'failed' | 'cancelled';
  resultCount?: number;
  newCount?: number;
}

export interface LocationSummary {
  location: string;
  total: number;
  newCount: number;
  averageRating?: number;
  parseHealth: string;
}
