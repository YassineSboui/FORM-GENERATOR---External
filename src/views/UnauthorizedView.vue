<template>
  <div
    class="flex flex-column align-content-center justify-content-center h-100"
  >
    <unauthorized class="unauthorized" />

    <div class="text-center">
      Vous n'êtes pas autorisé à accéder à cette page. Veuillez recommencer
      l'action depuis
      <span class="p-link" style="color: cornflowerblue" @click="goHome">
        Elise Dashboard.
      </span>
    </div>
  </div>
</template>
<script setup lang="ts">
import { useHttpRequest } from "@/store/httpRequest.store";
import { defineAsyncComponent } from "vue";
const app = useHttpRequest();
const goHome = () => window.location.replace(app.eliseUrl);
const Unauthorized = defineAsyncComponent(
  () => import(`@/assets/svg/unauthorized.svg`)
);
</script>
<style>
.unauthorized {
  height: 60vh;
}
svg#freepik_stories-security-on:not(.animated) .animable {
  opacity: 0;
}
svg#freepik_stories-security-on.animated #freepik--Device--inject-2 {
  animation: 1s 1 forwards cubic-bezier(0.36, -0.01, 0.5, 1.38) zoomIn;
  animation-delay: 0s;
}
svg#freepik_stories-security-on.animated #freepik--character-2--inject-2 {
  animation: 1s 1 forwards cubic-bezier(0.36, -0.01, 0.5, 1.38) lightSpeedLeft,
    1.5s Infinite linear wind;
  animation-delay: 0s, 1s;
}
svg#freepik_stories-security-on.animated #freepik--character-1--inject-2 {
  animation: 1s 1 forwards cubic-bezier(0.36, -0.01, 0.5, 1.38) lightSpeedRight,
    1.5s Infinite linear wind;
  animation-delay: 0s, 1s;
}
@keyframes zoomIn {
  0% {
    opacity: 0;
    transform: scale(0.5);
  }
  100% {
    opacity: 1;
    transform: scale(1);
  }
}
@keyframes lightSpeedLeft {
  from {
    transform: translate3d(-50%, 0, 0) skewX(20deg);
    opacity: 0;
  }
  60% {
    transform: skewX(-10deg);
    opacity: 1;
  }
  80% {
    transform: skewX(2deg);
  }
  to {
    opacity: 1;
    transform: translate3d(0, 0, 0);
  }
}
@keyframes wind {
  0% {
    transform: rotate(0deg);
  }
  25% {
    transform: rotate(1deg);
  }
  75% {
    transform: rotate(-1deg);
  }
}
@keyframes lightSpeedRight {
  from {
    transform: translate3d(50%, 0, 0) skewX(-20deg);
    opacity: 0;
  }
  60% {
    transform: skewX(10deg);
    opacity: 1;
  }
  80% {
    transform: skewX(-2deg);
  }
  to {
    opacity: 1;
    transform: translate3d(0, 0, 0);
  }
}
.animator-hidden {
  display: none;
}
.main-container {
  display: flex;
  overflow-y: auto;
  padding: 10px 5px 0px 5px;
  background-color: #efefef;
  height: 101vh !important;
}
</style>
