import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { delay, tap } from 'rxjs/operators';
import {
  WireframeSchema, WireframeNode, ComponentMapping,
  GeneratedCode, WireframeBlockType
} from '../models/wireframe.model';

const DASHBOARD_SCHEMA: WireframeNode = {
  type: 'column',
  children: [
    {
      type: 'header', label: 'App Header', sticky: true,
      mappedComponent: 'MatToolbar',
      children: [
        { type: 'logo', label: 'Logo', mappedComponent: 'BrandLogoComponent' },
        { type: 'nav', items: ['Dashboard', 'Reports', 'Analytics', 'Settings'], mappedComponent: 'MatButton' },
        { type: 'icon-button', label: 'Notifications', mappedComponent: 'MatIconButton' },
      ]
    },
    {
      type: 'row',
      children: [
        {
          type: 'sidebar', label: 'Navigation Sidebar', width: '240px',
          mappedComponent: 'MatSidenav',
          items: ['Overview', 'Analytics', 'Reports', 'Users', 'Settings']
        },
        {
          type: 'main', label: 'Main Content',
          children: [
            {
              type: 'row',
              children: [
                { type: 'stat-card', label: 'Total Users', value: '12,840', span: 3, mappedComponent: 'SummaryCardComponent' },
                { type: 'stat-card', label: 'Revenue', value: '$48,295', span: 3, mappedComponent: 'SummaryCardComponent' },
                { type: 'stat-card', label: 'Active Sessions', value: '1,284', span: 3, mappedComponent: 'SummaryCardComponent' },
                { type: 'stat-card', label: 'Conversion Rate', value: '3.6%', span: 3, mappedComponent: 'SummaryCardComponent' },
              ]
            },
            {
              type: 'data-table', label: 'Recent Orders', mappedComponent: 'MatTable',
              columns: ['ID', 'Customer', 'Amount', 'Status', 'Date'], rows: 5
            }
          ]
        }
      ]
    }
  ]
};

const LOGIN_SCHEMA: WireframeNode = {
  type: 'hero', label: 'Login Page',
  children: [
    {
      type: 'card', label: 'Login Card', mappedComponent: 'MatCard',
      children: [
        { type: 'logo', label: 'App Logo', mappedComponent: 'BrandLogoComponent' },
        { type: 'text', label: 'Welcome Back', placeholder: 'Sign in to your account' },
        {
          type: 'form', label: 'Login Form', mappedComponent: 'ReactiveFormsModule',
          children: [
            { type: 'input', label: 'Email', placeholder: 'Enter your email', mappedComponent: 'MatFormField' },
            { type: 'input', label: 'Password', placeholder: 'Enter password', mappedComponent: 'MatFormField' },
            { type: 'button', label: 'Sign In', mappedComponent: 'MatRaisedButton' },
          ]
        },
        { type: 'text', label: '', placeholder: 'Forgot password? · Create account' }
      ]
    }
  ]
};

const LANDING_SCHEMA: WireframeNode = {
  type: 'column',
  children: [
    {
      type: 'header', label: 'App Header', sticky: true, mappedComponent: 'MatToolbar',
      children: [
        { type: 'logo', label: 'Logo', mappedComponent: 'BrandLogoComponent' },
        { type: 'nav', items: ['Features', 'Pricing', 'About', 'Contact'], mappedComponent: 'MatButton' },
        { type: 'button', label: 'Get Started', mappedComponent: 'MatRaisedButton' }
      ]
    },
    {
      type: 'hero', label: 'Hero Section',
      children: [
        { type: 'text', label: 'Headline', placeholder: 'Build faster with AI' },
        { type: 'text', label: 'Subheadline', placeholder: 'Describe your UI and watch it come to life in seconds' },
        {
          type: 'row',
          children: [
            { type: 'button', label: 'Get Started Free', mappedComponent: 'MatRaisedButton' },
            { type: 'button', label: 'See Demo', mappedComponent: 'MatStrokedButton' }
          ]
        }
      ]
    },
    {
      type: 'grid', label: 'Features Grid',
      children: [
        { type: 'card', label: 'Feature 1', title: 'Fast Generation', mappedComponent: 'MatCard' },
        { type: 'card', label: 'Feature 2', title: 'Design System Aware', mappedComponent: 'MatCard' },
        { type: 'card', label: 'Feature 3', title: 'Export to Angular', mappedComponent: 'MatCard' },
      ]
    },
    { type: 'footer', label: 'Footer', mappedComponent: 'AppFooterComponent', items: ['Privacy', 'Terms', 'Contact'] }
  ]
};

const FORM_SCHEMA: WireframeNode = {
  type: 'column',
  children: [
    {
      type: 'header', label: 'App Header', mappedComponent: 'MatToolbar',
      children: [
        { type: 'logo', label: 'Logo', mappedComponent: 'BrandLogoComponent' },
        { type: 'text', label: 'Registration', placeholder: '' }
      ]
    },
    {
      type: 'main', label: 'Form Content',
      children: [
        {
          type: 'stepper', label: 'Multi-step Form', mappedComponent: 'MatStepper',
          children: [
            {
              type: 'card', label: 'Step 1 — Personal Info', mappedComponent: 'MatCard',
              children: [
                {
                  type: 'form', label: 'Personal Details', mappedComponent: 'ReactiveFormsModule',
                  children: [
                    { type: 'input', label: 'First Name', placeholder: 'Enter first name', mappedComponent: 'MatFormField' },
                    { type: 'input', label: 'Last Name', placeholder: 'Enter last name', mappedComponent: 'MatFormField' },
                    { type: 'input', label: 'Email', placeholder: 'Enter email address', mappedComponent: 'MatFormField' },
                    { type: 'input', label: 'Phone', placeholder: 'Enter phone number', mappedComponent: 'MatFormField' },
                    { type: 'button', label: 'Next →', mappedComponent: 'MatRaisedButton' }
                  ]
                }
              ]
            }
          ]
        }
      ]
    }
  ]
};

const DEFAULT_SCHEMA: WireframeNode = {
  type: 'column',
  children: [
    {
      type: 'header', label: 'App Header', mappedComponent: 'MatToolbar',
      children: [
        { type: 'logo', label: 'Logo' },
        { type: 'nav', items: ['Home', 'About', 'Contact'] }
      ]
    },
    {
      type: 'main', label: 'Main Content',
      children: [
        { type: 'text', label: 'Page Title', placeholder: 'Page Heading' },
        { type: 'card', label: 'Content Card', mappedComponent: 'MatCard',
          children: [{ type: 'text', placeholder: 'Main content area goes here' }]
        }
      ]
    },
    { type: 'footer', label: 'Footer', items: ['Privacy', 'Terms'] }
  ]
};

@Injectable({ providedIn: 'root' })
export class WireframeService {
  private _schema$ = new BehaviorSubject<WireframeSchema | null>(null);
  private _code$ = new BehaviorSubject<GeneratedCode | null>(null);
  private _history$ = new BehaviorSubject<WireframeSchema[]>([]);
  private _loading$ = new BehaviorSubject<boolean>(false);

  schema$ = this._schema$.asObservable();
  code$ = this._code$.asObservable();
  history$ = this._history$.asObservable();
  loading$ = this._loading$.asObservable();

  generateWireframe(prompt: string): Observable<WireframeSchema> {
    this._loading$.next(true);
    const schema = this.mockGenerate(prompt);
    return of(schema).pipe(
      delay(1600),
      tap(s => {
        this._schema$.next(s);
        this._loading$.next(false);
        const history = this._history$.getValue();
        this._history$.next([s, ...history].slice(0, 20));
      })
    );
  }

  generateCode(schema: WireframeSchema): GeneratedCode {
    const code: GeneratedCode = {
      html: this.buildHtml(schema.root, 0),
      ts: this.buildTs(schema),
      scss: this.buildScss(schema)
    };
    this._code$.next(code);
    return code;
  }

  loadVersion(schema: WireframeSchema): void {
    this._schema$.next(schema);
    this._code$.next(null);
  }

  private mockGenerate(prompt: string): WireframeSchema {
    const p = prompt.toLowerCase();
    let root: WireframeNode;
    let name: string;

    if (p.includes('dashboard') || p.includes('admin panel')) {
      root = DASHBOARD_SCHEMA; name = 'Dashboard';
    } else if (p.includes('login') || p.includes('sign in') || p.includes('auth')) {
      root = LOGIN_SCHEMA; name = 'Login Page';
    } else if (p.includes('landing') || p.includes('homepage') || p.includes('marketing')) {
      root = LANDING_SCHEMA; name = 'Landing Page';
    } else if (p.includes('register') || p.includes('signup') || p.includes('form')) {
      root = FORM_SCHEMA; name = 'Registration Form';
    } else {
      root = DEFAULT_SCHEMA; name = 'Page Layout';
    }

    return {
      id: Date.now().toString(),
      name,
      prompt,
      root,
      createdAt: new Date(),
      mappings: this.extractMappings(root)
    };
  }

  private extractMappings(node: WireframeNode): ComponentMapping[] {
    const mappings: ComponentMapping[] = [];
    const walk = (n: WireframeNode) => {
      if (n.mappedComponent) {
        mappings.push({
          blockLabel: n.label || n.type,
          blockType: n.type,
          componentName: n.mappedComponent,
          matchConfidence: n.mappedComponent.startsWith('Mat') ? 'fallback' : 'exact'
        });
      }
      n.children?.forEach(walk);
    };
    walk(node);
    return mappings;
  }

  private buildHtml(node: WireframeNode, depth: number): string {
    const indent = '  '.repeat(depth);
    const tag = this.htmlTag(node);
    if (!tag) return '';
    const children = (node.children || []).map(c => this.buildHtml(c, depth + 1)).filter(Boolean).join('\n');
    const inner = this.htmlInner(node, depth + 1);
    const content = inner || children;
    if (!content) return `${indent}${tag.open}${tag.close}`;
    return `${indent}${tag.open}\n${content}\n${indent}${tag.close}`;
  }

  private htmlTag(node: WireframeNode): { open: string; close: string } | null {
    const label = node.label || node.type;
    switch (node.type) {
      case 'column':    return { open: `<div class="d-flex flex-column gap-3">`, close: `</div>` };
      case 'row':       return { open: `<div class="d-flex flex-wrap gap-3">`, close: `</div>` };
      case 'grid':      return { open: `<div class="row row-cols-1 row-cols-md-3 g-3">`, close: `</div>` };
      case 'main':      return { open: `<main class="main-content flex-grow-1 p-3">`, close: `</main>` };
      case 'header':    return { open: `<mat-toolbar color="primary" class="app-toolbar">`, close: `</mat-toolbar>` };
      case 'sidebar':   return { open: `<mat-nav-list class="app-sidebar">`, close: `</mat-nav-list>` };
      case 'hero':      return { open: `<section class="hero-section">`, close: `</section>` };
      case 'footer':    return { open: `<footer class="app-footer">`, close: `</footer>` };
      case 'card':      return { open: `<mat-card>`, close: `</mat-card>` };
      case 'stat-card': return { open: `<mat-card class="stat-card">`, close: `</mat-card>` };
      case 'data-table':return { open: `<table mat-table [dataSource]="dataSource" class="mat-elevation-z2 w-100">`, close: `</table>` };
      case 'form':      return { open: `<form [formGroup]="form" (ngSubmit)="onSubmit()">`, close: `</form>` };
      case 'tabs':      return { open: `<mat-tab-group>`, close: `</mat-tab-group>` };
      case 'stepper':   return { open: `<mat-stepper linear>`, close: `</mat-stepper>` };
      case 'list':      return { open: `<mat-list>`, close: `</mat-list>` };
      case 'nav':       return { open: `<nav class="app-nav d-flex gap-2">`, close: `</nav>` };
      case 'logo':      return { open: `<span class="brand-logo">`, close: `</span>` };
      case 'button':    return { open: `<button mat-raised-button color="primary">`, close: `</button>` };
      case 'icon-button': return { open: `<button mat-icon-button>`, close: `</button>` };
      case 'text':      return { open: `<div class="text-block">`, close: `</div>` };
      case 'image':     return { open: `<div class="image-placeholder">`, close: `</div>` };
      case 'divider':   return { open: `<mat-divider>`, close: `</mat-divider>` };
      case 'search':    return { open: `<mat-form-field appearance="outline" class="search-field">`, close: `</mat-form-field>` };
      default:          return { open: `<div class="${node.type}-block">`, close: `</div>` };
    }
  }

  private htmlInner(node: WireframeNode, depth: number): string {
    const indent = '  '.repeat(depth);
    switch (node.type) {
      case 'sidebar':
        return (node.items || []).map(item =>
          `${indent}<a mat-list-item routerLink="/${item.toLowerCase()}">${item}</a>`
        ).join('\n');
      case 'nav':
        return (node.items || []).map(item =>
          `${indent}<a mat-button routerLink="/${item.toLowerCase()}">${item}</a>`
        ).join('\n');
      case 'stat-card':
        return `${indent}<mat-card-header>\n${indent}  <mat-card-title>${node.label}</mat-card-title>\n${indent}</mat-card-header>\n${indent}<mat-card-content>\n${indent}  <h2 class="stat-value">{{ ${this.toCamel(node.label || 'value')} }}</h2>\n${indent}</mat-card-content>`;
      case 'logo':
        return `${indent}{{ appName }}`;
      case 'button':
        return `${indent}${node.label || 'Submit'}`;
      case 'icon-button':
        return `${indent}<mat-icon>notifications</mat-icon>`;
      case 'text':
        return node.label ? `${indent}<h3>${node.label}</h3>\n${indent}<p>${node.placeholder || ''}</p>` : `${indent}<p>${node.placeholder || 'Content'}</p>`;
      case 'input':
        return `${indent}<mat-label>${node.label}</mat-label>\n${indent}<input matInput formControlName="${this.toCamel(node.label || 'field')}" placeholder="${node.placeholder || ''}">`;
      case 'data-table':
        return (node.columns || ['Column']).map(col =>
          `${indent}<ng-container matColumnDef="${this.toCamel(col)}">\n${indent}  <th mat-header-cell *matHeaderCellDef>${col}</th>\n${indent}  <td mat-cell *matCellDef="let row">{{ row.${this.toCamel(col)} }}</td>\n${indent}</ng-container>`
        ).join('\n') + `\n${indent}<tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>\n${indent}<tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>`;
      case 'footer':
        return (node.items || []).map(item => `${indent}<a href="#">${item}</a>`).join('\n');
      default:
        return '';
    }
  }

  private buildTs(schema: WireframeSchema): string {
    const name = schema.name.replace(/\s+/g, '');
    const columns = this.findTableColumns(schema.root);
    const stats = this.findStatCards(schema.root);
    return `import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-${schema.name.toLowerCase().replace(/\s+/g, '-')}',
  templateUrl: './${schema.name.toLowerCase().replace(/\s+/g, '-')}.component.html',
  styleUrls: ['./${schema.name.toLowerCase().replace(/\s+/g, '-')}.component.scss'],
})
export class ${name}Component implements OnInit {
  appName = '${schema.name}';
  form!: FormGroup;
${columns.length ? `  displayedColumns: string[] = [${columns.map(c => `'${this.toCamel(c)}'`).join(', ')}];
  dataSource = new MatTableDataSource<any>([]);` : ''}
${stats.map(s => `  ${this.toCamel(s.label || 'value')} = '${s.value || '0'}';`).join('\n')}

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  onSubmit(): void {
    if (this.form.valid) {
      console.log(this.form.value);
    }
  }
}
`;
  }

  private buildScss(schema: WireframeSchema): string {
    return `:host {
  display: block;
  height: 100%;
}

.app-toolbar {
  position: sticky;
  top: 0;
  z-index: 100;
  gap: 16px;

  .brand-logo {
    font-weight: 700;
    font-size: 1.2rem;
    flex: 1;
  }
}

.app-sidebar {
  width: 240px;
  min-height: calc(100vh - 64px);
  background: #fafafa;
  border-right: 1px solid #e0e0e0;
}

.main-content {
  flex: 1;
  overflow-y: auto;
}

.stat-card {
  .stat-value {
    font-size: 2rem;
    font-weight: 700;
    color: #3f51b5;
    margin: 8px 0 0;
  }
}

.hero-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 80px 24px;
  background: linear-gradient(135deg, #f5f7ff 0%, #e8eaf6 100%);
  gap: 24px;

  h3 { font-size: 2.5rem; font-weight: 700; margin: 0; }
}

.app-footer {
  display: flex;
  justify-content: center;
  gap: 24px;
  padding: 24px;
  background: #f5f5f5;
  border-top: 1px solid #e0e0e0;

  a { color: #666; text-decoration: none; font-size: 0.875rem; }
}

.image-placeholder {
  width: 100%;
  min-height: 200px;
  background: #e8e8e8;
  border: 2px dashed #bbb;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #999;
}
`;
  }

  private findTableColumns(node: WireframeNode): string[] {
    if (node.type === 'data-table') return node.columns || [];
    return (node.children || []).flatMap(c => this.findTableColumns(c));
  }

  private findStatCards(node: WireframeNode): WireframeNode[] {
    if (node.type === 'stat-card') return [node];
    return (node.children || []).flatMap(c => this.findStatCards(c));
  }

  private toCamel(s: string): string {
    return s.replace(/(?:^\w|[A-Z]|\b\w)/g, (w, i) => i === 0 ? w.toLowerCase() : w.toUpperCase()).replace(/\s+/g, '');
  }
}
