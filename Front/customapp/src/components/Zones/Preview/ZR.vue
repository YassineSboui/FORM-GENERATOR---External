<template>
  <div
    class="zone-page-header"
    v-if="internalValue.code.includes('FIXED_HEADER')"
  >
    <div v-for="i in Math.max(internalValue.rows.column1.length || 0)" :key="i">
      <div class="grid" style="width: 100%">
        <div class="ml-3" style="width: 100%">
          <zone-component
            :isRTL="isRTL"
            @AppRefs="handleRefs"
            v-model="internalValue.rows.column1[i - 1]"
            v-model:fields="fields"
            @handleInputChange="handleInputChange"
            @executeFun="executeFun"
            @focus="handleFocus($event)"
            @blur="handleBlur($event)"
            @mouseenter="handleMouseenter($event)"
            @mouseleave="handleMouseleave($event)"
          ></zone-component>
        </div>
      </div>
    </div>
  </div>
  <div v-else>
    <div
      class="grid mt-3"
      style="width: 100%"
      v-if="
        internalValue.isSection == false ||
        typeof internalValue.section == undefined
      "
    >
      <div class="ml-3" style="width: 100%">
        <div class="duplicatable-zone">
          <div class="flex head mb-2">
            {{ internalValue.header }}
          </div>
          <Divider />
          <div class="duplicatable-zone-content mt-4 px-3">
            <div
              v-for="i in internalrepeatableZone[internalValue.code] || 0"
              :key="i"
            >
              <Divider v-if="i > 1" />
              <div
                v-if="i > 1"
                class="flex justify-content-end"
                style="margin-bottom: -15px"
              >
                <Button
                  text
                  severity="danger"
                  icon="pi pi-times"
                  @click="deleteDuplicated(internalValue.code, i - 1)"
                />
              </div>
              <div
                v-for="j in Math.max(internalValue.rows.column1.length || 0)"
                :key="j"
              >
                <zone-component
                  :isRTL="isRTL"
                  :index="i - 1"
                  :sectionCode="internalValue.code"
                  @AppRefs="handleRefs"
                  v-model="internalValue.rows.column1[j - 1]"
                  v-model:fields="fields"
                  @handleInputChange="handleInputChange"
                  @executeFun="executeFun"
                  @focus="handleFocus($event)"
                  @blur="handleBlur($event)"
                  @mouseenter="handleMouseenter($event)"
                  @mouseleave="handleMouseleave($event)"
                  style="min-height: 60px"
                ></zone-component>
              </div>
            </div>
          </div>
          <div class="duplicatable-zone-footer">
            <div class="flex justify-content-end mr-2">
              <Button icon="pi pi-plus" @click="duplicate(internalValue)" />
            </div>
          </div>
        </div>
      </div>
    </div>
    <Panel
      @update:collapsed="handleCollapsed"
      :collapsed="panelCollapsed"
      toggleable
      style="width: 100%"
      v-else
    >
      <template #togglericon>
        <span class="material-icons">{{
          panelCollapsed ? "unfold_more" : "unfold_less"
        }}</span>
      </template>
      <template #header>
        <div class="flex head">
          <div class="haedDivider mr-2"></div>
          {{ internalValue?.header }}
        </div>
      </template>
      <div
        v-for="i in Math.max(internalValue.rows.column1.length || 0)"
        :key="i"
      >
        <div class="grid" style="width: 100%">
          <div class="ml-3" style="width: 100%">
            <zone-component
              :isRTL="isRTL"
              @AppRefs="handleRefs"
              v-model="internalValue.rows.column1[i - 1]"
              v-model:fields="fields"
              @handleInputChange="handleInputChange"
              @executeFun="executeFun"
              @focus="handleFocus($event)"
              @blur="handleBlur($event)"
              @mouseenter="handleMouseenter($event)"
              @mouseleave="handleMouseleave($event)"
              :language="language"
            ></zone-component>
          </div>
        </div>
      </div>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from "vue";

const props = defineProps({
  modelValue: {
    type: Object,
    required: true,
  },
  repeatableZoneChildrens: {
    type: Object,
    required: false,
    default: {},
  },
  Fields: {
    type: Object,
    required: true,
  },
  isRTL: {
    type: Boolean,
    required: false,
    default: false,
  },
  language: {
    type: String,
    required: false,
    default: "FR",
  },
});
const emit = defineEmits([
  "update:modelValue",
  "handleRefs",
  "executeFun",
  "inputChange",
  "update:fields",
  "repeatableZoneChildrens",
  "duplicate",
  "deleteDuplicated",
  "focus",
  "blur",
  "mouseleave",
  "mouseenter",
]);
onMounted(() => {
  const panelElement = document.querySelector(".p-panel-header");
  if (panelElement) {
    panelElement.addEventListener("click", (event) => {
      const target = event.target as HTMLElement;
      const isActionsClick = target?.closest(".p-panel-header-actions");
      if (!isActionsClick) {
        panelCollapsed.value = !panelCollapsed.value;
      }
    });
  }
});
const internalValue = computed({
  get: () => props.modelValue,
  set: (val) => emit("update:modelValue", val),
});
const fields = computed({
  get: () => props.Fields,
  set: (val) => emit("update:fields", val),
});
const panelCollapsed = computed({
  get: () => internalValue.value.toggled,
  set: (val) => {
    internalValue.value.toggled = val;
    emit("update:modelValue", internalValue.value);
  },
});
const handleCollapsed = (event: any) => {
  panelCollapsed.value = event;
};
const handleRefs = (event: any) => {
  emit("handleRefs", event);
};
const executeFun = (event: any) => {
  emit("executeFun", event);
};

const handleInputChange = (event: any) => {
  emit("inputChange", event);
};
const handleFocus = (e: any) => {
  emit("focus", e);
};
const handleBlur = (e: any) => {
  emit("blur", e);
};
const handleMouseenter = (e: any) => {
  emit("mouseenter", e);
};
const handleMouseleave = (e: any) => {
  emit("mouseleave", e);
};
const internalrepeatableZone = computed(() => props.repeatableZoneChildrens);
const isRTL = ref(props.isRTL);

const duplicate = (elem: any) => {
  const newValue = {
    ...internalrepeatableZone.value,
    [elem.code]: (internalrepeatableZone.value[elem.code] || 0) + 1,
  };
  emit("duplicate", newValue);
};
const deleteDuplicated = (elem: any, index: any) => {
  // First remove the item from fields by emitting to parent
  emit("deleteDuplicated", { elem, index });

  // Then update the count
  const newValue = {
    ...internalrepeatableZone.value,
    [elem]: Math.max(1, (internalrepeatableZone.value[elem] || 1) - 1),
  };
  emit("duplicate", newValue);
};
</script>

<style lang="scss">
.zone-page-header {
  position: fixed;
  top: 0;
  width: calc(100% - 15px);
  z-index: 1000;
  background-color: white;
  left: 0;
  padding: 0.5rem;
  // box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
  border-bottom: 1px solid #eaecee;
}

.p-panel-header {
  cursor: pointer;
}
</style>
