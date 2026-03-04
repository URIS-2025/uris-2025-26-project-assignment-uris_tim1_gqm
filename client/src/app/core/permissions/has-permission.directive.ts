import { Directive, Input, ElementRef, Renderer2, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { PermissionService } from './permission.service';
import { MatTooltip } from '@angular/material/tooltip';

export type PermissionBehavior = 'hide' | 'disable';

@Directive({
    selector: '[hasPermission]',
    standalone: true,
})
export class HasPermissionDirective implements OnInit, OnChanges {
    @Input('hasPermission') permission = '';
    @Input() permissionMode: PermissionBehavior = 'hide';

    constructor(
        private el: ElementRef,
        private renderer: Renderer2,
        private permissionService: PermissionService
    ) { }

    ngOnInit(): void {
        this._updateView();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['permission'] || changes['permissionMode']) {
            this._updateView();
        }
    }

    private _updateView(): void {
        if (!this.permission) return;

        const hasAccess = this.permissionService.has(this.permission);

        if (hasAccess) {
            // Restore visibility and state
            this.renderer.removeStyle(this.el.nativeElement, 'display');
            this.renderer.removeAttribute(this.el.nativeElement, 'disabled');
            this.renderer.removeClass(this.el.nativeElement, 'disabled-permission');
        } else {
            if (this.permissionMode === 'hide') {
                this.renderer.setStyle(this.el.nativeElement, 'display', 'none', 1); // 1 = !important flag internally
            } else if (this.permissionMode === 'disable') {
                this.renderer.setAttribute(this.el.nativeElement, 'disabled', 'true');
                this.renderer.addClass(this.el.nativeElement, 'disabled-permission');
                // Optional: add visual indicator or block pointer events manually if components don't respect disabled attribute
                this.renderer.setStyle(this.el.nativeElement, 'pointer-events', 'none');
                this.renderer.setStyle(this.el.nativeElement, 'opacity', '0.5');
            }
        }
    }
}
