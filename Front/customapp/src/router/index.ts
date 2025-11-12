import { getNeoFormVersion } from "@/api/api";
import { useAppStore } from "@/store/app.store";
import { useHttpRequest } from "@/store/httpRequest.store";
import { createRouter, createWebHistory } from "vue-router";
import { logger } from "@/api/api";
import { requireAuth, requireSuperAdmin } from "./authGuard";

const routes = [
  {
    path: "/",
    name: "home",
    component: () => import("../views/HomeView.vue"),
    meta: { fullMode: false },
    beforeEnter: requireAuth,
  },
  {
    path: "/form/:client/:guid",
    name: "form",
    component: () => import("../views/Form.vue"),
    meta: { fullMode: true },
  },
  {
    path: "/auth/callback",
    name: "auth-callback",
    component: () => import("../views/AuthCallback.vue"),
    meta: { fullMode: true },
  },
  {
    path: "/unauthorized",
    name: "unauthorized",
    component: () => import("@/views/UnauthorizedView.vue"),
    meta: { fullMode: true },
  },
  {
    path: "/admin/login",
    name: "AdminLogin",
    component: () => import("../views/AdminLogin.vue"),
    meta: { fullMode: true },
  },
  {
    path: "/admin/change-password",
    name: "ChangePassword",
    component: () => import("../views/ChangePassword.vue"),
    meta: { fullMode: true },
    beforeEnter: requireAuth,
  },
  {
    path: "/admin/dashboard",
    name: "AdminDashboard",
    component: () => import("../views/AdminDashboard.vue"),
    meta: { fullMode: true },
    beforeEnter: [requireAuth, requireSuperAdmin],
  },
  {
    path: "/:pathMatch(.*)*",
    component: () => import("@/views/UnauthorizedView.vue"),
    meta: { fullMode: true },
  },
];
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: routes,
});

router.beforeEach(async (to, from) => {
  const httpRequest = useHttpRequest();
  const { initLoader } = useAppStore();

  if (to.name == "form") {
    httpRequest.setLoading(true);
  }

  if (!httpRequest.apiUrl) {
    await httpRequest.fetchApiUrl();
  }

  if (!httpRequest.jwt && to.name !== "unauthorized") {
    try {
      if (to.name == "form") {
        const client = to.params.client as string;
        if (client) {
          httpRequest.setApiUrl(httpRequest.externalUrl + client);
        }
      }
      await initLoader();
    } catch (error) {
      console.error(
        "[Router] Failed to initialize JWT and loader for route:",
        String(to.name),
        error
      );
      logger.error(
        `[Router] initLoader failed for ${String(to.name)}: ${error}`
      );
    }
  }

  // Admin check removed - no authentication required
  // All routes are now accessible without role-based access control

  if (to.name !== "form") {
    httpRequest.setLoading(false);
  }
});
export default router;
