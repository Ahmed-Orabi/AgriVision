import { Component } from '@angular/core';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-theme-toggle',
  templateUrl: './theme-toggle.html',
  styleUrl: './theme-toggle.css',
})
export class ThemeToggleComponent {
  constructor(public theme: ThemeService) {}
}
