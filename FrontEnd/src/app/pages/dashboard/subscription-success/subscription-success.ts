import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';
import { SidebarComponent } from '../../../layout/sidebar/sidebar';
import { TopNavbarComponent } from '../../../layout/top-navbar/top-navbar';
import { FooterComponent } from '../../../layout/footer/footer';
import { BottomNavComponent } from '../../../layout/bottom-nav/bottom-nav';

@Component({
  selector: 'app-subscription-success',
  imports: [RouterLink, SidebarComponent, TopNavbarComponent, FooterComponent, BottomNavComponent],
  templateUrl: './subscription-success.html',
})
export class SubscriptionSuccessPage implements OnInit {
  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeDashboard.css","/assets/css/Subscription.css"], 'agri-page-dashboard', 'subscription-success');
  }
}
