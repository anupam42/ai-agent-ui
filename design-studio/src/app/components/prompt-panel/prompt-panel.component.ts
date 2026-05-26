import {
  Component, OnInit, ViewChild, ElementRef,
  AfterViewChecked, ChangeDetectionStrategy, DestroyRef, inject
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { WireframeService } from '../../services/wireframe.service';
import { ChatMessage } from '../../models/chat.model';

const EXAMPLE_PROMPTS = [
  { icon: 'dashboard',    text: 'Create a dashboard with sidebar navigation, stats cards, and a data table', tag: 'Dashboard' },
  { icon: 'login',        text: 'Build a login page with email and password form', tag: 'Auth' },
  { icon: 'web',          text: 'Design a landing page with hero section and features grid', tag: 'Marketing' },
  { icon: 'dynamic_form', text: 'Create a multi-step registration form', tag: 'Form' },
];

const WELCOME_MESSAGE = `Hey there! I'm your **NLW Design Studio** assistant.\n\nDescribe any UI screen and I'll generate a live wireframe + Angular code for you.\n\nPick an example below or type your own.`;

@Component({
  selector: 'app-prompt-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatSnackBarModule,
  ],
  templateUrl: './prompt-panel.component.html',
  styleUrls: ['./prompt-panel.component.scss'],
})
export class PromptPanelComponent implements OnInit, AfterViewChecked {
  @ViewChild('chatContainer') chatContainer!: ElementRef;
  @ViewChild('inputField')    inputField!: ElementRef<HTMLTextAreaElement>;

  private readonly destroyRef       = inject(DestroyRef);
  private readonly wireframeService = inject(WireframeService);
  private readonly snackBar         = inject(MatSnackBar);

  messages: ChatMessage[] = [];
  input        = '';
  loading      = false;
  readonly examples = EXAMPLE_PROMPTS;
  messageCount = 0;
  hoveredMsgId: string | null = null;
  copiedMsgId:  string | null = null;
  private shouldScroll = false;

  ngOnInit(): void {
    this.wireframeService.loading$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(l => (this.loading = l));
    this.pushAssistant('0', WELCOME_MESSAGE);
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  send(text?: string): void {
    const prompt = (text || this.input).trim();
    if (!prompt || this.loading) return;

    this.messages.push({
      id: Date.now().toString(),
      role: 'user',
      content: prompt,
      timestamp: new Date(),
    });
    this.input = '';
    this.messageCount++;
    this.shouldScroll = true;
    this.resetTextarea();

    const loadingId = (Date.now() + 1).toString();
    this.messages.push({
      id: loadingId,
      role: 'assistant',
      content: '',
      timestamp: new Date(),
      isLoading: true,
    });

    this.wireframeService
      .generateWireframe(prompt)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: schema => {
          const idx = this.messages.findIndex(m => m.id === loadingId);
          if (idx > -1) {
            this.messages[idx] = {
              id: loadingId,
              role: 'assistant',
              content: `Your **${schema.name}** wireframe is on the canvas. Hit **Generate Code →** to get the Angular components.`,
              timestamp: new Date(),
              wireframeId: schema.id,
              isLoading: false,
            };
          }
          this.messageCount++;
          this.shouldScroll = true;
        },
        error: () => {
          const idx = this.messages.findIndex(m => m.id === loadingId);
          if (idx > -1) {
            this.messages[idx] = {
              id: loadingId,
              role: 'assistant',
              content: 'Something went wrong. Please try again.',
              timestamp: new Date(),
              isLoading: false,
            };
          }
          this.shouldScroll = true;
        },
      });
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  autoResize(event: Event): void {
    const el = event.target as HTMLTextAreaElement;
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 120) + 'px';
  }

  clearChat(): void {
    this.messages = [];
    this.pushAssistant('0', WELCOME_MESSAGE);
    this.messageCount = 0;
    this.shouldScroll = true;
  }

  copyMessage(msg: ChatMessage): void {
    const plain = msg.content.replace(/\*\*(.+?)\*\*/g, '$1');
    navigator.clipboard.writeText(plain).then(() => {
      this.copiedMsgId = msg.id;
      this.snackBar.open('Copied to clipboard', '', { duration: 1500 });
      setTimeout(() => (this.copiedMsgId = null), 2000);
    });
  }

  toHtml(text: string): string {
    if (!text) return '';
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/`(.+?)`/g, '<code>$1</code>')
      .replace(/\n/g, '<br>');
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  get showExamples(): boolean {
    return this.messages.length <= 1;
  }

  private pushAssistant(id: string, content: string): void {
    this.messages.push({ id, role: 'assistant', content, timestamp: new Date() });
  }

  private scrollToBottom(): void {
    try {
      const el = this.chatContainer.nativeElement;
      el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' });
    } catch {}
  }

  private resetTextarea(): void {
    if (this.inputField?.nativeElement) {
      this.inputField.nativeElement.style.height = 'auto';
    }
  }
}