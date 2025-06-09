import { defineStore } from "pinia";
import axios from "axios";
import { usePVToastService } from "@/composable/usePVToastService";
import { logger } from "@/api/api";
import { useAppStore } from "@/store/app.store";
import keycloak from "@/keycloak"; // ✅ Utilise le token Keycloak globalement
axios.interceptors.request.use(
  (config) => {
    const appStore = useAppStore();
    const url = config.url || "";

    // ✅ Toujours ajouter le token Keycloak si disponible
    if (keycloak?.token) {
      config.headers = config.headers || {};
      config.headers["Authorization"] = `Bearer ${keycloak.token}`;
    }
    if (!url.includes("neoformexternal/local")) {
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
    console.error(error);
  }
);
axios.interceptors.response.use(null, (error) => {
  const toast = usePVToastService();
  const message = error?.response?.data?.Message;
  console.error(error);
  logger.error(error);

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
        console.error("Error fetching JWT:", error);
        logger.error(error);
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
        if (keycloak.authenticated) {
          await keycloak.logout();
        }
      } catch (error) {
        console.error("Error during logout:", error);
        logger.error(error);
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
