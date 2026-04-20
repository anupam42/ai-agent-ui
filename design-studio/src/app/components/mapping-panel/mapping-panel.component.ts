import { Component, OnInit } from '@angular/core';
import { NgClass } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { WireframeService } from '../../services/wireframe.service';
import { ComponentMapping } from '../../models/wireframe.model';

@Component({
  selector: 'app-mapping-panel',
  standalone: true,
  imports: [NgClass, MatIconModule, MatTooltipModule],
  templateUrl: './mapping-panel.component.html',
  styleUrls: ['./mapping-panel.component.scss'],
})
export class MappingPanelComponent implements OnInit {
  mappings: ComponentMapping[] = [];

  constructor(private wireframeService: WireframeService) {}

  ngOnInit(): void {
    this.wireframeService.schema$.subscribe(s => {
      this.mappings = s?.mappings ?? [];
    });
  }

  confidenceIcon(c: ComponentMapping['matchConfidence']): string {
    return c === 'exact' ? 'check_circle' : c === 'partial' ? 'adjust' : 'radio_button_unchecked';
  }

  confidenceLabel(c: ComponentMapping['matchConfidence']): string {
    return c === 'exact' ? 'Exact match' : c === 'partial' ? 'Partial match' : 'Fallback (Material)';
  }
}
