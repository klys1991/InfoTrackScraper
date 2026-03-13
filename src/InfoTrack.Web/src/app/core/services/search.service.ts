import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchRun } from '../models/solicitor.model';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private base = '/api';

  constructor(private http: HttpClient) {}

  startSearch(locations: string[]): Observable<{ runId: number }> {
    return this.http.post<{ runId: number }>(`${this.base}/search`, { locations });
  }

  getRun(runId: number): Observable<SearchRun> {
    return this.http.get<SearchRun>(`${this.base}/search/${runId}`);
  }

  cancelRun(runId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/search/${runId}`);
  }
}
