<template>
  <div class="table-container">
    <div class="column-1">
      <div
        class="ZR-config flex justify-content-between align-items-center flex-wrap"
      >
        <div class="ZD-config-name" style="color: #890c0a">
          {{ element.header }}
        </div>
        <div
          class="ZD-config-actions flex align-items-center flex-wrap"
          v-if="!element.code.includes('FIXED_HEADER')"
        >
          <side-bar
            :variables="variables"
            :models="models"
            style="margin-right: -10px"
            position="right"
            v-model="internalValue"
            :header="true"
            propertyComponent="NeoSectionComponentProperties"
            :title="`${$t('Sidebar.properties')} - ${internalValue.header}`"
            :systemVariables="systemVariables"
          ></side-bar>
          <div
            class="action col-fixed flex align-content-center"
            style="width: 50px; max-height: 40px"
            v-if="!isZS"
          >
            <Button @click="deleteItem(index, 'all')" class="content-btn" text>
              <span class="material-icons">close</span>
            </Button>
          </div>
        </div>
      </div>
      <draggable
        v-model="internalValue.rows.column1"
        class="repeatable-container"
        group="Zoneitems"
        handle=".draggable-itemElement"
        item-key="id"
        :clone="handleClone"
        style="width: 100%"
      >
        <template #item="{ element, index: ind }">
          <div class="flex">
            <D-NeoBasicZones
              v-model="internalValue.rows.column1[ind].rows"
              :variables="variables"
              :models="models"
              :element="element"
              :systemVariables="systemVariables"
            ></D-NeoBasicZones>
            <div
              class="action col-fixed flex align-content-center"
              style="width: 50px"
              @click="deleteItem(ind)"
            >
              <Button class="content-btn" text>
                <span class="material-icons">delete_forever</span>
              </Button>
            </div>
          </div>
        </template>
      </draggable>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
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
  element: {
    type: Object,
    required: true,
  },
  index: {
    type: Number,
    required: true,
  },
  isZS: {
    type: Boolean,
    required: false,
    default: false,
  },
  isRTL: {
    type: Boolean,
    required: false,
    default: false,
  },
  systemVariables: {
    type: Object,
    default: () => ({}),
  },
});
const { t } = useI18n();
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
const deleteItem = (index: number, all?: string) => {
  if (all == "all") {
    emit("delete", index);
  } else {
    internalValue.value.rows.column1.splice(index, 1);
  }
};
</script>

<style lang="scss"></style>
