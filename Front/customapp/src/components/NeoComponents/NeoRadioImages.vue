<template>
  <div class="neoradioimages">
    <div class="label">{{ options.label }}</div>
    <div class="flex input-container">
      <Carousel
        v-if="props.options.elements.length > 0"
        :value="props.options.elements"
        :numVisible="
          Math.min(props.options.elementsPerLine, props.options.elements.length)
        "
        :showNavigators="true"
        style="width: 100%"
      >
        <template #item="slotProps">
          <Card class="outlined-card" style="max-width: 10rem;">
            <template #content>
              <div>
                <input
                  type="radio"
                  :value="slotProps.data.name"
                  v-model="option"
                  @change="emitValue(slotProps.data.name)"
                />
                <span class="custom-title"> {{ slotProps.data.name }} </span>
                <div class="text-center">
                  <div>
                    <img
                      :src="slotProps.data.image"
                      :alt="slotProps.data.name"
                      class="w-12 mt-2"
                      />
                      <!-- width="280"
                      height="80" -->
                  </div>
                  <div>
                    <h6
                      class="pb-8"
                      style="font-size: 12px; font-weight: normal"
                    >
                      {{ slotProps.data.description }}
                    </h6>
                  </div>
                </div>
              </div>
            </template>
          </Card>
        </template>
      </Carousel>
      <div v-else>
        <Card
          class="outlined-card my-6"
          style="display: flex; justify-content: center"
        >
          <template #content>
            <div class="text-center">
              <h4>Aucun</h4>
            </div>
          </template>
        </Card>
      </div>
    </div>
    <div class="error-container"></div>
    <!-- Selected : {{option}} -->
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";

import Carousel from "primevue/carousel";
import Card from "primevue/card";

const props = defineProps({
  label: String,
  modelValue: {
    type: String,
    default: "",
  },
  options: {
    type: Object,
    default: () => ({
      name: "",
      label: "",
      required: true,
      readonly: false,
      disabled: false,
      hidden: false,
      editable: false,
      elementsPerLine: 4,
      elements: [
        { id: null, name: "", description: "", image: "image full path" },
      ],
      rules: [],
      events: [],
    }),
  },
});

const option = computed(() => {return props.modelValue});

const emit = defineEmits();

function emitValue(value: any) {
  emit("update:modelValue", value);
}
</script>

<style lang="scss">
@import "@/scss/variables";
.neoradioimages {
  width: 100%;
  .label {
    width: 100%;
    height: 20px;
    max-height: 20px;
  }
  .input-container {
    width: 100%;
    .input-vtextfield {
      width: 100%;
      .v-field__outline::after {
        border-style: none;
      }
      .v-field__outline::before {
        border-style: none;
      }
      input {
        border-radius: 4px;
        border: 1px solid transparent;
        border-style: none;
        background-color: $color-blue-catskill-white;
        color: $color-grey-tundora;
        height: 28px;
        min-height: 28px;
      }
      input:focus {
        border: 1px solid $color-blue-dark !important;
      }
      input:hover {
        border: 1px solid $color-blue-jungle-mist;
      }
      input:disabled {
        background-color: #fff;
        color: #565656;
        border: 1px solid #bfbcbc;
        resize: vertical;
        cursor: default;
      }
    }
  }
  .error-container {
    width: 100%;
    height: 20px;
    max-height: 20px;
  }
}
.outlined-card {
  display: flex!important;
  justify-content: center!important;
  border: 2px solid #007ba7!important;
  border-radius: 15px!important;
  margin: 8px!important;
  height: 225px!important;
}
.custom-title {
  display: flex;
  justify-content: center;
  font-weight: bold;
  font-size: 14px;
  color: #165c77;
}
.p-card .p-card-content{
  padding: 0px;
}
.p-card .p-card-body{
  height: 100%;
}
.p-carousel-item {
  display: flex;
  justify-content: center;
}
</style>
