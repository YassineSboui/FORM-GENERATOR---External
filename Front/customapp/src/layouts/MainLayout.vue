<template>
  <div class="main-layout">
    <Loader v-if="app.loading" />
    <div v-show="!app.loading">
      <!-- Unified Modern Navbar -->
      <div v-if="!isFullMode && !isClient" ref="navbar" class="unified-navbar">
        <div class="navbar-container">
          <!-- Left Section: Logo and Title -->
          <div class="navbar-left">
            <div class="logo-wrapper">
              <img src="@/assets/logoelise2.png" class="navbar-logo" />
            </div>
            <div class="navbar-title">NeoForm Externe</div>
          </div>

          <!-- Right Section: Actions -->
          <!-- <div class="navbar-right">
            <Button
              class="navbar-icon-btn"
              text
              rounded
              v-tooltip.bottom="'Notifications'"
            >
              <span class="material-icons">notifications</span>
            </Button>
            <Button
              class="navbar-icon-btn"
              text
              rounded
              v-tooltip.bottom="'Paramètres'"
            >
              <span class="material-icons">settings</span>
            </Button>
            <Button
              @click="onLogout"
              label="Quitter"
              icon="pi pi-sign-out"
              class="navbar-logout-btn"
            ></Button>
          </div> -->
        </div>
      </div>

      <!-- Main Content Area -->
      <div
        :class="['main-container', { 'full-height': !isNavbarRendered }]"
        :style="isFullMode ? '' : 'padding-top:10px'"
      >
        <!-- Sidebar Menu -->
        <div
          v-if="!isFullMode && !isClient"
          class="menu-container"
          :class="isMenuOpen ? ' mr-2' : 'menu-closed'"
          style="
            overflow-x: hidden;
            overflow-y: auto;
            max-height: calc(100vh - 100px);
          "
        >
          <Menu :model="items" class="menu-vertical">
            <template #item="{ label, item, props }">
              <router-link
                v-slot="{ isActive, href, navigate }"
                :to="item.route"
                custom
              >
                <a :href="href" @click="navigate" v-bind="props.action">
                  <span class="material-icons"> {{ item.icon }}</span>
                  <span
                    v-bind="props.label"
                    style="margin-left: 7px"
                    class="flex align-items-center"
                  >
                    {{ label }}
                  </span>
                </a>
              </router-link>
            </template>
          </Menu>
        </div>

        <!-- Content Area -->
        <div class="form-container">
          <ConfirmDialog></ConfirmDialog>
          <Toast position="bottom-center" />
          <slot></slot>
        </div>
      </div>
    </div>
    <version></version>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, watch } from "vue";
import { itemsMenu } from "@/data/itemsMenu";
import { useRoute } from "vue-router";
import { useAppStore } from "@/store/app.store";
import { useHttpRequest } from "@/store/httpRequest.store";
import Loader from "@/components/Loader.vue";
import Version from "@/components/Version.vue";

const app = useHttpRequest();
const store = useAppStore();
const items = ref(itemsMenu);
const route = useRoute();

const isMenuOpen = computed(() => {
  return store.isMenuOpen;
});

const toggle = () => {
  store.toggleMenu();
};

const isFullMode = computed(() => {
  return route.meta.fullMode ?? true;
});

const containerHeight = computed(() => {
  return window.innerHeight - 100;
});

const onLogout = () => {
  app.logout();
};

const isClient = computed(() => import.meta.env.MODE === "client");

const isNavbarRendered = ref(false);
const navbar = ref(null);

onMounted(() => {
  if (navbar.value) {
    isNavbarRendered.value = true;
  }
});

watch(
  navbar,
  (newVal) => {
    if (newVal) {
      isNavbarRendered.value = true;
    }
  },
  { deep: true }
);
</script>

<style lang="scss" scoped>
.main-layout {
  width: 100%;
  height: 100%;
}

.full-height {
  padding: 5px !important;
  height: 102vh !important;
}

/* Unified Modern Navbar */
.unified-navbar {
  background: linear-gradient(135deg, #0c3849 0%, #0a6e89 100%);
  box-shadow: 0 2px 8px rgba(12, 56, 73, 0.2);
  position: sticky;
  top: 0;
  z-index: 1000;
  animation: slideDown 0.4s ease-out;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.navbar-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  position: relative;
  overflow: hidden;
}



/* Left Section */
.navbar-left {
  display: flex;
  align-items: center;
  gap: 1rem;
  position: relative;
  z-index: 1;
}

.logo-wrapper {
  width: 45px;
  height: 45px;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(10px);
  border: 2px solid rgba(255, 255, 255, 0.25);
  padding: 0.5rem;
  transition: all 0.3s ease;
}

.logo-wrapper:hover {
  background: rgba(255, 255, 255, 0.25);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.navbar-logo {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.navbar-title {
  font-size: 1.5rem;
  font-weight: 800;
  color: white;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
  letter-spacing: 0.5px;
}

/* Right Section */
.navbar-right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  position: relative;
  z-index: 1;
}

.navbar-icon-btn {
  color: white !important;
  width: 42px !important;
  height: 42px !important;
  background: rgba(255, 255, 255, 0.1) !important;
  border: 2px solid rgba(255, 255, 255, 0.2) !important;
  transition: all 0.3s ease !important;
  backdrop-filter: blur(5px);
}

.navbar-icon-btn:hover {
  background: rgba(255, 255, 255, 0.2) !important;
  border-color: rgba(255, 255, 255, 0.3) !important;
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.navbar-icon-btn .material-icons {
  font-size: 1.25rem;
}

.navbar-logout-btn {
  background: rgba(239, 83, 80, 0.2) !important;
  color: white !important;
  border: 2px solid rgba(239, 83, 80, 0.4) !important;
  border-radius: 6px !important;
  padding: 0.625rem 1.5rem !important;
  font-weight: 600 !important;
  font-size: 0.95rem !important;
  transition: all 0.2s ease !important;
  backdrop-filter: blur(5px);
  margin-left: 0.5rem;
}

.navbar-logout-btn:hover {
  background: rgba(239, 83, 80, 0.35) !important;
  border-color: rgba(239, 83, 80, 0.7) !important;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(239, 83, 80, 0.4);
}

.navbar-logout-btn .pi {
  font-size: 1rem;
}

/* Responsive Design */
@media (max-width: 768px) {
  .navbar-container {
    padding: 0.875rem 1rem;
  }

  .navbar-title {
    font-size: 1.15rem;
  }

  .logo-wrapper {
    width: 38px;
    height: 38px;
  }

  .navbar-icon-btn {
    width: 38px !important;
    height: 38px !important;
  }

  .navbar-logout-btn {
    padding: 0.5rem 1rem !important;
    font-size: 0.875rem !important;
  }

  .navbar-logout-btn .p-button-label {
    display: none;
  }

  .navbar-logout-btn .pi {
    margin-right: 0 !important;
  }
}
</style>
