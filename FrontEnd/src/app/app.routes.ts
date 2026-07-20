import { Routes } from '@angular/router';
import { LandingPage } from './pages/landing/landing/landing';
import { LoginPage } from './pages/auth/login/login';
import { RegisterPage } from './pages/auth/register/register';
import { ForgetPasswordPage } from './pages/auth/forget-password/forget-password';
import { ResetPasswordPage } from './pages/auth/reset-password/reset-password';
import { ChangePasswordPage } from './pages/auth/change-password/change-password';
import { EmailVerificationSentPage } from './pages/auth/email-verification-sent/email-verification-sent';
import { EmailVerifiedSuccessfullyPage } from './pages/auth/email-verified-successfully/email-verified-successfully';
import { PasswordResetSuccessfullyPage } from './pages/auth/password-reset-successfully/password-reset-successfully';
import { ResendEmailVerificationPage } from './pages/auth/resend-email-verification/resend-email-verification';
import { AnalyticsPage } from './pages/dashboard/analytics/analytics';
import { CropRecommendationPage } from './pages/dashboard/crop-recommendation/crop-recommendation';
import { FertilizerRecommendationPage } from './pages/dashboard/fertilizer-recommendation/fertilizer-recommendation';
import { IrrigationRecommendationPage } from './pages/dashboard/irrigation-recommendation/irrigation-recommendation';
import { PlantGrowthStagePage } from './pages/dashboard/plant-growth-stage/plant-growth-stage';
import { DiseaseDetectionPage } from './pages/dashboard/disease-detection/disease-detection';
import { PestDetectionPage } from './pages/dashboard/pest-detection/pest-detection';
import { DiseasePestDetectionPage } from './pages/dashboard/disease-pest-detection/disease-pest-detection';
import { FarmStatePage } from './pages/dashboard/farm-state/farm-state';
import { AddFarmPage } from './pages/dashboard/add-farm/add-farm';
import { EditFarmPage } from './pages/dashboard/edit-farm/edit-farm';
import { ProfilePage } from './pages/dashboard/profile/profile';
import { SettingsPage } from './pages/dashboard/settings/settings';
import { SettingsPasswordPage } from './pages/dashboard/settings-password/settings-password';
import { SubscriptionSuccessPage } from './pages/dashboard/subscription-success/subscription-success';
import { NotificationsPage } from './pages/dashboard/notifications/notifications';

export const routes: Routes = [
  { path: '', component: LandingPage },
  { path: 'auth/login', component: LoginPage },
  { path: 'auth/register', component: RegisterPage },
  { path: 'auth/forgot-password', component: ForgetPasswordPage },
  { path: 'auth/reset-password', component: ResetPasswordPage },
  { path: 'auth/change-password', component: ChangePasswordPage },
  { path: 'auth/email-verification-sent', component: EmailVerificationSentPage },
  { path: 'auth/email-verified-successfully', component: EmailVerifiedSuccessfullyPage },
  { path: 'auth/password-reset-successfully', component: PasswordResetSuccessfullyPage },
  { path: 'auth/resend-email-verification', component: ResendEmailVerificationPage },
  { path: 'dashboard/analytics', component: AnalyticsPage },
  { path: 'dashboard/ai/crop-recommendation', component: CropRecommendationPage },
  { path: 'dashboard/ai/fertilizer-recommendation', component: FertilizerRecommendationPage },
  { path: 'dashboard/ai/irrigation-recommendation', component: IrrigationRecommendationPage },
  { path: 'dashboard/ai/plant-growth-stage', component: PlantGrowthStagePage },
  { path: 'dashboard/ai/disease-detection', component: DiseaseDetectionPage },
  { path: 'dashboard/ai/pest-detection', component: PestDetectionPage },
  { path: 'dashboard/ai/disease-pest-detection', component: DiseasePestDetectionPage },
  { path: 'dashboard/farm/state', component: FarmStatePage },
  { path: 'dashboard/farm/add', component: AddFarmPage },
  { path: 'dashboard/farm/edit', component: EditFarmPage },
  { path: 'dashboard/user/profile', component: ProfilePage },
  { path: 'dashboard/user/settings', component: SettingsPage },
  { path: 'dashboard/user/settings-password', component: SettingsPasswordPage },
  { path: 'dashboard/user/subscription-success', component: SubscriptionSuccessPage },
  { path: 'dashboard/notifications', component: NotificationsPage },
  { path: 'index.html', redirectTo: '', pathMatch: 'full' },
  { path: 'Authentication/Login.html', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: 'Authentication/Register.html', redirectTo: 'auth/register', pathMatch: 'full' },
  { path: 'Authentication/ForgetPassword.html', redirectTo: 'auth/forgot-password', pathMatch: 'full' },
  { path: 'Authentication/ResetPassword.html', redirectTo: 'auth/reset-password', pathMatch: 'full' },
  { path: 'Authentication/ChangePassword.html', redirectTo: 'auth/change-password', pathMatch: 'full' },
  { path: 'Authentication/EmailVerificationSent.html', redirectTo: 'auth/email-verification-sent', pathMatch: 'full' },
  { path: 'Authentication/EmailVerifiedSuccessfully.html', redirectTo: 'auth/email-verified-successfully', pathMatch: 'full' },
  { path: 'Authentication/PasswordResetSuccessfully.html', redirectTo: 'auth/password-reset-successfully', pathMatch: 'full' },
  { path: 'Authentication/ResendEmailVerification.html', redirectTo: 'auth/resend-email-verification', pathMatch: 'full' },
  { path: 'Dashboard/analytics/Analytics.html', redirectTo: 'dashboard/analytics', pathMatch: 'full' },
  { path: 'Dashboard/ai/CropRecommendation.html', redirectTo: 'dashboard/ai/crop-recommendation', pathMatch: 'full' },
  { path: 'Dashboard/ai/FertilizerRecommendation.html', redirectTo: 'dashboard/ai/fertilizer-recommendation', pathMatch: 'full' },
  { path: 'Dashboard/ai/IrrigationRecommendation.html', redirectTo: 'dashboard/ai/irrigation-recommendation', pathMatch: 'full' },
  { path: 'Dashboard/ai/PlantGrowthStage.html', redirectTo: 'dashboard/ai/plant-growth-stage', pathMatch: 'full' },
  { path: 'Dashboard/ai/DiseaseDetection.html', redirectTo: 'dashboard/ai/disease-detection', pathMatch: 'full' },
  { path: 'Dashboard/ai/PestDetection.html', redirectTo: 'dashboard/ai/pest-detection', pathMatch: 'full' },
  { path: 'Dashboard/ai/DiseasePestDetection.html', redirectTo: 'dashboard/ai/disease-pest-detection', pathMatch: 'full' },
  { path: 'Dashboard/farm/FarmState.html', redirectTo: 'dashboard/farm/state', pathMatch: 'full' },
  { path: 'Dashboard/farm/AddFarm.html', redirectTo: 'dashboard/farm/add', pathMatch: 'full' },
  { path: 'Dashboard/farm/EditFarm.html', redirectTo: 'dashboard/farm/edit', pathMatch: 'full' },
  { path: 'Dashboard/user/Profile.html', redirectTo: 'dashboard/user/profile', pathMatch: 'full' },
  { path: 'Dashboard/user/Settings.html', redirectTo: 'dashboard/user/settings', pathMatch: 'full' },
  { path: 'Dashboard/user/Settings-Password.html', redirectTo: 'dashboard/user/settings-password', pathMatch: 'full' },
  { path: 'Dashboard/user/SubscriptionSuccess.html', redirectTo: 'dashboard/user/subscription-success', pathMatch: 'full' },
  { path: 'Dashboard/notifications/Notifications.html', redirectTo: 'dashboard/notifications', pathMatch: 'full' },
  { path: '**', redirectTo: '' },
];
