// Request DTOs
export interface RegisterRequestDto {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface RefreshTokenRequestDto {
  refreshToken: string;
}

export interface ChangePasswordRequestDto {
  oldPassword: string;
  newPassword: string;
}

export interface ForgotPasswordRequestDto {
  email: string;
}

export interface ResetPasswordRequestDto {
  token: string;
  newPassword: string;
}

export interface UpdateProfileDto {
  id: string;
  userName: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

// Response DTOs
export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  userId: string;
  userName: string;
  email: string;
  expiresIn: number;
}

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  role?: number; // 0 = Admin, 1 = User
  isEmailConfirmed?: boolean;
  isActive?: boolean;
  isLocked?: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface ApiResponse<T = any> {
  data?: T;
  message?: string;
  success: boolean;
}

export interface ApiError {
  Message: string;
  ErrorCode: string;
  Details?: any;
}

export interface ErrorResponse {
  message: string;
}
