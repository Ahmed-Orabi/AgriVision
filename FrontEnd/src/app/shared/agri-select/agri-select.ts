import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, Input } from '@angular/core';
import { SelectOption } from '../data/select-options';

@Component({
  selector: 'app-agri-select',
  imports: [CommonModule],
  host: {
    '[class.agri-select-open]': 'open',
  },
  template: `
    <div class="agri-select">
      <select
        class="agri-select-native"
        [id]="selectId"
        [attr.data-required]="required ? 'true' : null"
        [value]="value"
        (change)="setValue($any($event.target).value)"
        tabindex="-1"
        aria-hidden="true"
      >
        <option value="" [disabled]="required">{{ placeholder }}</option>
        <option *ngFor="let option of normalizedOptions" [value]="option.value">
          {{ option.label }}
        </option>
      </select>

      <button
        class="agri-select-trigger"
        type="button"
        [class.is-placeholder]="!value"
        [attr.aria-expanded]="open"
        [attr.aria-required]="required"
        [attr.aria-controls]="selectId + '-menu'"
        (click)="toggle($event)"
      >
        <span>{{ selectedLabel }}</span>
        <i class="bi bi-chevron-down" aria-hidden="true"></i>
      </button>

      <div class="agri-select-menu" [id]="selectId + '-menu'" role="listbox">
        <button
          class="agri-select-option"
          type="button"
          *ngFor="let option of normalizedOptions"
          [class.selected]="option.value === value"
          (click)="choose(option.value, $event)"
          role="option"
          [attr.aria-selected]="option.value === value"
        >
          <span>{{ option.label }}</span>
          <i class="bi bi-check2" aria-hidden="true"></i>
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        width: 100%;
        min-width: 0;
        position: relative;
      }

      :host.agri-select-open {
        z-index: 400;
      }

      .agri-select {
        position: relative;
        width: 100%;
        min-width: 0;
      }

      .agri-select-native {
        position: absolute;
        inset: 0;
        width: 100%;
        height: 100%;
        opacity: 0;
        pointer-events: none;
      }

      .agri-select-trigger {
        width: 100%;
        min-width: 0;
        min-height: 46px;
        border: 1px solid rgba(34, 118, 82, 0.22);
        border-radius: 16px;
        padding: 0 13px 0 15px;
        display: inline-flex;
        align-items: center;
        justify-content: space-between;
        gap: 12px;
        background:
          linear-gradient(135deg, rgba(236, 255, 246, 0.98), rgba(215, 245, 229, 0.92)),
          radial-gradient(circle at 12% 0%, rgba(118, 218, 163, 0.2), transparent 38%);
        color: #103927;
        box-shadow:
          inset 0 1px 0 rgba(255, 255, 255, 0.86),
          0 8px 20px rgba(17, 72, 50, 0.08);
        font: inherit;
        font-weight: 900;
        text-align: left;
        cursor: pointer;
        transition: transform 0.18s ease, border-color 0.18s ease, box-shadow 0.18s ease;
      }

      .agri-select-trigger span {
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      .agri-select-trigger i {
        flex: 0 0 auto;
        color: #27b879;
        font-size: 14px;
        transition: transform 0.18s ease;
      }

      .agri-select-trigger:hover,
      .agri-select-trigger:focus-visible {
        transform: translateY(-1px);
        border-color: rgba(39, 184, 121, 0.58);
        box-shadow:
          inset 0 1px 0 rgba(255, 255, 255, 0.9),
          0 0 0 4px rgba(48, 184, 125, 0.12),
          0 12px 26px rgba(17, 72, 50, 0.12);
        outline: none;
      }

      :host.agri-select-open .agri-select-trigger i {
        transform: rotate(180deg);
      }

      .agri-select-trigger.is-placeholder {
        color: rgba(16, 57, 39, 0.68);
      }

      .agri-select-menu {
        position: absolute;
        top: calc(100% + 8px);
        left: 0;
        right: 0;
        display: grid;
        gap: 4px;
        max-height: min(260px, 44dvh);
        overflow: auto;
        padding: 7px;
        border-radius: 18px;
        border: 1px solid rgba(39, 184, 121, 0.24);
        background:
          linear-gradient(180deg, rgba(249, 255, 251, 0.99), rgba(232, 248, 239, 0.98)),
          radial-gradient(circle at 100% 0%, rgba(132, 218, 168, 0.22), transparent 42%);
        box-shadow: 0 22px 45px rgba(8, 40, 27, 0.22);
        opacity: 0;
        visibility: hidden;
        transform: translateY(-6px) scale(0.98);
        transform-origin: top center;
        transition: opacity 0.16s ease, transform 0.16s ease, visibility 0.16s ease;
      }

      :host.agri-select-open .agri-select-menu {
        opacity: 1;
        visibility: visible;
        transform: translateY(0) scale(1);
      }

      .agri-select-option {
        min-height: 38px;
        border: 0;
        border-radius: 13px;
        background: transparent;
        color: #143c2b;
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 10px;
        padding: 0 11px;
        font: inherit;
        font-size: 14px;
        font-weight: 850;
        text-align: left;
        cursor: pointer;
        transition: background 0.16s ease, color 0.16s ease, transform 0.16s ease;
      }

      .agri-select-option i {
        opacity: 0;
        color: #168b5e;
      }

      .agri-select-option:hover,
      .agri-select-option:focus-visible {
        background: rgba(51, 178, 121, 0.12);
        color: #0b2d20;
        transform: translateX(2px);
        outline: none;
      }

      .agri-select-option.selected {
        background: linear-gradient(135deg, rgba(43, 177, 116, 0.22), rgba(141, 219, 171, 0.2));
        color: #0d4a31;
      }

      .agri-select-option.selected i {
        opacity: 1;
      }

      :host-context(body.theme-dark) .agri-select-trigger {
        border-color: rgba(137, 220, 178, 0.28);
        background:
          linear-gradient(135deg, rgba(34, 72, 53, 0.98), rgba(22, 52, 38, 0.98)),
          radial-gradient(circle at 15% 0%, rgba(86, 198, 135, 0.18), transparent 42%);
        color: #f0fbf5;
        box-shadow:
          inset 0 1px 0 rgba(255, 255, 255, 0.06),
          0 12px 26px rgba(0, 0, 0, 0.18);
      }

      :host-context(body.theme-dark) .agri-select-trigger.is-placeholder {
        color: rgba(224, 244, 234, 0.78);
      }

      :host-context(body.theme-dark) .agri-select-trigger:hover,
      :host-context(body.theme-dark) .agri-select-trigger:focus-visible {
        border-color: rgba(126, 229, 174, 0.6);
        box-shadow:
          0 0 0 4px rgba(98, 209, 150, 0.12),
          0 16px 30px rgba(0, 0, 0, 0.24);
      }

      :host-context(body.theme-dark) .agri-select-menu {
        border-color: rgba(128, 222, 173, 0.28);
        background:
          linear-gradient(180deg, rgba(24, 55, 40, 0.99), rgba(15, 42, 30, 0.99)),
          radial-gradient(circle at 100% 0%, rgba(92, 191, 134, 0.18), transparent 44%);
        box-shadow: 0 24px 45px rgba(0, 0, 0, 0.34);
      }

      :host-context(body.theme-dark) .agri-select-option {
        color: rgba(241, 250, 245, 0.92);
      }

      :host-context(body.theme-dark) .agri-select-option:hover,
      :host-context(body.theme-dark) .agri-select-option:focus-visible {
        background: rgba(124, 215, 165, 0.14);
        color: #ffffff;
      }

      :host-context(body.theme-dark) .agri-select-option.selected {
        background: linear-gradient(135deg, rgba(92, 201, 139, 0.28), rgba(39, 120, 82, 0.28));
        color: #ffffff;
      }

      @media (max-width: 768px) {
        .agri-select-trigger {
          min-height: 44px;
          border-radius: 15px;
          padding-inline: 13px;
          font-size: 13.5px;
        }

        .agri-select-menu {
          max-height: min(232px, 42dvh);
          border-radius: 16px;
          padding: 6px;
        }

        .agri-select-option {
          min-height: 36px;
          font-size: 13px;
          padding-inline: 10px;
        }
      }
    `,
  ],
})
export class AgriSelectComponent {
  @Input() selectId = '';
  @Input() placeholder = 'Select option';
  @Input() required = false;
  @Input() options: SelectOption[] = [];

  open = false;
  @Input() value = '';

  constructor(private elementRef: ElementRef<HTMLElement>) {}

  get normalizedOptions(): { label: string; value: string }[] {
    return this.options.map((option) =>
      typeof option === 'string' ? { label: option, value: option } : option,
    );
  }

  get selectedLabel(): string {
    return this.normalizedOptions.find((option) => option.value === this.value)?.label ?? this.placeholder;
  }

  toggle(event: MouseEvent): void {
    event.stopPropagation();
    this.open = !this.open;
  }

  choose(value: string, event: MouseEvent): void {
    event.stopPropagation();
    this.setValue(value);
    this.open = false;
  }

  setValue(value: string): void {
    this.value = value;
  }

  @HostListener('document:click', ['$event'])
  closeWhenClickingOutside(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target as Node)) {
      this.open = false;
    }
  }
}
