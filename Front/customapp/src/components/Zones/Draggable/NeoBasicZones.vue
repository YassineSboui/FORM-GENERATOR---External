<template>
  <!-- Z1000 -->
  <div v-if="element.zone === 'Z1000'" style="width: 100%">
    <D-Z1000
      style="width: 100%"
      v-model="internalValue.column1"
      :variables="variables"
      :models="models"
      :systemVariables="systemVariables"

    ></D-Z1000>
  </div>
  <!-- Z1100 -->
  <div v-else-if="element.zone === 'Z1100'" style="width: 100%">
    <D-Z1100
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
      :systemVariables="systemVariables"

    ></D-Z1100>
  </div>
  <!-- Z1111 -->
  <div v-else-if="element.zone === 'Z1111'" style="width: 100%">
    <D-Z1111
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
      :systemVariables="systemVariables"

    ></D-Z1111>
  </div>
  <!-- Z1011 -->
  <div v-else-if="element.zone === 'Z1011'" style="width: 100%">
    <D-Z1011
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
      :systemVariables="systemVariables"

    ></D-Z1011>
  </div>
  <!-- Z1110 -->
  <div v-else-if="element.zone === 'Z1110'" style="width: 100%">
    <D-Z1110
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
      :systemVariables="systemVariables"

    ></D-Z1110>
  </div>
  <div v-else-if="element.zone === 'Z0111'" style="width: 100%">
    <D-Z0111
      style="width: 100%"
      v-model="internalValue"
      :variables="variables"
      :models="models"
      :systemVariables="systemVariables"

    ></D-Z0111>
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
  systemVariables: {
    type: Object,
    default: () => ({}),
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
