import { Routes } from '@angular/router';
import { Login } from './auth/components/login/login';
import { Register } from './auth/components/register/register';
import { ForgotPassword } from './auth/components/forgot-password/forgot-password';
import { Me } from './auth/components/me/me';
import { ChangePassword } from './auth/components/change-password/change-password';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'forgot-password', component: ForgotPassword },
  { path: 'me', component: Me },
  { path: 'change-password', component: ChangePassword },
];
