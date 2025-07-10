<template>
  <div class="auth-callback-container">
    <div
      class="flex justify-content-center align-items-center flex-column"
      style="height: 100vh"
    >
      <div class="text-center">
        <i
          class="pi pi-spin pi-spinner"
          style="font-size: 2rem; margin-bottom: 1rem"
        ></i>

        <p>Traitement de l'authentification...</p>
        <p class="text-sm text-gray-600">
          Veuillez patienter pendant que nous complétons le processus
          d'authentification.
        </p>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";

export default defineComponent({
  setup() {
    const route = useRoute();
    const router = useRouter();

    onMounted(() => {
      console.log("AuthCallback component mounted");
      console.log("Current URL:", window.location.href);
      console.log("Route query:", route.query);

      // Get the authorization code and state from the URL
      const code = route.query.code as string;
      const state = route.query.state as string;
      const error = route.query.error as string;
      const errorDescription = route.query.error_description as string;

      console.log("Authorization code:", code);
      console.log("State:", state);

      // Check for OIDC error responses
      if (error) {
        console.error("OIDC Error:", error, errorDescription);
        router.push({
          name: "unauthorized",
          query: {
            error: error,
            error_description: errorDescription,
          },
        });
        return;
      }

      // Verify the state parameter for security
      const storedState = sessionStorage.getItem("oidc_state");
      console.log("Stored state:", storedState);

      if (!code || !state) {
        console.error("Missing code or state parameter");
        router.push({ name: "unauthorized" });
        return;
      }

      if (state !== storedState) {
        console.error("State parameter mismatch - possible CSRF attack");
        console.error("Expected state:", storedState, "Received state:", state);
        router.push({ name: "unauthorized" });
        return;
      }

      // Get the original return URL
      const returnUrl = sessionStorage.getItem("return_url");
      console.log("Return URL:", returnUrl);

      if (!returnUrl) {
        console.error("No return URL found");
        router.push({ name: "unauthorized" });
        return;
      }

      console.log("Authentication successful, setting flag and redirecting...");

      // Store authentication success flag
      sessionStorage.setItem("oidc_authenticated", "true");

      // Clean up session storage
      sessionStorage.removeItem("oidc_state");
      sessionStorage.removeItem("return_url");
      // Keep original_query until Form.vue restores it

      // Small delay to ensure the flag is set before redirect
      setTimeout(() => {
        console.log("Redirecting to:", returnUrl);
        window.location.href = returnUrl;
      }, 100);
    });

    return {};
  },
});
</script>

<style scoped>
.auth-callback-container {
  background-color: #f8f9fa;
  min-height: 100vh;
}
</style>
