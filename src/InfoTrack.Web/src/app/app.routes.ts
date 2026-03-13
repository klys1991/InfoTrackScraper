import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'search', pathMatch: 'full' },
  {
    path: 'search',
    loadComponent: () =>
      import('./features/search/search.component').then(m => m.SearchComponent)
  },
  {
    path: 'results/:runId',
    loadComponent: () =>
      import('./features/results/results.component').then(m => m.ResultsComponent)
  },
  {
    path: 'history',
    loadComponent: () =>
      import('./features/history/history.component').then(m => m.HistoryComponent)
  },
  { path: '**', redirectTo: 'search' }
];
