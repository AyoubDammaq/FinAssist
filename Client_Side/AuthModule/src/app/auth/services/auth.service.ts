import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, catchError, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponseDto,
  ChangePasswordRequestDto,
  ForgotPasswordRequestDto,
  LoginRequestDto,
  RefreshTokenRequestDto,
  RegisterRequestDto,
  ResetPasswordRequestDto,
  UpdateProfileDto,
  UserDto,
} from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'current_user';

  private readonly currentUserSubject = new BehaviorSubject<UserDto | null>(this.getCurrentUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {}

  // Authentification

  register(data: RegisterRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.API_URL}/register`, data).pipe(
      tap((response) => this.handleAuthResponse(response)),
      catchError(this.handleError),
    );
  }

  login(data: LoginRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.API_URL}/login`, data).pipe(
      tap((response) => this.handleAuthResponse(response)),
      catchError(this.handleError),
    );
  }

  logout(email: string): Observable<any> {
    return this.http
      .post(`${this.API_URL}/logout`, JSON.stringify(email), {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
      })
      .pipe(
        tap(() => this.clearAuthData()),
        catchError(this.handleError),
      );
  }

  refreshToken(refreshToken: string): Observable<AuthResponseDto> {
    const data: RefreshTokenRequestDto = { refreshToken };
    return this.http.post<AuthResponseDto>(`${this.API_URL}/refresh`, data).pipe(
      tap((response) => this.handleAuthResponse(response)),
      catchError(this.handleError),
    );
  }

  // Gestion de mot de passe

  changePassword(data: ChangePasswordRequestDto): Observable<void> {
    return this.http
      .post<void>(`${this.API_URL}/change-password`, data)
      .pipe(catchError(this.handleError));
  }

  forgotPassword(data: ForgotPasswordRequestDto): Observable<void> {
    return this.http
      .post<void>(`${this.API_URL}/forgot-password`, data)
      .pipe(catchError(this.handleError));
  }

  resetPassword(data: ResetPasswordRequestDto): Observable<void> {
    return this.http
      .post<void>(`${this.API_URL}/reset-password`, data)
      .pipe(catchError(this.handleError));
  }

  // Gestion de profil

  updateProfile(data: UpdateProfileDto): Observable<void> {
    return this.http.put<void>(`${this.API_URL}/profile`, data).pipe(catchError(this.handleError));
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`).pipe(catchError(this.handleError));
  }

  // Requêtes utilisateurs

  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.API_URL}/users`).pipe(catchError(this.handleError));
  }

  getUserById(id: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.API_URL}/users/${id}`).pipe(catchError(this.handleError));
  }

  getUserByUsername(username: string): Observable<UserDto> {
    return this.http
      .get<UserDto>(`${this.API_URL}/users/username/${username}`)
      .pipe(catchError(this.handleError));
  }

  getUserByEmail(email: string): Observable<UserDto> {
    return this.http
      .get<UserDto>(`${this.API_URL}/users/email/${email}`)
      .pipe(catchError(this.handleError));
  }

  healthCheck(): Observable<any> {
    return this.http.get(`${this.API_URL}/health`).pipe(catchError(this.handleError));
  }

  // Gestion des tokens et état de l'utilisateur

  private handleAuthResponse(response: AuthResponseDto): void {
    localStorage.setItem(this.TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);

    const user: UserDto = {
      id: response.userId,
      username: response.username,
      email: response.email,
      createdAt: new Date(),
    };

    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getCurrentUser(): UserDto | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    if (userJson) {
      return JSON.parse(userJson);
    }
    return null;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token;
  }

  logoutLocal(): void {
    this.clearAuthData();
    this.router.navigate(['/login']);
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'Une erreur est survenue';

    if (error.error instanceof ErrorEvent) {
      // Erreur côté client
      errorMessage = error.error.message;
    } else if (error.error?.message) {
      // Erreur côté serveur
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    }

    console.error('API Error:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
