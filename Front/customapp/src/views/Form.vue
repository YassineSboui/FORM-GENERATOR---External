<template>
  <div
    class="form-viewer-container"
    :dir="isRTL ? 'rtl' : 'ltr'"
    v-if="object && isAuthenticated"
  >
    <div class="form-viewer-container-header" v-if="showFormHeader">
      <div class="flex justify-content-start">
        <div v-if="formName">{{ formName }}</div>
      </div>
    </div>
    <div class="form-viewer-container-content">
      <div v-if="form.length === 0">
        <div></div>
      </div>
      <div v-else>
        <div class="form-viewer">
          <component-form
            @done="handleDone"
            v-model="form"
            @update:isSubmit="handleIsSubmit"
            :isSubmit="isSubmit"
            :isFormDisplay="{
              value: true,
              objectId: object?.id,
              objectGuid: object?.guid,
            }"
            :configForm="configForm"
            :stepper="{
              isStepper: isStepper,
              steps: steps,
              showPageNames: showPageNames,
              names: names,
            }"
            :isRTL="isRTL"
            :showPageNum="showPageNumF"
            @update:showPageNum="showPageNumF = $event"
            :executeNavigateNext="executeNavigateNext"
            @update:executeNavigateNext="executeNavigateNext = $event"
            :language="language"
          ></component-form>
        </div>
      </div>
    </div>
    <div
      class="form-viewer-container-footer flex justify-content-between align-items-center"
      :class="{ 'custom-padding-rtl': isRTL }"
      v-if="isFormDisplay.value && form.length !== 0"
    >
      <!-- Left: ToggleSwitch -->
      <div>
        <ToggleSwitch
          v-model="isDarkMode"
          class="mt-1 ml-4"
          :style="{
            color: isDarkMode ? '#fff' : '#FFEA00',
          }"
        >
          <template #handle="{ checked }">
            <i
              :class="['pi', checked ? 'pi-moon' : 'pi-sun']"
              :style="{
                padding: '0 8px',
                color: checked ? '#fff' : '#FFEA00',
              }"
            />
          </template>
        </ToggleSwitch>
      </div>
      <!-- Right: Navigation Buttons -->
      <div class="flex justify-content-end gap-1">
        <div class="col flex justify-content-start gap-1">
          <div>
            <Button
              v-if="!newDoc"
              v-show="showPageNumF === 1 || showPageNames"
              @click="cancel"
              class="mr-2"
            >
              {{ cancelButtonText }}
            </Button>
          </div>
          <div>
            <Button
              v-show="showPageNumF > 1 && !showPageNames"
              @click="showPageNumF > 1 ? showPageNumF-- : showPageNumF"
              class="mr-2"
            >
              {{ previousButtonText }}
            </Button>
          </div>
        </div>
        <div class="col flex justify-content-end gap-1">
          <div>
            <Button
              v-show="showPageNumF < steps && !showPageNames"
              @click="navigateToPage(showPageNumF + 1)"
            >
              {{ nextButtonText }}
            </Button>
          </div>
          <div>
            <Button
              v-if="showPageNumF === steps || steps === 0"
              @click="submit()"
              class="ml-2"
            >
              {{ submitButtonText }}
            </Button>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div
    v-else-if="!isAuthenticated && authRequired"
    class="form-viewer-container-content"
  >
    <div
      class="flex justify-content-center align-items-center flex-column"
      style="height: 100%"
    >
      <div class="text-center">
        <i
          class="pi pi-spin pi-spinner"
          style="font-size: 2rem; margin-bottom: 1rem"
        ></i>
        <p>Authenticating...</p>
        <p class="text-sm text-gray-600">
          Please wait while we redirect you to the authentication provider.
        </p>
      </div>
    </div>
  </div>
  <div v-else class="form-viewer-container-content">
    <div
      class="flex justify-content-center align-items-center"
      style="height: 100%"
    >
      <img
        src="@/assets/images/not-found.png"
        alt="Not Found"
        style="max-width: 75vw; max-height: 75vh"
      />
    </div>
  </div>
</template>

<script lang="ts">
import {
  computed,
  defineComponent,
  onBeforeMount,
  onMounted,
  ref,
  type Ref,
  watch,
} from "vue";
import { useRoute, useRouter } from "vue-router";
import { fetchOneObject, GetAuthInfo } from "@/api/api";
import { useI18n } from "vue-i18n";
import { i18n } from "@/main"; // Import i18n from main.ts
import { useAppStore } from "@/store/app.store";
import { usePrimeVue } from "primevue/config";
import { useHttpRequest } from "@/store/httpRequest.store";
import { definePreset, palette } from "@primeuix/themes";
import Aura from "@primeuix/themes/aura";
import Lara from "@primeuix/themes/lara";
import Nora from "@primeuix/themes/nora";
import Material from "@primeuix/themes/material";
import arabic from "@/i18n/ar";
import french from "@/i18n/fr";
import keycloak from "@/keycloak";
export default defineComponent({
  setup() {
    const { t } = useI18n();
    const appStore = useAppStore();
    const httpRequest = useHttpRequest();
    const object: Ref<ObjectModel | null> = ref(null);
    const route = useRoute();
    const router = useRouter();
    const formID = ref(route.params.guid as string);
    const done = ref(0);
    const isStepper = ref(false);
    const isRTL = ref(false);
    const isMultilingual = ref(false);
    const steps = ref(0);
    const showPageNames = ref(false);
    const names = ref({} as any);
    const formName = ref();
    const language = ref("FR");
    const paramValue = ref({ ...route.query });
    const showFormHeader = ref(paramValue.value.showFormHeader === "true");
    const newDoc = ref(paramValue.value.newDoc === "true");
    const showPageNumF = ref(1);
    const languages: Ref<any[]> = ref([]);
    const languagesList: Ref<any[]> = ref([]);
    const formfound = ref(true);
    const PrimeVue = usePrimeVue();
    const localFormConfig = ref({} as any);
    const isAuthenticated = ref(false);
    const authRequired = ref(false);
    const authConfig = ref(null as any);

    const themePresets = {
      lara: Lara,
      nora: Nora,
      material: Material,
      aura: Aura,
    } as any;

    function hexToPalette(hex: string) {
      return palette(hex) as any;
    }

    const applyDynamicTheme = () => {
      const themeConfig = localFormConfig.value.externalFormTheme;
      if (!themeConfig) {
        console.warn("No externalFormTheme found.");
        return;
      }

      console.log("Loading dynamic theme:", themeConfig);

      const selectedTheme = themeConfig.theme?.toLowerCase() || "aura";
      var primaryColor = themeConfig.primary || "#1976D2";
      // if its start with # the set it as is, else add #
      if (!primaryColor.startsWith("#")) {
        primaryColor = "#" + primaryColor;
      }
      appStore.setPrimaryColor(primaryColor);
      const surfaceColor = themeConfig.surface || "#ffffff";

      const preset = themePresets[selectedTheme] || Aura;
      const MyPreset = definePreset(preset, {
        semantic: {
          primary: hexToPalette(primaryColor),
          colorScheme: {
            light: {
              surface: hexToPalette(surfaceColor),
            },
            dark: {
              surface: hexToPalette(surfaceColor),
            },
          },
        },
      });

      PrimeVue.config.theme = {
        preset: MyPreset,
        options: {
          darkModeSelector: isDarkMode.value,
        },
      };
    };
    onBeforeMount(async () => {
      appStore.setExternalAuth(
        route.query.code as string,
        route.params.guid as string
      );

      if (route.query.code && route.params.guid) {
        try {
          const authInfo = await GetAuthInfo({
            code: route.query.code as string,
            guid: route.params.guid as string,
          });
          console.log("Auth info received:", authInfo);

          if (!authInfo.valid) {
            router.push({ name: "unauthorized" });
            return;
          }

          if (authInfo.authtype === "oidc") {
            authRequired.value = true;
            authConfig.value = authInfo.authconfig;

            // For Azure AD and other OIDC providers, we need to handle this differently
            try {
              // Check if we're returning from successful authentication
              const authFlag = sessionStorage.getItem("oidc_authenticated");
              console.log("Checking authentication flag:", authFlag);

              if (authFlag === "true") {
                // We have successfully authenticated
                isAuthenticated.value = true;
                console.log("Successfully authenticated with OIDC");

                // Clean up the authentication flag
                sessionStorage.removeItem("oidc_authenticated");

                // Restore the original URL parameters
                // Restore the original query string
                const originalQuery = sessionStorage.getItem("original_query");
                console.log("Restoring original query:", originalQuery);

                if (originalQuery) {
                  const cleanUrl = `${window.location.origin}${window.location.pathname}${originalQuery}`;
                  console.log("Cleaning URL to:", cleanUrl);
                  window.history.replaceState({}, document.title, cleanUrl);
                }

                // Clean up session storage
                sessionStorage.removeItem("original_query");

                // Clean up session storage
                sessionStorage.removeItem("original_query");
              } else {
                // We need to redirect to the OIDC provider
                console.log("Redirecting to OIDC provider...");

                // Store the original query string for preserving URL structure
                sessionStorage.setItem(
                  "original_query",
                  window.location.search
                );

                // Store the current form URL to return to after authentication
                sessionStorage.setItem("return_url", window.location.href);

                // Generate a random state for security
                const randomState = Math.random().toString(36).substring(2, 15);
                sessionStorage.setItem("oidc_state", randomState);

                // Use a generic callback URL with the correct base path from Vite config
                const callbackUrl = `${window.location.origin}/neoformext/front/auth/callback`;

                // Build the authorization URL
                let authority = authInfo.authconfig.Authority;
                // Ensure the authority has the https:// protocol
                if (
                  !authority.startsWith("http://") &&
                  !authority.startsWith("https://")
                ) {
                  authority = "https://" + authority;
                }

                let authEndpoint;
                if (authority.includes("auth0.com")) {
                  // Auth0 uses /authorize
                  authEndpoint = authority + "/authorize";
                } else if (authority.includes("microsoftonline.com")) {
                  // Azure AD uses /oauth2/v2.0/authorize
                  authEndpoint = authority + "/oauth2/v2.0/authorize";
                } else if (authority.includes("accounts.google.com")) {
                  // Google uses /o/oauth2/v2/auth
                  authEndpoint = authority + "/o/oauth2/v2/auth";
                } else if (authority.includes("okta.com")) {
                  // Okta uses /oauth2/v1/authorize
                  authEndpoint = authority + "/oauth2/v1/authorize";
                } else if (authority.includes("keycloak")) {
                  // Keycloak - check if the authority already includes the realm path
                  if (authority.includes("/auth/realms/")) {
                    authEndpoint = authority + "/protocol/openid-connect/auth";
                  } else {
                    // Assume default realm if not specified
                    authEndpoint =
                      authority +
                      "/auth/realms/master/protocol/openid-connect/auth";
                  }
                } else {
                  // Generic OIDC providers typically use /authorize
                  authEndpoint = authority + "/authorize";
                }

                const authUrl = new URL(authEndpoint);
                authUrl.searchParams.append(
                  "client_id",
                  authInfo.authconfig.ClientId
                );
                authUrl.searchParams.append("response_type", "code");
                authUrl.searchParams.append("redirect_uri", callbackUrl);
                authUrl.searchParams.append("scope", authInfo.authconfig.Scope);
                authUrl.searchParams.append("state", randomState);

                // Add any provider-specific parameters if they exist in authconfig
                if (authInfo.authconfig.AdditionalParams) {
                  try {
                    const additionalParams =
                      typeof authInfo.authconfig.AdditionalParams === "string"
                        ? JSON.parse(authInfo.authconfig.AdditionalParams)
                        : authInfo.authconfig.AdditionalParams;

                    for (const [key, value] of Object.entries(
                      additionalParams
                    )) {
                      authUrl.searchParams.append(key, value as string);
                    }
                  } catch (error) {
                    console.warn(
                      "Failed to parse additional parameters",
                      error
                    );
                  }
                }

                // Redirect to the authorization endpoint
                window.location.href = authUrl.toString();
                return;
              }
            } catch (oidcError) {
              console.error("OIDC authentication failed:", oidcError);
              return;
            }
          } else {
            // No authentication required or different auth type
            isAuthenticated.value = true;
          }
        } catch (error) {
          console.error("Failed to get auth info:", error);
          // Continue loading the form even if auth check fails
          isAuthenticated.value = true;
        }
      } else {
        console.log("No external auth code or GUID found.");
        isAuthenticated.value = true;
      }

      // Only proceed to load the form if authenticated
      if (isAuthenticated.value) {
        console.log("Auth Updated");
        object.value = await fetchOneObject(formID.value);

        localFormConfig.value = JSON.parse(
          object.value?.objectJson
        ).objectConfig.formConfig;
        applyDynamicTheme();
        formName.value = localFormConfig.value.formName;
        isStepper.value = localFormConfig.value.isStepper;
        isRTL.value = localFormConfig.value.isRTL;
        isMultilingual.value = localFormConfig.value.isMultilingual;
        languages.value = localFormConfig.value.languages;
        // languagesList.value = convertLanguages(languages.value);
        console.log("languages.value", languages.value);
        steps.value = localFormConfig.value.stepNumber;
        showPageNames.value = localFormConfig.value.showPageNames;
        if (isStepper.value) {
          names.value = JSON.parse(
            object.value?.objectJson
          ).objectConfig.formTemplate[0].config.names;
        }
        i18n.global.locale.value = isRTL.value ? "ar" : "fr";
        isRTL.value
          ? (PrimeVue.config.locale = { ...arabic.LocaleOptions })
          : (PrimeVue.config.locale = { ...french.LocaleOptions });
        console.log("i18n locale set to:", i18n.global.locale.value);
      }
    });

    const form: Ref<any[]> = computed(() => {
      return object.value
        ? JSON.parse(object.value.objectJson).objectConfig.formTemplate
        : [];
    });
    const configForm: Ref<any[]> = computed(() => {
      return object.value
        ? JSON.parse(object.value.objectJson).objectConfig.formConfig
        : [];
    });
    const isFormDisplay = ref({
      value: true,
      objectId: object.value?.id ?? "65",
    });
    const isSubmit = ref(false);
    const submit = () => {
      isSubmit.value = true;
    };
    const cancel = () => {
      if (window.self === window.top) {
        router.go(-1);
      } else {
        window.parent.postMessage("EliseCustomActionDone", "*");
      }
    };
    const handleIsSubmit = (value: boolean) => {
      isSubmit.value = value;
    };
    const handleDone = (event: any) => {
      if (event == 1) {
        done.value = 1;
      } else {
        done.value = 2;
      }
      document.body.classList.remove("grayOutBody");
    };
    const showForm = computed(() => {
      return done.value;
    });
    const dynamicHeaderHeight = computed(() => {
      return showFormHeader.value ? "50px" : "0px";
    });
    document.documentElement.style.setProperty(
      "--dynamic-header-height",
      dynamicHeaderHeight.value
    );
    const navigateNext = (page: number) => {
      showPageNumF.value = page + 1;
    };
    const submitStepper = () => {
      done.value = 1;
    };
    const executeNavigateNext = ref(false);
    const navigateToPage = (page: number) => {
      if (page > steps.value || page < 1) {
        return;
      }
      executeNavigateNext.value = true;
    };

    // Computed properties for button texts
    const cancelButtonText = computed(() => t("buttons.cancel"));
    const previousButtonText = computed(() => t("buttons.previous"));
    const nextButtonText = computed(() => t("buttons.next"));
    const submitButtonText = computed(() => t("buttons.validate"));

    const isDarkMode = ref(false);

    // Watch for dark mode toggle and update PrimeVue theme
    watch(isDarkMode, (val) => {
      PrimeVue.config.theme.options = {
        ...PrimeVue.config.theme.options,
        darkModeSelector: val,
      };
    });
    watch(isDarkMode, () => {
      applyDynamicTheme();
    });
    watch(isDarkMode, (val) => {
      if (val) {
        document.body.classList.add("dark");
      } else {
        document.body.classList.remove("dark");
      }
    });

    // Optionally, initialize from system preference
    // onMounted(() => {
    //   isDarkMode.value = window.matchMedia(
    //     "(prefers-color-scheme: dark)"
    //   ).matches;
    // });

    return {
      form,
      formID,
      object,
      isFormDisplay,
      isSubmit,
      done,
      configForm,
      showForm,
      isStepper,
      isRTL,
      steps,
      showPageNames,
      names,
      formName,
      showFormHeader,
      dynamicHeaderHeight,
      showPageNumF,
      executeNavigateNext,
      newDoc,
      language,
      languages,
      isMultilingual,
      languagesList,
      isAuthenticated,
      authRequired,
      authConfig,
      t,
      submit,
      handleIsSubmit,
      handleDone,
      cancel,
      navigateNext,
      submitStepper,
      navigateToPage,
      formfound,
      cancelButtonText,
      previousButtonText,
      nextButtonText,
      submitButtonText,
      isDarkMode,
    };
  },
});
</script>

<style lang="scss">
.form-viewer-container {
  position: relative;
  &-header {
    padding: 15px;
    box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
    color: #266c87;
    position: fixed;
    width: 100%;
    top: 0;
    z-index: 1000;
    background-color: white;
  }
  &-content {
    padding: 15px;
    margin-top: var(--dynamic-header-height);
    margin-bottom: 50px;
    overflow-y: auto;
    height: calc(100% - 100px);
    .form-viewer {
      width: 100%;
      overflow-y: auto;
      overflow-x: hidden;
    }
  }
  &-footer {
    padding: 15px;
    box-shadow: 0 -2px 4px 0 rgba(0, 0, 0, 0.1);
    color: #266c87;
    z-index: 1000 !important;
    position: fixed;
    width: 100%;
    bottom: 0;
    background-color: white;
  }
}
.form-container {
  max-height: 100vh !important;
  padding: unset !important;
}
.main-container {
  background-color: white !important;
}
.form-viewer-container-content {
  padding: 15px;
  margin-top: var(--dynamic-header-height);
  margin-bottom: 60px; /* Footer height */
  height: calc(
    100vh - var(--dynamic-header-height) - 100px
  ); /* 50px is the footer height */
  overflow-y: auto; /* Enable vertical scrolling */
}
.form-viewer-container-footer {
  padding: 15px;
  box-shadow: 0 -2px 4px 0 rgba(0, 0, 0, 0.1);
  color: #266c87;
  position: fixed;
  width: 100%;
  bottom: 0;
  background-color: white;
  height: 60px; /* Fixed footer height */
  z-index: 1000 !important;
}
.form-viewer-container-content {
  scroll-behavior: smooth; /* Smooth scrolling */
}
.custom-padding-rtl {
  padding-left: 60px;
}
.stepper {
  position: relative;
  .pages-headers {
    position: fixed;
    width: 100%;
    top: 0;
    z-index: 1000;
  }
}
.zone-page-sticky-header {
  .zone-page-header {
    position: fixed;
    top: 0;
    width: 100%;
    z-index: 1000;
    background-color: white;
    left: 0;
  }
}
.main-container {
  display: flex;
  overflow-y: auto;
  padding: 10px 5px 0px 5px;
  background-color: #efefef;
  height: 90vh !important;
  .form-container {
    padding: 15px;
    background-color: rgb(255, 255, 255);
    width: 100%;
    border-radius: 10px;
    overflow-x: hidden;
    overflow-y: hidden;
    height: 100%;
  }
}
body.dark {
  background-color: #181818 !important; // or any dark color you prefer
}

body.dark .form-viewer-container,
body.dark .main-container,
body.dark .main-container .form-container,
body.dark .form-viewer-container-content,
body.dark .form-viewer-container-footer,
body.dark .form-viewer-container-header {
  background-color: #181818 !important;
  color: #fff !important;
}
</style>
