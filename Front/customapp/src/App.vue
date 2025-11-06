<template>
  <div class="app">
    <MainLayout>
      <router-view></router-view>
    </MainLayout>
  </div>
</template>

<script lang="ts" setup>
import { MainLayout } from "@/layouts";
</script>

<style lang="scss">
@keyframes slidedown-icon {
  0% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(10px);
  }
  100% {
    transform: translateY(0);
  }
}

.slidedown-icon {
  animation: slidedown-icon;
  animation-duration: 3s;
  animation-iteration-count: infinite;
}
.full-height {
  padding: 5px !important;
  height: 102vh !important;
}
.p-toast {
  width: 50vw !important;
}
</style>
