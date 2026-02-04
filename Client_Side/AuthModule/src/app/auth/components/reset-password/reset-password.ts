
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ResetPasswordRequestDto } from '../../models/auth.models';

@Component({
	selector: 'app-reset-password',
	templateUrl: './reset-password.html',
	styleUrl: './reset-password.css',
})
export class ResetPassword {
	resetForm: FormGroup;
	submitted = false;
	successMessage = '';
	errorMessage = '';

	constructor(
		private readonly fb: FormBuilder,
		private readonly authService: AuthService,
		private readonly route: ActivatedRoute
	) {
		const email = this.route.snapshot.queryParamMap.get('email') || '';
		const resetToken = this.route.snapshot.queryParamMap.get('resetToken') || '';
		this.resetForm = this.fb.group({
			email: [{ value: email, disabled: true }, [Validators.required, Validators.email, Validators.maxLength(256)]],
			resetToken: [{ value: resetToken, disabled: true }, [Validators.required, Validators.maxLength(500)]],
			newPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(256)]],
			confirmNewPassword: ['', [Validators.required]]
		}, { validators: this.passwordsMatchValidator });
	}

	passwordsMatchValidator(form: FormGroup) {
		const newPassword = form.get('newPassword')?.value;
		const confirmNewPassword = form.get('confirmNewPassword')?.value;
		return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
	}

	get f() { return this.resetForm.controls; }

	onSubmit() {
		this.submitted = true;
		this.successMessage = '';
		this.errorMessage = '';
		if (this.resetForm.invalid) {
			return;
		}
		const dto: ResetPasswordRequestDto = {
			email: this.resetForm.getRawValue().email,
			resetToken: this.resetForm.getRawValue().resetToken,
			newPassword: this.f['newPassword'].value,
			confirmNewPassword: this.f['confirmNewPassword'].value
		};
		this.authService.resetPassword(dto).subscribe({
			next: () => {
				this.successMessage = 'Password reset successful.';
				this.resetForm.reset();
				this.submitted = false;
			},
			error: (err) => {
				this.errorMessage = err?.error?.message || 'Password reset failed.';
			}
		});
	}
}
