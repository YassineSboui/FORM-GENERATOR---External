<template>
  <div class="loader-container">
    <div class="container" style="zoom: 200%">
      <span></span>
      <span></span>
      <span></span>
      <span></span>
    </div>
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
} /* From Uiverse.io by terenceodonoghue */
.container {
  position: absolute;
  top: 50%;
  left: 50%;
  border-radius: 50%;
  height: 96px;
  width: 96px;
  animation: rotate_3922 1.2s linear infinite;
  background-color: #9b59b6;
  background-image: linear-gradient(#9b59b6, #84cdfa, #5ad1cd);
}

.container span {
  position: absolute;
  border-radius: 50%;
  height: 100%;
  width: 100%;
  background-color: #9b59b6;
  background-image: linear-gradient(#9b59b6, #84cdfa, #5ad1cd);
}

.container span:nth-of-type(1) {
  filter: blur(5px);
}

.container span:nth-of-type(2) {
  filter: blur(10px);
}

.container span:nth-of-type(3) {
  filter: blur(25px);
}

.container span:nth-of-type(4) {
  filter: blur(50px);
}

.container::after {
  content: "";
  position: absolute;
  top: 10px;
  left: 10px;
  right: 10px;
  bottom: 10px;
  background-color: #e0e0e0;
  border: solid 5px #e0e0e0;
  border-radius: 50%;
}

@keyframes rotate_3922 {
  from {
    transform: translate(-50%, -50%) rotate(0deg);
  }

  to {
    transform: translate(-50%, -50%) rotate(360deg);
  }
}
</style>
