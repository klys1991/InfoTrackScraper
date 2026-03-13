import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { interval, Subject, switchMap, takeUntil, takeWhile } from 'rxjs';
import { SearchService } from '../../core/services/search.service';
import { LocationService } from '../../core/services/location.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    CommonModule, FormsModule,
    MatChipsModule, MatButtonModule, MatInputModule,
    MatFormFieldModule, MatProgressSpinnerModule,
    MatIconModule, MatCardModule
  ],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.scss']
})
export class SearchComponent implements OnInit, OnDestroy {
  locations: string[] = [];
  selectedLocations = new Set<string>();
  customLocations = new Set<string>();
  customLocation = '';
  isRunning = false;
  runFailed = false;
  currentRunId?: number;
  private destroy$ = new Subject<void>();
  private cancelPoll$ = new Subject<void>();

  constructor(
    private searchService: SearchService,
    private locationService: LocationService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.locationService.getDefaults().subscribe(locs => {
      this.locations = locs;
      this.locations.forEach(l => this.selectedLocations.add(l));
      this.cdr.markForCheck();
    });
  }

  toggleLocation(location: string) {
    if (this.selectedLocations.has(location)) {
      this.selectedLocations.delete(location);
    } else {
      this.selectedLocations.add(location);
    }
    this.cdr.markForCheck();
  }

  isSelected(location: string): boolean {
    return this.selectedLocations.has(location);
  }

  isCustom(location: string): boolean {
    return this.customLocations.has(location);
  }

  get defaultLocationsList(): string[] {
    return this.locations.filter(l => !this.customLocations.has(l));
  }

  get customLocationsList(): string[] {
    return this.locations.filter(l => this.customLocations.has(l));
  }

  addCustomLocation() {
    const loc = this.customLocation.trim();
    if (loc && !this.locations.includes(loc)) {
      this.locations = [...this.locations, loc];
      this.customLocations.add(loc);
      this.selectedLocations.add(loc);
      this.customLocation = '';
      this.cdr.markForCheck();
    }
  }

  removeLocation(location: string, event: Event) {
    event.stopPropagation();
    this.customLocations.delete(location);
    this.selectedLocations.delete(location);
    this.locations = this.locations.filter(l => l !== location);
    this.cdr.markForCheck();
  }

  startScrape() {
    if (this.selectedLocations.size === 0 || this.isRunning) return;
    this.isRunning = true;
    this.runFailed = false;

    this.searchService.startSearch([...this.selectedLocations]).subscribe({
      next: ({ runId }) => {
        this.currentRunId = runId;
        this.pollForCompletion(runId);
      },
      error: () => {
        this.isRunning = false;
        this.runFailed = true;
      }
    });
  }

  cancelScrape() {
    if (!this.currentRunId) return;
    this.cancelPoll$.next(); // stop polling immediately
    this.searchService.cancelRun(this.currentRunId).subscribe();
    this.isRunning = false;
    this.cdr.markForCheck();
  }

  private pollForCompletion(runId: number) {
    this.cancelPoll$ = new Subject<void>();
    interval(2000).pipe(
      switchMap(() => this.searchService.getRun(runId)),
      takeWhile(run => run.status === 'running' || run.status === 'pending', true),
      takeUntil(this.cancelPoll$),
      takeUntil(this.destroy$)
    ).subscribe(run => {
      if (run.status === 'completed') {
        this.router.navigate(['/results', runId]);
      } else if (run.status === 'failed' || run.status === 'cancelled') {
        this.isRunning = false;
        this.runFailed = run.status === 'failed';
        this.cdr.markForCheck();
      }
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
