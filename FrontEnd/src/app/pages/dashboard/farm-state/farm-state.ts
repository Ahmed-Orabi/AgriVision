import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';
import { AgriSelectComponent } from '../../../shared/agri-select/agri-select';
import { FARM_OPTIONS } from '../../../shared/data/select-options';

@Component({
  selector: 'app-farm-state',
  imports: [RouterLink, SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent, AgriSelectComponent],
  templateUrl: './farm-state.html',
})
export class FarmStatePage implements OnInit {
  readonly farmOptions = FARM_OPTIONS;

  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeDashboard.css","/assets/css/FarmState.css"], 'agri-page-dashboard', 'farm-state');
  }
}
