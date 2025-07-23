<template>
  <div class="flex justify-content-end">
    <div class="ZD-config-actions flex align-items-center flex-wrap">
      <side-bar
        :variables="variables"
        :models="models"
        style="margin-right: -10px"
        position="right"
        v-model="internalValue"
        :header="true"
        propertyComponent="NeoSplitterComponentProperties"
        :title="`${$t('Sidebar.properties')} - ${internalValue.header}`"
        :systemVariables="systemVariables"
      ></side-bar>
      <div
        class="action col-fixed flex align-content-center"
        style="width: 50px; max-height: 40px"
      >
        <Button @click="deleteItem(index, 'all')" class="content-btn" text>
          <span class="material-icons">close</span>
        </Button>
      </div>
    </div>
  </div>
  <Splitter
    style="min-height: 200px"
    class="mb-5"
    @resizeend="handleResize($event, element)"
  >
    <SplitterPanel
      class="flex align-items-center justify-content-center"
      :size="element.sizes[0]"
    >
      <!-- group="Zoneitems" -->
      <draggable
        v-model="internalValue.rows.column1"
        class="splitter-container"
        handle=".draggable-itemElement"
        :group="{
          name: 'Zoneitems',
          put: ['Zoneitems', 'repeatableZone'],
        }"
        item-key="id"
        :clone="handleClone"
        style="width: 100%"
      >
        <template #item="{ element, index: ind }">
          <div class="flex">
            <div v-if="element.zone === 'ZR'" style="width: 100%">
              <D-ZR
                v-model="internalValue.rows.column1[ind]"
                :variables="variables"
                :models="models"
                :element="element"
                :index="ind"
                @delete="deleteItem"
                :isZS="true"
                :systemVariables="systemVariables"
              >
              </D-ZR>
            </div>
            <D-NeoBasicZones
              v-else
              v-model="internalValue.rows.column1[ind].rows"
              :variables="variables"
              :models="models"
              :element="element"
              :systemVariables="systemVariables"
            ></D-NeoBasicZones>
            <div
              class="action col-fixed flex align-content-center"
              style="width: 50px"
              @click="deleteItem(ind, 'page:1')"
            >
              <Button class="content-btn" text>
                <span class="material-icons">delete_forever</span>
              </Button>
            </div>
          </div>
        </template>
      </draggable>
    </SplitterPanel>
    <SplitterPanel
      class="flex align-items-center justify-content-center"
      :size="element.sizes[1]"
    >
      <draggable
        v-model="internalValue.rows.column2"
        class="splitter-container"
        :group="{
          name: 'Zoneitems',
          put: ['Zoneitems', 'repeatableZone'],
        }"
        handle=".draggable-itemElement"
        item-key="id"
        :clone="handleClone"
        style="width: 100%"
      >
        <template #item="{ element, index: ind }">
          <div class="flex">
            <div v-if="element.zone === 'ZR'" style="width: 100%">
              <D-ZR
                v-model="internalValue.rows.column2[ind]"
                :variables="variables"
                :models="models"
                :element="element"
                :index="ind"
                @delete="deleteItem"
                :isZS="true"
                :systemVariables="systemVariables"
              >
              </D-ZR>
            </div>
            <D-NeoBasicZones
              v-else
              v-model="internalValue.rows.column2[ind].rows"
              :variables="variables"
              :models="models"
              :element="element"
              :systemVariables="systemVariables"
            ></D-NeoBasicZones>

            <div
              class="action col-fixed flex align-content-center"
              style="width: 50px"
              @click="deleteItem(ind, 'page:2')"
            >
              <Button class="content-btn" text>
                <span class="material-icons">delete_forever</span>
              </Button>
            </div>
          </div>
        </template>
      </draggable>
    </SplitterPanel>
  </Splitter>
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
  systemVariables: {
    type: Object,
    default: () => ({}),
  },
});
const emit = defineEmits(["clone", "update:modelValue", "resize", "delete"]);
const { t } = useI18n();
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
  } else if (all == "page:1") {
    internalValue.value.rows.column1.splice(index, 1);
  } else if (all == "page:2") {
    internalValue.value.rows.column2.splice(index, 1);
  }
};
const handleResize = (e: any, zone: any) => {
  emit("resize", e, zone);
};
</script>

<style lang="scss"></style>
