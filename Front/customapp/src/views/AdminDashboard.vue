<template>
  <div class="admin-dashboard">
    <div class="dashboard-container">
      <div class="dashboard-header">
        <div class="header-layout">
          <button class="back-button" @click="goBack" title="Back to Home">
            <i class="pi pi-arrow-left"></i>
          </button>
          <div class="header-content">
            <h1>
              <i class="pi pi-users"></i>
              Admin Dashboard
            </h1>
            <p>Manage administrators and system users</p>
          </div>
        </div>
      </div>

      <div class="dashboard-content">
        <!-- Create Admin Section -->
        <div class="card create-admin-card">
          <h2>
            <i class="pi pi-user-plus"></i>
            Create New Admin
          </h2>

          <form @submit.prevent="handleCreateAdmin" class="create-form">
            <div class="form-row">
              <div class="form-group">
                <label for="userName">Username</label>
                <input
                  id="userName"
                  v-model="newAdmin.userName"
                  type="text"
                  placeholder="Enter username"
                  :disabled="isCreating"
                  required
                />
              </div>

              <div class="form-group">
                <label for="email">Email</label>
                <input
                  id="email"
                  v-model="newAdmin.email"
                  type="email"
                  placeholder="Enter email address"
                  :disabled="isCreating"
                  required
                />
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label for="fullName">Full Name</label>
                <input
                  id="fullName"
                  v-model="newAdmin.fullName"
                  type="text"
                  placeholder="Enter full name"
                  :disabled="isCreating"
                  required
                />
              </div>

              <div class="form-group">
                <label for="password">Password</label>
                <input
                  id="password"
                  v-model="newAdmin.password"
                  type="password"
                  placeholder="Enter password"
                  :disabled="isCreating"
                  required
                />
                <small class="password-hint">
                  Must contain: uppercase, lowercase, non-alphanumeric character
                </small>
              </div>
            </div>

            <div v-if="createError" class="error-message">
              <i class="pi pi-exclamation-circle"></i>
              {{ createError }}
            </div>

            <button type="submit" class="create-button" :disabled="isCreating">
              <span v-if="!isCreating">
                <i class="pi pi-plus"></i>
                Create Admin
              </span>
              <span v-else class="loading">
                <i class="pi pi-spinner pi-spin"></i>
                Creating...
              </span>
            </button>
          </form>
        </div>

        <!-- Users List Section -->
        <div class="card users-card">
          <h2>
            <i class="pi pi-list"></i>
            System Users
          </h2>

          <div v-if="isLoadingUsers" class="loading-state">
            <i class="pi pi-spinner pi-spin"></i>
            Loading users...
          </div>

          <div v-else-if="loadError" class="error-message">
            <i class="pi pi-exclamation-circle"></i>
            {{ loadError }}
          </div>

          <div v-else-if="users.length === 0" class="empty-state">
            <i class="pi pi-inbox"></i>
            <p>No users found</p>
          </div>

          <div v-else class="users-table-container">
            <table class="users-table">
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Full Name</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Created</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in users" :key="user.id">
                  <td>
                    <strong>{{ user.userName }}</strong>
                  </td>
                  <td>{{ user.fullName || "-" }}</td>
                  <td>{{ user.email || "-" }}</td>
                  <td>
                    <span
                      class="role-badge"
                      :class="isUserSuperAdmin(user) ? 'super-admin' : 'admin'"
                    >
                      {{ isUserSuperAdmin(user) ? "SuperAdmin" : "Admin" }}
                    </span>
                  </td>
                  <td>{{ formatDate(user.createdAt) }}</td>
                  <td>
                    <button
                      v-if="!isUserSuperAdmin(user)"
                      class="delete-button"
                      @click="confirmDelete(user)"
                      :disabled="isDeleting"
                      :title="`Delete ${user.userName}`"
                    >
                      <i class="pi pi-trash"></i>
                    </button>
                    <span
                      v-else
                      class="protected-label"
                      title="SuperAdmin accounts cannot be deleted"
                    >
                      <i class="pi pi-lock"></i>
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Dialog -->
    <div v-if="showDeleteDialog" class="modal-overlay" @click="cancelDelete">
      <div class="modal-dialog" @click.stop>
        <div class="modal-header">
          <h3>
            <i class="pi pi-exclamation-triangle"></i>
            Confirm Delete
          </h3>
        </div>
        <div class="modal-body">
          <p>Are you sure you want to delete the user:</p>
          <p class="user-info">
            <strong>{{ userToDelete?.userName }}</strong>
            <span v-if="userToDelete?.fullName"
              >({{ userToDelete.fullName }})</span
            >
          </p>
          <p class="warning">This action cannot be undone.</p>
        </div>
        <div class="modal-footer">
          <button
            class="modal-button cancel"
            @click="cancelDelete"
            :disabled="isDeleting"
          >
            Cancel
          </button>
          <button
            class="modal-button delete"
            @click="handleDelete"
            :disabled="isDeleting"
          >
            <span v-if="!isDeleting">Delete</span>
            <span v-else>
              <i class="pi pi-spinner pi-spin"></i>
              Deleting...
            </span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { authService } from "@/api/authService";
import type { UserDto } from "@/api/authService";

const router = useRouter();

// State
const users = ref<UserDto[]>([]);
const isLoadingUsers = ref(false);
const isCreating = ref(false);
const isDeleting = ref(false);
const loadError = ref<string | null>(null);
const createError = ref<string | null>(null);
const showDeleteDialog = ref(false);
const userToDelete = ref<UserDto | null>(null);

// Form state
const newAdmin = ref({
  userName: "",
  email: "",
  fullName: "",
  password: "",
});

// Check authorization on mount
onMounted(async () => {
  if (!authService.isSuperAdmin()) {
    router.push("/");
    return;
  }

  await loadUsers();
});

// Helper function to check if user is SuperAdmin
function isUserSuperAdmin(user: UserDto): boolean {
  // Check if email is from neoform domain
  return (
    user.email?.toLowerCase().includes("@neoform.com") ||
    user.userName?.toLowerCase() === "superadmin"
  );
}

async function loadUsers() {
  isLoadingUsers.value = true;
  loadError.value = null;

  try {
    users.value = await authService.getUsers();
  } catch (error: any) {
    loadError.value = error.response?.data?.message || "Failed to load users";
  } finally {
    isLoadingUsers.value = false;
  }
}

async function handleCreateAdmin() {
  isCreating.value = true;
  createError.value = null;

  try {
    await authService.createAdmin({
      userName: newAdmin.value.userName,
      email: newAdmin.value.email,
      fullName: newAdmin.value.fullName,
      password: newAdmin.value.password,
    });

    // Clear form
    newAdmin.value = {
      userName: "",
      email: "",
      fullName: "",
      password: "",
    };

    // Reload users
    await loadUsers();
  } catch (error: any) {
    createError.value =
      error.response?.data?.message || "Failed to create admin";
  } finally {
    isCreating.value = false;
  }
}

function confirmDelete(user: UserDto) {
  userToDelete.value = user;
  showDeleteDialog.value = true;
}

function cancelDelete() {
  if (!isDeleting.value) {
    showDeleteDialog.value = false;
    userToDelete.value = null;
  }
}

async function handleDelete() {
  if (!userToDelete.value) return;

  isDeleting.value = true;

  try {
    await authService.deleteUser(userToDelete.value.id);

    // Remove from local list
    users.value = users.value.filter((u) => u.id !== userToDelete.value!.id);

    // Close dialog
    showDeleteDialog.value = false;
    userToDelete.value = null;
  } catch (error: any) {
    alert(error.response?.data?.message || "Failed to delete user");
  } finally {
    isDeleting.value = false;
  }
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

function goBack() {
  router.push("/");
}
</script>

<style scoped>
.admin-dashboard {
  min-height: 100vh;
  background: #f5f5f5;
}

.dashboard-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem 1rem;
  max-height: 95vh;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Custom Scrollbar for Dashboard Container */
.dashboard-container::-webkit-scrollbar {
  width: 8px;
}

.dashboard-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

.dashboard-container::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 10px;
}

.dashboard-container::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
}

.dashboard-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 2rem;
  border-radius: 16px;
  margin-bottom: 2rem;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
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
}

.back-button:hover {
  background: rgba(255, 255, 255, 0.3);
  border-color: rgba(255, 255, 255, 0.5);
  transform: translateX(-4px) scale(1.05);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.back-button:active {
  transform: translateX(-2px) scale(1);
}

.header-content {
  flex: 1;
}

.header-content h1 {
  margin: 0;
  font-size: 2rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.header-content p {
  margin: 0.5rem 0 0;
  opacity: 0.9;
}

.dashboard-content {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.card {
  background: white;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.card h2 {
  margin: 0 0 1.5rem;
  font-size: 1.5rem;
  color: #333;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.create-form {
  max-width: 800px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: #333;
  font-size: 0.9rem;
}

.form-group input,
.form-group select {
  padding: 0.75rem;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #667eea;
}

.form-group input:disabled,
.form-group select:disabled {
  background: #f5f5f5;
  cursor: not-allowed;
}

.password-hint {
  margin-top: 0.25rem;
  font-size: 0.8rem;
  color: #666;
  font-style: italic;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  background: #ffebee;
  border: 1px solid #ef5350;
  border-radius: 8px;
  color: #c62828;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}

.create-button {
  padding: 0.875rem 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.create-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(102, 126, 234, 0.5);
}

.create-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.loading-state,
.empty-state {
  text-align: center;
  padding: 3rem;
  color: #666;
}

.loading-state i,
.empty-state i {
  font-size: 3rem;
  margin-bottom: 1rem;
  display: block;
}

.users-table-container {
  overflow-x: auto;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
}

.users-table th {
  background: #f5f5f5;
  padding: 1rem;
  text-align: left;
  font-weight: 600;
  color: #333;
  border-bottom: 2px solid #e0e0e0;
}

.users-table td {
  padding: 1rem;
  border-bottom: 1px solid #e0e0e0;
}

.users-table tbody tr:hover {
  background: #f9f9f9;
}

.role-badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
}

.role-badge.super-admin {
  background: #e3f2fd;
  color: #1976d2;
}

.role-badge.admin {
  background: #f3e5f5;
  color: #7b1fa2;
}

.delete-button {
  padding: 0.5rem 0.75rem;
  background: #ffebee;
  color: #c62828;
  border: 1px solid #ef5350;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.delete-button:hover:not(:disabled) {
  background: #ef5350;
  color: white;
}

.delete-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.protected-label {
  color: #999;
  font-size: 1.25rem;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.modal-dialog {
  background: white;
  border-radius: 12px;
  max-width: 450px;
  width: 90%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.modal-header {
  padding: 1.5rem;
  border-bottom: 1px solid #e0e0e0;
}

.modal-header h3 {
  margin: 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #c62828;
}

.modal-body {
  padding: 1.5rem;
}

.modal-body p {
  margin: 0 0 0.75rem;
}

.user-info {
  font-size: 1.125rem;
  color: #333;
}

.warning {
  color: #c62828;
  font-weight: 600;
  margin-top: 1rem !important;
}

.modal-footer {
  padding: 1rem 1.5rem;
  border-top: 1px solid #e0e0e0;
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
}

.modal-button {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.modal-button.cancel {
  background: #f5f5f5;
  color: #666;
}

.modal-button.cancel:hover:not(:disabled) {
  background: #e0e0e0;
}

.modal-button.delete {
  background: #ef5350;
  color: white;
}

.modal-button.delete:hover:not(:disabled) {
  background: #c62828;
}

.modal-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.loading {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

@media (max-width: 768px) {
  .form-row {
    grid-template-columns: 1fr;
  }

  .users-table {
    font-size: 0.875rem;
  }

  .users-table th,
  .users-table td {
    padding: 0.75rem 0.5rem;
  }
}
</style>
