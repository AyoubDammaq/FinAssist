import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ForgotPasswordRequestDto } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, CommonModule, RouterModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  forgotPasswordForm: FormGroup;
  isSubmitting = false;
  emailSent = false;
  errorMessage: string = '';

  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  constructor() {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    if (this.forgotPasswordForm.valid) {
      this.isSubmitting = true;
      this.errorMessage = '';

      const forgotPasswordData: ForgotPasswordRequestDto = this.forgotPasswordForm.value;

      this.authService.forgotPassword(forgotPasswordData).subscribe({
        next: () => {
          console.log('Password reset email sent');
          this.isSubmitting = false;
          this.emailSent = true;
        },
        error: (error) => {
          console.error('Forgot password error:', error);
          this.errorMessage = error.message || "Erreur lors de l'envoi de l'email";
          this.isSubmitting = false;
        },
      });
    } else {
      this.forgotPasswordForm.get('email')?.markAsTouched();
    }
  }

  get email() {
    return this.forgotPasswordForm.get('email');
  }
}
