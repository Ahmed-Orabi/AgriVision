import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DashboardUiService } from '../../services/dashboard-ui';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
})
export class SidebarComponent {
  constructor(public ui: DashboardUiService, private router: Router) {}

  logout(): void {
    localStorage.removeItem('accessToken');
    sessionStorage.removeItem('accessToken');
    localStorage.removeItem('user');
    localStorage.removeItem('role');
    localStorage.removeItem('subscriptionStatus');
    localStorage.removeItem('isLoggedIn');
    this.router.navigateByUrl('/auth/login');
  }
}
