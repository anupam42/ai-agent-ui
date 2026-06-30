import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { filter, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { WireframeService } from './wireframe.service';
import { WireframeSchema } from '../models/wireframe.model';
import { HistorySession, GroupedSessions } from '../models/history.model';

const STORAGE_KEY   = 'ds_history_sessions';
const MAX_SESSIONS  = 50;
const API_BASE      = 'https://localhost:7211/api/history';

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private readonly http             = inject(HttpClient);
  private readonly wireframeService = inject(WireframeService);

  private readonly _sessions$ = new BehaviorSubject<HistorySession[]>(this.loadLocal());
  readonly sessions$ = this._sessions$.asObservable();

  constructor() {
    // Load persisted sessions from backend on startup
    this.fetchFromBackend();

    // Auto-save whenever a new wireframe is generated
    this.wireframeService.schema$
      .pipe(filter((s): s is WireframeSchema => s !== null))
      .subscribe(schema => this.addSession(schema));
  }

  // ── Public API ───────────────────────────────────────────────

  get sessions(): HistorySession[] {
    return this._sessions$.getValue();
  }

  getGrouped(query: string = ''): GroupedSessions[] {
    const q = query.trim().toLowerCase();
    const all = q
      ? this.sessions.filter(
          s =>
            s.schemaName.toLowerCase().includes(q) ||
            s.prompt.toLowerCase().includes(q),
        )
      : this.sessions;

    const today: HistorySession[]     = [];
    const yesterday: HistorySession[] = [];
    const thisWeek: HistorySession[]  = [];
    const older: HistorySession[]     = [];

    const now              = new Date();
    const startOfToday     = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const startOfYesterday = new Date(startOfToday);
    startOfYesterday.setDate(startOfYesterday.getDate() - 1);
    const startOfWeek = new Date(startOfToday);
    startOfWeek.setDate(startOfWeek.getDate() - 7);

    for (const s of all) {
      const d = new Date(s.createdAt);
      if (d >= startOfToday)     today.push(s);
      else if (d >= startOfYesterday) yesterday.push(s);
      else if (d >= startOfWeek) thisWeek.push(s);
      else                       older.push(s);
    }

    const groups: GroupedSessions[] = [];
    if (today.length)     groups.push({ label: 'Today',     sessions: today });
    if (yesterday.length) groups.push({ label: 'Yesterday', sessions: yesterday });
    if (thisWeek.length)  groups.push({ label: 'This week', sessions: thisWeek });
    if (older.length)     groups.push({ label: 'Older',     sessions: older });
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
    this.http.delete(`${API_BASE}/${id}`)
      .pipe(catchError(() => of(null)))
      .subscribe();
    this.saveLocal(this.sessions.filter(s => s.id !== id));
  }

  clearAll(): void {
    this.http.delete(API_BASE)
      .pipe(catchError(() => of(null)))
      .subscribe();
    this.saveLocal([]);
  }

  // ── Private ──────────────────────────────────────────────────

  private addSession(schema: WireframeSchema): void {
    if (this.sessions.some(s => s.id === schema.id)) return;

    const session: HistorySession = {
      id:         schema.id,
      schemaName: schema.name,
      prompt:     schema.prompt,
      schema,
      createdAt:  new Date().toISOString(),
    };

    const updated = [session, ...this.sessions].slice(0, MAX_SESSIONS);
    this.saveLocal(updated);

    // Persist to backend
    this.http.post<HistorySession>(API_BASE, {
      id:         session.id,
      schemaName: session.schemaName,
      prompt:     session.prompt,
      schema:     session.schema,
      createdAt:  session.createdAt,
    }).pipe(catchError(() => of(null))).subscribe();
  }

  private fetchFromBackend(): void {
    this.http.get<HistorySession[]>(API_BASE)
      .pipe(catchError(() => of(null)))
      .subscribe(sessions => {
        if (!sessions) return; // backend unreachable — keep localStorage data
        const updated = sessions.slice(0, MAX_SESSIONS);
        this.saveLocal(updated);
      });
  }

  private saveLocal(sessions: HistorySession[]): void {
    this._sessions$.next(sessions);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(sessions));
    } catch { /* quota exceeded */ }
  }

  private loadLocal(): HistorySession[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }
}
