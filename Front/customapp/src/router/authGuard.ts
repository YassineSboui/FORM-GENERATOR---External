// src/router/authGuard.ts
import type { NavigationGuardNext, RouteLocationNormalized } from "vue-router";
import { authService } from "@/api/authService";

/**
 * Route guard to protect admin routes
 */
export function requireAuth(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
): void {
  const isAuthenticated = authService.isAuthenticated();

  if (!isAuthenticated) {
    // Not authenticated - redirect to login
    next({
      name: "AdminLogin",
      query: { redirect: to.fullPath }, // Save where they wanted to go
    });
  } else {
    const user = authService.getAuthUser();

    // Check if password change is required
    if (user?.mustChangePassword && to.name !== "ChangePassword") {
      // Must change password first
      next({ name: "ChangePassword" });
    } else {
      // Authenticated and password is OK
      next();
    }
  }
}

/**
 * Route guard for SuperAdmin-only routes
 */
export function requireSuperAdmin(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
): void {
  const isSuperAdmin = authService.isSuperAdmin();

  if (!isSuperAdmin) {
    // Not SuperAdmin - redirect to home
    next({ name: "Home" });
  } else {
    next();
  }
}
