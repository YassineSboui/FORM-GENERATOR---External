<template>
  <div
    class="flex flex-column align-content-center justify-content-center h-100 unauthorized-container"
  >
    <unauthorized class="unauthorized" />

    <div class="text-center">
      Vous n'êtes pas autorisé à accéder à cette page. Veuillez recommencer
      l'opération ou contacter votre administrateur.
    </div>
  </div>
</template>
<script setup lang="ts">
import { useHttpRequest } from "@/store/httpRequest.store";
import { defineAsyncComponent } from "vue";

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
.unauthorized-container {
  max-height: 95vh;
  overflow-y: auto;
  overflow-x: hidden;
}

/* Custom Scrollbar for Unauthorized Container */
.unauthorized-container::-webkit-scrollbar {
  width: 8px;
}

.unauthorized-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 10px;
}

.unauthorized-container::-webkit-scrollbar-thumb {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 10px;
}

.unauthorized-container::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
}

.main-container {
  display: flex;
  overflow-y: auto;
  padding: 10px 5px 0px 5px;
  background-color: #efefef;
  height: 101vh !important;
}
</style>
