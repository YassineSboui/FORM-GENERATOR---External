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

        <p>{{ $t("AuthCallback.processingAuthentication") }}</p>
        <p class="text-sm text-gray-600">
          {{ $t("AuthCallback.pleaseWait") }}
        </p>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import axios from "axios";
import { useHttpRequest } from "@/store/httpRequest.store";
import { validateOidcCode } from "@/api/api";

export default defineComponent({
  setup() {
    const route = useRoute();
    const router = useRouter();
    const httpRequest = useHttpRequest();
    const isValidating = ref(true);

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
      // http://localhost:5174/neoformext/front/form/client2/61adbe8f-59ad-4122-bb60-3d061bf3f0eb?code=55179767d3fd4a6bbb50e541c6dec80a
      // extract the "client2"

      // Extract client identifier from the return URL
      let clientId = "";
      if (returnUrl) {
        const urlParts = returnUrl.split("/");
        const formIndex = urlParts.findIndex((part) => part === "form");
        if (formIndex !== -1 && formIndex + 1 < urlParts.length) {
          clientId = urlParts[formIndex + 1];
          console.log("Extracted client ID:", clientId);
        }
      }

      // Get the personal code from the original query parameter
      const originalQuery = sessionStorage.getItem("original_query") || "";
      const personalCode = originalQuery.split("=")[1] || "";
      console.log("Personal code from original query:", personalCode);

      if (!returnUrl) {
        console.error("No return URL found");
        router.push({ name: "unauthorized" });
        return;
      }

      console.log("Authentication in progress, validating with backend...");

      // Retrieve the original guid from sessionStorage
      const originalGuid = sessionStorage.getItem("original_guid") || "";

      // Call the backend to validate the OIDC code
      try {
        console.log("Extracted client ID:", clientId);
        console.log("Using personal code:", personalCode);
        // Use our API service to validate the code
        validateOidcCode(code, state, originalGuid, clientId, personalCode)
          .then((response) => {
            if (response.success) {
              console.log("Backend validation successful");
              // Store authentication success flag
              sessionStorage.setItem("oidc_authenticated", "true");

              // Clean up session storage
              sessionStorage.removeItem("oidc_state");
              sessionStorage.removeItem("return_url");
              // Keep original_query until Form.vue restores it

              // Redirect to the original URL
              console.log("Redirecting to:", returnUrl);
              window.location.href = returnUrl;
            } else {
              console.error(
                "Backend validation failed:",
                response.error || "Unknown error"
              );
              router.push({
                name: "unauthorized",
                query: {
                  error: "validation_failed",
                  error_description:
                    response.error || "Authentication validation failed",
                },
              });
            }
          })
          .catch((error) => {
            console.error("OIDC validation error:", error);

            // Check if the error is due to the endpoint not existing (404) or not implemented (501)
            if (
              error.response &&
              (error.response.status === 404 || error.response.status === 501)
            ) {
              // If the validate-oidc endpoint doesn't exist yet, fall back to the existing flow
              // You can remove this fallback once your backend endpoint is implemented
              console.warn(
                "Falling back to client-side validation (less secure) - Backend endpoint not implemented yet"
              );
              sessionStorage.setItem("oidc_authenticated", "true");

              // Clean up session storage
              sessionStorage.removeItem("oidc_state");
              sessionStorage.removeItem("return_url");

              // Redirect to the original URL
              console.log("Redirecting to:", returnUrl);
              window.location.href = returnUrl;
            } else {
              // For any other error, redirect to unauthorized
              router.push({
                name: "unauthorized",
                query: {
                  error: "validation_error",
                  error_description:
                    error.response?.data?.error ||
                    error.message ||
                    "Authentication validation error",
                },
              });
            }
          });
      } catch (error) {
        console.error("Failed to validate OIDC code:", error);
        router.push({ name: "unauthorized" });
      }
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
