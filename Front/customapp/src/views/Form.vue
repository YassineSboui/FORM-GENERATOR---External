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
        <div class="form-viewer form-container-wrapper">
          <div class="form-card pb-5">
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
              :systemVariables="systemVariables"
            ></component-form>
          </div>
        </div>
      </div>
    </div>
    <div
      class="form-viewer-container-footer"
      :class="{ 'custom-padding-rtl': isRTL }"
      v-if="isFormDisplay.value && form.length !== 0"
    >
      <div class="footer-content">
        <!-- Left: ToggleSwitch -->
        <div>
          <ToggleSwitch
            v-model="isDarkMode"
            class="mt-1"
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
                v-if="
                  !newDoc &&
                  systemVariables.DISPLAY_BUTTON_CANCEL !== false &&
                  systemVariables.DISPLAY_BUTTON_CANCEL !== 'false'
                "
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
                v-if="
                  (showPageNumF === steps || steps === 0) &&
                  systemVariables.DISPLAY_BUTTON_OK !== false &&
                  systemVariables.DISPLAY_BUTTON_OK !== 'false'
                "
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
        <p>{{ $t("Authentication.authenticating") }}</p>
        <p class="text-sm text-gray-600">
          {{ $t("Authentication.redirectingMessage") }}
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

  <!-- Email Authentication Dialog -->
  <Dialog
    v-model:visible="showEmailDialog"
    modal
    :header="$t('EmailAuth.accessVerification')"
    :style="{ width: '450px' }"
    :closable="false"
  >
    <div class="flex flex-column align-items-center p-4">
      <i
        class="pi pi-envelope"
        style="
          font-size: 3rem;
          color: var(--primary-color);
          margin-bottom: 1rem;
        "
      ></i>
      <h3 class="text-center mb-3">
        {{ $t("EmailAuth.emailVerificationRequired") }}
      </h3>
      <p class="text-center mb-4">
        {{ $t("EmailAuth.enterEmailMessage") }}
      </p>

      <div class="w-full">
        <label for="email" class="block text-sm font-medium mb-2">{{
          $t("EmailAuth.emailAddress")
        }}</label>
        <InputText
          id="email"
          v-model="emailInput"
          type="email"
          :placeholder="$t('EmailAuth.enterEmailPlaceholder')"
          class="w-full"
          :class="{ 'p-invalidCustom': emailValidationError }"
          @keyup.enter="handleEmailSubmit"
        />
        <small
          v-if="emailValidationError"
          class="p-errorCustom"
          style="color: #ef4444"
          >{{ emailValidationError }}</small
        >
      </div>
    </div>

    <template #footer>
      <div class="flex justify-content-end gap-2">
        <Button
          :label="$t('EmailAuth.cancel')"
          icon="pi pi-times"
          severity="secondary"
          @click="handleEmailCancel"
        />
        <Button
          :label="$t('EmailAuth.continue')"
          icon="pi pi-check"
          :loading="emailLoading"
          @click="handleEmailSubmit"
        />
      </div>
    </template>
  </Dialog>

  <!-- OTP Verification Dialog -->
  <Dialog
    v-model:visible="showOTPDialog"
    modal
    :header="$t('EmailAuth.verifyYourEmail')"
    :style="{ width: '450px' }"
    :closable="false"
  >
    <div class="flex flex-column align-items-center p-4">
      <i
        class="pi pi-shield"
        style="
          font-size: 3rem;
          color: var(--primary-color);
          margin-bottom: 1rem;
        "
      ></i>
      <h3 class="text-center mb-3">
        {{ $t("EmailAuth.enterVerificationCode") }}
      </h3>
      <p class="text-center mb-4">
        {{ $t("EmailAuth.codeSentTo") }}<br />
        <strong>{{ userEmail }}</strong>
      </p>

      <div class="w-full text-center">
        <label for="otp" class="block text-sm font-medium mb-2">{{
          $t("EmailAuth.verificationCode")
        }}</label>
        <InputOtp
          v-model="otpInput"
          :length="6"
          integerOnly
          class="mb-3"
          @complete="handleOTPComplete"
        />
        <small
          v-if="otpValidationError"
          class="p-errorCustom block mb-3"
          style="color: #ef4444"
          >{{ otpValidationError }}</small
        >

        <div class="text-center">
          <p class="text-sm text-600 mb-2">
            {{ $t("EmailAuth.didntReceiveCode") }}
          </p>
          <Button
            :label="$t('EmailAuth.resendCode')"
            link
            class="p-0"
            :loading="resendLoading"
            @click="handleResendOTP"
          />
        </div>
      </div>
    </div>

    <template #footer>
      <div class="flex justify-content-end gap-2">
        <Button
          :label="$t('EmailAuth.back')"
          icon="pi pi-arrow-left"
          severity="secondary"
          @click="handleOTPBack"
        />
        <Button
          :label="$t('EmailAuth.verify')"
          icon="pi pi-check"
          :loading="otpLoading"
          @click="handleOTPSubmit"
        />
      </div>
    </template>
  </Dialog>
</template>

<script lang="ts">
import {
  computed,
  defineComponent,
  onBeforeMount,
  onBeforeUnmount,
  ref,
  type Ref,
  watch,
} from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  fetchOneObject,
  GetAuthInfo,
  validateEmailInvitation,
  sendEmailOTP,
  verifyEmailOTP,
  validateAuthToken,
} from "@/api/api";
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
    const systemVariables: Ref<any> = ref({});
    const formfound = ref(true);
    const PrimeVue = usePrimeVue();
    const localFormConfig = ref({} as any);
    const isAuthenticated = ref(false);
    const authRequired = ref(false);
    const authConfig = ref(null as any);

    // Email authentication variables
    const showEmailDialog = ref(false);
    const showOTPDialog = ref(false);
    const emailInput = ref("");
    const otpInput = ref("");
    const emailValidationError = ref("");
    const otpValidationError = ref("");
    const resendLoading = ref(false);
    const userEmail = ref("");
    const emailLoading = ref(false);
    const otpLoading = ref(false);
    const authToken = ref(""); // Store secure authentication token (prevents sessionStorage manipulation)

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

    // Email authentication functions
    const validateEmail = (email: string) => {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return emailRegex.test(email);
    };

    const validateEmailInvitationAPI = async (email: string) => {
      try {
        emailLoading.value = true;
        emailValidationError.value = "";

        const result = await validateEmailInvitation(
          email,
          route.params.guid as string,
          route.query.code as string,
          route.params.client as string
        );
        return result;
      } catch (error: any) {
        emailValidationError.value =
          error.message || t("EmailAuth.failedToValidateEmail");
        throw error;
      } finally {
        emailLoading.value = false;
      }
    };

    const sendOTP = async (email: string) => {
      try {
        otpLoading.value = true;
        otpValidationError.value = "";

        const result = await sendEmailOTP(
          email,
          route.params.guid as string,
          route.query.code as string,
          route.params.client as string
        );
        return result;
      } catch (error: any) {
        otpValidationError.value =
          error.message || t("EmailAuth.failedToSendOTP");
        throw error;
      } finally {
        otpLoading.value = false;
      }
    };

    const verifyOTP = async (email: string, otp: string) => {
      try {
        otpLoading.value = true;
        otpValidationError.value = "";

        const result = await verifyEmailOTP(
          email,
          otp,
          route.params.guid as string,
          route.query.code as string,
          route.params.client as string
        );
        return result;
      } catch (error: any) {
        otpValidationError.value =
          error.message || t("EmailAuth.otpVerificationFailed");
        throw error;
      } finally {
        otpLoading.value = false;
      }
    };

    // Validate stored authentication token for security
    const validateStoredAuth = async () => {
      try {
        const storedToken = sessionStorage.getItem("email_auth_token");
        const storedEmail = sessionStorage.getItem("authenticated_email");

        if (!storedToken || !storedEmail) {
          console.log("No stored authentication token or email found");
          return false;
        }

        // For ALL email authentication (both OTP and non-OTP), verify with server
        // This prevents sessionStorage manipulation attacks
        console.log("Validating stored authentication with server...");

        // Check if this is a simple session token (non-OTP case) or a JWT token (OTP case)
        if (storedToken.startsWith("email_session_")) {
          console.log(
            "Simple session token detected, verifying email with server..."
          );

          // For non-OTP authentication, re-validate the email with the server
          // This ensures the email is still authorized and prevents sessionStorage bypass
          try {
            const emailValidationResult = await validateEmailInvitation(
              storedEmail,
              route.params.guid as string,
              route.query.code as string,
              route.params.client as string
            );

            if (emailValidationResult.valid) {
              console.log("Stored email is still valid on server");
              authToken.value = storedToken;
              userEmail.value = storedEmail;
              return true;
            } else {
              console.log(
                "Stored email is no longer valid on server, clearing session"
              );
              // Clear invalid session data
              sessionStorage.removeItem("email_authenticated");
              sessionStorage.removeItem("authenticated_email");
              sessionStorage.removeItem("email_auth_token");
              return false;
            }
          } catch (emailError) {
            console.error("Email re-validation failed:", emailError);
            // Clear potentially compromised session data
            sessionStorage.removeItem("email_authenticated");
            sessionStorage.removeItem("authenticated_email");
            sessionStorage.removeItem("email_auth_token");
            return false;
          }
        } else {
          // This should be a JWT token, validate it with the server
          console.log("JWT token detected, validating with server...");
          const result = await validateAuthToken(
            storedToken,
            storedEmail,
            route.params.guid as string,
            route.query.code as string,
            route.params.client as string
          );

          if (result.valid) {
            console.log("Stored authentication token is valid");
            authToken.value = storedToken;
            userEmail.value = storedEmail;
            return true;
          } else {
            console.log(
              "Stored authentication token is invalid, clearing session"
            );
            // Clear invalid session data
            sessionStorage.removeItem("email_authenticated");
            sessionStorage.removeItem("authenticated_email");
            sessionStorage.removeItem("email_auth_token");
            return false;
          }
        }
      } catch (error) {
        console.error("Error validating stored auth token:", error);
        // Clear potentially compromised session data
        sessionStorage.removeItem("email_authenticated");
        sessionStorage.removeItem("otp_authenticated");
        sessionStorage.removeItem("authenticated_email");
        sessionStorage.removeItem("email_auth_token");
        return false;
      }
    };

    // Email Dialog Handlers
    const handleEmailSubmit = async () => {
      emailValidationError.value = "";

      if (!emailInput.value.trim()) {
        emailValidationError.value = t("EmailAuth.enterEmailAddress");
        return;
      }

      if (!validateEmail(emailInput.value)) {
        emailValidationError.value = t("EmailAuth.enterValidEmail");
        return;
      }

      try {
        userEmail.value = emailInput.value;

        if (authConfig.value?.authtype === "otp") {
          // For OTP auth type, send OTP directly without validating email invitation
          await sendOTP(emailInput.value);
          showEmailDialog.value = false;
          showOTPDialog.value = true;
        } else {
          // For invitation auth type, validate email first
          const result = await validateEmailInvitationAPI(emailInput.value);

          if (result.valid) {
            showEmailDialog.value = false;
            console.log("authConfig:", authConfig.value);
            // Need OTP verification
            await sendOTP(emailInput.value);
            showOTPDialog.value = true;
          } else {
            emailValidationError.value = t("EmailAuth.emailNotAuthorized");
          }
        }
      } catch (error: any) {
        console.error("Email validation error:", error);

        // Handle different HTTP status codes
        if (error.response?.status === 400) {
          const errorMessage = error.response?.data?.message || error.message;
          if (
            errorMessage.includes("not authorized") ||
            errorMessage.includes("whitelist")
          ) {
            emailValidationError.value = t("EmailAuth.emailNotAuthorized");
          } else if (
            errorMessage.includes("rate limit") ||
            errorMessage.includes("too many")
          ) {
            emailValidationError.value = t("EmailAuth.tooManyAttempts");
          } else {
            emailValidationError.value = t("EmailAuth.invalidEmailFormat");
          }
        } else if (error.response?.status === 500) {
          emailValidationError.value = t("EmailAuth.serverError");
        } else if (error.response?.status === 404) {
          emailValidationError.value = t("EmailAuth.formNotFound");
        } else {
          emailValidationError.value =
            error.message || t("EmailAuth.failedToValidateEmail");
        }
      }
    };

    const handleEmailCancel = () => {
      clearAuthenticationData();
      showEmailDialog.value = false;
      router.push({ name: "unauthorized" });
    };

    // OTP Dialog Handlers
    const handleOTPSubmit = async () => {
      otpValidationError.value = "";

      if (!otpInput.value || otpInput.value.length !== 6) {
        otpValidationError.value = t("EmailAuth.enterSixDigitCode");
        return;
      }

      if (!/^\d{6}$/.test(otpInput.value)) {
        otpValidationError.value = t("EmailAuth.codeMustBeSixDigits");
        return;
      }

      try {
        const result = await verifyEmailOTP(
          userEmail.value,
          otpInput.value,
          route.params.guid as string,
          route.query.code as string,
          route.params.client as string
        );

        if (result.valid) {
          if (result.token) {
            authToken.value = result.token;
            showOTPDialog.value = false;
            completeEmailAuthentication();
          } else {
            otpValidationError.value = t("EmailAuth.verificationFailed");
          }
        } else {
          otpValidationError.value = t("EmailAuth.invalidVerificationCode");
        }
      } catch (error: any) {
        console.error("OTP verification error:", error);

        // Handle different HTTP status codes
        if (error.response?.status === 400) {
          const errorMessage = error.response?.data?.message || error.message;
          if (
            errorMessage.includes("expired") ||
            errorMessage.includes("invalid")
          ) {
            otpValidationError.value = t("EmailAuth.invalidOrExpiredCode");
          } else if (
            errorMessage.includes("rate limit") ||
            errorMessage.includes("too many")
          ) {
            otpValidationError.value = t("EmailAuth.tooManyAttempts");
          } else {
            otpValidationError.value = t("EmailAuth.invalidVerificationCode");
          }
        } else if (error.response?.status === 500) {
          otpValidationError.value = t("EmailAuth.serverError");
        } else {
          otpValidationError.value =
            error.message || t("EmailAuth.verificationFailed");
        }
      }
    };

    const handleOTPComplete = (value: string) => {
      if (value && value.length === 6) {
        handleOTPSubmit();
      }
    };

    const handleOTPBack = () => {
      showOTPDialog.value = false;
      showEmailDialog.value = true;
      otpInput.value = "";
      otpValidationError.value = "";
    };

    const handleResendOTP = async () => {
      try {
        resendLoading.value = true;
        await sendOTP(userEmail.value);
        otpValidationError.value = "";
        // Show success message briefly
        otpValidationError.value = t("EmailAuth.codeSentSuccessfully");
        setTimeout(() => {
          otpValidationError.value = "";
        }, 3000);
      } catch (error: any) {
        console.error("Resend OTP error:", error);

        // Handle different HTTP status codes
        if (error.response?.status === 400) {
          const errorMessage = error.response?.data?.message || error.message;
          if (
            errorMessage.includes("rate limit") ||
            errorMessage.includes("too many")
          ) {
            otpValidationError.value = t("EmailAuth.tooManyAttempts");
          } else {
            otpValidationError.value = t("EmailAuth.failedToResendCode");
          }
        } else if (error.response?.status === 500) {
          otpValidationError.value = t("EmailAuth.serverError");
        } else {
          otpValidationError.value =
            error.message || t("EmailAuth.failedToResendCode");
        }
      } finally {
        resendLoading.value = false;
      }
    };

    const openEmailDialog = () => {
      emailInput.value = "";
      emailValidationError.value = "";
      showEmailDialog.value = true;
    };

    // Clear authentication data (for logout or security purposes)
    const clearAuthenticationData = () => {
      sessionStorage.removeItem("email_authenticated");
      sessionStorage.removeItem("otp_authenticated");
      sessionStorage.removeItem("authenticated_email");
      sessionStorage.removeItem("email_auth_token");
      authToken.value = "";
      userEmail.value = "";
      isAuthenticated.value = false;
    };

    const completeEmailAuthentication = async () => {
      // Store secure authentication data based on auth type
      if (authConfig.value?.authtype === "otp") {
        sessionStorage.setItem("otp_authenticated", "true");
      } else {
        sessionStorage.setItem("email_authenticated", "true");
      }

      sessionStorage.setItem("authenticated_email", userEmail.value);
      sessionStorage.setItem("email_auth_token", authToken.value); // Store secure token
      isAuthenticated.value = true;

      // Clear any existing error messages
      emailValidationError.value = "";
      otpValidationError.value = "";

      // Clear input fields
      emailInput.value = "";
      otpInput.value = "";

      // Ensure all dialogs are closed
      showEmailDialog.value = false;
      showOTPDialog.value = false;

      console.log("Successfully authenticated with email:", userEmail.value);

      // Load the form data after authentication
      await loadFormData();
    };

    // Extract form loading logic into a separate function
    const loadFormData = async () => {
      try {
        // For email authentication, validate token before loading sensitive data
        if (
          authRequired.value &&
          (authConfig.value?.authtype === "invitation" ||
            authConfig.value?.authtype === "otp")
        ) {
          if (!authToken.value) {
            console.error("No authentication token available");
            clearAuthenticationData();
            openEmailDialog();
            return;
          }

          // Always re-validate authentication with server before loading form data
          // This prevents sessionStorage manipulation attacks for both OTP and non-OTP cases
          console.log(
            "Re-validating authentication before loading form data..."
          );
          const isValid = await validateStoredAuth();
          if (!isValid) {
            console.error(
              "Authentication validation failed, requiring re-authentication"
            );
            openEmailDialog();
            return;
          }
        }

        console.log("Loading form data...");
        object.value = await fetchOneObject(formID.value);

        localFormConfig.value = JSON.parse(
          object.value?.objectJson
        ).objectConfig.formConfig;
        applyDynamicTheme();
        // Initialize system variables
        systemVariables.value = localFormConfig.value.systemVariables || {
          BUTTON_CANCEL: "Annuler",
          BUTTON_OK: "Valider",
          FORM_UID: "",
          DISPLAY_BUTTON_CANCEL: true,
          DISPLAY_BUTTON_OK: true,
        };
        formName.value = localFormConfig.value.formName;
        isStepper.value = localFormConfig.value.isStepper;
        isRTL.value = localFormConfig.value.isRTL;
        isMultilingual.value = localFormConfig.value.isMultilingual;
        languages.value = localFormConfig.value.languages;
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
        console.log("Form data loaded successfully");
      } catch (error) {
        console.error("Failed to load form data:", error);
      }
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

                  // Clean up session storage after successful restoration
                  sessionStorage.removeItem("original_query");
                  sessionStorage.removeItem("original_guid");
                }

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

                // Store the original guid for token validation
                sessionStorage.setItem(
                  "original_guid",
                  route.params.guid as string
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
          } else if (authInfo.authtype === "invitation") {
            authRequired.value = true;
            authConfig.value = authInfo.authconfig;

            // Check if already authenticated with email
            const emailAuthFlag = sessionStorage.getItem("email_authenticated");
            const authenticatedEmail = sessionStorage.getItem(
              "authenticated_email"
            );

            console.log("Checking email authentication flag:", emailAuthFlag);

            if (emailAuthFlag === "true" && authenticatedEmail) {
              // Validate stored authentication with server for security
              // This prevents sessionStorage manipulation attacks by verifying
              // both the email and token with the backend server
              console.log(
                "Validating stored email authentication with server..."
              );
              const isTokenValid = await validateStoredAuth();

              if (isTokenValid) {
                // Authentication is valid
                isAuthenticated.value = true;
                console.log(
                  "Server confirmed authentication is valid for email:",
                  authenticatedEmail
                );
                // Load form data since we're already authenticated
                await loadFormData();
              } else {
                // Token/email validation failed, require re-authentication
                console.log(
                  "Server validation failed - stored authentication is invalid, requiring re-authentication"
                );
                setTimeout(() => {
                  openEmailDialog();
                }, 500);
                return;
              }
            } else {
              // Need to authenticate with email
              console.log("Opening email authentication dialog...");
              // Don't load form data yet, wait for authentication
              // Delay the dialog to ensure the component is fully mounted
              setTimeout(() => {
                openEmailDialog();
              }, 500);
              return; // Exit early, don't load form data
            }
          } else if (authInfo.authtype === "otp") {
            authRequired.value = true;
            authConfig.value = authInfo.authconfig;

            // Check if already authenticated with OTP
            const otpAuthFlag = sessionStorage.getItem("otp_authenticated");
            const authenticatedEmail = sessionStorage.getItem(
              "authenticated_email"
            );

            console.log("Checking OTP authentication flag:", otpAuthFlag);

            if (otpAuthFlag === "true" && authenticatedEmail) {
              // Validate stored authentication with server for security
              console.log(
                "Validating stored OTP authentication with server..."
              );
              const isTokenValid = await validateStoredAuth();

              if (isTokenValid) {
                // Authentication is valid
                isAuthenticated.value = true;
                console.log(
                  "Server confirmed OTP authentication is valid for email:",
                  authenticatedEmail
                );
                // Load form data since we're already authenticated
                await loadFormData();
              } else {
                // Token/email validation failed, require re-authentication
                console.log(
                  "Server validation failed - stored OTP authentication is invalid, requiring re-authentication"
                );
                setTimeout(() => {
                  openEmailDialog();
                }, 500);
                return;
              }
            } else {
              // Need to authenticate with OTP
              console.log("Opening OTP authentication dialog...");
              // Don't load form data yet, wait for authentication
              // Delay the dialog to ensure the component is fully mounted
              setTimeout(() => {
                openEmailDialog();
              }, 500);
              return; // Exit early, don't load form data
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
        isAuthenticated.value = false;
        router.push({ name: "unauthorized" });
      }

      // Only proceed to load the form if authenticated
      if (isAuthenticated.value) {
        console.log("Auth Updated");
        await loadFormData();
      }
    });

    const form: Ref<any[]> = computed(() => {
      return object.value
        ? JSON.parse(object.value.objectJson).objectConfig.formTemplate
        : [];
    });
    const configForm: Ref<any[]> = computed({
      get: () => {
        return object.value
          ? JSON.parse(object.value.objectJson).objectConfig.formConfig
          : {};
      },
      set: (value) => {
        if (object.value) {
          const objectJson = JSON.parse(object.value.objectJson);
          objectJson.objectConfig.formConfig = value;
          object.value.objectJson = JSON.stringify(objectJson);
        }
      },
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
    const cancelButtonText = computed(
      () => systemVariables.value.BUTTON_CANCEL || t("FormButtons.cancel")
    );

    const previousButtonText = computed(() => t("buttons.previous"));
    const nextButtonText = computed(() => t("buttons.next"));
    const submitButtonText = computed(
      () => systemVariables.value.BUTTON_OK || t("FormButtons.validate")
    );

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

    // Periodic security check to detect session manipulation
    const startSecurityCheck = () => {
      if (authRequired.value && authConfig.value?.authtype === "invitation") {
        const securityInterval = setInterval(async () => {
          const storedAuth = sessionStorage.getItem("email_authenticated");
          const storedEmail = sessionStorage.getItem("authenticated_email");
          const storedToken = sessionStorage.getItem("email_auth_token");

          // Check if session data was manually modified or manipulated
          if (storedAuth === "true" && storedEmail && !storedToken) {
            console.warn(
              "Security alert: Authentication token missing - possible sessionStorage manipulation"
            );
            clearAuthenticationData();
            openEmailDialog();
            clearInterval(securityInterval);
            return;
          }

          // Additional check: verify the stored email hasn't been changed to a different email
          if (
            isAuthenticated.value &&
            userEmail.value &&
            storedEmail !== userEmail.value
          ) {
            console.warn(
              "Security alert: Stored email does not match authenticated email - possible sessionStorage manipulation"
            );
            clearAuthenticationData();
            openEmailDialog();
            clearInterval(securityInterval);
            return;
          }

          // Validate authentication periodically (every 5 minutes)
          // This includes server-side validation for both OTP and non-OTP emails
          // Prevents sessionStorage manipulation attacks including email changes
          if (isAuthenticated.value && authToken.value) {
            try {
              const isValid = await validateStoredAuth();
              if (!isValid) {
                console.warn(
                  "Security alert: Authentication validation failed - session may have been compromised"
                );
                clearAuthenticationData();
                openEmailDialog();
                clearInterval(securityInterval);
              }
            } catch (error) {
              console.error("Security check failed:", error);
              clearAuthenticationData();
              openEmailDialog();
              clearInterval(securityInterval);
            }
          }
        }, 5 * 60 * 1000); // Check every 5 minutes

        // Store interval ID to clear it later if needed
        (window as any).securityCheckInterval = securityInterval;
      }
    };

    // Start security monitoring after authentication
    watch(isAuthenticated, (newValue) => {
      if (newValue && authRequired.value) {
        setTimeout(startSecurityCheck, 1000);
      }
    });

    // Cleanup security interval on component unmount
    onBeforeUnmount(() => {
      if ((window as any).securityCheckInterval) {
        clearInterval((window as any).securityCheckInterval);
      }
    });

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
      // Email authentication
      emailValidationError,
      otpValidationError,
      previousButtonText,
      cancelButtonText,
      submitButtonText,
      showEmailDialog,
      systemVariables,
      nextButtonText,
      showOTPDialog,
      resendLoading,
      emailLoading,
      otpLoading,
      emailInput,
      userEmail,
      formfound,
      isDarkMode,
      otpInput,
      handleEmailSubmit,
      handleEmailCancel,
      handleOTPSubmit,
      handleOTPComplete,
      handleOTPBack,
      handleResendOTP,
      openEmailDialog,
      validateEmail,
      handleIsSubmit,
      navigateToPage,
      submitStepper,
      navigateNext,
      handleDone,
      submit,
      cancel,
      t,
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
    padding: 20px;
    margin-top: var(--dynamic-header-height);
    margin-bottom: 50px;
    overflow-y: auto;
    // height: calc(100% - 100px);
    background-color: #f8f9fa; /* Soft light gray background */

    height: calc(
      100vh - var(--dynamic-header-height) - 5px
    ); /* 60px is the footer height */
    .form-viewer {
      width: 100%;
      overflow-y: auto;
      overflow-x: hidden;
    }

    .form-container-wrapper {
      display: flex;
      justify-content: center;
      padding: 20px 0;
    }

    .form-card {
      background-color: white;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
      padding: 30px;
      margin: 0 20px;
      width: 100%;
      max-width: 1200px; /* Limit maximum width for better readability */
      border: 1px solid rgba(0, 0, 0, 0.05);
      transition: box-shadow 0.3s ease;
      margin-top: 20px;
      margin-bottom: 50px;
      border-radius: 20px;
      &:hover {
        box-shadow: 0 6px 20px rgba(0, 0, 0, 0.12);
      }
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

.form-viewer-container-footer {
  padding: 15px 20px;
  box-shadow: unset !important;
  color: #266c87;
  position: fixed;
  width: 100%;
  bottom: 0;
  background-color: unset !important;
  height: 60px;
  z-index: 1000 !important;
  display: flex;
  justify-content: center;
  align-items: center;

  .footer-content {
    width: 100%;
    max-width: 1200px;
    margin: 0 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    background-color: white;
    box-shadow: 0 -2px 4px 0 rgba(0, 0, 0, 0.1);
    margin-left: -5px;
    padding: 15px;
    border-radius: 20px;
  }
}
body.dark .footer-content {
  background-color: #1e1e1e !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3) !important;
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
    width: calc(100% - 35px);
    top: 0;
    z-index: 1000;
  }
}
.zone-page-sticky-header {
  .zone-page-header {
    position: fixed;
    top: 0;
    width: calc(100% - 15px);
    z-index: 1000;
    background-color: white;
    left: 0;
  }
}
.main-container {
  display: flex;
  overflow-y: auto;
  padding: 10px 5px 0px 5px;
  background-color: #f8f9fa;
  // height: 90vh !important;
  .form-container {
    padding: 15px;
    background-color: #f8f9fa;
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

/* Dark mode for form card */
body.dark .form-viewer-container-content {
  background-color: #121212 !important; /* Darker background for content area */
}

body.dark .form-card {
  background-color: #1e1e1e !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3) !important;

  &:hover {
    box-shadow: 0 6px 20px rgba(0, 0, 0, 0.4) !important;
  }
}

/* Dark mode footer improvements */
body.dark .form-viewer-container-footer {
  background-color: unset !important;
} /* Error message styling */
.p-errorCustom {
  color: #ef4444 !important;
  font-weight: 500;
}

body.dark .p-errorCustom {
  color: #f87171 !important;
}

/* Input error styling */
.p-invalidCustom {
  border-color: #ef4444 !important;
  box-shadow: 0 0 0 1px #ef4444 !important;
}

body.dark .p-invalidCustom {
  border-color: #f87171 !important;
  box-shadow: 0 0 0 1px #f87171 !important;
}
</style>
