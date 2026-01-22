import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { UserDto } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-me',
  imports: [CommonModule, RouterModule],
  templateUrl: './me.html',
  styleUrl: './me.css',
})
export class Me implements OnInit {
  user: UserDto | null = null;
  isLoading = true;
  errorMessage: string = '';

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit() {
    // Vérifier si l'utilisateur est authentifié
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    // Récupérer l'utilisateur du localStorage
    this.user = this.authService.getCurrentUser();

    if (this.user) {
      // Optionnel: Récupérer les données complètes depuis l'API
      this.authService.getUserById(this.user.id).subscribe({
        next: (userData) => {
          this.user = userData;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error fetching user:', error);
          // Utiliser les données du localStorage en cas d'erreur
          this.isLoading = false;
        },
      });
    } else {
      this.isLoading = false;
      this.errorMessage = 'Utilisateur non trouvé';
    }
  }

  getInitials(): string {
    if (!this.user) return '';

    if (this.user.firstName && this.user.lastName) {
      return `${this.user.firstName[0]}${this.user.lastName[0]}`.toUpperCase();
    }

    return this.user.username.substring(0, 2).toUpperCase();
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
}
