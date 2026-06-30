import { Injectable, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { WireframeService } from './wireframe.service';
import { WireframeSchema } from '../models/wireframe.model';
import { HistorySession, GroupedSessions } from '../models/history.model';

const STORAGE_KEY = 'ds_history_sessions';
const MAX_SESSIONS = 50;

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private readonly wireframeService = inject(WireframeService);

  private readonly _sessions$ = new BehaviorSubject<HistorySession[]>(this.load());
  readonly sessions$ = this._sessions$.asObservable();

  constructor() {
    this.wireframeService.schema$
      .pipe(filter((s): s is WireframeSchema => s !== null))
      .subscribe(schema => this.addSession(schema));
  }

  get sessions(): HistorySession[] {
    return this._sessions$.getValue();
  }

  getGrouped(filter: string = ''): GroupedSessions[] {
    const q = filter.trim().toLowerCase();
    const all = q
      ? this.sessions.filter(
          s =>
            s.schemaName.toLowerCase().includes(q) ||
            s.prompt.toLowerCase().includes(q),
        )
      : this.sessions;

    const today: HistorySession[] = [];
    const yesterday: HistorySession[] = [];
    const thisWeek: HistorySession[] = [];
    const older: HistorySession[] = [];

    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const startOfYesterday = new Date(startOfToday);
    startOfYesterday.setDate(startOfYesterday.getDate() - 1);
    const startOfWeek = new Date(startOfToday);
    startOfWeek.setDate(startOfWeek.getDate() - 7);

    for (const s of all) {
      const d = new Date(s.createdAt);
      if (d >= startOfToday) today.push(s);
      else if (d >= startOfYesterday) yesterday.push(s);
      else if (d >= startOfWeek) thisWeek.push(s);
      else older.push(s);
    }

    const groups: GroupedSessions[] = [];
    if (today.length) groups.push({ label: 'Today', sessions: today });
    if (yesterday.length) groups.push({ label: 'Yesterday', sessions: yesterday });
    if (thisWeek.length) groups.push({ label: 'This week', sessions: thisWeek });
    if (older.length) groups.push({ label: 'Older', sessions: older });
    return groups;
  }

  restoreSession(session: HistorySession): void {
    const schema: WireframeSchema = {
      ...session.schema,
      createdAt: new Date(session.schema.createdAt),
    };
    this.wireframeService.loadVersion(schema);
  }

  deleteSession(id: string): void {
    const updated = this.sessions.filter(s => s.id !== id);
    this.save(updated);
  }

  clearAll(): void {
    this.save([]);
  }

  private addSession(schema: WireframeSchema): void {
    const existing = this.sessions;
    if (existing.some(s => s.id === schema.id)) return;

    const session: HistorySession = {
      id: schema.id,
      schemaName: schema.name,
      prompt: schema.prompt,
      schema,
      createdAt: new Date().toISOString(),
    };

    this.save([session, ...existing].slice(0, MAX_SESSIONS));
  }

  private save(sessions: HistorySession[]): void {
    this._sessions$.next(sessions);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(sessions));
    } catch { /* storage full */ }
  }

  private load(): HistorySession[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }
}
