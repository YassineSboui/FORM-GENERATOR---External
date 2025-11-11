import { defineStore } from "pinia";
import axios from "axios";
import { usePVToastService } from "@/composable/usePVToastService";
import { logger } from "@/api/api";
import { useAppStore } from "@/store/app.store";

axios.interceptors.request.use(
  (config) => {
    const appStore = useAppStore();
    const url = config.url || "";

    // No Keycloak authentication - removed
    if (!url.includes("neoformexternal/local") && !url.includes("auth-type")) {
      if (!config.params) {
        config.params = {};
      }
      if (appStore.guid) {
        config.params.guid = appStore.guid;
      }
      if (appStore.code) {
        config.params.code = appStore.code;
      }
    }

    if (import.meta.env.DEV) config.withCredentials = true;
    return config;
  },
  (error) => {
    console.error("[HTTP] Request interceptor error:", error);
  }
);
axios.interceptors.response.use(null, (error) => {
  const toast = usePVToastService();
  const message = error?.response?.data?.Message;
  const status = error?.response?.status;
  const url = error?.config?.url || "unknown";

  console.error(`[HTTP] Response error (${status}) for ${url}:`, error);
  logger.error(`[HTTP] ${status} - ${url}: ${error.message || error}`);

  if (message && message !== "Notice Not Found") {
    toast.add({
      severity: "error",
      summary: "Opss...!",
      detail: message + "",
      life: 3000,
    });
  }
  //if (error.response?.status === 401) router.push({ name: "unauthorized" });
  return Promise.reject(error);
});

export const useHttpRequest = defineStore("httpRequest", {
  state: () => ({
    jwt: "",
    apiUrl: "",
    externalUrl: "",
    loading: false,
    version: "",
    userIsAdmin: false,
    debugMode: true,
    executionTimeout: 300000, // 5 minutes
  }),

  actions: {
    async fetchApiUrl() {
      const { data } = await axios.get(
        import.meta.env.BASE_URL + "config.json"
      );
      this.apiUrl = "";
      this.externalUrl = data.API_URL;
      this.userIsAdmin = data.GLB_USER === "admin";
      this.debugMode = data.ENABLE_SERVER_LOG;
      this.executionTimeout = data.EXECUTION_TIMEOUT ?? 5000; // Load execution timeout
    },

    async fetchJwt(guid: string) {
      try {
        const response = await axios.get(this.apiUrl + "/Hook/auth", {
          params: {
            guid,
          },
        });
        this.jwt = response.data.jwt;
        //  axios.defaults.headers.common["Authorization"] = `Bearer ${this.jwt}`;
      } catch (error) {
        console.error(
          "[HttpRequest] Failed to fetch JWT for guid:",
          guid,
          error
        );
        logger.error(
          `[HttpRequest] fetchJwt failed for guid ${guid}: ${error}`
        );
        throw error;
      }
    },
    sendGetRequest(options: any) {
      return new Promise<any>((resolve, reject) => {
        if (options.data == undefined) {
          options.data = {};
        }
        return axios.get(options);
      });
    },
    async logout() {
      try {
        // No Keycloak authentication - logout just clears local state
        console.log("[HttpRequest] Logout called - clearing session state");
        this.jwt = "";
      } catch (error) {
        console.error("[HttpRequest] Error during logout:", error);
        logger.error(`[HttpRequest] logout failed: ${error}`);
      }
    },

    setLoading(_loading: boolean) {
      this.loading = _loading;
    },
    setApiUrl(url: string) {
      this.apiUrl = url;
    },
  },
});
