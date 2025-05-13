import { getNeoFormVersion } from "@/api/api";
import { useAppStore } from "@/store/app.store";
import { useHttpRequest } from "@/store/httpRequest.store";
import { createRouter, createWebHistory } from "vue-router";
import { logger } from "@/api/api";

const routes = [
  {
    path: "/form/:guid",
    name: "form",
    component: () => import("../views/Form.vue"),
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
      if (import.meta.env.DEV && !from.name) {
        await httpRequest.storeGuidDevMode(
          "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        );
        await httpRequest.fetchJwt("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        httpRequest.version = await getNeoFormVersion();
        await initLoader();
      } else {
        const { Guid } = to.query;
        if (Guid && !from.name) {
          await httpRequest.fetchJwt(Guid.toString());
        }
        httpRequest.version = await getNeoFormVersion();
        // if (import.meta.env.MODE !== "client") {
        await initLoader();
        // }
      }
    } catch (error) {
      console.error("error jwt", error);
      logger.error(error);
      return { name: "unauthorized" };
    }
  }
  // Check if the current route is not '/form/:guid' and '/ref/:guid' and set loading to false
  if (to.name !== "form") {
    httpRequest.setLoading(false);
  }
});
export default router;
