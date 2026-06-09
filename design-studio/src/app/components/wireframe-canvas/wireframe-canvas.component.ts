import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  AfterViewInit,
  AfterViewChecked,
  ViewChild,
  ElementRef,
  HostListener,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
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
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatSnackBarModule,
    WireframeBlockComponent,
  ],
  templateUrl: './wireframe-canvas.component.html',
  styleUrls: ['./wireframe-canvas.component.scss'],
})
export class WireframeCanvasComponent implements OnInit, AfterViewInit, AfterViewChecked, OnDestroy {
  @Input() isMaximized = false;
  @Input() isDark = false;
  @Output() toggleMaximize = new EventEmitter<void>();
  @Output() toggleTheme = new EventEmitter<void>();

  @ViewChild('previewFrame') private previewFrame?: ElementRef<HTMLIFrameElement>;
  @ViewChild('canvasBody') private canvasBodyRef?: ElementRef<HTMLDivElement>;

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

  resizeW: number | null = null;
  resizeH: number | null = null;
  isMouseOverCanvas = false;

  private dragStartX = 0;
  private dragStartY = 0;
  private offsetStartX = 0;
  private offsetStartY = 0;
  private activeHandle: string | null = null;
  private resizeStartX = 0;
  private resizeStartY = 0;
  private resizeStartW = 0;
  private resizeStartH = 0;

  private readonly onWheelHandler = (e: WheelEvent): void => {
    if (!e.ctrlKey) return;
    e.preventDefault();
    const rect = this.canvasBodyRef!.nativeElement.getBoundingClientRect();
    const cursorX = e.clientX - rect.left;
    const cursorY = e.clientY - rect.top;
    const delta = e.deltaY > 0 ? -0.05 : 0.05;
    const newZoom = Math.min(this.MAX_ZOOM, Math.max(this.MIN_ZOOM,
      parseFloat((this.zoom + delta).toFixed(2))));
    if (newZoom === this.zoom) return;
    const worldX = (cursorX - this.offsetX) / this.zoom;
    const worldY = (cursorY - this.offsetY) / this.zoom;
    this.zoom = newZoom;
    this.offsetX = cursorX - worldX * this.zoom;
    this.offsetY = cursorY - worldY * this.zoom;
  };

  readonly MIN_ZOOM = 0.1;
  readonly MAX_ZOOM = 3;

  readonly viewportOptions: ViewportOption[] = [
    { value: 'mobile',  label: 'Mobile',  width: 402,  height: 874,  icon: 'smartphone' },
    { value: 'tablet',  label: 'Tablet',  width: 1133, height: 744,  icon: 'tablet' },
    { value: 'desktop', label: 'Browser', width: 1440, height: 1024, icon: 'desktop_windows' },
  ];

  private lastPreviewKey = '';

  constructor(
    private readonly wireframeService: WireframeService,
    private readonly snackBar: MatSnackBar,
  ) {}

  ngAfterViewInit(): void {
    this.canvasBodyRef?.nativeElement.addEventListener('wheel', this.onWheelHandler, { passive: false });
  }

  ngOnDestroy(): void {
    this.canvasBodyRef?.nativeElement.removeEventListener('wheel', this.onWheelHandler);
  }

  ngOnInit(): void {
    this.wireframeService.schema$.subscribe((s) => {
      this.schema = s;
      this.resizeW = null;
      this.resizeH = null;
      setTimeout(() => this.fitToCanvas());
    });
    this.wireframeService.loading$.subscribe((l) => {
      this.loading = l;
      if (l) setTimeout(() => this.fitToCanvas());
    });
    this.wireframeService.history$.subscribe((h) => (this.history = h));
    this.wireframeService.code$.subscribe((c) => {
      this.code = c ?? { html: '', ts: '', scss: '' };
      this.lastPreviewKey = '';
      if (this.code.html) {
        setTimeout(() => { this.syncIframe(); this.fitToCanvas(); });
      }
    });
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

  get frameWidth(): string {
    return `${this.resizeW ?? this.currentViewport.width}px`;
  }

  get frameHeight(): string {
    return `${this.resizeH ?? this.currentViewport.height}px`;
  }

  get frameDimensionLabel(): string {
    const w = this.resizeW ?? this.currentViewport.width;
    const h = this.resizeH ?? this.currentViewport.height;
    return `${w} × ${h}`;
  }

  get zoomPercent(): number {
    return Math.round(this.zoom * 100);
  }

  get versionLabel(): string {
    return `v${this.history.length || 1}`;
  }

  selectViewport(v: Viewport): void {
    this.viewport = v;
    this.resizeW = null;
    this.resizeH = null;
    this.showViewportDropdown = false;
  }

  onHandleMouseDown(e: MouseEvent, handle: string): void {
    e.stopPropagation();
    e.preventDefault();
    this.activeHandle = handle;
    this.resizeStartX = e.clientX;
    this.resizeStartY = e.clientY;
    this.resizeStartW = this.resizeW ?? this.currentViewport.width;
    this.resizeStartH = this.resizeH ?? this.currentViewport.height;
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
    this.fitToCanvas();
  }

  fitToCanvas(): void {
    const el = this.canvasBodyRef?.nativeElement;
    if (!el) return;
    const w = el.clientWidth;
    const h = el.clientHeight;
    const fw = this.resizeW ?? this.currentViewport.width;
    const fh = this.resizeH ?? this.currentViewport.height;
    this.zoom = parseFloat(
      Math.min(this.MAX_ZOOM, Math.max(this.MIN_ZOOM, Math.min(w / fw, h / fh) * 0.85)).toFixed(2)
    );
    this.offsetX = (w - fw * this.zoom) / 2;
    this.offsetY = (h - fh * this.zoom) / 2;
  }

  onCanvasMouseEnter(): void { this.isMouseOverCanvas = true; }
  onCanvasMouseLeave(): void {
    this.isMouseOverCanvas = false;
    this.isDragging = false;
    this.activeHandle = null;
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
    if (this.activeHandle) {
      const dx = e.clientX - this.resizeStartX;
      const dy = e.clientY - this.resizeStartY;
      if (this.activeHandle.includes('r')) this.resizeW = Math.max(320, this.resizeStartW + dx);
      if (this.activeHandle.includes('l')) this.resizeW = Math.max(320, this.resizeStartW - dx);
      if (this.activeHandle.includes('b')) this.resizeH = Math.max(200, this.resizeStartH + dy);
      if (this.activeHandle.includes('t')) this.resizeH = Math.max(200, this.resizeStartH - dy);
      return;
    }
    if (!this.isDragging) return;
    this.offsetX = this.offsetStartX + (e.clientX - this.dragStartX);
    this.offsetY = this.offsetStartY + (e.clientY - this.dragStartY);
  }

  onCanvasMouseUp(): void {
    this.isDragging = false;
    this.activeHandle = null;
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
    if (this.isMouseOverCanvas && e.ctrlKey && e.key === '=') { this.zoomIn(); e.preventDefault(); }
    if (this.isMouseOverCanvas && e.ctrlKey && e.key === '-') { this.zoomOut(); e.preventDefault(); }
    if (this.isMouseOverCanvas && e.ctrlKey && e.key === '0') { this.resetZoom(); e.preventDefault(); }
  }

  ngAfterViewChecked(): void {
    this.syncIframe();
  }

  private syncIframe(): void {
    if (!this.previewFrame || !this.code.html) return;
    const key = this.code.html + this.code.scss;
    if (key === this.lastPreviewKey) return;
    this.lastPreviewKey = key;
    this.previewFrame.nativeElement.srcdoc = this.buildPreviewDoc();
  }

  private buildPreviewDoc(): string {
    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
  <style>
    *, *::before, *::after { box-sizing: border-box; }
    body { margin: 0; font-family: Inter, 'Segoe UI', sans-serif; font-size: 14px; background: #fff; }
    ${this.code.scss}
  </style>
</head>
<body>
  ${this.code.html}
  <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"><\/script>
</body>
</html>`;
  }
}
