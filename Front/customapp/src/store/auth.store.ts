// src/store/auth.store.ts
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { authService, type LoginResponse } from "@/api/authService";

export const useAuthStore = defineStore("auth", () => {
  // State
  const user = ref<LoginResponse | null>(authService.getAuthUser());
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Computed
  const isAuthenticated = computed(
    () => !!user.value && authService.isAuthenticated()
  );
  const isSuperAdmin = computed(
    () => user.value?.roles?.includes("SuperAdmin") ?? false
  );
  const mustChangePassword = computed(
    () => user.value?.mustChangePassword ?? false
  );

  // Actions
  async function login(username: string, password: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await authService.login({ username, password });
      authService.saveAuthData(response);
      user.value = response;
      return true;
    } catch (err: any) {
      error.value =
        err.response?.data?.message || "Invalid username or password";
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function changePassword(
    currentPassword: string,
    newPassword: string
  ): Promise<boolean> {
    isLoading.value = true;
    error.value = null;

    try {
      await authService.changePassword({ currentPassword, newPassword });

      // Update mustChangePassword flag
      if (user.value) {
        user.value.mustChangePassword = false;
        authService.saveAuthData(user.value);
      }

      return true;
    } catch (err: any) {
      error.value = err.response?.data?.message || "Failed to change password";
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function logout(): void {
    authService.logout();
    user.value = null;
  }

  function clearError(): void {
    error.value = null;
  }

  return {
    // State
    user,
    isLoading,
    error,

    // Computed
    isAuthenticated,
    isSuperAdmin,
    mustChangePassword,

    // Actions
    login,
    changePassword,
    logout,
    clearError,
  };
});
