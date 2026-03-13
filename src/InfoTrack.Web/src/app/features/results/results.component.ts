import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartOptions } from 'chart.js';
import { forkJoin } from 'rxjs';
import { HistoryService } from '../../core/services/history.service';
import { LocationSummary, Solicitor } from '../../core/models/solicitor.model';

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterLink,
    MatCardModule, MatTableModule, MatSelectModule,
    MatFormFieldModule, MatButtonModule, MatChipsModule,
    MatIconModule, MatTooltipModule, BaseChartDirective
  ],
  templateUrl: './results.component.html',
  styleUrls: ['./results.component.scss']
})
export class ResultsComponent implements OnInit {
  runId!: number;
  summaries: LocationSummary[] = [];
  solicitors: Solicitor[] = [];
  filteredSolicitors: Solicitor[] = [];

  chartData: ChartData<'bar'> = { labels: [], datasets: [] };
  chartOptions: ChartOptions<'bar'> = {
    responsive: true,
    plugins: { legend: { display: false } },
    scales: {
      x: { title: { display: true, text: 'Location' } },
      y: { title: { display: true, text: 'Count' }, beginAtZero: true, ticks: { stepSize: 1 } }
    }
  };

  selectedLocation = '';
  selectedSort = '';
  displayedColumns = ['isNew', 'name', 'location', 'phone', 'address', 'rating', 'website'];

  constructor(
    private route: ActivatedRoute,
    private historyService: HistoryService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.runId = Number(this.route.snapshot.paramMap.get('runId'));
    this.loadData();
  }

  private loadData() {
    forkJoin({
      summary: this.historyService.getSummary(this.runId),
      results: this.historyService.getResults(this.runId)
    }).subscribe(({ summary, results }) => {
      this.summaries = summary;
      this.solicitors = results;
      this.filteredSolicitors = results;
      this.cdr.markForCheck();
      this.chartData = {
        labels: summary.map(s => s.location),
        datasets: [{
          data: summary.map(s => s.total),
          backgroundColor: 'rgba(124, 58, 237, 0.7)',
          borderColor: 'rgba(91, 33, 182, 1)',
          borderWidth: 1,
          borderRadius: 6,
        }]
      };
    });
  }

  applyFilters() {
    this.historyService
      .getResults(this.runId, this.selectedLocation || undefined, this.selectedSort || undefined)
      .subscribe(results => {
        this.filteredSolicitors = results;
        this.cdr.markForCheck();
      });
  }

  uniqueLocations(): string[] {
    return [...new Set(this.solicitors.map(s => s.location))];
  }

  healthClass(health: string): string {
    return {
      healthy: 'health-healthy',
      degraded: 'health-degraded',
      structure_changed: 'health-structure',
      empty: 'health-empty'
    }[health] ?? '';
  }
}
