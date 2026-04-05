import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [FormsModule],
})
export class AppComponent {
  @ViewChild('codeBlock') codeBlock!: ElementRef;

  public message = '';
  public chatHistory: string[] = [];

  public visible = {
    chat: true,
    design: false,
    code: false,
  };

  public sendMessage(): void {
    if (!this.message.trim()) return;

    this.chatHistory.push(this.message);

    const dummyResponse = `Here's your design..`;
    this.chatHistory.push(dummyResponse);

    this.visible.design = true;
    this.visible.code = true;

    this.message = '';
  }

  public toggle(panel: 'chat' | 'design' | 'code'): void {
    this.visible[panel] = !this.visible[panel];
  }

  public async copyCode() {
    const code = this.codeBlock.nativeElement.innerText;

    try {
      await navigator.clipboard.writeText(code);
      console.log('✅ Code copied!');
      alert('Copied to clipboard ✔️');
    } catch (err) {
      console.error('❌ Copy failed:', err);
    }
  }
}
