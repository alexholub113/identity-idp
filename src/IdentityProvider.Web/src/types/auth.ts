export interface LoginFormData {
    email: string;
    password: string;
    rememberMe: boolean;
}

export interface LoginResponse {
    success: boolean;
    token?: string;
    message?: string;
    user?: UserInfo;
}

export interface UserInfo {
    id: string;
    email: string;
    name?: string;
    roles?: string[];
}

export interface ApiError {
    message: string;
    code?: string;
    details?: string;
}
