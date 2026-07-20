import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';
import { AgriSelectComponent } from '../../../shared/agri-select/agri-select';
import { CROP_OPTIONS, FARM_UNIT_OPTIONS, SOIL_OPTIONS } from '../../../shared/data/select-options';

@Component({
  selector: 'app-add-farm',
  imports: [RouterLink, SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent, AgriSelectComponent],
  templateUrl: './add-farm.html',
})
export class AddFarmPage implements OnInit {
  readonly unitOptions = FARM_UNIT_OPTIONS;
  readonly soilOptions = SOIL_OPTIONS;
  readonly cropOptions = CROP_OPTIONS;

  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeDashboard.css","/assets/css/AddFarm.css"], 'agri-page-dashboard', 'add-farm');
  }
}
