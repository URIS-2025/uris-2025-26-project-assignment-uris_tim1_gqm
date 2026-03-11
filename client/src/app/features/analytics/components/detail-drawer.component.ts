import { Component, Input, Output, EventEmitter, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoalTreeNode, StrategyTreeNode } from '../../../core/api/api.models';

@Component({
    selector: 'app-detail-drawer',
    standalone: true,
    imports: [CommonModule],
    templateUrl: 'detail-drawer.component.html',
    styleUrl: 'detail-drawer.component.scss',
    encapsulation: ViewEncapsulation.None
})
export class DetailDrawerComponent {
    @Input() isOpen = false;
    @Input() selectedType: 'goal' | 'strategy' | null = null;
    @Input() selectedGoal: GoalTreeNode | null = null;
    @Input() selectedStrategy: StrategyTreeNode | null = null;
    @Output() close = new EventEmitter<void>();
}
