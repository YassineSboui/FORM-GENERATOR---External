<template>
  <div class="min-h-screen bg-gray-50 p-4">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="bg-white rounded-lg shadow-sm p-4 mb-6">
        <div class="flex justify-content-between items-center">
          <div>
            <h1 class="text-3xl font-bold text-gray-900">
              Gestion des Clients
            </h1>
            <p class="text-gray-600 mt-1">
              Gérez vos clients et leurs clés API
            </p>
          </div>
          <Button
            label="Ajouter un client"
            icon="pi pi-plus"
            class="p-button-success"
            @click="showDialog = true"
            variant="text"
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
      :style="{ width: '500px' }"
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

        <div class="field">
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
          />
          <Button
            label="Enregistrer"
            icon="pi pi-check"
            class="p-button-success"
            @click="confirmAddClient"
            :loading="saving"
          />
        </div>
      </template>
    </Dialog>

    <!-- Dialogue pour Modifier un Client -->
    <Dialog
      v-model:visible="showEditDialog"
      header="Modifier le client"
      modal
      :style="{ width: '500px' }"
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

        <div class="field w-full">
          <label
            for="editApiKey"
            class="block text-sm font-medium text-gray-700 mb-2"
          >
            Clé API
          </label>
          <div class="grid">
            <div class="col-10">
              <InputText
                id="editApiKey"
                v-model="editingClientApiKey"
                placeholder="Clé API du client"
                :type="showEditApiKey ? 'text' : 'password'"
                class="flex-1"
              />
            </div>
            <div class="col-2">
              <Button
                :icon="showEditApiKey ? 'pi pi-eye-slash' : 'pi pi-eye'"
                class="p-button-outline"
                @click="showEditApiKey = !showEditApiKey"
                type="button"
              />
            </div>
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
          />
          <Button
            label="Enregistrer"
            icon="pi pi-check"
            class="p-button-success"
            @click="saveEditClient"
            :loading="saving"
          />
        </div>
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import {
  fetchClients,
  addClient,
  deleteClientById,
  updateClient,
  getClientApiKey,
} from "@/api/api"; // adjust the path if needed

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
    console.error("Failed to load clients", error);
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
    console.error("Failed to add client", error);
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
    console.error("Failed to update client", error);
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
        console.error("Failed to delete client", error);
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
    console.error("Failed to copy API key", error);
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
        console.error("Failed to regenerate API key", error);
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

onMounted(() => {
  loadClients();
});
</script>

<style scoped></style>
