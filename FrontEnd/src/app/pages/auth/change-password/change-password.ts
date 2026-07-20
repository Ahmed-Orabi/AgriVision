import { RouterLink } from '@angular/router';
import { Component, OnInit } from '@angular/core';
import { PageStylesService } from '../../../services/page-styles';

@Component({
  selector: 'app-change-password',
  imports: [RouterLink],
  templateUrl: './change-password.html',
})
export class ChangePasswordPage implements OnInit {
  constructor(private pageStyles: PageStylesService) {}

  ngOnInit(): void {
    this.pageStyles.setStyles(["/assets/css/themeAuthentication.css"], 'agri-page-auth');
  }
}
