import { Component, OnInit } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { WireframeService } from '../../services/wireframe.service';
import { GeneratedCode } from '../../models/wireframe.model';

@Component({
  selector: 'app-code-preview',
  standalone: true,
  imports: [MatTabsModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './code-preview.component.html',
  styleUrls: ['./code-preview.component.scss'],
})
export class CodePreviewComponent implements OnInit {
  code: GeneratedCode | null = null;
  copiedTab: string | null = null;

  constructor(private wireframeService: WireframeService) {}

  ngOnInit(): void {
    this.wireframeService.code$.subscribe(c => (this.code = c));
  }

  copy(content: string, tab: string): void {
    navigator.clipboard.writeText(content).then(() => {
      this.copiedTab = tab;
      setTimeout(() => (this.copiedTab = null), 2000);
    });
  }

  download(content: string, filename: string): void {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  downloadAll(): void {
    if (!this.code) return;
    this.download(this.code.html, 'component.html');
    setTimeout(() => this.download(this.code!.ts, 'component.ts'), 200);
    setTimeout(() => this.download(this.code!.scss, 'component.scss'), 400);
  }
}
