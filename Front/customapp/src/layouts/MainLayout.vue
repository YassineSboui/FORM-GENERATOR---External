<template>
  <div class="main-layout">
    <Loader v-if="app.loading" />
    <div v-show="!app.loading">
      <!-- Admin Navbar -->
      <div v-if="!isFullMode && !isClient">
        <div class="grid" ref="navbar">
          <div class="col-12">
            <Toolbar class="admin-bar">
              <template #start class="pl-1">
                <img src="@/assets/logoelise2.png" class="icon" />
                <div class="title">NeoForm Externe</div>
              </template>
              <template #end>
                <Button
                  @click="onLogout"
                  label="Quitter le mode administration"
                  class="admin-btn"
                ></Button>
              </template>
            </Toolbar>
          </div>
        </div>

        <!-- Menu Bar -->
        <div class="grid p-0">
          <div class="col-12 p-0">
            <Toolbar class="menu-bar">
              <template #end>
                <Button class="menu-icon p-1" text rounded>
                  <span class="material-icons"> notifications </span>
                </Button>
                <Button class="menu-icon mr-3 p-1" text rounded>
                  <span class="material-icons"> settings </span>
                </Button>
              </template>
            </Toolbar>
          </div>
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
</style>
