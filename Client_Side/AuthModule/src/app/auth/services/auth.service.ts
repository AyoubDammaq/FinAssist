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
    console.log('🔐 Auth Response:', response);
    
    localStorage.setItem(this.TOKEN_KEY, response.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);

    // Essayer d'abord avec les données de la réponse API
    let userId = response.userId;
    let userName = response.userName;
    let email = response.email;

    // Si les données ne sont pas dans la réponse, les extraire du token
    if (!userId || !userName || !email) {
      console.log('⚠️ User info incomplete in API response, extracting from token...');
      const tokenInfo = this.getUserInfoFromToken();
      if (tokenInfo) {
        userId = userId || tokenInfo.id || '';
        userName = userName || tokenInfo.userName || '';
        email = email || tokenInfo.email || '';
      }
    }

    const user: UserDto = {
      id: userId,
      userName: userName,
      email: email,
      createdAt: new Date(),
    };

    console.log('👤 User to be stored:', user);
    
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    this.currentUserSubject.next(user);
    
    console.log('✅ User stored in localStorage');
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

  /**
   * Extrait l'ID de l'utilisateur depuis le token JWT
   * @returns L'ID de l'utilisateur ou null si non trouvé
   */
  getUserIdFromToken(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      // Décoder le payload JWT (la partie entre les deux points)
      const payload = token.split('.')[1];
      const decodedPayload = JSON.parse(atob(payload));
      
      console.log('🔍 Decoded JWT payload:', decodedPayload);
      
      // Les claims possibles pour l'ID utilisateur en ASP.NET
      // "sub" (standard JWT), "nameid", ou "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      const userId = 
        decodedPayload.sub || 
        decodedPayload.nameid || 
        decodedPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
        decodedPayload.userId ||
        decodedPayload.id;
      
      console.log('👤 Extracted User ID:', userId);
      
      return userId || null;
    } catch (error) {
      console.error('Error decoding JWT token:', error);
      return null;
    }
  }

  /**
   * Extrait les informations utilisateur complètes depuis le token JWT
   * @returns Objet avec les informations de l'utilisateur
   */
  getUserInfoFromToken(): Partial<UserDto> | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const payload = token.split('.')[1];
      const decodedPayload = JSON.parse(atob(payload));
      
      // Extraire les informations courantes
      const userId = 
        decodedPayload.sub || 
        decodedPayload.nameid || 
        decodedPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      
      const email = 
        decodedPayload.email || 
        decodedPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
      
      const userName = 
        decodedPayload.unique_name || 
        decodedPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        decodedPayload.username;
      
      const roleString = 
        decodedPayload.role || 
        decodedPayload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      
      // Convert role string to number: "Admin" = 0, "User" = 1
      let roleNumber: number | undefined;
      if (roleString) {
        roleNumber = roleString === 'Admin' ? 0 : 1;
      }

      return {
        id: userId,
        email: email,
        userName: userName,
        role: roleNumber,
        createdAt: new Date()
      };
    } catch (error) {
      console.error('Error extracting user info from token:', error);
      return null;
    }
  }

  logoutLocal(): void {
    this.clearAuthData();
    this.router.navigate(['/login']);
  }

  private readonly handleError = (error: any): Observable<never> => {
    this.logErrorDetails(error);
    const errorMessage = this.extractErrorMessage(error);
    console.log('✅ Final extracted message:', errorMessage);
    return throwError(() => errorMessage);
  };

  private logErrorDetails(error: any): void {
    console.group('🔴 API ERROR DETAILS');
    console.log('1. Error Status:', error.status);
    console.log('2. Error StatusText:', error.statusText);
    console.log('3. Error Object:', error);
    console.log('4. Error.error type:', typeof error.error);
    console.log('5. Error.error value:', error.error);

    if (error.error) {
      console.log('6. Error.error keys:', Object.keys(error.error));
      console.log('7. Error.error.Message:', error.error.Message);
      console.log('8. Error.error.message:', error.error.message);
      console.log('9. Error.error.ErrorCode:', error.error.ErrorCode);
      console.log('10. Error.error stringified:', JSON.stringify(error.error));
    }
    console.groupEnd();
  }

  private extractErrorMessage(error: any): string {
    if (error.error instanceof ErrorEvent) {
      return error.error.message;
    }

    if (error.error) {
      return this.parseErrorBody(error.error);
    }

    return error.message || 'Une erreur est survenue';
  }

  private parseErrorBody(errorBody: any): string {
    // 1. Erreur de validation ASP.NET Core
    const validationMessage = this.extractValidationErrors(errorBody);
    if (validationMessage) {
      return validationMessage;
    }

    // 2. Nouvelle structure API: { Message: string, ErrorCode: string, Details?: any }
    if (errorBody.Message && typeof errorBody.Message === 'string') {
      return errorBody.Message;
    }

    // 3. Ancienne structure: { message: string }
    if (errorBody.message && typeof errorBody.message === 'string') {
      return errorBody.message;
    }

    // 4. Simple title
    if (errorBody.title && typeof errorBody.title === 'string') {
      return errorBody.title;
    }

    // 5. Réponse simple string
    if (typeof errorBody === 'string') {
      return errorBody;
    }

    return 'Une erreur est survenue';
  }

  private extractValidationErrors(errorBody: any): string | null {
    if (!errorBody.errors || typeof errorBody.errors !== 'object') {
      return null;
    }

    const validationErrors: string[] = [];
    for (const field in errorBody.errors) {
      const fieldErrors = errorBody.errors[field];
      if (Array.isArray(fieldErrors)) {
        validationErrors.push(...fieldErrors);
      }
    }

    if (validationErrors.length > 0) {
      return validationErrors.join(' ');
    }

    return errorBody.title || null;
  }
}
