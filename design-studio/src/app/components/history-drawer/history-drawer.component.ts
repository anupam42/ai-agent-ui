import {
  Component, OnInit, OnDestroy, Output, EventEmitter,
  ChangeDetectionStrategy, ChangeDetectorRef, inject,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { Subscription } from 'rxjs';
import { HistoryService } from '../../services/history.service';
import { HistorySession, GroupedSessions } from '../../models/history.model';

@Component({
  selector: 'app-history-drawer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, MatIconModule, MatButtonModule, MatTooltipModule, MatRippleModule],
  templateUrl: './history-drawer.component.html',
  styleUrls: ['./history-drawer.component.scss'],
})
export class HistoryDrawerComponent implements OnInit, OnDestroy {
  @Output() closeDrawer = new EventEmitter<void>();

  private readonly historyService = inject(HistoryService);
  private readonly cdr = inject(ChangeDetectorRef);
  private sub?: Subscription;

  searchQuery = '';
  groups: GroupedSessions[] = [];
  confirmClear = false;

  ngOnInit(): void {
    this.sub = this.historyService.sessions$.subscribe(() => {
      this.refresh();
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  onSearch(): void {
    this.refresh();
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.refresh();
  }

  restore(session: HistorySession): void {
    this.historyService.restoreSession(session);
    this.closeDrawer.emit();
  }

  deleteSession(event: Event, id: string): void {
    event.stopPropagation();
    this.historyService.deleteSession(id);
  }

  clearAll(): void {
    if (!this.confirmClear) {
      this.confirmClear = true;
      setTimeout(() => { this.confirmClear = false; this.cdr.markForCheck(); }, 3000);
      return;
    }
    this.historyService.clearAll();
    this.confirmClear = false;
  }

  get totalCount(): number {
    return this.historyService.sessions.length;
  }

  get isEmpty(): boolean {
    return this.groups.length === 0;
  }

  iconFor(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('dashboard') || n.includes('admin')) return 'dashboard';
    if (n.includes('login') || n.includes('auth') || n.includes('sign')) return 'lock_person';
    if (n.includes('landing') || n.includes('marketing') || n.includes('homepage')) return 'public';
    if (n.includes('form') || n.includes('registration')) return 'dynamic_form';
    return 'web_asset';
  }

  colorFor(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('dashboard') || n.includes('admin')) return '#6366f1';
    if (n.includes('login') || n.includes('auth')) return '#f59e0b';
    if (n.includes('landing') || n.includes('marketing')) return '#10b981';
    if (n.includes('form') || n.includes('registration')) return '#3b82f6';
    return '#8b5cf6';
  }

  timeAgo(isoString: string): string {
    const diff = Date.now() - new Date(isoString).getTime();
    const mins = Math.floor(diff / 60_000);
    if (mins < 1) return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24) return `${hrs}h ago`;
    const days = Math.floor(hrs / 24);
    if (days === 1) return 'Yesterday';
    if (days < 7) return `${days}d ago`;
    return new Date(isoString).toLocaleDateString([], { month: 'short', day: 'numeric' });
  }

  private refresh(): void {
    this.groups = this.historyService.getGrouped(this.searchQuery);
    this.cdr.markForCheck();
  }
}
