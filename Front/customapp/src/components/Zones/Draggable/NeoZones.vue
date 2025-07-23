<template>
  <div v-if="element.zone === 'ZR'">
    <D-ZR
      v-model="internalValue"
      :variables="variables"
      :models="models"
      :element="element"
      :index="index"
      @delete="deleteItem"
      :systemVariables="systemVariables"
    >
    </D-ZR>
  </div>
  <div v-else-if="element.zone === 'ZS'">
    <D-ZS
      v-model="internalValue"
      :variables="variables"
      :models="models"
      :element="element"
      :index="index"
      @delete="deleteItem"
      @resize="handleResize"
      :systemVariables="systemVariables"
    >
    </D-ZS>
  </div>
  <D-NeoBasicZones
    v-else
    v-model="internalValue.rows"
    :variables="variables"
    :models="models"
    :element="element"
    :systemVariables="systemVariables"
  ></D-NeoBasicZones>
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = defineProps({
  modelValue: {
    type: Object,
    required: true,
  },
  variables: {
    type: Array,
    required: true,
  },
  models: {
    type: Array,
    required: true,
  },
  element: {
    type: Object,
    required: true,
  },
  index: {
    type: Number,
    required: true,
  },
  systemVariables: {
    type: Object,
    default: () => ({}),
  },
});
const emit = defineEmits(["clone", "update:modelValue", "resize", "delete"]);
const internalValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});
const handleClone = (item: any) => {
  let cloneMe = JSON.parse(JSON.stringify(item));
  delete cloneMe.id;
  return cloneMe;
};
const deleteItem = (index: number, zone?: string, Z?: any, col?: number) => {
  emit("delete", index);
};
const handleResize = (e: any, zone: any) => {
  emit("resize", e, zone);
};
</script>

<style scoped></style>
