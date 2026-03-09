<template>
  <div class="admin-login">
    <!-- Animated Background -->
    <div class="background-animation">
      <div class="bubble"></div>
      <div class="bubble"></div>
      <div class="bubble"></div>
      <div class="bubble"></div>
      <div class="bubble"></div>
    </div>

    <div class="login-container">
      <div class="login-card">
        <div class="login-header">
          <div class="icon-wrapper">
            <i class="pi pi-shield"></i>
          </div>
          <h1>Portail Administrateur</h1>
          <p>Générateur de Formulaires</p>
        </div>

        <!-- <div v-if="defaultCredentialsInfo" class="info-box">
          <i class="pi pi-info-circle"></i>
          <div>
            <strong>Identifiants SuperAdmin par défaut:</strong>
            <div>Nom d'utilisateur: <code>superadmin</code></div>
            <div>Mot de passe: <code>SuperAdmin@123</code></div>
          </div>
        </div> -->

        <form @submit.prevent="handleLogin" class="login-form">
          <div class="form-group">
            <label for="username">
              <i class="pi pi-user"></i>
              Nom d'utilisateur
            </label>
            <input
              id="username"
              v-model="username"
              type="text"
              placeholder="Entrez votre nom d'utilisateur"
              :disabled="isLoading"
              required
              autocomplete="username"
            />
          </div>

          <div class="form-group">
            <label for="password">
              <i class="pi pi-lock"></i>
              Mot de passe
            </label>
            <div class="password-input">
              <input
                id="password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                placeholder="Entrez votre mot de passe"
                :disabled="isLoading"
                required
                autocomplete="current-password"
              />
              <button
                type="button"
                class="toggle-password"
                @click="showPassword = !showPassword"
                :disabled="isLoading"
                tabindex="-1"
              >
                <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
              </button>
            </div>
          </div>

          <div v-if="error" class="error-message">
            <i class="pi pi-exclamation-circle"></i>
            {{ error }}
          </div>

          <button type="submit" class="login-button" :disabled="isLoading">
            <span v-if="!isLoading">
              <i class="pi pi-sign-in"></i>
              Se connecter
            </span>
            <span v-else class="loading">
              <i class="pi pi-spinner pi-spin"></i>
              Connexion en cours...
            </span>
          </button>
        </form>

        <div class="login-footer">
          <p>© {{ currentYear }} Générateur de Formulaires - External</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAuthStore } from "@/store/auth.store";

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

// Form state
const username = ref("");
const password = ref("");
const showPassword = ref(false);
const defaultCredentialsInfo = ref(false);

// Computed
const isLoading = computed(() => authStore.isLoading);
const error = computed(() => authStore.error);
const currentYear = computed(() => new Date().getFullYear());

// Check if already authenticated
onMounted(() => {
  if (authStore.isAuthenticated) {
    const redirect = (route.query.redirect as string) || "/";
    router.push(redirect);
  }

  // Show default credentials info on first visit (can be stored in localStorage)
  const hasSeenInfo = localStorage.getItem("hasSeenDefaultCredentials");
  if (!hasSeenInfo) {
    defaultCredentialsInfo.value = true;
    localStorage.setItem("hasSeenDefaultCredentials", "true");
  }
});

async function handleLogin() {
  authStore.clearError();

  const success = await authStore.login(username.value, password.value);

  if (success) {
    // Check if user must change password
    if (authStore.mustChangePassword) {
      router.push("/admin/change-password");
    } else {
      // Redirect to original destination or home
      const redirect = (route.query.redirect as string) || "/";
      router.push(redirect);
    }
  }
}
</script>

<style scoped>
.admin-login {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0a6e89 0%, #fbc02d 100%);
  padding: 1rem;
  position: relative;
  overflow: hidden;
  border-radius: 24px;
}

/* Animated Background */
.background-animation {
  position: absolute;
  width: 100%;
  height: 100%;
  top: 0;
  left: 0;
  overflow: hidden;
  z-index: 0;
}

.bubble {
  position: absolute;
  bottom: -100px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 50%;
  animation: rise 15s infinite ease-in;
}

.bubble:nth-child(1) {
  width: 40px;
  height: 40px;
  left: 10%;
  animation-delay: 0s;
  animation-duration: 12s;
}

.bubble:nth-child(2) {
  width: 60px;
  height: 60px;
  left: 30%;
  animation-delay: 2s;
  animation-duration: 15s;
}

.bubble:nth-child(3) {
  width: 30px;
  height: 30px;
  left: 50%;
  animation-delay: 4s;
  animation-duration: 18s;
}

.bubble:nth-child(4) {
  width: 50px;
  height: 50px;
  left: 70%;
  animation-delay: 0s;
  animation-duration: 20s;
}

.bubble:nth-child(5) {
  width: 45px;
  height: 45px;
  left: 90%;
  animation-delay: 3s;
  animation-duration: 14s;
}

@keyframes rise {
  to {
    bottom: 110%;
    transform: translateX(100px) rotate(360deg);
  }
}

.login-container {
  width: 100%;
  max-width: 450px;
  position: relative;
  z-index: 10;
}

.login-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(12, 56, 73, 0.15);
  overflow: hidden;
  animation: slideUp 0.4s ease-out;
  backdrop-filter: blur(10px);
  position: relative;
  z-index: 10;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(15px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.login-header {
  background: linear-gradient(135deg, #0c3849 0%, #0a6e89 100%);
  color: white;
  padding: 3rem 2rem;
  text-align: center;
  position: relative;
  overflow: hidden;
  flex-shrink: 0;
}

.icon-wrapper {
  width: 80px;
  height: 80px;
  margin: 0 auto 1rem;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  backdrop-filter: blur(5px);
  border: 3px solid rgba(255, 255, 255, 0.25);
}

.login-header h1 {
  margin: 0;
  font-size: 2.25rem;
  font-weight: 800;
  position: relative;
  z-index: 1;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.login-header p {
  margin: 0.75rem 0 0;
  opacity: 0.95;
  font-size: 1.05rem;
  font-weight: 500;
  position: relative;
  z-index: 1;
}

.info-box {
  margin: 2rem;
  padding: 1.25rem;
  background: #fff9e6;
  border: 2px solid #fbc02d;
  border-radius: 8px;
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  animation: fadeInScale 0.5s ease-out 0.3s both;
  box-shadow: 0 4px 15px rgba(25, 118, 210, 0.1);
  word-break: break-word;
}

@keyframes fadeInScale {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.info-box .pi {
  color: #1976d2;
  font-size: 1.5rem;
  flex-shrink: 0;
  margin-top: 0.125rem;
}

.info-box > div {
  flex: 1;
  min-width: 0;
}

.info-box code {
  background: rgba(13, 71, 161, 0.15);
  padding: 0.25rem 0.5rem;
  border-radius: 6px;
  font-family: "Courier New", monospace;
  font-weight: 700;
  color: #0d47a1;
  border: 1px solid rgba(13, 71, 161, 0.2);
  word-break: break-all;
  display: inline-block;
}

.login-form {
  padding: 2rem;
  overflow-y: auto;
  overflow-x: hidden;
  flex: 1;
}

/* Custom Scrollbar for Form */
.login-form::-webkit-scrollbar {
  width: 8px;
}

.login-form::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

.login-form::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #0a6e89 0%, #fbc02d 100%);
  border-radius: 10px;
}

.login-form::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(135deg, #fbc02d 0%, #0a6e89 100%);
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
  font-weight: 700;
  color: #333;
  font-size: 0.95rem;
}

.form-group label i {
  color: #0a6e89;
  font-size: 1.1rem;
}

.form-group input {
  width: 100%;
  padding: 1rem 1.25rem;
  border: 2px solid #e0e0e0;
  border-radius: 16px;
  font-size: 1.05rem;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-sizing: border-box;
  background: #fafafa;
}

.form-group input:hover {
  border-color: #b0b0b0;
  background: #fff;
}

.form-group input:focus {
  outline: none;
  border-color: #0a6e89;
  box-shadow: 0 0 0 4px rgba(10, 110, 137, 0.15);
  background: #fff;
  transform: translateY(-2px);
}

.form-group input:disabled {
  background: #f5f5f5;
  cursor: not-allowed;
  opacity: 0.6;
}

.password-input {
  position: relative;
}

.password-input input {
  padding-right: 3.5rem;
}

.toggle-password {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  padding: 0.5rem;
  font-size: 1.25rem;
  transition: all 0.3s;
  border-radius: 8px;
}

.toggle-password:hover:not(:disabled) {
  color: #0a6e89;
  background: rgba(10, 110, 137, 0.1);
  transform: translateY(-50%) scale(1.1);
}

.toggle-password:disabled {
  cursor: not-allowed;
  opacity: 0.4;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.25rem;
  background: linear-gradient(135deg, #ffebee 0%, #ffcdd2 100%);
  border: 2px solid #ef5350;
  border-radius: 12px;
  color: #c62828;
  font-size: 0.9rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
  animation: shake 0.5s ease-in-out;
}

@keyframes shake {
  0%,
  100% {
    transform: translateX(0);
  }
  10%,
  30%,
  50%,
  70%,
  90% {
    transform: translateX(-5px);
  }
  20%,
  40%,
  60%,
  80% {
    transform: translateX(5px);
  }
}

.error-message .pi {
  font-size: 1.25rem;
}

.login-button {
  width: 100%;
  padding: 1.125rem;
  background: #fbc02d;
  color: #0c3849;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 2px 4px rgba(251, 192, 45, 0.2);
  position: relative;
  overflow: hidden;
}

.login-button:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(251, 192, 45, 0.3);
  background: #f9a825;
}

.login-button:active:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.5);
}

.login-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.login-button span {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
}

.login-button .loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
}

.login-footer {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  padding: 1.25rem;
  text-align: center;
  color: #6c757d;
  font-size: 0.9rem;
  font-weight: 500;
  flex-shrink: 0;
}

.login-footer p {
  margin: 0;
}
</style>
