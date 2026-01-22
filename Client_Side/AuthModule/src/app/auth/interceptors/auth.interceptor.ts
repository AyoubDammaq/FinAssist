import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  // Ne pas ajouter le token pour les requêtes d'authentification
  const isAuthRequest = req.url.includes('/login') || 
                        req.url.includes('/register') || 
                        req.url.includes('/refresh') ||
                        req.url.includes('/forgot-password') ||
                        req.url.includes('/reset-password') ||
                        req.url.includes('/health');

  if (token && !isAuthRequest) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error) => {
      // Si erreur 401, tenter de rafraîchir le token
      if (error.status === 401 && !isAuthRequest) {
        const refreshToken = authService.getRefreshToken();
        
        if (refreshToken) {
          return authService.refreshToken(refreshToken).pipe(
            switchMap(() => {
              // Réessayer la requête avec le nouveau token
              const newToken = authService.getToken();
              const clonedReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`
                }
              });
              return next(clonedReq);
            }),
            catchError((refreshError) => {
              // Si le refresh échoue, déconnecter l'utilisateur
              authService.logoutLocal();
              return throwError(() => refreshError);
            })
          );
        } else {
          authService.logoutLocal();
        }
      }
      
      return throwError(() => error);
    })
  );
};
