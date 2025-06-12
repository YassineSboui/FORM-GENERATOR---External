<template>
  <div class="neoselect" v-show="!isHidden">
    <div class="label" v-if="!isParentNeoTable">
      <label class="label-container">
        {{ label }}
        <span
          v-show="options.required"
          style="color: red; margin-left: 5px; margin-right: 5px"
        >
          *
        </span></label
      >
    </div>
    <div class="description" v-if="description">
      {{ description }}
    </div>

    <div
      class="input-container"
      :style="{
        height: isParentNeoTable ? '30px' : '60px',
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <MultiSelect
        v-if="returnObject"
        v-model="itemValue"
        display="chip"
        :disabled="isDisabled"
        :options="items ?? options.elements"
        optionLabel="name"
        class="w-full neoMultiSelectDropdown"
        :pt="{
          root: { class: 'w-full ' },
          labelContainer: {
            class: 'labelcontainerMultiSelect',
          },
          token: {
            class: 'token',
          },
        }"
      />
      <MultiSelect
        v-else
        v-model="itemValue"
        display="chip"
        :disabled="isDisabled"
        :options="items ?? options.elements"
        optionLabel="name"
        optionValue="code"
        class="w-full neoMultiSelectDropdown"
        :pt="{
          root: { class: 'w-full ' },
          labelContainer: {
            class: 'labelcontainerMultiSelect',
          },
          token: {
            class: 'token',
          },
        }"
      />
    </div>
  </div>
</template>

<script lang="ts">
import { computed } from "vue";

export default {
  props: {
    items: {
      type: Array,
      required: true,
    },
    label: {
      type: String,
      required: true,
    },
    modelValue: {
      // type: Object,
      required: true,
    },
    returnObject: {
      type: Boolean,
      default: false,
    },
    options: {
      type: Object,
      default: () => ({
        name: "",
        label: "Test",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        selectedSource: "",
        elements: [{ code: "", name: "" }],
        rules: [],
        events: [],
        description: "",
        tooltipDescription: false,
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
  },
  setup(props, { emit }) {
    const itemValue = computed({
      get() {
        return props.modelValue;
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });
    const isDisabled = computed({
      get(): boolean {
        return props.options.disabled;
      },
      set(value: boolean) {
        props.options.disabled = value;
        emit("update:options", props.options);
      },
    });
    const isHidden = computed({
      get(): boolean {
        return props.options.hidden;
      },
      set(value: boolean) {
        props.options.hidden = value;
        emit("update:options", props.options);
      },
    });
    const selectionItems = computed(() => {
      if (props.options.selectedSource == "manual") {
        return props.options.elements;
      } else {
        return props.items;
      }
    });
    const description = computed(() => {
      return props.options.description;
    });
    function deactivateField() {
      isDisabled.value = true;
    }
    function activateField() {
      isDisabled.value = false;
    }
    function hideField() {
      isHidden.value = true;
    }
    function showField() {
      isHidden.value = false;
    }
    return {
      isDisabled,
      isHidden,
      itemValue,
      selectionItems,
      deactivateField,
      activateField,
      hideField,
      showField,
      description,
      // updateField,
    };
  },
};
</script>

<style lang="scss">
@import "@/scss/variables";
.neoselect {
  width: 100%;
  .label {
    height: 20px;
    max-height: 20px;
    display: flex;
    flex: 1;
    flex-direction: row;
    .label-container {
      color: #165c77;
      min-width: 150px;
      align-items: center;
      display: flex;
      padding-bottom: 5px;
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
      .label .label-container-modified {
        text-align: right;
      }
    }
  }
  .input-container {
    width: 100%;
    height: 60px;
    max-height: 60px;

    .neoMultiSelectDropdown {
      background-color: #f3f8f9;
      padding-right: 0.2rem;
      span {
        font-family: Trebuchet MS, sans-serif;
        font-size: 12px;
      }
    }
    .labelcontainerMultiSelect {
      height: 38.5px;
      margin-top: -4px;
      margin-bottom: 2px;
      border: var(--p-primary-color);
    }

    .token {
      height: 28px !important;
      color: white !important;
      background-color: var(--p-primary-color) !important;
    }
  }
}
</style>
