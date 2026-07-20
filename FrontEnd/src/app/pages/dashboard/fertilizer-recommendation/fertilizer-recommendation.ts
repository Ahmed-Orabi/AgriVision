import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';
import { AgriSelectComponent } from '../../../shared/agri-select/agri-select';
import { CROP_OPTIONS, SOIL_OPTIONS } from '../../../shared/data/select-options';

@Component({
  selector: 'app-fertilizer-recommendation',
  imports: [SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent, AgriSelectComponent],
  templateUrl: './fertilizer-recommendation.html',
})
export class FertilizerRecommendationPage implements OnInit {
  readonly soilOptions = SOIL_OPTIONS;
  readonly cropOptions = CROP_OPTIONS;

  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeDashboard.css","/assets/css/FertilizerRecommendation.css"], 'agri-page-dashboard', 'fertilizer');
  }
}
