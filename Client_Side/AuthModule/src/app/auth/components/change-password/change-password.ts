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
        oldPassword: ['', [Validators.required]],
        newPassword: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', [Validators.required]],
      },
      { validators: this.passwordMatchValidator },
    );
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword');
    const confirmPassword = control.get('confirmPassword');

    if (!newPassword || !confirmPassword) {
      return null;
    }

    return newPassword.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  onSubmit() {
    if (this.changePasswordForm.valid) {
      this.isSubmitting = true;
      this.errorMessage = '';
      this.successMessage = '';

      const { oldPassword, newPassword } = this.changePasswordForm.value;
      const changePasswordData: ChangePasswordRequestDto = {
        oldPassword,
        newPassword,
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

  get oldPassword() {
    return this.changePasswordForm.get('oldPassword');
  }

  get newPassword() {
    return this.changePasswordForm.get('newPassword');
  }

  get confirmPassword() {
    return this.changePasswordForm.get('confirmPassword');
  }
}
