<template>
  <div class="p-6">
    <h1 class="text-2xl font-bold mb-4">Manage Clients</h1>

    <!-- Add Client Button -->
    <Button
      label="Add Client"
      icon="pi pi-plus"
      class="p-button-success p-button-sm mb-4"
      @click="showDialog = true"
    />

    <!-- Client DataTable -->
    <DataTable
      :value="clientsArray"
      dataKey="clientId"
      paginator
      rows="5"
      stripedRows
      responsiveLayout="scroll"
    >
      <Column field="clientId" header="Client ID" sortable></Column>
      <Column field="url" header="Client URL" sortable></Column>
      <Column header="Actions">
        <template #body="slotProps">
          <Button
            label="Delete"
            icon="pi pi-trash"
            class="p-button-danger p-button-sm"
            @click="confirmDeleteClient(slotProps.data.clientId)"
          />
        </template>
      </Column>
    </DataTable>

    <!-- Dialog to Add Client -->
    <Dialog
      v-model:visible="showDialog"
      header="Add New Client"
      modal
      class="w-96"
    >
      <div class="flex flex-col gap-3">
        <InputText v-model="newClientId" placeholder="Client ID" />
        <InputText v-model="newClientUrl" placeholder="Client URL" />
      </div>

      <template #footer>
        <Button
          label="Cancel"
          icon="pi pi-times"
          class="p-button-text"
          @click="showDialog = false"
        />
        <Button
          label="Save"
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

// PrimeVue components
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import InputText from "primevue/inputtext";
import Dialog from "primevue/dialog";
import ConfirmDialog from "primevue/confirmdialog";
import Toast from "primevue/toast";

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
