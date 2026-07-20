import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';
import { AgriSelectComponent } from '../../../shared/agri-select/agri-select';
import {
  GROWTH_STAGE_OPTIONS,
  IRRIGATION_CROP_OPTIONS,
  IRRIGATION_SOIL_OPTIONS,
  SEASON_OPTIONS,
} from '../../../shared/data/select-options';

@Component({
  selector: 'app-irrigation-recommendation',
  imports: [SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent, AgriSelectComponent],
  templateUrl: './irrigation-recommendation.html',
})
export class IrrigationRecommendationPage implements OnInit {
  readonly soilOptions = IRRIGATION_SOIL_OPTIONS;
  readonly irrigationCropOptions = IRRIGATION_CROP_OPTIONS;
  readonly growthStageOptions = GROWTH_STAGE_OPTIONS;
  readonly seasonOptions = SEASON_OPTIONS;

  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(
      ['/assets/css/themeDashboard.css', '/assets/css/FertilizerRecommendation.css'],
      'agri-page-dashboard',
      'irrigation',
    );
  }
}
