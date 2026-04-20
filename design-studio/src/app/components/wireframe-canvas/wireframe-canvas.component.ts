import { Component, OnInit } from '@angular/core';
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
  schema: WireframeSchema | null = null;
  loading = false;
  viewport: Viewport = 'desktop';
  history: WireframeSchema[] = [];

  constructor(private wireframeService: WireframeService) {}

  ngOnInit(): void {
    this.wireframeService.schema$.subscribe(s => (this.schema = s));
    this.wireframeService.loading$.subscribe(l => (this.loading = l));
    this.wireframeService.history$.subscribe(h => (this.history = h));
  }

  generateCode(): void {
    if (this.schema) {
      this.wireframeService.generateCode(this.schema);
    }
  }

  loadVersion(schema: WireframeSchema): void {
    this.wireframeService.loadVersion(schema);
  }

  get canvasWidth(): string {
    if (this.viewport === 'tablet') return '768px';
    if (this.viewport === 'mobile') return '375px';
    return '100%';
  }
}
