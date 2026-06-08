import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  HostListener,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { WireframeService } from '../../services/wireframe.service';
import { WireframeSchema, GeneratedCode } from '../../models/wireframe.model';
import { WireframeBlockComponent } from '../wireframe-block/wireframe-block.component';

export type Viewport = 'mobile' | 'tablet' | 'desktop';
type Tool = 'arrow' | 'hand';

interface ViewportOption {
  value: Viewport;
  label: string;
  width: number;
  height: number;
  icon: string;
}

@Component({
  selector: 'app-wireframe-canvas',
  standalone: true,
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatSnackBarModule,
    WireframeBlockComponent,
  ],
  templateUrl: './wireframe-canvas.component.html',
  styleUrls: ['./wireframe-canvas.component.scss'],
})
export class WireframeCanvasComponent implements OnInit {
  @Input() isMaximized = false;
  @Output() toggleMaximize = new EventEmitter<void>();

  schema: WireframeSchema | null = null;
  loading = false;
  viewport: Viewport = 'desktop';
  history: WireframeSchema[] = [];
  code: GeneratedCode = { html: '', ts: '', scss: '' };

  tool: Tool = 'arrow';
  offsetX = 0;
  offsetY = 0;
  zoom = 1;
  isDragging = false;
  showViewportDropdown = false;

  private dragStartX = 0;
  private dragStartY = 0;
  private offsetStartX = 0;
  private offsetStartY = 0;

  readonly MIN_ZOOM = 0.1;
  readonly MAX_ZOOM = 3;

  readonly viewportOptions: ViewportOption[] = [
    { value: 'mobile',  label: 'Mobile',  width: 402,  height: 874,  icon: 'smartphone' },
    { value: 'tablet',  label: 'Tablet',  width: 1133, height: 744,  icon: 'tablet' },
    { value: 'desktop', label: 'Browser', width: 1440, height: 1024, icon: 'desktop_windows' },
  ];

  constructor(
    private readonly sanitizer: DomSanitizer,
    private readonly wireframeService: WireframeService,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngOnInit(): void {
    this.wireframeService.schema$.subscribe((s) => {
      this.schema = s;
      this.offsetX = 0;
      this.offsetY = 0;
      this.zoom = 1;
    });
    this.wireframeService.loading$.subscribe((l) => (this.loading = l));
    this.wireframeService.history$.subscribe((h) => (this.history = h));
    this.wireframeService.code$.subscribe(
      (c) => (this.code = c ?? { html: '', ts: '', scss: '' }),
    );
    this.wireframeService.error$.subscribe((msg) => {
      if (msg) {
        this.snackBar.open(msg, 'Dismiss', {
          duration: 5000,
          panelClass: ['canvas-error-snack'],
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
        });
      }
    });
  }

  get currentViewport(): ViewportOption {
    return this.viewportOptions.find(v => v.value === this.viewport)!;
  }

  get canvasWidth(): string {
    const v = this.currentViewport;
    return v.value === 'desktop' ? '100%' : `${v.width}px`;
  }

  get frameDimensionLabel(): string {
    const v = this.currentViewport;
    return `${v.width} × ${v.height}`;
  }

  get zoomPercent(): number {
    return Math.round(this.zoom * 100);
  }

  get versionLabel(): string {
    return `v${this.history.length || 1}`;
  }

  selectViewport(v: Viewport): void {
    this.viewport = v;
    this.showViewportDropdown = false;
  }

  toggleViewportDropdown(e: MouseEvent): void {
    e.stopPropagation();
    this.showViewportDropdown = !this.showViewportDropdown;
  }

  generateCode(): void {
    if (this.schema) this.wireframeService.generateCode(this.schema);
  }

  loadVersion(schema: WireframeSchema): void {
    this.wireframeService.loadVersion(schema);
  }

  selectTool(t: Tool): void {
    this.tool = t;
  }

  zoomIn(): void {
    this.zoom = Math.min(this.MAX_ZOOM, parseFloat((this.zoom + 0.1).toFixed(2)));
  }

  zoomOut(): void {
    this.zoom = Math.max(this.MIN_ZOOM, parseFloat((this.zoom - 0.1).toFixed(2)));
  }

  resetZoom(): void {
    this.zoom = 1;
    this.offsetX = 0;
    this.offsetY = 0;
  }

  onCanvasMouseDown(e: MouseEvent): void {
    if (this.tool !== 'hand') return;
    this.isDragging = true;
    this.dragStartX = e.clientX;
    this.dragStartY = e.clientY;
    this.offsetStartX = this.offsetX;
    this.offsetStartY = this.offsetY;
    e.preventDefault();
  }

  onCanvasMouseMove(e: MouseEvent): void {
    if (!this.isDragging) return;
    this.offsetX = this.offsetStartX + (e.clientX - this.dragStartX);
    this.offsetY = this.offsetStartY + (e.clientY - this.dragStartY);
  }

  onCanvasMouseUp(): void {
    this.isDragging = false;
  }

  onCanvasWheel(e: WheelEvent): void {
    if (!e.ctrlKey) return;
    e.preventDefault();
    const delta = e.deltaY > 0 ? -0.05 : 0.05;
    this.zoom = Math.min(this.MAX_ZOOM, Math.max(this.MIN_ZOOM,
      parseFloat((this.zoom + delta).toFixed(2))));
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.showViewportDropdown = false;
  }

  @HostListener('document:keydown', ['$event'])
  onKeyDown(e: KeyboardEvent): void {
    const tag = (e.target as HTMLElement).tagName;
    if (tag === 'TEXTAREA' || tag === 'INPUT') return;
    if (e.key === 'h' || e.key === 'H') this.tool = 'hand';
    if (e.key === 'v' || e.key === 'V') this.tool = 'arrow';
    if (e.key === 'Escape') { this.tool = 'arrow'; this.showViewportDropdown = false; }
    if (e.ctrlKey && e.key === '=') { this.zoomIn(); e.preventDefault(); }
    if (e.ctrlKey && e.key === '-') { this.zoomOut(); e.preventDefault(); }
    if (e.ctrlKey && e.key === '0') { this.resetZoom(); e.preventDefault(); }
  }

  safeHtml(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }
}
