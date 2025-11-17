// src/api/authService.ts
import axios, { type AxiosInstance } from "axios";
import { useHttpRequest } from "../store/httpRequest.store";

// Create axios instance factory that uses httpRequest.externalUrl
const createApiClient = (): AxiosInstance => {
  const httpRequest = useHttpRequest();

  const apiClient = axios.create({
    baseURL: httpRequest.externalUrl,
    headers: {
      "Content-Type": "application/json",
    },
  });

  // Add token to all requests automatically
  apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem("authToken");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  // Handle token expiration automatically
  apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error.response?.status === 401) {
        // Token expired or invalid - redirect to login
        localStorage.removeItem("authToken");
        localStorage.removeItem("authUser");

        if (
          window.location.href.includes("/neoformext/front/admin/login") ===
          false
        ) {
          window.location.href = "/neoformext/front/admin/login";
        }
      }
      return Promise.reject(error);
    }
  );

  return apiClient;
};

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  fullName: string;
  roles: string[];
  mustChangePassword: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface CreateAdminRequest {
  username: string;
  email?: string;
  password: string;
  fullName: string;
  isSuperAdmin: boolean;
}

export interface UserDto {
  id: string;
  userName: string;
  email: string;
  fullName: string;
  createdAt: string;
  createdBy: string | null;
}

export const authService = {
  /**
   * Login admin user
   */
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const apiClient = createApiClient();
    const response = await apiClient.post<LoginResponse>(
      "/admin/login",
      credentials
    );
    return response.data;
  },

  /**
   * Change password for authenticated user
   */
  async changePassword(
    data: ChangePasswordRequest
  ): Promise<{ message: string }> {
    const apiClient = createApiClient();
    const response = await apiClient.post("/admin/change-password", data);
    return response.data;
  },

  /**
   * Create new admin user (SuperAdmin only)
   */
  async createAdmin(
    data: CreateAdminRequest
  ): Promise<{ message: string; username: string }> {
    const apiClient = createApiClient();
    const response = await apiClient.post("/admin/create-admin", data);
    return response.data;
  },

  /**
   * Get all users (SuperAdmin only)
   */
  async getUsers(): Promise<UserDto[]> {
    const apiClient = createApiClient();
    const response = await apiClient.get("/admin/users");
    return response.data;
  },

  /**
   * Delete user (SuperAdmin only)
   */
  async deleteUser(userId: string): Promise<{ message: string }> {
    const apiClient = createApiClient();
    const response = await apiClient.delete(`/admin/users/${userId}`);
    return response.data;
  },

  /**
   * Save token and user data to localStorage
   */
  saveAuthData(loginResponse: LoginResponse): void {
    localStorage.setItem("authToken", loginResponse.token);
    localStorage.setItem(
      "authUser",
      JSON.stringify({
        username: loginResponse.username,
        fullName: loginResponse.fullName,
        roles: loginResponse.roles,
        mustChangePassword: loginResponse.mustChangePassword,
      })
    );
  },

  /**
   * Get stored user data
   */
  getAuthUser(): LoginResponse | null {
    const userData = localStorage.getItem("authUser");
    return userData ? JSON.parse(userData) : null;
  },

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem("authToken");
  },

  /**
   * Check if user is SuperAdmin
   */
  isSuperAdmin(): boolean {
    const user = this.getAuthUser();
    return user?.roles?.includes("SuperAdmin") ?? false;
  },

  /**
   * Logout user
   */
  logout(): void {
    localStorage.removeItem("authToken");
    localStorage.removeItem("authUser");
  },
};
