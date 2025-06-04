<template>
  <div class="loader-container">
    <div class="loaderApp"></div>
  </div>
</template>

<style lang="scss">
.loader-container {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100vh;
}

.loaderApp {
  border-width: 2px;
  margin: 4px;
  border: 16px solid #f3f3f3; /* Light grey */
  border-top: 16px solid #0a6e89;
  border-radius: 50%;
  width: 40vh;
  height: 40vh;
  max-width: 200px;
  max-height: 200px;
  animation: spin 2s linear infinite;
  /* Remove absolute positioning */
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
