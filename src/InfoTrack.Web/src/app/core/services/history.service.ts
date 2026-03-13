import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LocationSummary, SearchRun, Solicitor } from '../models/solicitor.model';

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private base = '/api/history';

  constructor(private http: HttpClient) {}

  getHistory(): Observable<SearchRun[]> {
    return this.http.get<SearchRun[]>(this.base);
  }

  getResults(runId: number, location?: string, sortBy?: string): Observable<Solicitor[]> {
    const params: Record<string, string> = {};
    if (location) params['location'] = location;
    if (sortBy)   params['sortBy'] = sortBy;
    return this.http.get<Solicitor[]>(`${this.base}/${runId}/results`, { params });
  }

  getSummary(runId: number): Observable<LocationSummary[]> {
    return this.http.get<LocationSummary[]>(`${this.base}/${runId}/summary`);
  }
}
