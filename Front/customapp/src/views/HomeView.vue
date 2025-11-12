<template>
  <div class="min-h-screen bg-gray-50 p-4">
    <div class="max-w-7xl mx-auto home-container">
      <!-- Unified Header with Admin Info and Page Title -->
      <div v-if="authStore.isAuthenticated" class="unified-header mb-6">
        <!-- Top Section: Admin Info -->
        <div class="admin-section">
          <div class="user-info">
            <div class="icon-wrapper">
              <i class="pi pi-user-circle"></i>
            </div>
            <div class="user-details">
              <span class="user-name">{{
                authStore.user?.fullName || authStore.user?.username
              }}</span>
              <span
                class="user-role"
                :class="{ 'super-admin': authStore.isSuperAdmin }"
              >
                {{ authStore.isSuperAdmin ? "SuperAdmin" : "Administrateur" }}
              </span>
            </div>
          </div>
          <div class="admin-actions">
            <Button
              v-if="authStore.isSuperAdmin"
              label="Tableau de bord"
              icon="pi pi-th-large"
              class="p-button-text action-btn"
              @click="goToDashboard"
            />
            <Button
              label="Changer mot de passe"
              icon="pi pi-key"
              class="p-button-text action-btn"
              @click="goToChangePassword"
            />
            <Button
              label="Déconnexion"
              icon="pi pi-sign-out"
              class="p-button-text p-button-danger action-btn"
              @click="confirmLogout"
            />
          </div>
        </div>

        <!-- Bottom Section: Page Title and Add Button -->
        <div class="page-section">
          <div class="page-info">
            <h1 class="page-title">Gestion des Clients</h1>
            <p class="page-subtitle">Gérez vos clients et leurs clés API</p>
          </div>
          <Button
            label="Ajouter un client"
            icon="pi pi-plus"
            class="add-client-btn"
            @click="showDialog = true"
          />
        </div>
      </div>

      <!-- Tableau des Clients -->
      <div class="bg-white rounded-lg shadow-sm">
        <DataTable
          :value="clientsArray"
          dataKey="clientId"
          paginator
          rows="10"
          stripedRows
          responsiveLayout="scroll"
          class="p-datatable-lg"
          :loading="loading"
        >
          <template #empty>
            <div class="text-center p-4">
              <i class="pi pi-inbox text-4xl text-gray-400 mb-4"></i>
              <p class="text-gray-500 text-lg">Aucun client trouvé</p>
              <p class="text-gray-400">
                Commencez par ajouter votre premier client
              </p>
            </div>
          </template>

          <Column
            field="clientId"
            header="ID Client"
            sortable
            class="font-medium"
          >
            <template #body="slotProps">
              <div class="flex items-center">
                <i class="pi pi-user text-blue-500 mr-2"></i>
                <span class="font-semibold text-gray-900">{{
                  slotProps.data.clientId
                }}</span>
              </div>
            </template>
          </Column>

          <Column field="url" header="URL du Client" sortable>
            <template #body="slotProps">
              <div class="flex items-center">
                <i class="pi pi-globe text-green-500 mr-2"></i>
                <span
                  class="text-blue-600 hover:text-blue-800 cursor-pointer"
                  @click="openUrl(slotProps.data.url)"
                >
                  {{ slotProps.data.url }}
                </span>
              </div>
            </template>
          </Column>

          <Column field="apiKey" header="Clé API" class="min-w-64">
            <template #body="slotProps">
              <div class="flex items-center gap-2">
                <div class="flex-1 bg-gray-50 rounded-lg p-3 border">
                  <div class="flex items-center justify-between">
                    <span
                      v-if="!slotProps.data.showApiKey"
                      class="text-gray-500 font-mono text-sm"
                    >
                      ••••••••••••••••••••••••••••••••
                    </span>
                    <span
                      v-else
                      class="text-gray-900 font-mono text-sm break-all"
                    >
                      {{ slotProps.data.apiKey || "Non définie" }}
                    </span>
                    <div class="flex gap-1 ml-2">
                      <Button
                        :icon="
                          slotProps.data.showApiKey
                            ? 'pi pi-eye-slash'
                            : 'pi pi-eye'
                        "
                        class="p-button-text p-button-sm"
                        @click="toggleApiKeyVisibility(slotProps.data.clientId)"
                        :disabled="!slotProps.data.apiKey"
                        v-tooltip="
                          slotProps.data.showApiKey ? 'Masquer' : 'Afficher'
                        "
                      />
                      <Button
                        icon="pi pi-copy"
                        class="p-button-text p-button-sm"
                        @click="copyApiKey(slotProps.data.apiKey)"
                        :disabled="!slotProps.data.apiKey"
                        v-tooltip="'Copier la clé API'"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </template>
          </Column>

          <Column
            header="Actions"
            class="text-center"
            :style="{ width: '280px' }"
          >
            <template #body="slotProps">
              <div class="flex gap-2 justify-center">
                <Button
                  label="Modifier"
                  icon="pi pi-pencil"
                  class="p-button-info p-button-sm"
                  @click="() => confirmEditClient(slotProps.data.clientId)"
                />
                <Button
                  label="Régénérer"
                  icon="pi pi-refresh"
                  class="p-button-warning p-button-sm"
                  @click="() => regenerateApiKey(slotProps.data.clientId)"
                  :disabled="!slotProps.data.apiKey"
                />
                <Button
                  label="Supprimer"
                  icon="pi pi-trash"
                  class="p-button-danger p-button-sm"
                  @click="confirmDeleteClient(slotProps.data.clientId)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </div>
    </div>

    <!-- Dialogue pour Ajouter un Client -->
    <Dialog
      v-model:visible="showDialog"
      header="Ajouter un nouveau client"
      modal
      :style="{
        width: '500px',
        '--p-primary-color': '#667eea',
        '--p-button-success-background': '#667eea',
        '--p-button-success-hover-background': '#764ba2',
      }"
      class="p-fluid"
    >
      <div class="grid gap-4">
        <div class="field">
          <label
            for="clientId"
            class="block text-sm font-medium text-gray-700 mb-2"
          >
            ID du Client *
          </label>
          <InputText
            id="clientId"
            v-model="newClientId"
            placeholder="Entrez l'ID du client"
            class="w-full"
            :class="{ 'p-invalid': !newClientId && showValidation }"
          />
          <small v-if="!newClientId && showValidation" class="p-error">
            L'ID du client est requis
          </small>
        </div>

        <div class="field">
          <label
            for="clientUrl"
            class="block text-sm font-medium text-gray-700 mb-2"
          >
            URL du Client *
          </label>
          <InputText
            id="clientUrl"
            v-model="newClientUrl"
            placeholder="https://example.com"
            class="w-full"
            :class="{
              'p-invalid':
                (!newClientUrl || !isValidUrl(newClientUrl)) && showValidation,
            }"
          />
          <small v-if="!newClientUrl && showValidation" class="p-error">
            L'URL du client est requise
          </small>
          <small
            v-else-if="
              newClientUrl && !isValidUrl(newClientUrl) && showValidation
            "
            class="p-error"
          >
            Veuillez entrer une URL valide
          </small>
        </div>

        <div class="field col-12">
          <label
            for="clientApiKey"
            class="block text-sm font-medium text-gray-700 mb-2"
          >
            Clé API (optionnel)
          </label>
          <InputText
            id="clientApiKey"
            v-model="newClientApiKey"
            placeholder="Laissez vide pour génération automatique"
            class="w-full"
          />
          <small class="text-gray-500">
            Si laissée vide, une clé API sera générée automatiquement
          </small>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2">
          <Button
            label="Annuler"
            icon="pi pi-times"
            class="p-button-text"
            @click="closeAddDialog"
            :style="{
              background: 'transparent',
              border: '2px solid rgba(102, 126, 234, 0.3)',
              color: '#667eea',
            }"
          />
          <Button
            label="Enregistrer"
            icon="pi pi-check"
            @click="confirmAddClient"
            :loading="saving"
            :style="{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none',
              color: 'white',
            }"
          />
        </div>
      </template>
    </Dialog>

    <!-- Dialogue pour Modifier un Client -->
    <Dialog
      v-model:visible="showEditDialog"
      header="Modifier le client"
      modal
      :style="{
        width: '500px',
        '--p-primary-color': '#667eea',
        '--p-button-success-background': '#667eea',
        '--p-button-success-hover-background': '#764ba2',
      }"
      class="p-fluid"
    >
      <div class="grid gap-4">
        <div class="field">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            ID du Client
          </label>
          <InputText
            v-model="editingClientId"
            disabled
            class="w-full bg-gray-100"
          />
        </div>

        <div class="field">
          <label
            for="editUrl"
            class="block text-sm font-medium text-gray-700 mb-2"
          >
            URL du Client *
          </label>
          <InputText
            id="editUrl"
            v-model="editingClientUrl"
            placeholder="https://example.com"
            class="w-full"
            :class="{
              'p-invalid':
                (!editingClientUrl || !isValidUrl(editingClientUrl)) &&
                showEditValidation,
            }"
          />
          <small v-if="!editingClientUrl && showEditValidation" class="p-error">
            L'URL du client est requise
          </small>
          <small
            v-else-if="
              editingClientUrl &&
              !isValidUrl(editingClientUrl) &&
              showEditValidation
            "
            class="p-error"
          >
            Veuillez entrer une URL valide
          </small>
        </div>

        <div class="field">
          <label
            for="editApiKey"
            class="block text-sm font-medium text-gray-700 mb-2"
          >
            Clé API
          </label>
          <div class="flex gap-2 w-full">
            <InputText
              id="editApiKey"
              v-model="editingClientApiKey"
              placeholder="Clé API du client"
              :type="showEditApiKey ? 'text' : 'password'"
              class="flex-1"
            />
            <Button
              :icon="showEditApiKey ? 'pi pi-eye-slash' : 'pi pi-eye'"
              class="p-button-outlined"
              @click="showEditApiKey = !showEditApiKey"
              type="button"
            />
          </div>
          <small class="text-gray-500">
            Modifiez la clé API du client si nécessaire
          </small>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2">
          <Button
            label="Annuler"
            icon="pi pi-times"
            class="p-button-text"
            @click="closeEditDialog"
            :style="{
              background: 'transparent',
              border: '2px solid rgba(102, 126, 234, 0.3)',
              color: '#667eea',
            }"
          />
          <Button
            label="Enregistrer"
            icon="pi pi-check"
            @click="saveEditClient"
            :loading="saving"
            :style="{
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              border: 'none',
              color: 'white',
            }"
          />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { useAuthStore } from "@/store/auth.store";
import {
  fetchClients,
  addClient,
  deleteClientById,
  updateClient,
  getClientApiKey,
} from "@/api/api"; // adjust the path if needed

const router = useRouter();
const authStore = useAuthStore();

const clientsArray = ref([]);
const newClientId = ref("");
const newClientUrl = ref("");
const newClientApiKey = ref("");
const showDialog = ref(false);
const editingClientId = ref("");
const editingClientUrl = ref("");
const editingClientApiKey = ref("");
const showEditDialog = ref(false);
const showEditApiKey = ref(false);
const loading = ref(false);
const saving = ref(false);
const showValidation = ref(false);
const showEditValidation = ref(false);

const toast = useToast();
const confirm = useConfirm();

const loadClients = async () => {
  loading.value = true;
  try {
    const data = await fetchClients();
    clientsArray.value = await Promise.all(
      Object.entries(data).map(async ([clientId, url]) => {
        try {
          const apiKey = await getClientApiKey(clientId);
          return {
            clientId,
            url,
            apiKey,
            showApiKey: false,
          };
        } catch (error) {
          console.warn(`Failed to load API key for client ${clientId}:`, error);
          return {
            clientId,
            url,
            apiKey: null,
            showApiKey: false,
          };
        }
      })
    );
  } catch (error) {
    console.error("[HomeView] loadClients failed:", error);
    toast.add({
      severity: "error",
      summary: "Erreur",
      detail: "Impossible de charger les clients",
      life: 3000,
    });
  } finally {
    loading.value = false;
  }
};

const confirmAddClient = async () => {
  showValidation.value = true;

  if (
    !newClientId.value ||
    !newClientUrl.value ||
    !isValidUrl(newClientUrl.value)
  ) {
    toast.add({
      severity: "warn",
      summary: "Validation",
      detail: "Veuillez remplir tous les champs requis correctement",
      life: 3000,
    });
    return;
  }

  saving.value = true;
  try {
    const clientData = {
      clientId: newClientId.value,
      url: newClientUrl.value,
    };

    // Add API key if provided
    if (newClientApiKey.value.trim()) {
      clientData.apiKey = newClientApiKey.value.trim();
    }

    await addClient(clientData);
    toast.add({
      severity: "success",
      summary: "Succès",
      detail: "Client ajouté avec succès",
      life: 3000,
    });
    closeAddDialog();
    await loadClients();
  } catch (error) {
    console.error(
      "[HomeView] confirmAddClient failed for clientId:",
      newClientId.value,
      error
    );
    toast.add({
      severity: "error",
      summary: "Erreur",
      detail: "Impossible d'ajouter le client",
      life: 3000,
    });
  } finally {
    saving.value = false;
  }
};

const confirmEditClient = async (clientId) => {
  const client = clientsArray.value.find((c) => c.clientId === clientId);
  if (client) {
    editingClientId.value = client.clientId;
    editingClientUrl.value = client.url;
    editingClientApiKey.value = client.apiKey || "";
    showEditApiKey.value = false;
    showEditValidation.value = false;
    showEditDialog.value = true;
  }
};

const saveEditClient = async () => {
  showEditValidation.value = true;

  if (
    !editingClientId.value ||
    !editingClientUrl.value ||
    !isValidUrl(editingClientUrl.value)
  ) {
    toast.add({
      severity: "warn",
      summary: "Validation",
      detail: "Veuillez remplir tous les champs requis correctement",
      life: 3000,
    });
    return;
  }

  saving.value = true;
  try {
    await updateClient(editingClientId.value, {
      url: editingClientUrl.value,
      apiKey: editingClientApiKey.value || null,
    });
    toast.add({
      severity: "success",
      summary: "Succès",
      detail: "Client mis à jour avec succès",
      life: 3000,
    });
    closeEditDialog();
    await loadClients();
  } catch (error) {
    console.error(
      "[HomeView] confirmEditClient failed for clientId:",
      editingClientId.value,
      error
    );
    toast.add({
      severity: "error",
      summary: "Erreur",
      detail: "Impossible de mettre à jour le client",
      life: 3000,
    });
  } finally {
    saving.value = false;
  }
};

const confirmDeleteClient = (clientId) => {
  confirm.require({
    message: "Are you sure you want to delete this client?",
    header: "Confirm Delete",
    icon: "pi pi-exclamation-triangle",
    acceptClass: "p-button-danger",
    accept: async () => {
      try {
        await deleteClientById(clientId);
        toast.add({
          severity: "success",
          summary: "Deleted",
          detail: "Client deleted successfully",
          life: 3000,
        });
        await loadClients();
      } catch (error) {
        console.error(
          "[HomeView] confirmDeleteClient failed for clientId:",
          clientId,
          error
        );
        toast.add({
          severity: "error",
          summary: "Error",
          detail: "Failed to delete client",
          life: 3000,
        });
      }
    },
  });
};

// API Key management methods
const toggleApiKeyVisibility = (clientId) => {
  const client = clientsArray.value.find((c) => c.clientId === clientId);
  if (client) {
    client.showApiKey = !client.showApiKey;
  }
};

const copyApiKey = async (apiKey) => {
  if (!apiKey) return;

  try {
    await navigator.clipboard.writeText(apiKey);
    toast.add({
      severity: "success",
      summary: "Copié",
      detail: "Clé API copiée dans le presse-papiers",
      life: 2000,
    });
  } catch (error) {
    console.error("[HomeView] copyApiKey failed:", error);
    toast.add({
      severity: "error",
      summary: "Erreur",
      detail: "Impossible de copier la clé API",
      life: 3000,
    });
  }
};

const regenerateApiKey = (clientId) => {
  confirm.require({
    message:
      "Êtes-vous sûr de vouloir régénérer la clé API ? L'ancienne clé ne fonctionnera plus.",
    header: "Confirmer la régénération",
    icon: "pi pi-exclamation-triangle",
    acceptClass: "p-button-warning",
    accept: async () => {
      try {
        // Generate a new API key (you could also call a backend endpoint for this)
        const newApiKey = generateApiKey();
        const client = clientsArray.value.find((c) => c.clientId === clientId);
        if (client) {
          await updateClient(client.clientId, {
            url: client.url,
            apiKey: newApiKey || null,
          });
          toast.add({
            severity: "success",
            summary: "Succès",
            detail: "Clé API régénérée avec succès",
            life: 3000,
          });
          await loadClients();
        }
      } catch (error) {
        console.error(
          "[HomeView] regenerateApiKey failed for clientId:",
          clientId,
          error
        );
        toast.add({
          severity: "error",
          summary: "Erreur",
          detail: "Échec de la régénération de la clé API",
          life: 3000,
        });
      }
    },
  });
};

// Utility functions
const isValidUrl = (url) => {
  try {
    new URL(url);
    return true;
  } catch {
    return false;
  }
};

const openUrl = (url) => {
  window.open(url, "_blank");
};

const closeAddDialog = () => {
  showDialog.value = false;
  newClientId.value = "";
  newClientUrl.value = "";
  newClientApiKey.value = "";
  showValidation.value = false;
};

const closeEditDialog = () => {
  showEditDialog.value = false;
  editingClientId.value = "";
  editingClientUrl.value = "";
  editingClientApiKey.value = "";
  showEditApiKey.value = false;
  showEditValidation.value = false;
};

// Utility function to generate API keys
const generateApiKey = () => {
  const chars =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  let result = "";
  for (let i = 0; i < 32; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
};

// Admin actions
const goToDashboard = () => {
  router.push("/admin/dashboard");
};

const goToChangePassword = () => {
  router.push("/admin/change-password");
};

const confirmLogout = () => {
  confirm.require({
    message: "Êtes-vous sûr de vouloir vous déconnecter?",
    header: "Confirmer la déconnexion",
    icon: "pi pi-exclamation-triangle",
    acceptClass: "p-button-danger",
    accept: async () => {
      authStore.logout();
      router.push("/admin/login");
    },
  });
};

onMounted(() => {
  loadClients();
});
</script>

<style scoped>
/* Scrollable Container */
.home-container {
  max-height: 95vh;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Custom Scrollbar for Home Container */
.home-container::-webkit-scrollbar {
  width: 8px;
}

.home-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

.home-container::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 10px;
}

.home-container::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
}

/* Unified Header with Purple Gradient Theme */
.unified-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 24px;
  box-shadow: 0 10px 40px rgba(102, 126, 234, 0.4);
  position: relative;
  overflow: hidden;
  animation: slideDown 0.6s ease-out;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Animated Background Effect */
.unified-header::before {
  content: "";
  position: absolute;
  top: -50%;
  right: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(
    circle,
    rgba(255, 255, 255, 0.1) 0%,
    transparent 70%
  );
  animation: rotate 20s linear infinite;
}

@keyframes rotate {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

/* Admin Section (Top Part) */
.admin-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.75rem 2rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
  position: relative;
  z-index: 1;
  animation: fadeIn 0.6s ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  color: white;
}

.icon-wrapper {
  width: 60px;
  height: 60px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(10px);
  border: 2px solid rgba(255, 255, 255, 0.3);
  transition: all 0.3s ease;
  animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
  0%,
  100% {
    box-shadow: 0 0 0 0 rgba(255, 255, 255, 0.4);
  }
  50% {
    box-shadow: 0 0 0 10px rgba(255, 255, 255, 0);
  }
}

.icon-wrapper .pi {
  font-size: 2rem;
  color: white;
}

.icon-wrapper:hover {
  transform: scale(1.1) rotate(5deg);
  background: rgba(255, 255, 255, 0.3);
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.user-name {
  font-weight: 700;
  font-size: 1.35rem;
  color: white;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
  letter-spacing: 0.5px;
}

.user-role {
  font-size: 0.875rem;
  font-weight: 600;
  padding: 0.4rem 1.125rem;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.25);
  display: inline-block;
  width: fit-content;
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  transition: all 0.3s ease;
  color: white;
}

.user-role:hover {
  background: rgba(255, 255, 255, 0.35);
  transform: translateX(5px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.user-role.super-admin {
  background: linear-gradient(
    135deg,
    rgba(255, 215, 0, 0.35) 0%,
    rgba(255, 193, 7, 0.35) 100%
  );
  border-color: rgba(255, 215, 0, 0.6);
  font-weight: 700;
  box-shadow: 0 0 20px rgba(255, 215, 0, 0.4);
  animation: glow 2s ease-in-out infinite;
}

@keyframes glow {
  0%,
  100% {
    box-shadow: 0 0 20px rgba(255, 215, 0, 0.4);
  }
  50% {
    box-shadow: 0 0 30px rgba(255, 215, 0, 0.6);
  }
}

.admin-actions {
  display: flex;
  gap: 0.75rem;
  align-items: center;
  flex-wrap: wrap;
}

.admin-actions :deep(.action-btn) {
  color: white !important;
  border: 2px solid rgba(255, 255, 255, 0.3) !important;
  border-radius: 12px !important;
  padding: 0.75rem 1.5rem !important;
  font-weight: 600 !important;
  font-size: 0.95rem !important;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
  backdrop-filter: blur(8px);
  background: rgba(255, 255, 255, 0.1) !important;
}

.admin-actions :deep(.action-btn:hover) {
  background: rgba(255, 255, 255, 0.25) !important;
  border-color: rgba(255, 255, 255, 0.5) !important;
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.25);
}

.admin-actions :deep(.p-button-danger) {
  border-color: rgba(239, 83, 80, 0.5) !important;
  background: rgba(239, 83, 80, 0.15) !important;
}

.admin-actions :deep(.p-button-danger:hover) {
  background: rgba(239, 83, 80, 0.3) !important;
  border-color: rgba(239, 83, 80, 0.8) !important;
}

/* Page Section (Bottom Part) */
.page-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 2rem 2rem 2.25rem;
  position: relative;
  z-index: 1;
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(5px);
}

.page-info {
  color: white;
}

.page-title {
  font-size: 2.25rem;
  font-weight: 800;
  margin: 0;
  color: white;
  text-shadow: 0 3px 15px rgba(0, 0, 0, 0.2);
  letter-spacing: 0.5px;
}

.page-subtitle {
  margin: 0.75rem 0 0;
  font-size: 1.05rem;
  font-weight: 500;
  color: rgba(255, 255, 255, 0.95);
  text-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.add-client-btn {
  background: white !important;
  color: #667eea !important;
  border: none !important;
  border-radius: 12px !important;
  padding: 0.875rem 2rem !important;
  font-weight: 700 !important;
  font-size: 1rem !important;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.2) !important;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
}

.add-client-btn:hover {
  transform: translateY(-3px) scale(1.05) !important;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3) !important;
  background: #f8f9fa !important;
}

/* Responsive Design */
@media (max-width: 768px) {
  .unified-header {
    border-radius: 16px;
  }

  .admin-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 1.5rem;
    padding: 1.5rem 1.25rem;
  }

  .admin-actions {
    width: 100%;
    justify-content: flex-start;
  }

  .admin-actions :deep(.action-btn) {
    flex: 1;
    min-width: 110px;
    padding: 0.625rem 1rem !important;
    font-size: 0.875rem !important;
  }

  .page-section {
    flex-direction: column;
    align-items: flex-start;
    gap: 1.5rem;
    padding: 1.75rem 1.25rem;
  }

  .page-title {
    font-size: 1.75rem;
  }

  .page-subtitle {
    font-size: 0.95rem;
  }

  .add-client-btn {
    width: 100%;
    padding: 0.875rem 1.5rem !important;
  }

  .icon-wrapper {
    width: 50px;
    height: 50px;
  }

  .icon-wrapper .pi {
    font-size: 1.75rem;
  }

  .user-name {
    font-size: 1.15rem;
  }
}

/* Table Styling */
:deep(.p-datatable) {
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(102, 126, 234, 0.15);
}

:deep(.p-datatable .p-datatable-header) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  padding: 1.5rem;
  color: white;
}

:deep(.p-datatable .p-datatable-thead > tr > th) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  font-weight: 700;
  padding: 1.25rem 1rem;
  border: none;
  font-size: 0.95rem;
  letter-spacing: 0.5px;
}

:deep(.p-datatable .p-datatable-tbody > tr) {
  transition: all 0.3s ease;
}

:deep(.p-datatable .p-datatable-tbody > tr:hover) {
  background: rgba(102, 126, 234, 0.08) !important;
  transform: translateX(5px);
}

:deep(.p-datatable .p-datatable-tbody > tr > td) {
  padding: 1.25rem 1rem;
  border-color: rgba(102, 126, 234, 0.1);
}

:deep(.p-paginator) {
  background: #f8f9fa;
  border-top: 2px solid rgba(102, 126, 234, 0.2);
  padding: 1rem;
}

:deep(.p-paginator .p-paginator-pages .p-paginator-page.p-highlight) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-color: #667eea;
  color: white;
}

/* Button Styling in Table */
:deep(.p-button.p-button-sm) {
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  border-radius: 8px;
  font-weight: 600;
  transition: all 0.3s ease;
}

:deep(.p-button.p-button-success) {
  background: #10b981;
  border-color: #10b981;
}

:deep(.p-button.p-button-success:hover) {
  background: #059669;
  border-color: #059669;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
}

:deep(.p-button.p-button-info) {
  background: #3b82f6;
  border-color: #3b82f6;
}

:deep(.p-button.p-button-info:hover) {
  background: #2563eb;
  border-color: #2563eb;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

:deep(.p-button.p-button-warning) {
  background: #f59e0b;
  border-color: #f59e0b;
}

:deep(.p-button.p-button-warning:hover) {
  background: #d97706;
  border-color: #d97706;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.4);
}

:deep(.p-button.p-button-danger) {
  background: #ef4444;
  border-color: #ef4444;
}

:deep(.p-button.p-button-danger:hover) {
  background: #dc2626;
  border-color: #dc2626;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
}

/* Modal/Dialog Styling */
:deep(.p-dialog) {
  border-radius: 24px;
  overflow: hidden;
  box-shadow: 0 25px 80px rgba(102, 126, 234, 0.45);
  border: 1px solid rgba(102, 126, 234, 0.1);
}

:deep(.p-dialog .p-dialog-header) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  color: white !important;
  padding: 2rem 2rem;
  border: none;
  position: relative;
  overflow: hidden;
}

:deep(.p-dialog .p-dialog-header::before) {
  content: "";
  position: absolute;
  top: -50%;
  right: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(
    circle,
    rgba(255, 255, 255, 0.1) 0%,
    transparent 70%
  );
  animation: rotate 15s linear infinite;
}

:deep(.p-dialog .p-dialog-title) {
  font-size: 1.5rem;
  font-weight: 800;
  letter-spacing: 0.5px;
  text-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  position: relative;
  z-index: 1;
  color: white !important;
}

:deep(.p-dialog .p-dialog-header-icons .p-dialog-header-icon) {
  color: white !important;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
  transition: all 0.3s ease;
  background: rgba(255, 255, 255, 0.1);
  position: relative;
  z-index: 1;
}

:deep(.p-dialog .p-dialog-header-icons .p-dialog-header-icon:hover) {
  background: rgba(255, 255, 255, 0.25);
  transform: rotate(90deg) scale(1.1);
}

:deep(.p-dialog .p-dialog-content) {
  padding: 2.5rem 2rem;
  background: white;
}

:deep(.p-dialog .p-dialog-footer) {
  padding: 1.75rem 2rem;
  background: linear-gradient(180deg, #fafafa 0%, #f3f4f6 100%);
  border-top: 2px solid rgba(102, 126, 234, 0.15);
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

/* Input Fields in Dialog */
:deep(.p-dialog .p-inputtext) {
  border-radius: 12px;
  border: 2px solid #e5e7eb;
  padding: 0.875rem 1.125rem;
  font-size: 1rem;
  transition: all 0.3s ease;
}

:deep(.p-dialog .p-inputtext:focus) {
  border-color: #667eea;
  box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.15);
  transform: translateY(-2px);
}

:deep(.p-dialog .p-inputtext.p-invalid) {
  border-color: #ef4444;
}

/* Dialog Buttons - Simplified styling */
:deep(.p-dialog .p-button) {
  border-radius: 12px !important;
  padding: 0.875rem 2rem !important;
  font-weight: 700 !important;
  font-size: 1rem !important;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
}

:deep(.p-dialog .p-button .p-button-icon) {
  font-size: 1.125rem;
}

:deep(.p-dialog .p-button .p-button-label) {
  font-weight: 700;
  letter-spacing: 0.3px;
}

/* Confirm Dialog */
:deep(.p-confirm-dialog) {
  border-radius: 20px;
  box-shadow: 0 20px 60px rgba(239, 68, 68, 0.35);
}

:deep(.p-confirm-dialog .p-dialog-header) {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
}

:deep(.p-confirm-dialog .p-confirm-dialog-icon) {
  color: #ef4444;
  font-size: 3rem;
}

/* Labels and Small Text in Dialog */
:deep(.p-dialog label) {
  font-weight: 700;
  color: #374151;
  margin-bottom: 0.5rem;
  font-size: 0.95rem;
}

:deep(.p-dialog small) {
  font-size: 0.875rem;
  color: #6b7280;
}

:deep(.p-dialog small.p-error) {
  color: #ef4444;
  font-weight: 600;
}

/* Enhanced Field Styling */
:deep(.field) {
  margin-bottom: 1.5rem;
}

:deep(.field label) {
  display: block;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 0.75rem;
  font-size: 0.95rem;
  letter-spacing: 0.3px;
}

:deep(.field .p-inputtext) {
  width: 100%;
  border-radius: 12px;
  border: 2px solid #e5e7eb;
  padding: 1rem 1.25rem;
  font-size: 1rem;
  transition: all 0.3s ease;
  background: #ffffff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

:deep(.field .p-inputtext:hover) {
  border-color: #c7d2fe;
  background: #fafbff;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.1);
}

:deep(.field .p-inputtext:focus) {
  border-color: #667eea;
  box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.15),
    0 4px 12px rgba(102, 126, 234, 0.2);
  transform: translateY(-2px);
  background: #ffffff;
  outline: none;
}

:deep(.field .p-inputtext.p-invalid) {
  border-color: #ef4444;
  background: #fef2f2;
}

:deep(.field .p-inputtext.p-invalid:focus) {
  box-shadow: 0 0 0 4px rgba(239, 68, 68, 0.15);
}

:deep(.field small) {
  display: block;
  margin-top: 0.5rem;
  font-size: 0.875rem;
  color: #6b7280;
  line-height: 1.4;
}

:deep(.field small.p-error) {
  color: #ef4444;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 0.375rem;
}

:deep(.field small.p-error::before) {
  content: "⚠";
  font-size: 1rem;
}

/* Grid in Dialog */
:deep(.p-dialog .grid) {
  display: flex;
  flex-direction: column;
  gap: 0;
}

/* Close Button Enhancement */
:deep(.p-dialog-header-close) {
  width: 2.5rem !important;
  height: 2.5rem !important;
  border-radius: 50% !important;
  background: rgba(255, 255, 255, 0.1) !important;
  color: white !important;
  transition: all 0.3s ease !important;
}

:deep(.p-dialog-header-close:hover) {
  background: rgba(255, 255, 255, 0.25) !important;
  transform: rotate(90deg) scale(1.1) !important;
}

:deep(.p-dialog-header-close:focus) {
  box-shadow: 0 0 0 4px rgba(255, 255, 255, 0.2) !important;
}
</style>
