import { Component } from '@angular/core';
import { NgClass } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatRippleModule } from '@angular/material/core';
import { PromptPanelComponent } from './components/prompt-panel/prompt-panel.component';
import { WireframeCanvasComponent } from './components/wireframe-canvas/wireframe-canvas.component';
import { CodePreviewComponent } from './components/code-preview/code-preview.component';
import { MappingPanelComponent } from './components/mapping-panel/mapping-panel.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    NgClass,
    MatIconModule, MatButtonModule, MatTooltipModule, MatRippleModule,
    PromptPanelComponent, WireframeCanvasComponent,
    CodePreviewComponent, MappingPanelComponent,
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  readonly version = '0.1.0';
  isMaximized = false;

  toggleMaximize(): void {
    this.isMaximized = !this.isMaximized;
  }
}
