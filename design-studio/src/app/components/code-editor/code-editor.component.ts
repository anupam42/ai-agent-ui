import {
  Component, Input, Output, EventEmitter,
  AfterViewInit, OnChanges, OnDestroy,
  ViewChild, ElementRef, SimpleChanges, NgZone,
} from '@angular/core';
import { EditorView, basicSetup } from 'codemirror';
import { EditorState, Extension } from '@codemirror/state';
import { html } from '@codemirror/lang-html';
import { javascript } from '@codemirror/lang-javascript';
import { css } from '@codemirror/lang-css';
import { oneDark } from '@codemirror/theme-one-dark';

const lightTheme = EditorView.theme({
  '&': { backgroundColor: '#f6f8fa', color: '#24292e' },
  '&.cm-focused': { outline: 'none' },
  '.cm-content': { caretColor: '#24292e', padding: '0' },
  '.cm-gutters': {
    backgroundColor: '#f0f1f3',
    color: '#9ca3af',
    borderRight: '1px solid #e5e7eb',
  },
  '.cm-activeLineGutter': { backgroundColor: '#e8eaed' },
  '.cm-activeLine': { backgroundColor: 'rgba(0,0,0,0.035)' },
  '&.cm-focused .cm-cursor': { borderLeftColor: '#24292e' },
  '&.cm-focused .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection': {
    backgroundColor: 'rgba(99,102,241,0.2)',
  },
  '.cm-gutterElement': { padding: '0 10px 0 6px' },
  '.cm-foldGutter': { width: '14px' },
}, { dark: false });

function langFor(lang: string): Extension {
  if (lang === 'html') return html();
  if (lang === 'typescript') return javascript({ typescript: true });
  return css();
}

@Component({
  selector: 'app-code-editor',
  standalone: true,
  template: `<div #host class="cm-wrap"></div>`,
  styles: [`
    :host {
      display: flex;
      flex: 1;
      min-height: 0;
      overflow: hidden;
    }
    .cm-wrap {
      flex: 1;
      overflow: hidden;
    }
    :host ::ng-deep .cm-editor {
      height: 100%;
      font-size: 12px;
    }
    :host ::ng-deep .cm-scroller {
      font-family: 'JetBrains Mono', ui-monospace, monospace !important;
      overflow: auto;
      line-height: 1.7;
    }
    :host ::ng-deep .cm-focused {
      outline: none !important;
    }
  `],
})
export class CodeEditorComponent implements AfterViewInit, OnChanges, OnDestroy {
  @Input() value = '';
  @Input() language = 'html';
  @Input() isDark = false;
  @Output() valueChange = new EventEmitter<string>();

  @ViewChild('host') host!: ElementRef<HTMLElement>;
  private view: EditorView | null = null;

  constructor(private zone: NgZone) {}

  ngAfterViewInit(): void {
    this.zone.runOutsideAngular(() => this.build());
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.view) return;

    if (changes['isDark'] || changes['language']) {
      const current = this.view.state.doc.toString();
      this.view.destroy();
      this.zone.runOutsideAngular(() => this.build(current));
      return;
    }

    if (changes['value'] && !changes['value'].firstChange) {
      const current = this.view.state.doc.toString();
      if (current !== this.value) {
        this.view.dispatch({
          changes: { from: 0, to: current.length, insert: this.value },
        });
      }
    }
  }

  ngOnDestroy(): void {
    this.view?.destroy();
  }

  private build(doc = this.value): void {
    const state = EditorState.create({
      doc,
      extensions: [
        basicSetup,
        langFor(this.language),
        this.isDark ? oneDark : lightTheme,
        EditorView.updateListener.of(upd => {
          if (upd.docChanged) {
            this.zone.run(() => this.valueChange.emit(upd.state.doc.toString()));
          }
        }),
      ],
    });

    this.view = new EditorView({ state, parent: this.host.nativeElement });
  }
}
