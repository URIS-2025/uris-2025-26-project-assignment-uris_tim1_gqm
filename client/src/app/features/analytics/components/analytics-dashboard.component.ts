import { Component, Input, OnChanges, SimpleChanges, ViewEncapsulation, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxChartsModule, LegendPosition, Color, ScaleType } from '@swimlane/ngx-charts';
import { GoalAnalytics, GoalInsightSimple, StrategyInsightSimple } from '../../../core/api/api.models';

interface ChartData {
    name: string;
    value: number;
}

@Component({
    selector: 'app-analytics-dashboard',
    standalone: true,
    imports: [CommonModule, NgxChartsModule],
    templateUrl: 'analytics-dashboard.component.html',
    styleUrl: 'analytics-dashboard.component.scss',
    encapsulation: ViewEncapsulation.None
})
export class AnalyticsDashboardComponent implements OnChanges {
    @Input() analytics: GoalAnalytics | null = null;

    statusData: ChartData[] = [];
    probabilityData: ChartData[] = [];
    depthData: ChartData[] = [];
    refinementData: ChartData[] = [];

    // Fixed chart view sizes for ngx-charts (required for proper rendering)
    chartViewDonut: [number, number] = [260, 200];
    chartViewBar: [number, number] = [260, 160];

    legendBelow = LegendPosition.Below;

    statusColorScheme: Color = {
        name: 'statusColors',
        selectable: true,
        group: ScaleType.Ordinal,
        domain: ['#3F51B5', '#2CB1A1', '#94a3b8', '#ef4444', '#f59e0b']
    };

    probabilityColorScheme: Color = {
        name: 'probabilityColors',
        selectable: true,
        group: ScaleType.Ordinal,
        domain: ['#3F51B5', '#4f64c7', '#6074d3', '#7084df', '#8094eb']
    };

    depthColorScheme: Color = {
        name: 'depthColors',
        selectable: true,
        group: ScaleType.Ordinal,
        domain: ['#2CB1A1', '#38c4b4', '#44d7c7', '#50ead9', '#5cf2e2']
    };

    refinementColorScheme: Color = {
        name: 'refinementColors',
        selectable: true,
        group: ScaleType.Ordinal,
        domain: ['#3F51B5', '#2CB1A1']
    };

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['analytics'] && this.analytics) {
            this.prepareChartData();
        }
    }

    private prepareChartData(): void {
        if (!this.analytics) return;

        // Status distribution
        this.statusData = Object.entries(this.analytics.statusDistribution)
            .filter(([, value]) => value > 0)
            .map(([key, value]) => ({ name: key, value }));

        // Probability distribution (use full ranges including zeros for context)
        this.probabilityData = Object.entries(this.analytics.probabilityDistribution)
            .map(([key, value]) => ({ name: key, value }));

        // Depth distribution
        this.depthData = Object.entries(this.analytics.depthDistribution)
            .map(([key, value]) => ({ name: `L${key}`, value }));

        // Refinement distribution — uses correct field name from API
        this.refinementData = Object.entries(this.analytics.refinementDistribution)
            .filter(([, value]) => value > 0)
            .map(([key, value]) => ({ name: key, value }));
    }
}
