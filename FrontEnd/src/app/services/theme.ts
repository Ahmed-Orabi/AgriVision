import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'agri-theme';
  private darkModeAllowed = true;
  isDark = false;

  constructor() {
    const saved = localStorage.getItem(this.storageKey);
    const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false;
    this.isDark = saved ? saved === 'dark' : prefersDark;
    this.apply();
  }

  toggle(): void {
    this.isDark = !this.isDark;
    localStorage.setItem(this.storageKey, this.isDark ? 'dark' : 'light');
    this.apply();
  }

  setDarkModeAllowed(allowed: boolean): void {
    this.darkModeAllowed = allowed;
    this.apply();
  }

  private apply(): void {
    const useDark = this.darkModeAllowed && this.isDark;
    document.body.classList.toggle('theme-dark', useDark);
    document.body.classList.toggle('theme-light', !useDark);
    document.documentElement.style.colorScheme = useDark ? 'dark' : 'light';
  }
}
