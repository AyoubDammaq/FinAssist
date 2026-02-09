
# AuthModule Overview

This project is an Angular-based authentication module designed to provide secure and user-friendly authentication workflows, including login, registration, password management, and user profile features.

## Project Structure
- **src/app/auth/components/**: Contains all authentication-related UI components (login, register, change-password, etc.), each in its own folder with HTML, CSS, TypeScript, and test files.
- **src/app/auth/services/auth.service.ts**: Central service for authentication logic and API communication.
- **src/app/auth/interceptors/auth.interceptor.ts**: Handles HTTP request interception for authentication tokens.
- **src/app/auth/models/auth.models.ts**: Defines TypeScript interfaces and types for authentication data.
- **src/app/auth/guards/**: (If present) Route guards for protecting authenticated routes.
- **src/app/app.routes.ts**: Application route definitions, including auth routes.

## Key Features
- **Reactive Forms**: All forms use Angular Reactive Forms for validation and state management.
- **Password Management**: Change, reset, and forgot password flows with UI feedback and validation.
- **Token Handling**: Auth tokens are managed via HTTP interceptors for secure API requests.
- **Component Isolation**: Each feature is modular and easy to extend or maintain.

## Developer Workflows
- **Start Dev Server**: `ng serve` (or `npm start` if configured)
- **Build**: `ng build`
- **Unit Tests**: `ng test`
- **Scaffold Components**: `ng generate component <name>`

## Custom Patterns
- **Password Visibility**: Password fields use toggle logic for show/hide (see change-password component).
- **Form Feedback**: Error and success messages are displayed using template bindings and component state.
- **Navigation**: Uses Angular Router (`routerLink`) for navigation between auth pages.

## Extending the Module
- Add new features by creating a new folder in `components/` and updating routes as needed.
- Place shared logic in `services/` and shared types in `models/`.

## References
- See each component folder for implementation details and best practices.
- For Angular CLI usage, see the official [Angular CLI documentation](https://angular.dev/tools/cli).
