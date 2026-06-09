import { Component, Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
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
    MatIconModule, MatButtonModule, MatTooltipModule, MatRippleModule,
    PromptPanelComponent, WireframeCanvasComponent,
    CodePreviewComponent, MappingPanelComponent,
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  readonly version = '0.1.8';
  isMaximized = false;
  isDark = false;

  constructor(@Inject(DOCUMENT) private document: Document) {}

  toggleMaximize(): void {
    this.isMaximized = !this.isMaximized;
  }

  toggleTheme(): void {
    this.isDark = !this.isDark;
    this.document.documentElement.setAttribute(
      'data-theme',
      this.isDark ? 'dark' : 'light'
    );
  }
}
