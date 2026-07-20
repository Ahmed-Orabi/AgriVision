import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';

@Component({
  selector: 'app-plant-growth-stage',
  imports: [SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent],
  templateUrl: './plant-growth-stage.html',
})
export class PlantGrowthStagePage implements OnInit {
  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeDashboard.css", "/assets/css/DiseasePestDetection.css"], 'agri-page-dashboard', 'plant-growth-stage');
  }
}
