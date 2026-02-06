import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  AbstractControl,
  AbstractControlOptions,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ResetPasswordRequestDto } from '../../models/auth.models';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
  imports: [CommonModule, ReactiveFormsModule],
})
export class ResetPassword {
  resetForm: FormGroup;
  submitted = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {
    const email = this.route.snapshot.queryParamMap.get('email') || '';
    // Accept both 'resetToken' and 'token' as query param
    let resetToken = this.route.snapshot.queryParamMap.get('resetToken');
    if (!resetToken) {
      resetToken = this.route.snapshot.queryParamMap.get('token') || '';
    }

    const formOptions: AbstractControlOptions = {
      validators: [this.passwordsMatchValidator],
    };

    this.resetForm = this.fb.group(
      {
        email: [{ value: email, disabled: true }, [Validators.required, Validators.email, Validators.maxLength(256)]],
        resetToken: [{ value: resetToken, disabled: true }, [Validators.required, Validators.maxLength(500)]],
        newPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(256)]],
        confirmNewPassword: ['', [Validators.required]],
      },
      formOptions
    );
  }

  // Note: must accept AbstractControl per the deprecation message
  private passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmNewPassword = control.get('confirmNewPassword')?.value;
    return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
  }

  get f() {
    return this.resetForm.controls;
  }

  onSubmit() {
    this.submitted = true;
    this.successMessage = '';
    this.errorMessage = '';

    if (this.resetForm.invalid) return;

    const raw = this.resetForm.getRawValue();
    const dto: ResetPasswordRequestDto = {
      email: raw.email,
      resetToken: raw.resetToken,
      newPassword: this.f['newPassword'].value,
      confirmNewPassword: this.f['confirmNewPassword'].value,
    };

    this.authService.resetPassword(dto).subscribe({
      next: () => {
        this.successMessage = 'Password reset successful. Redirecting to login...';
        this.resetForm.reset();
        this.submitted = false;
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Password reset failed.';
      },
    });
  }
}