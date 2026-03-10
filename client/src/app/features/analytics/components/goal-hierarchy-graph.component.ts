import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ViewEncapsulation, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxGraphModule, Node, Edge, Layout, GraphComponent } from '@swimlane/ngx-graph';
import { DagreLayout, Orientation } from '@swimlane/ngx-graph';
import { GoalTreeNode, StrategyTreeNode, GoalNodeData, StrategyNodeData } from '../../../core/api/api.models';

@Component({
    selector: 'app-goal-hierarchy-graph',
    standalone: true,
    imports: [CommonModule, NgxGraphModule],
    templateUrl: 'goal-hierarchy-graph.component.html',
    styleUrl: 'goal-hierarchy-graph.component.scss',
    encapsulation: ViewEncapsulation.None
})
export class GoalHierarchyGraphComponent implements OnChanges {
    @Input() rootGoal: GoalTreeNode | null = null;
    @Output() nodeSelected = new EventEmitter<{ type: 'goal' | 'strategy'; data: GoalTreeNode | StrategyTreeNode }>();

    nodes: Node[] = [];
    edges: Edge[] = [];
    layout: Layout = new DagreLayout();

    @ViewChild('graphRef') graphRef?: GraphComponent;

    constructor() {
        (this.layout as DagreLayout).settings = {
            orientation: Orientation.TOP_TO_BOTTOM,
            marginX: 40,
            marginY: 50,
            edgePadding: 0,
            rankPadding: 80,
            nodePadding: 50,
            multigraph: true,
            compound: true
        };
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['rootGoal']) {
            if (this.rootGoal) {
                this.buildGraph();
            } else {
                this.nodes = [];
                this.edges = [];
            }
        }
    }

    private buildGraph(): void {
        if (!this.rootGoal) return;
        this.nodes = [];
        this.edges = [];
        this.processGoalNode(this.rootGoal);
        // Trigger reference change so ngx-graph detects updates
        this.nodes = [...this.nodes];
        this.edges = [...this.edges];
    }

    private processGoalNode(goal: GoalTreeNode, parentStrategyId?: string): void {
        const goalNodeId = `goal-${goal.id}`;

        // Avoid duplicate nodes (DAG can reference same goal from multiple strategies)
        if (this.nodes.find(n => n.id === goalNodeId)) return;

        this.nodes.push({
            id: goalNodeId,
            label: goal.focus,
            dimension: { width: 270, height: 130 },
            data: { type: 'goal', goal } as GoalNodeData
        });

        if (parentStrategyId) {
            this.edges.push({
                id: `edge-${parentStrategyId}-${goalNodeId}`,
                source: parentStrategyId,
                target: goalNodeId,
                data: { isInfluence: false }
            });
        }

        for (const strategy of goal.strategies) {
            const stratNodeId = `strategy-${strategy.id}`;

            if (!this.nodes.find(n => n.id === stratNodeId)) {
                this.nodes.push({
                    id: stratNodeId,
                    label: strategy.name,
                    dimension: { width: 190, height: 72 },
                    data: { type: 'strategy', strategy, parentGoalId: goal.id } as StrategyNodeData
                });
            }

            // Goal → Strategy edge
            this.edges.push({
                id: `edge-${goalNodeId}-${stratNodeId}`,
                source: goalNodeId,
                target: stratNodeId,
                data: { isInfluence: false }
            });

            for (const childInfluence of strategy.childGoals) {
                const childGoalNodeId = `goal-${childInfluence.goal.id}`;

                // Strategy → Child Goal (influence edge)
                if (!this.edges.find(e => e.id === `edge-${stratNodeId}-${childGoalNodeId}`)) {
                    this.edges.push({
                        id: `edge-${stratNodeId}-${childGoalNodeId}`,
                        source: stratNodeId,
                        target: childGoalNodeId,
                        data: {
                            isInfluence: true,
                            influenceType: childInfluence.influenceType,
                            strength: childInfluence.strength,
                            confidence: childInfluence.confidence
                        }
                    });
                }

                this.processGoalNode(childInfluence.goal, stratNodeId);
            }
        }
    }

    onNodeClick(node: any): void {
        const data = node?.data;
        if (!data) return;
        if (data.type === 'goal') {
            this.nodeSelected.emit({ type: 'goal', data: (data as GoalNodeData).goal });
        } else if (data.type === 'strategy') {
            this.nodeSelected.emit({ type: 'strategy', data: (data as StrategyNodeData).strategy });
        }
    }

    triggerFit(): void {
        this.graphRef?.zoomToFit();
    }

    triggerCenter(): void {
        this.graphRef?.panToNodeId(this.nodes[0]?.id ?? '');
    }
}
