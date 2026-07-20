import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';

@Component({
  selector: 'app-password-reset-successfully',
  imports: [RouterLink],
  templateUrl: './password-reset-successfully.html',
})
export class PasswordResetSuccessfullyPage implements OnInit {
  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeAuthentication.css"], 'agri-page-auth');
  }
}
