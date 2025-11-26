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
import { validateOidcCode, createSessionToken } from "@/api/api";

export default defineComponent({
  setup() {
    const route = useRoute();
    const router = useRouter();
    const httpRequest = useHttpRequest();
    const isValidating = ref(true);

    onMounted(() => {
      console.log(
        "[AuthCallback] Component mounted - URL:",
        window.location.href.substring(0, 100) + "..."
      );
      console.log(
        "[AuthCallback] Route query params:",
        Object.keys(route.query).join(", ")
      );

      // Get the authorization code and state from the URL
      const code = route.query.code as string;
      const state = route.query.state as string;
      const error = route.query.error as string;
      const errorDescription = route.query.error_description as string;

      console.log(
        "[AuthCallback] Authorization code exists:",
        !!code,
        "State exists:",
        !!state
      );

      // Check for OIDC error responses
      if (error) {
        console.error(
          "[AuthCallback] OIDC Error received:",
          error,
          "Description:",
          errorDescription
        );
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
      console.log(
        "[AuthCallback] State verification - stored exists:",
        !!storedState,
        "matches received:",
        state === storedState
      );

      if (!code || !state) {
        console.error(
          "[AuthCallback] Missing required parameter - code:",
          !!code,
          "state:",
          !!state
        );
        router.push({ name: "unauthorized" });
        return;
      }

      if (state !== storedState) {
        console.error(
          "[AuthCallback] State mismatch - possible CSRF attack. Expected:",
          storedState?.substring(0, 20) + "...",
          "Received:",
          state?.substring(0, 20) + "..."
        );
        router.push({ name: "unauthorized" });
        return;
      }

      // Get the original return URL
      const returnUrl = sessionStorage.getItem("return_url");
      console.log(
        "[AuthCallback] Return URL retrieved, length:",
        returnUrl?.length || 0
      );
      // http://localhost:5174/neoformext/front/form/client2/61adbe8f-59ad-4122-bb60-3d061bf3f0eb?code=55179767d3fd4a6bbb50e541c6dec80a
      // extract the "client2"

      // Extract client identifier from the return URL
      let clientId = "";
      if (returnUrl) {
        const urlParts = returnUrl.split("/");
        const formIndex = urlParts.findIndex((part) => part === "form");
        if (formIndex !== -1 && formIndex + 1 < urlParts.length) {
          clientId = urlParts[formIndex + 1];
          console.log("[AuthCallback] Extracted client ID from URL:", clientId);
        }
      }

      // Get the personal code from the original query parameter
      const originalQuery = sessionStorage.getItem("original_query") || "";
      let personalCode = "";
      if (originalQuery) {
        const params = new URLSearchParams(
          originalQuery.startsWith("?") ? originalQuery : "?" + originalQuery
        );
        personalCode = params.get("code") || "";
      }
      console.log(
        "[AuthCallback] Personal code extracted:",
        personalCode ? "yes" : "no"
      );

      if (!returnUrl) {
        console.error("[AuthCallback] No return URL found in sessionStorage");
        router.push({ name: "unauthorized" });
        return;
      }

      console.log("[AuthCallback] Starting backend validation...");

      // Retrieve the original guid from sessionStorage
      const originalGuid = sessionStorage.getItem("original_guid") || "";

      // Call the backend to validate the OIDC code
      try {
        console.log("[AuthCallback] Backend validation - clientId:", clientId);
        console.log(
          "[AuthCallback] Backend validation - Using personalCode:",
          !!personalCode
        );
        // Use our API service to validate the code
        validateOidcCode(code, state, originalGuid, clientId, personalCode)
          .then(async (response) => {
            if (response.success) {
              console.log("[AuthCallback] Backend validation successful");

              // ✅ Create unified session token for OIDC
              try {
                const sessionResult = await createSessionToken({
                  guid: originalGuid,
                  code: personalCode,
                  authType: "oidc",
                  clientId: clientId,
                  oidcUserId: response.userId || "oidc-user",
                  email: response.email,
                });

                // Store session token
                sessionStorage.setItem(
                  "email_auth_token",
                  sessionResult.sessionToken
                );
                sessionStorage.setItem("auth_type", "oidc");
                sessionStorage.setItem("oidc_authenticated", "true");

                if (response.email) {
                  sessionStorage.setItem("authenticated_email", response.email);
                }

                console.log("✅ OIDC session token created successfully");
              } catch (sessionError) {
                console.error(
                  "Failed to create OIDC session token:",
                  sessionError
                );
                // Fallback to old method
                sessionStorage.setItem("oidc_authenticated", "true");
              }

              // Clean up session storage
              sessionStorage.removeItem("oidc_state");
              sessionStorage.removeItem("return_url");
              // Keep original_query until Form.vue restores it

              // Redirect to the original URL
              console.log(
                "[AuthCallback] Redirecting to return URL, length:",
                returnUrl.length
              );
              window.location.href = returnUrl;
            } else {
              console.error(
                "[AuthCallback] Backend validation failed:",
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
            console.error(
              "[AuthCallback] OIDC validation error:",
              error.response?.status || error.message
            );

            // Check if the error is due to the endpoint not existing (404) or not implemented (501)
            if (
              error.response &&
              (error.response.status === 404 || error.response.status === 501)
            ) {
              // If the validate-oidc endpoint doesn't exist yet, fall back to the existing flow
              // You can remove this fallback once your backend endpoint is implemented
              console.warn(
                "[AuthCallback] Falling back to client-side validation (less secure) - Backend endpoint not implemented yet"
              );
              sessionStorage.setItem("oidc_authenticated", "true");

              // Clean up session storage
              sessionStorage.removeItem("oidc_state");
              sessionStorage.removeItem("return_url");

              // Redirect to the original URL
              console.log(
                "[AuthCallback] Fallback redirect to return URL, length:",
                returnUrl.length
              );
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
        console.error("[AuthCallback] Failed to validate OIDC code:", error);
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
  max-height: 95vh;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Custom Scrollbar for Auth Callback Container */
.auth-callback-container::-webkit-scrollbar {
  width: 8px;
}

.auth-callback-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

.auth-callback-container::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #0a6e89 0%, #fbc02d 100%);
  border-radius: 10px;
}

.auth-callback-container::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(135deg, #fbc02d 0%, #0a6e89 100%);
}
</style>
