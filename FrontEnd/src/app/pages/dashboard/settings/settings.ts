import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';
import { AgriSelectComponent } from '../../../shared/agri-select/agri-select';
import { COUNTRY_OPTIONS } from '../../../shared/data/select-options';

@Component({
  selector: 'app-settings',
  imports: [RouterLink, SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent, AgriSelectComponent],
  templateUrl: './settings.html',
})
export class SettingsPage implements OnInit {
  readonly countryOptions = COUNTRY_OPTIONS;

  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeDashboard.css","/assets/css/Settings.css"], 'agri-page-dashboard', 'settings');
  }
}
