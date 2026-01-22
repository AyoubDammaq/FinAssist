import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { UserDto } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-me',
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './me.html',
  styleUrl: './me.css',
})
export class Me implements OnInit {
  user: UserDto | null = null;
  isLoading = true;
  errorMessage: string = '';
  isEditMode = false;
  isSaving = false;
  successMessage: string = '';
  profileForm: FormGroup;

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  constructor() {
    this.profileForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
      firstName: [''],
      lastName: [''],
      phoneNumber: [''],
    });
  }

  ngOnInit() {
    // Vérifier si l'utilisateur est authentifié
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    // Récupérer l'utilisateur du localStorage
    this.user = this.authService.getCurrentUser();

    // Si pas d'utilisateur dans localStorage, essayer d'extraire du token
    if (!this.user) {
      const tokenInfo = this.authService.getUserInfoFromToken();
      if (tokenInfo?.id) {
        this.user = tokenInfo as UserDto;
      }
    }

    if (this.user) {
      // Obtenir l'ID depuis le token si nécessaire
      let userId = this.user.id;
      if (!userId || userId === 'undefined') {
        userId = this.authService.getUserIdFromToken() || '';
      }

      // Vérifier si l'ID existe avant d'appeler l'API
      if (userId && userId !== 'undefined') {
        // Récupérer les données complètes depuis l'API
        this.authService.getUserById(userId).subscribe({
          next: (userData) => {
            this.user = userData;
            this.isLoading = false;
          },
          error: (error) => {
            console.error('Error fetching user:', error);
            // Utiliser les données du localStorage/token en cas d'erreur
            this.isLoading = false;
          },
        });
      } else {
        // Pas d'ID valide, utiliser uniquement les données du localStorage
        console.warn('No valid user ID found, using localStorage data only');
        this.isLoading = false;
      }
    } else {
      this.isLoading = false;
      this.errorMessage = 'Utilisateur non trouvé';
    }
  }

  getInitials(): string {
    if (!this.user?.userName) return '';

    if (this.user.firstName && this.user.lastName) {
      return `${this.user.firstName[0]}${this.user.lastName[0]}`.toUpperCase();
    }

    return this.user.userName.substring(0, 2).toUpperCase();
  }

  onLogout() {
    if (this.user?.email) {
      this.authService.logout(this.user.email).subscribe({
        next: () => {
          console.log('Logged out successfully');
          this.router.navigate(['/login']);
        },
        error: (error) => {
          console.error('Logout error:', error);
          // Déconnecter localement même en cas d'erreur
          this.authService.logoutLocal();
        },
      });
    } else {
      this.authService.logoutLocal();
    }
  }

  onChangePassword() {
    this.router.navigate(['/change-password']);
  }

  getRoleDisplay(): string {
    if (this.user?.role === undefined || this.user?.role === null) return 'User';
    // 0 = Admin, 1 = User
    return this.user.role === 0 ? 'Admin' : 'User';
  }


  onEditProfile() {
    if (!this.user) return;
    
    this.isEditMode = true;
    this.successMessage = '';
    this.errorMessage = '';
    
    // Populate form with current user data
    this.profileForm.patchValue({
      userName: this.user.userName || '',
      firstName: this.user.firstName || '',
      lastName: this.user.lastName || '',
      phoneNumber: this.user.phoneNumber || '',
    });
  }

  onCancelEdit() {
    this.isEditMode = false;
    this.profileForm.reset();
    this.successMessage = '';
    this.errorMessage = '';
  }

  onSaveProfile() {
    if (this.profileForm.invalid || !this.user?.id) return;

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const updateData = {
      id: this.user.id,
      userName: this.profileForm.value.userName,
      firstName: this.profileForm.value.firstName || '',
      lastName: this.profileForm.value.lastName || '',
      phoneNumber: this.profileForm.value.phoneNumber || '',
    };

    console.log('📤 Sending update profile data:', updateData);

    this.authService.updateProfile(updateData).subscribe({
      next: () => {
        this.successMessage = 'Profile updated successfully!';
        this.isSaving = false;
        
        // Refresh user data
        if (this.user?.id) {
          this.authService.getUserById(this.user.id).subscribe({
            next: (userData) => {
              this.user = userData;
              this.isEditMode = false;
              // Clear success message after 3 seconds
              setTimeout(() => {
                this.successMessage = '';
              }, 3000);
            },
            error: (error) => {
              console.error('Error refreshing user data:', error);
              this.isEditMode = false;
            },
          });
        }
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        this.errorMessage = error;
        this.isSaving = false;
      },
    });
  }
  getAccountStatus(): string {
    if (!this.user) return 'Unknown';
    if (this.user.isLocked) return 'Locked';
    if (!this.user.isActive) return 'Inactive';
    if (!this.user.isEmailConfirmed) return 'Email not confirmed';
    return 'Active';
  }

  getStatusClass(): string {
    if (!this.user) return '';
    if (this.user.isLocked) return 'status-locked';
    if (!this.user.isActive) return 'status-inactive';
    if (!this.user.isEmailConfirmed) return 'status-pending';
    return 'status-active';
  }
}
