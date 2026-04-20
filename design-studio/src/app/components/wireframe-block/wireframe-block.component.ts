import { Component, Input } from '@angular/core';
import { NgClass, NgStyle } from '@angular/common';
import { WireframeNode } from '../../models/wireframe.model';

@Component({
  selector: 'app-wireframe-block',
  standalone: true,
  imports: [NgClass, NgStyle, WireframeBlockComponent],
  templateUrl: './wireframe-block.component.html',
  styleUrls: ['./wireframe-block.component.scss'],
})
export class WireframeBlockComponent {
  @Input() node!: WireframeNode;
  @Input() depth: number = 0;

  getRows(count: number = 5): number[] {
    return Array.from({ length: count }, (_, i) => i);
  }

  firstTabChildren(node: WireframeNode): WireframeNode[] {
    return node.children?.[0]?.children ?? [];
  }
}
