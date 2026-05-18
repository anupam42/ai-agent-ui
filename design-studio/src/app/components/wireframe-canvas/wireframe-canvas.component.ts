import { Component, Input, Output, EventEmitter, OnInit, HostListener } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { WireframeService } from '../../services/wireframe.service';
import { WireframeSchema } from '../../models/wireframe.model';
import { WireframeBlockComponent } from '../wireframe-block/wireframe-block.component';

type Viewport = 'desktop' | 'tablet' | 'mobile';
type Tool = 'arrow' | 'hand';

@Component({
  selector: 'app-wireframe-canvas',
  standalone: true,
  imports: [
    DatePipe, FormsModule,
    MatButtonModule, MatButtonToggleModule, MatIconModule, MatTooltipModule,
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

  // canvas pan state
  tool: Tool = 'arrow';
  offsetX = 0;
  offsetY = 0;
  isDragging = false;
  private dragStartX = 0;
  private dragStartY = 0;
  private offsetStartX = 0;
  private offsetStartY = 0;

  constructor(private wireframeService: WireframeService) {}

  ngOnInit(): void {
    this.wireframeService.schema$.subscribe(s => {
      this.schema = s;
      // re-centre when new schema loads
      this.offsetX = 0;
      this.offsetY = 0;
    });
    this.wireframeService.loading$.subscribe(l => (this.loading = l));
    this.wireframeService.history$.subscribe(h => (this.history = h));
  }

  generateCode(): void {
    if (this.schema) this.wireframeService.generateCode(this.schema);
  }

  loadVersion(schema: WireframeSchema): void {
    this.wireframeService.loadVersion(schema);
  }

  get canvasWidth(): string {
    if (this.viewport === 'tablet') return '768px';
    if (this.viewport === 'mobile') return '375px';
    return '100%';
  }

  selectTool(t: Tool): void {
    this.tool = t;
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

  @HostListener('document:keydown', ['$event'])
  onKeyDown(e: KeyboardEvent): void {
    const tag = (e.target as HTMLElement).tagName;
    if (tag === 'TEXTAREA' || tag === 'INPUT') return;
    if (e.key === 'h' || e.key === 'H') this.tool = 'hand';
    if (e.key === 'v' || e.key === 'V') this.tool = 'arrow';
    if (e.key === 'Escape') this.tool = 'arrow';
  }
}
