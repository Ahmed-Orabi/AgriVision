import { RouterLink } from '@angular/router';
import { Component } from '@angular/core';
import { DashboardUiService } from '../../services/dashboard-ui';
import { ThemeToggleComponent } from '../../shared/theme-toggle/theme-toggle';

@Component({
  selector: 'app-top-navbar',
  imports: [RouterLink, ThemeToggleComponent],
  templateUrl: './top-navbar.html',
})
export class TopNavbarComponent {
  constructor(public ui: DashboardUiService) {}
}
