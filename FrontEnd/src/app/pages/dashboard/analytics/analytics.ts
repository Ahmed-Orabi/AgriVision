import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';
import { AgriSelectComponent } from '../../../shared/agri-select/agri-select';
import { FARM_OPTIONS } from '../../../shared/data/select-options';

@Component({
  selector: 'app-analytics',
  imports: [SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent, AgriSelectComponent],
  templateUrl: './analytics.html',
})
export class AnalyticsPage implements OnInit {
  readonly farmOptions = FARM_OPTIONS;

  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeDashboard.css","/assets/css/Analytics.css"], 'agri-page-dashboard', 'analytics');
  }
}
