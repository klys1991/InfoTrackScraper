import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar class="app-toolbar">
      <div class="toolbar-brand">
        <mat-icon class="brand-icon">gavel</mat-icon>
        <span class="brand-name">InfoTrack</span>
        <span class="brand-sub">Solicitor Scraper</span>
      </div>
      <span class="flex-spacer"></span>
      <nav class="toolbar-nav">
        <a mat-button routerLink="/search" routerLinkActive="active-link">
          <mat-icon>search</mat-icon> Search
        </a>
        <a mat-button routerLink="/history" routerLinkActive="active-link">
          <mat-icon>history</mat-icon> History
        </a>
      </nav>
    </mat-toolbar>
    <main class="app-main">
      <router-outlet />
    </main>
  `,
  styles: [`
    .app-toolbar {
      background: linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
      color: #fff;
      box-shadow: 0 2px 16px rgba(13,71,161,.4);
      padding: 0 32px;
      height: 62px;
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .toolbar-brand {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    .brand-icon { font-size: 22px; opacity: 0.9; }
    .brand-name { font-size: 18px; font-weight: 700; letter-spacing: -0.3px; }
    .brand-sub  { font-size: 13px; font-weight: 400; opacity: 0.65; margin-left: 2px; }
    .flex-spacer { flex: 1; }
    .toolbar-nav { display: flex; gap: 4px; }
    .toolbar-nav a {
      color: rgba(255,255,255,0.82);
      border-radius: 22px !important;
      font-size: 14px;
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 0 16px;
      transition: background 0.18s ease, color 0.18s ease, box-shadow 0.18s ease;
    }
    .toolbar-nav a:hover {
      background: rgba(255,255,255,0.14) !important;
      color: #fff;
      box-shadow: none !important;
      transform: none !important;
    }
    .active-link {
      background: rgba(255,255,255,0.22) !important;
      color: #fff !important;
    }
    .app-main {
      padding: 36px 24px;
      max-width: 1200px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      align-items: center;
    }
    .app-main > * { width: 100%; }
  `]
})
export class AppComponent {}
