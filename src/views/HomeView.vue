<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Gestion des Clients</h1>

    <!-- Bouton Ajouter un Client -->
    <Button
      label="Ajouter un client"
      icon="pi pi-plus"
      class="p-button-success p-button-sm mb-4"
      @click="showDialog = true"
    />

    <!-- Tableau des Clients -->
    <DataTable
      :value="clientsArray"
      dataKey="clientId"
      paginator
      rows="5"
      stripedRows
      responsiveLayout="scroll"
    >
      <Column field="clientId" header="ID Client" sortable></Column>
      <Column field="url" header="URL du Client" sortable></Column>
      <Column header="Actions">
        <template #body="slotProps">
          <Button
            label="Supprimer"
            icon="pi pi-trash"
            class="p-button-danger p-button-sm"
            @click="confirmDeleteClient(slotProps.data.clientId)"
          />
        </template>
      </Column>
    </DataTable>

    <!-- Dialogue pour Ajouter un Client -->
    <Dialog
      v-model:visible="showDialog"
      header="Ajouter un nouveau client"
      modal
      class="w-96"
    >
      <div class="flex flex-col gap-3">
        <InputText v-model="newClientId" placeholder="ID Client" />
        <InputText v-model="newClientUrl" placeholder="URL du Client" />
      </div>

      <template #footer>
        <Button
          label="Annuler"
          icon="pi pi-times"
          class="p-button-text"
          @click="showDialog = false"
        />
        <Button
          label="Enregistrer"
          icon="pi pi-check"
          class="p-button-success"
          @click="confirmAddClient"
        />
      </template>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { fetchClients, addClient, deleteClientById } from "@/api/api"; // adjust the path if needed

const clientsArray = ref([]);
const newClientId = ref("");
const newClientUrl = ref("");
const showDialog = ref(false);

const toast = useToast();
const confirm = useConfirm();

const loadClients = async () => {
  try {
    const data = await fetchClients();
    clientsArray.value = Object.entries(data).map(([clientId, url]) => ({
      clientId,
      url,
    }));
  } catch (error) {
    console.error("Failed to load clients", error);
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Failed to load clients",
      life: 3000,
    });
  }
};

const confirmAddClient = async () => {
  if (!newClientId.value || !newClientUrl.value) {
    toast.add({
      severity: "warn",
      summary: "Warning",
      detail: "Please fill in both fields",
      life: 3000,
    });
    return;
  }
  try {
    await addClient(newClientId.value, newClientUrl.value);
    toast.add({
      severity: "success",
      summary: "Success",
      detail: "Client added successfully",
      life: 3000,
    });
    newClientId.value = "";
    newClientUrl.value = "";
    showDialog.value = false;
    await loadClients();
  } catch (error) {
    console.error("Failed to add client", error);
    toast.add({
      severity: "error",
      summary: "Error",
      detail: "Failed to add client",
      life: 3000,
    });
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

onMounted(() => {
  loadClients();
});
</script>

<style scoped></style>
