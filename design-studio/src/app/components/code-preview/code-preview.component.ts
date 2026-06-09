import { Component, Input, OnInit } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { WireframeService } from '../../services/wireframe.service';
import { GeneratedCode } from '../../models/wireframe.model';
import { CodeEditorComponent } from '../code-editor/code-editor.component';

type Lang = 'angular' | 'typescript' | 'scss';

@Component({
  selector: 'app-code-preview',
  standalone: true,
  imports: [MatTabsModule, MatButtonModule, MatIconModule, MatTooltipModule, CodeEditorComponent],
  templateUrl: './code-preview.component.html',
  styleUrls: ['./code-preview.component.scss'],
})
export class CodePreviewComponent implements OnInit {
  @Input() isDark = false;

  code: GeneratedCode | null = null;
  copiedTab: string | null = null;
  formatting = false;

  htmlValue = '';
  tsValue   = '';
  scssValue = '';

  // Cached prettier imports — loaded once on first use
  private fmtFn: ((src: string, opts: object) => Promise<string>) | null = null;
  private plugins: Record<Lang, unknown[]> | null = null;

  constructor(private wireframeService: WireframeService) {}

  ngOnInit(): void {
    this.wireframeService.code$.subscribe(c => {
      this.code = c;
      if (c) {
        this.htmlValue = c.html;
        this.tsValue   = c.ts;
        this.scssValue = c.scss;
        this.formatAll();
      }
    });
  }

  async formatAll(): Promise<void> {
    this.formatting = true;
    try {
      const [html, ts, scss] = await Promise.all([
        this.fmt(this.htmlValue, 'angular'),
        this.fmt(this.tsValue,   'typescript'),
        this.fmt(this.scssValue, 'scss'),
      ]);
      this.htmlValue = html;
      this.tsValue   = ts;
      this.scssValue = scss;
    } finally {
      this.formatting = false;
    }
  }

  async formatTab(lang: Lang, tab: 'html' | 'ts' | 'scss'): Promise<void> {
    this.formatting = true;
    try {
      if (tab === 'html')  this.htmlValue  = await this.fmt(this.htmlValue,  lang);
      if (tab === 'ts')    this.tsValue    = await this.fmt(this.tsValue,    lang);
      if (tab === 'scss')  this.scssValue  = await this.fmt(this.scssValue,  lang);
    } finally {
      this.formatting = false;
    }
  }

  private async fmt(code: string, lang: Lang): Promise<string> {
    try {
      await this.loadPrettier();
      return await (this.fmtFn as Function)(code, {
        parser:     lang,
        plugins:    this.plugins![lang],
        printWidth: 100,
        tabWidth:   2,
        singleQuote: true,
      });
    } catch {
      return code; // fall back to original on any parse error
    }
  }

  private async loadPrettier(): Promise<void> {
    if (this.fmtFn) return;
    const [
      standalone,
      htmlMod,
      estreeMod,
      typescriptMod,
      postcssMod,
    ] = await Promise.all([
      import('prettier/standalone'),
      import('prettier/plugins/html'),
      import('prettier/plugins/estree'),
      import('prettier/plugins/typescript'),
      import('prettier/plugins/postcss'),
    ]);
    this.fmtFn  = (standalone as any).format;
    const html  = (htmlMod       as any).default ?? htmlMod;
    const est   = (estreeMod     as any).default ?? estreeMod;
    const ts    = (typescriptMod as any).default ?? typescriptMod;
    const css   = (postcssMod    as any).default ?? postcssMod;
    this.plugins = {
      angular:    [html],
      typescript: [est, ts],
      scss:       [css],
    };
  }

  copy(content: string, tab: string): void {
    navigator.clipboard.writeText(content).then(() => {
      this.copiedTab = tab;
      setTimeout(() => (this.copiedTab = null), 2000);
    });
  }

  download(content: string, filename: string): void {
    const blob = new Blob([content], { type: 'text/plain' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href     = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  downloadAll(): void {
    this.download(this.htmlValue, 'component.html');
    setTimeout(() => this.download(this.tsValue,   'component.ts'),   200);
    setTimeout(() => this.download(this.scssValue, 'component.scss'), 400);
  }
}
