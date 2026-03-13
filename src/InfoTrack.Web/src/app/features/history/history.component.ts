import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HistoryService } from '../../core/services/history.service';
import { SearchRun } from '../../core/models/solicitor.model';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatListModule, MatChipsModule,
    MatButtonModule, MatIconModule
  ],
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
  runs: SearchRun[] = [];

  constructor(
    private historyService: HistoryService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.historyService.getHistory().subscribe(runs => {
      this.runs = runs;
      this.cdr.markForCheck();
    });
  }

  viewResults(runId: number) {
    this.router.navigate(['/results', runId]);
  }

  statusColor(status: string): string {
    return { completed: 'accent', failed: 'warn', running: 'primary', pending: '' }[status] ?? '';
  }
}
