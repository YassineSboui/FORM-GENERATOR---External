<template>
  <div class="table-container">
    <div class="column-1">
      <draggable
        v-model="internalValue.column1"
        class="element-container"
        group="Elementitems"
        handle=".draggable-itemElement"
        :clone="handleClone"
        item-key="id"
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
                @click="deleteChildItem(internalValue.column1, ind + 1)"
                text
                severity="danger"
                rounded
                size="small"
                class="item-btn"
              >
                <span class="material-icons">close</span>
              </Button>
            </div>
            <div style="margin-top: -15px">
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
    <div class="column-1">
      <draggable
        v-model="internalValue.column2"
        class="element-container"
        group="Elementitems"
        handle=".draggable-itemElement"
        :clone="handleClone"
        item-key="id"
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
                @click="deleteChildItem(internalValue.column2, ind + 1)"
                text
                severity="danger"
                rounded
                size="small"
                class="item-btn"
              >
                <span class="material-icons">close</span>
              </Button>
            </div>
            <div style="margin-top: -15px">
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
    <div class="column-1">
      <draggable
        v-model="internalValue.column3"
        class="element-container"
        group="Elementitems"
        handle=".draggable-itemElement"
        :clone="handleClone"
        item-key="id"
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
                @click="deleteChildItem(internalValue.column3, ind + 1)"
                text
                severity="danger"
                rounded
                size="small"
                class="item-btn"
              >
                <span class="material-icons">close</span>
              </Button>
            </div>
            <div style="margin-top: -15px">
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
    <div class="column-1">
      <draggable
        v-model="internalValue.column4"
        class="element-container"
        group="Elementitems"
        handle=".draggable-itemElement"
        :clone="handleClone"
        item-key="id"
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
                @click="deleteChildItem(internalValue.column4, ind + 1)"
                text
                severity="danger"
                rounded
                size="small"
                class="item-btn"
              >
                <span class="material-icons">close</span>
              </Button>
            </div>
            <div style="margin-top: -15px">
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
import { useI18n } from "vue-i18n";
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
  systemVariables: {
    type: Object,
    default: () => ({}),
  },
});
const { t } = useI18n();
const emit = defineEmits(["clone", "update:modelValue"]);
const internalValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});
const handleClone = (item: any) => {
  let cloneMe = JSON.parse(JSON.stringify(item));
  delete cloneMe.id;
  return cloneMe;
};
const store = useAppStore();
const deleteChildItem = (modelValue: any, index: any) => {
  modelValue.splice(index - 1, 1);
  store.addIdAfterDelete(modelValue[index - 1].options.name);
};
</script>

<style lang="scss"></style>
