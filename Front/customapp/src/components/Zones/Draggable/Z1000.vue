<template>
  <div class="table-container">
    <div class="column-1">
      <draggable
        v-model="internalValue"
        class="element-container"
        group="Elementitems"
        handle=".draggable-itemElement"
        item-key="id"
        :clone="handleClone"
      >
        <template #item="{ element, index: ind }">
          <div class="draggable-itemElement">
            <div
              class="top-right flex justify-content-end align-content-center flex-wrap"
            >
              <side-bar
                :variables="variables"
                :models="models"
                position="right"
                v-model="element.options"
                :header="true"
                :validations="element.validations"
                :propertyComponent="element.properties"
                :title="`${$t('Sidebar.properties')} - ${element.options.name}`"
                :systemVariables="systemVariables"
              ></side-bar>
              <Button
                @click="deleteChildItem(internalValue, ind + 1)"
                text
                severity="danger"
                rounded
                size="small"
                class="item-btn"
              >
                <span class="material-icons">close</span>
              </Button>
            </div>
            <div
              :style="{
                marginTop: '-15px',
                maxWidth:
                  element.component == 'NeoTableComponent'
                    ? `${win.screen.width * 0.7}px`
                    : 'none',
              }"
            >
              <component-card
                :component="element.component"
                :label="
                  ($t(element.text) !== element.text
                    ? $t(element.text)
                    : element.text) +
                  ' - ' +
                  element.options.name
                "
                :options="element.options"
              ></component-card>
            </div>
          </div>
        </template>
      </draggable>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useAppStore } from "@/store/app.store";
const props = defineProps({
  modelValue: {
    type: Array,
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
var win = window;
const handleClone = (item: any) => {
  // emit("clone",item);
  let cloneMe = JSON.parse(JSON.stringify(item));
  delete cloneMe.id;
  return cloneMe;
};
const store = useAppStore();
const deleteChildItem = (modelValue: any, index: any) => {
  store.addIdAfterDelete(modelValue[index - 1].options.name);
  modelValue.splice(index - 1, 1);
};
</script>

<style lang="scss"></style>
