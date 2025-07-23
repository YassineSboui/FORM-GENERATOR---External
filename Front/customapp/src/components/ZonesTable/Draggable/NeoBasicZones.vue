<template>
  <!-- Z1000T -->
  <div v-if="element.zone === 'Z1000'" style="width: 100%">
    <D-Z1000T
      style="width: 100%"
      v-model="internalValue.column1"
      :variables="variables"
      :models="models"
    ></D-Z1000T>
  </div>
  <!-- Z1100T -->
  <div v-else-if="element.zone === 'Z1100'" style="width: 100%">
    <D-Z1100T
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
    ></D-Z1100T>
  </div>
  <!-- Z1111T -->
  <div v-else-if="element.zone === 'Z1111'" style="width: 100%">
    <D-Z1111T
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
    ></D-Z1111T>
  </div>
  <!-- Z1011T -->
  <div v-else-if="element.zone === 'Z1011'" style="width: 100%">
    <D-Z1011T
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
    ></D-Z1011T>
  </div>
  <!-- Z1110T -->
  <div v-else-if="element.zone === 'Z1110'" style="width: 100%">
    <D-Z1110T
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
    ></D-Z1110T>
  </div>
  <div v-else-if="element.zone === 'Z0111'" style="width: 100%">
    <D-Z0111T
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
    ></D-Z0111T>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useAppStore } from "@/store/app.store";
const props = defineProps({
  modelValue: {
    type: Object,
    required: true,
  },
  variables: {
    type: Array,
    required: false,
  },
  models: {
    type: Array,
    required: false,
  },
  element: {
    type: Object,
    required: true,
  },
});
const emit = defineEmits(["clone", "update:modelValue"]);
const internalValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});
const store = useAppStore();
var win = window;
const handleClone = (item: any) => {
  let cloneMe = JSON.parse(JSON.stringify(item));
  delete cloneMe.id;
  return cloneMe;
};
const deleteChildItem = (modelValue: any, index: any) => {
  store.addIdAfterDelete(modelValue[index - 1].options.name);
  modelValue.splice(index - 1, 1);
};
</script>

<style lang="scss"></style>
