import { Injectable } from '@angular/core';
import { ThemeService } from './theme';

@Injectable({ providedIn: 'root' })
export class PageStylesService {
  private readonly managedAttribute = 'data-agri-page-style';
  private readonly managedHrefAttribute = 'data-agri-href';
  private readonly enhancementStylesheet = '/assets/css/angular-enhancements.css';
  private readonly knownStylesheets = [
    '/assets/css/themeAuthentication.css',
    '/assets/css/themeDashboard.css',
    '/assets/css/landing.css',
    '/assets/css/Analytics.css',
    '/assets/css/CropRecommendation.css',
    '/assets/css/FertilizerRecommendation.css',
    '/assets/css/DiseasePestDetection.css',
    '/assets/css/FarmState.css',
    '/assets/css/AddFarm.css',
    '/assets/css/Profile.css',
    '/assets/css/Settings.css',
    '/assets/css/Subscription.css',
    this.enhancementStylesheet,
  ];

  constructor(private theme: ThemeService) {
    this.knownStylesheets.forEach((href) => this.ensureLink(href));
  }

  setStyles(styles: string[], pageClass = '', pageKey = ''): void {
    document.body.className = document.body.className
      .split(/\s+/)
      .filter((className) => className && !className.startsWith('agri-page-'))
      .join(' ');
    delete document.body.dataset['page'];

    if (pageClass) {
      document.body.classList.add(pageClass);
    }
    if (pageKey) {
      document.body.dataset['page'] = pageKey;
    }
    this.theme.setDarkModeAllowed(pageClass === 'agri-page-dashboard');

    const activeStyles = new Set([...styles, this.enhancementStylesheet]);
    this.knownStylesheets.forEach((href) => this.setEnabled(href, activeStyles.has(href)));
  }

  private ensureLink(href: string): HTMLLinkElement {
    const existing = document.querySelector<HTMLLinkElement>(
      `link[${this.managedAttribute}][${this.managedHrefAttribute}="${href}"]`,
    );
    if (existing) return existing;

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    link.disabled = true;
    link.setAttribute(this.managedAttribute, 'true');
    link.setAttribute(this.managedHrefAttribute, href);
    document.head.appendChild(link);
    return link;
  }

  private setEnabled(href: string, enabled: boolean): void {
    const link = this.ensureLink(href);
    link.disabled = !enabled;
  }
}
