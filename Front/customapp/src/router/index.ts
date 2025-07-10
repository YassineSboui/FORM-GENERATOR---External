import { getNeoFormVersion } from "@/api/api";
import { useAppStore } from "@/store/app.store";
import { useHttpRequest } from "@/store/httpRequest.store";
import { createRouter, createWebHistory } from "vue-router";
import { logger } from "@/api/api";
import keycloak from "@/keycloak";
const routes = [
  {
    path: "/",
    name: "home",
    component: () => import("../views/HomeView.vue"),
    meta: { fullMode: false, requiresAdmin: true }, // 🔐 ici
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
      console.error("error jwt", error);
      logger.error(error);
    }
  }

  // 🔐 Si la route nécessite un rôle Admin
  if (to.meta.requiresAdmin) {
    console.log(keycloak.tokenParsed);
    const roles =
      keycloak.tokenParsed?.resource_access?.NeoFormExt?.roles || [];
    const isAdmin = roles.includes("Admin");

    if (!isAdmin) {
      return { name: "unauthorized" };
    }
  }

  if (to.name !== "form") {
    httpRequest.setLoading(false);
  }
});
export default router;
