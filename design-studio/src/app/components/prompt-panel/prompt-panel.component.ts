import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgClass, DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { WireframeService } from '../../services/wireframe.service';
import { ChatMessage } from '../../models/chat.model';

const EXAMPLE_PROMPTS = [
  'Create a dashboard with sidebar navigation, stats cards, and a data table',
  'Build a login page with email and password form',
  'Design a landing page with hero section and features grid',
  'Create a multi-step registration form',
];

@Component({
  selector: 'app-prompt-panel',
  standalone: true,
  imports: [FormsModule, NgClass, DatePipe, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './prompt-panel.component.html',
  styleUrls: ['./prompt-panel.component.scss'],
})
export class PromptPanelComponent implements OnInit, AfterViewChecked {
  @ViewChild('chatContainer') chatContainer!: ElementRef;

  messages: ChatMessage[] = [];
  input = '';
  loading = false;
  examples = EXAMPLE_PROMPTS;

  constructor(private wireframeService: WireframeService) {}

  ngOnInit(): void {
    this.wireframeService.loading$.subscribe(l => (this.loading = l));
    this.messages.push({
      id: '0',
      role: 'assistant',
      content: 'Hi! Describe the UI screen you want to build and I\'ll generate an interactive wireframe for you.',
      timestamp: new Date(),
    });
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  send(text?: string): void {
    const prompt = (text || this.input).trim();
    if (!prompt || this.loading) return;

    this.messages.push({ id: Date.now().toString(), role: 'user', content: prompt, timestamp: new Date() });
    this.input = '';

    const loadingId = (Date.now() + 1).toString();
    this.messages.push({ id: loadingId, role: 'assistant', content: '', timestamp: new Date(), isLoading: true });

    this.wireframeService.generateWireframe(prompt).subscribe(schema => {
      const idx = this.messages.findIndex(m => m.id === loadingId);
      if (idx > -1) {
        this.messages[idx] = {
          id: loadingId,
          role: 'assistant',
          content: `Generated **${schema.name}** wireframe with ${schema.mappings?.length ?? 0} mapped components. Click **Generate Code →** to get the Angular code.`,
          timestamp: new Date(),
          wireframeId: schema.id,
          isLoading: false,
        };
      }
    });
  }

  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  private scrollToBottom(): void {
    try {
      this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
    } catch {}
  }
}
