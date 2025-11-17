<template>
  <div class="change-password">
    <!-- Animated Background -->
    <div class="background-animation">
      <div class="circle"></div>
      <div class="circle"></div>
      <div class="circle"></div>
    </div>

    <div class="password-container">
      <div class="password-card">
        <div class="password-header">
          <div class="header-layout">
            <button class="back-button" @click="goBack" title="Retour à l'accueil">
              <i class="pi pi-arrow-left"></i>
            </button>
            <div class="header-content">
              <div class="icon-wrapper">
                <i class="pi pi-shield header-icon"></i>
              </div>
              <h1>Changer le Mot de Passe</h1>
              <p v-if="mustChange">Vous devez changer votre mot de passe avant de continuer</p>
              <p v-else>Mettez à jour votre mot de passe pour plus de sécurité</p>
            </div>
          </div>
        </div>

        <form @submit.prevent="handleChangePassword" class="password-form">
          <div class="form-group">
            <label for="currentPassword">
              <i class="pi pi-key"></i>
              Mot de passe actuel
            </label>
            <div class="password-input">
              <input
                id="currentPassword"
                v-model="currentPassword"
                :type="showCurrent ? 'text' : 'password'"
                placeholder="Entrez votre mot de passe actuel"
                :disabled="isLoading"
                required
                autocomplete="current-password"
              />
              <button
                type="button"
                class="toggle-password"
                @click="showCurrent = !showCurrent"
                :disabled="isLoading"
                tabindex="-1"
              >
                <i :class="showCurrent ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
              </button>
            </div>
          </div>

          <div class="form-group">
            <label for="newPassword">
              <i class="pi pi-lock"></i>
              Nouveau mot de passe
            </label>
            <div class="password-input">
              <input
                id="newPassword"
                v-model="newPassword"
                :type="showNew ? 'text' : 'password'"
                placeholder="Entrez un nouveau mot de passe"
                :disabled="isLoading"
                required
                autocomplete="new-password"
                @input="validatePassword"
              />
              <button
                type="button"
                class="toggle-password"
                @click="showNew = !showNew"
                :disabled="isLoading"
                tabindex="-1"
              >
                <i :class="showNew ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
              </button>
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword">
              <i class="pi pi-check-circle"></i>
              Confirmer le mot de passe
            </label>
            <div class="password-input">
              <input
                id="confirmPassword"
                v-model="confirmPassword"
                :type="showConfirm ? 'text' : 'password'"
                placeholder="Confirmez le nouveau mot de passe"
                :disabled="isLoading"
                required
                autocomplete="new-password"
                @input="validatePassword"
              />
              <button
                type="button"
                class="toggle-password"
                @click="showConfirm = !showConfirm"
                :disabled="isLoading"
                tabindex="-1"
              >
                <i :class="showConfirm ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
              </button>
            </div>
          </div>

          <!-- Password Requirements -->
          <div class="requirements">
            <p class="requirements-title">
              <i class="pi pi-info-circle"></i>
              Exigences du mot de passe:
            </p>
            <div class="requirement-item" :class="{ valid: validation.minLength }">
              <i :class="validation.minLength ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
              Au moins 8 caractères
            </div>
            <div class="requirement-item" :class="{ valid: validation.hasUpperCase }">
              <i :class="validation.hasUpperCase ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
              Une lettre majuscule
            </div>
            <div class="requirement-item" :class="{ valid: validation.hasLowerCase }">
              <i :class="validation.hasLowerCase ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
              Une lettre minuscule
            </div>
            <div class="requirement-item" :class="{ valid: validation.hasDigit }">
              <i :class="validation.hasDigit ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
              Un chiffre
            </div>
            <div class="requirement-item" :class="{ valid: validation.hasSpecialChar }">
              <i :class="validation.hasSpecialChar ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
              Un caractère spécial (@$!%*?&#)
            </div>
            <div class="requirement-item" :class="{ valid: validation.passwordsMatch }">
              <i :class="validation.passwordsMatch ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
              Les mots de passe correspondent
            </div>
          </div>

          <div v-if="error" class="error-message">
            <i class="pi pi-exclamation-circle"></i>
            {{ error }}
          </div>

          <button
            type="submit"
            class="submit-button"
            :disabled="isLoading || !isFormValid"
          >
            <span v-if="!isLoading">
              <i class="pi pi-check"></i>
              Changer le mot de passe
            </span>
            <span v-else class="loading">
              <i class="pi pi-spinner pi-spin"></i>
              Modification en cours...
            </span>
          </button>

          <button
            v-if="!mustChange"
            type="button"
            class="cancel-button"
            @click="handleCancel"
            :disabled="isLoading"
          >
            <i class="pi pi-times"></i>
            Annuler
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/store/auth.store';

const router = useRouter();
const authStore = useAuthStore();

// Form state
const currentPassword = ref('');
const newPassword = ref('');
const confirmPassword = ref('');
const showCurrent = ref(false);
const showNew = ref(false);
const showConfirm = ref(false);

// Validation state
const validation = ref({
  minLength: false,
  hasUpperCase: false,
  hasLowerCase: false,
  hasDigit: false,
  hasSpecialChar: false,
  passwordsMatch: false,
});

// Computed
const isLoading = computed(() => authStore.isLoading);
const error = computed(() => authStore.error);
const mustChange = computed(() => authStore.mustChangePassword);

const isFormValid = computed(() => {
  return (
    validation.value.minLength &&
    validation.value.hasUpperCase &&
    validation.value.hasLowerCase &&
    validation.value.hasDigit &&
    validation.value.hasSpecialChar &&
    validation.value.passwordsMatch &&
    currentPassword.value.length > 0
  );
});

// Validate password requirements in real-time
function validatePassword() {
  const password = newPassword.value;
  
  validation.value.minLength = password.length >= 8;
  validation.value.hasUpperCase = /[A-Z]/.test(password);
  validation.value.hasLowerCase = /[a-z]/.test(password);
  validation.value.hasDigit = /\d/.test(password);
  validation.value.hasSpecialChar = /[@$!%*?&#]/.test(password);
  validation.value.passwordsMatch =
    password.length > 0 && password === confirmPassword.value;
}

// Watch confirm password changes
function checkPasswordsMatch() {
  validation.value.passwordsMatch =
    newPassword.value.length > 0 &&
    newPassword.value === confirmPassword.value;
}

async function handleChangePassword() {
  authStore.clearError();

  // Double-check validation
  if (!isFormValid.value) {
    return;
  }

  const success = await authStore.changePassword(
    currentPassword.value,
    newPassword.value
  );

  if (success) {
    // Mark that default password has been changed (hide credentials info box)
    localStorage.setItem('defaultPasswordChanged', 'true');
    // Login again to refresh session
    await authStore.login(
      authStore.user?.username || '',
      newPassword.value,
    );
    // Password changed successfully
    router.push('/');
  }
}

function handleCancel() {
  router.back();
}

function goBack() {
  router.push('/');
}

// Check authentication on mount
onMounted(() => {
  if (!authStore.isAuthenticated) {
    router.push('/admin/login');
  }
});
</script>

<style scoped>
.change-password {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0a6e89 0%, #fbc02d 100%);
  padding: 2rem 1rem;
  position: relative;
  overflow-x: hidden;
  overflow-y: auto;
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

/* Floating circle animations removed for professional design */



.password-container {
  width: 100%;
  max-width: 550px;
  position: relative;
  z-index: 1;
}

.password-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(12, 56, 73, 0.15);
  overflow: hidden;
  animation: slideUp 0.4s ease-out;
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

.password-header {
  background: linear-gradient(135deg, #0c3849 0%, #0a6e89 100%);
  color: white;
  padding: 2.5rem 2rem;
  position: relative;
  overflow: hidden;
  flex-shrink: 0;
}

.header-layout {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.back-button {
  background: rgba(255, 255, 255, 0.2);
  border: 2px solid rgba(255, 255, 255, 0.3);
  color: white;
  width: 48px;
  height: 48px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  transition: all 0.3s ease;
  backdrop-filter: blur(10px);
  flex-shrink: 0;
  z-index: 2;
}

.back-button:hover {
  background: rgba(255, 255, 255, 0.3);
  border-color: rgba(255, 255, 255, 0.5);
  transform: translateX(-2px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.back-button:active {
  transform: translateX(-2px) scale(1);
}

.header-content {
  flex: 1;
  text-align: center;
}



.icon-wrapper {
  width: 70px;
  height: 70px;
  margin: 0 auto 1rem;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(5px);
  border: 3px solid rgba(255, 255, 255, 0.25);
}

.header-icon {
  font-size: 2.5rem;
  position: relative;
  z-index: 1;
}

.password-header h1 {
  margin: 0;
  font-size: 2rem;
  font-weight: 800;
  position: relative;
  z-index: 1;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.password-header p {
  margin: 0.75rem 0 0;
  opacity: 0.95;
  font-size: 1rem;
  font-weight: 500;
  position: relative;
  z-index: 1;
}

.password-form {
  padding: 2rem;
  overflow-y: auto;
  overflow-x: hidden;
  flex: 1;
}

/* Custom Scrollbar for Form */
.password-form::-webkit-scrollbar {
  width: 8px;
}

.password-form::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

.password-form::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #0a6e89 0%, #fbc02d 100%);
  border-radius: 10px;
}

.password-form::-webkit-scrollbar-thumb:hover {
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
  border-radius: 12px;
  font-size: 1rem;
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

.requirements {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border: 2px solid #dee2e6;
  border-radius: 16px;
  padding: 1.25rem;
  margin-bottom: 1.5rem;
  animation: fadeIn 0.5s ease-out 0.2s both;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.requirements-title {
  margin: 0 0 1rem;
  font-weight: 700;
  color: #333;
  font-size: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.requirements-title i {
  color: #0a6e89;
}

.requirement-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem;
  color: #666;
  font-size: 0.9rem;
  transition: all 0.3s;
  border-radius: 8px;
  margin-bottom: 0.25rem;
}

.requirement-item:hover {
  background: rgba(10, 110, 137, 0.05);
}

.requirement-item.valid {
  color: #2e7d32;
  font-weight: 600;
  transition: color 0.2s ease;
}

.requirement-item .pi {
  font-size: 1.125rem;
  transition: all 0.3s;
}

.requirement-item.valid .pi-check-circle {
  color: #2e7d32;
}

.requirement-item .pi-circle {
  color: #ccc;
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
  0%, 100% { transform: translateX(0); }
  10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
  20%, 40%, 60%, 80% { transform: translateX(5px); }
}

.error-message .pi {
  font-size: 1.25rem;
}

.submit-button {
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
  margin-bottom: 1rem;
  position: relative;
  overflow: hidden;
}

.submit-button:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(251, 192, 45, 0.3);
  background: #f9a825;
}

.submit-button:active:not(:disabled) {
  transform: translateY(-1px);
}

.submit-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.submit-button span {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
}

.cancel-button {
  width: 100%;
  padding: 1rem;
  background: white;
  color: #666;
  border: 2px solid #e0e0e0;
  border-radius: 12px;
  font-size: 1.05rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.cancel-button:hover:not(:disabled) {
  background: #f5f5f5;
  border-color: #bbb;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.cancel-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}
</style>
