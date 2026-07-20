import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DashboardUiService } from '../../services/dashboard-ui';
import { ThemeService } from '../../services/theme';

@Component({
  selector: 'app-bottom-nav',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './bottom-nav.html',
})
export class BottomNavComponent {
  constructor(public ui: DashboardUiService, public theme: ThemeService) {}
}
