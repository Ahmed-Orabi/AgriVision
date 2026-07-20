import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';

@Component({
  selector: 'app-email-verification-sent',
  imports: [RouterLink],
  templateUrl: './email-verification-sent.html',
})
export class EmailVerificationSentPage implements OnInit {
  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeAuthentication.css"], 'agri-page-auth');
  }
}
