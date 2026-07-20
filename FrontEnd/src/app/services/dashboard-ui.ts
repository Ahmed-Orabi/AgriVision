import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class DashboardUiService {
  sidebarOpen = false;
  notificationsOpen = false;

  openSidebar(): void {
    this.sidebarOpen = true;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  toggleNotifications(): void {
    this.notificationsOpen = !this.notificationsOpen;
  }

  closeNotifications(): void {
    this.notificationsOpen = false;
  }

  closeAll(): void {
    this.sidebarOpen = false;
    this.notificationsOpen = false;
  }
}
