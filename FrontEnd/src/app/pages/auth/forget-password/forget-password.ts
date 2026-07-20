import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';

@Component({
  selector: 'app-forget-password',
  imports: [RouterLink],
  templateUrl: './forget-password.html',
})
export class ForgetPasswordPage implements OnInit {
  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeAuthentication.css"], 'agri-page-auth');
  }
}
