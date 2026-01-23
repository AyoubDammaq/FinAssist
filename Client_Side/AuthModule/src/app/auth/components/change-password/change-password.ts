import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ChangePasswordRequestDto } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-change-password',
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css',
})
export class ChangePassword {
  changePasswordForm: FormGroup;
  isSubmitting = false;
  errorMessage: string = '';
  successMessage: string = '';
  showOldPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  constructor() {
    // Vérifier si l'utilisateur est authentifié
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
    }

    this.changePasswordForm = this.fb.group(
      {
        currentPassword: ['', [Validators.required]],
        newPassword: ['', [Validators.required, Validators.minLength(6)]],
        confirmNewPassword: ['', [Validators.required]],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword');
    const confirmNewPassword = control.get('confirmNewPassword');

    if (!newPassword || !confirmNewPassword) {
      return null;
    }

    return newPassword.value === confirmNewPassword.value ? null : { passwordMismatch: true };
  }

  onSubmit() {
    if (this.changePasswordForm.valid) {
      this.isSubmitting = true;
      this.errorMessage = '';
      this.successMessage = '';

      // Get user ID from token or current user
      const userId = this.authService.getUserIdFromToken();
      if (!userId) {
        this.errorMessage = 'Utilisateur non authentifié';
        this.isSubmitting = false;
        return;
      }

      const { currentPassword, newPassword, confirmNewPassword } = this.changePasswordForm.value;
      const changePasswordData: ChangePasswordRequestDto = {
        userId,
        currentPassword,
        newPassword,
        confirmNewPassword,
      };

      this.authService.changePassword(changePasswordData).subscribe({
        next: () => {
          console.log('Password changed successfully');
          this.successMessage = 'Votre mot de passe a été changé avec succès!';
          this.isSubmitting = false;
          this.changePasswordForm.reset();

          // Rediriger vers le profil après 2 secondes
          setTimeout(() => {
            this.router.navigate(['/me']);
          }, 2000);
        },
        error: (error) => {
          console.error('Change password error:', error);
          this.errorMessage = error.message || 'Erreur lors du changement de mot de passe';
          this.isSubmitting = false;
        },
      });
    } else {
      Object.keys(this.changePasswordForm.controls).forEach((key) => {
        this.changePasswordForm.get(key)?.markAsTouched();
      });
    }
  }

  togglePasswordVisibility(field: 'old' | 'new' | 'confirm') {
    switch (field) {
      case 'old':
        this.showOldPassword = !this.showOldPassword;
        break;
      case 'new':
        this.showNewPassword = !this.showNewPassword;
        break;
      case 'confirm':
        this.showConfirmPassword = !this.showConfirmPassword;
        break;
    }
  }

  get currentPassword() {
    return this.changePasswordForm.get('currentPassword');
  }

  get newPassword() {
    return this.changePasswordForm.get('newPassword');
  }

  get confirmNewPassword() {
    return this.changePasswordForm.get('confirmNewPassword');
  }
}
