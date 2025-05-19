<template>
  {}
  <Button
    v-if="version"
    text
    rounded
    style="
      position: fixed;
      left: 0px;
      bottom: 13px;
      padding: 0px !important;
      height: 1.5rem;
      z-index: 99999999999;
    "
    v-tooltip.right="`${version} - ${versionDate.substring(0, 10)}`"
    icon="pi pi-info-circle"
  />
</template>

<script setup lang="ts">
import { useHttpRequest } from "@/store/httpRequest.store";
import { computed, onBeforeMount } from "vue";
const app = useHttpRequest();

const version = computed(() => {
  const vers = app.version.split(".");
  vers.pop();
  return vers.join(".");
});
const versionDate = computed(() => {
  const vers = app.version.split(".");
  if (vers.length > 0) {
    const date = new Date(2000, 0, 1);
    date.setDate(date.getDate() + Number.parseInt(vers[2]));
    date.setSeconds(date.getSeconds() + Number.parseInt(vers[3]) * 2);
    return date.toLocaleString();
  }
  return "";
});
</script>
