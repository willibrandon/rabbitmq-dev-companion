import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5052/api';

export interface LoginRequest {
    username: string;
    password: string;
}

export interface LoginResponse {
    token: string;
    expiresAt: string;
}

export interface User {
    id: number;
    username: string;
    email: string;
    role: string;
}

class AuthService {
    private token: string | null = null;

    constructor() {
        // Try to load token from localStorage on initialization
        this.token = localStorage.getItem('auth_token');
        if (this.token) {
            this.setAuthHeader(this.token);
        }
    }

    private setAuthHeader(token: string) {
        axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    }

    async login(username: string, password: string): Promise<LoginResponse> {
        const response = await axios.post<LoginResponse>(
            `${API_BASE_URL}/auth/login`,
            { username, password }
        );

        this.token = response.data.token;
        localStorage.setItem('auth_token', this.token);
        this.setAuthHeader(this.token);

        return response.data;
    }

    logout() {
        this.token = null;
        localStorage.removeItem('auth_token');
        delete axios.defaults.headers.common['Authorization'];
    }

    isAuthenticated(): boolean {
        return !!this.token;
    }

    async getCurrentUser(): Promise<User | null> {
        if (!this.token) return null;

        try {
            const response = await axios.get<User>(`${API_BASE_URL}/auth/me`);
            return response.data;
        } catch {
            this.logout();
            return null;
        }
    }

    getToken(): string | null {
        return this.token;
    }
}

export const authService = new AuthService(); 