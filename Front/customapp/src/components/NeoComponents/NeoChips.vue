<template>
  <div class="neoChips" v-show="!isHidden">
    <div class="label" v-if="label">
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
    <div class="input-container">
      <Chips
        v-model="internalValue"
        :label="options.label"
        :disabled="isDisabled"
        :required="options.required"
        :readonly="options.readonly"
        :hidden="options.hidden"
        :rules="['required']"
        @input="$emit('update:modelValue', internalValue)"
      >
      </Chips>
    </div>
  </div>
</template>

<script lang="ts">
import { computed } from "vue";

export default {
  props: {
    label: String,
    modelValue: {
      type: Array,
      default: [],
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
        rules: [],
        events: [],
      }),
    },
  },
  setup(props, { emit }) {
    const internalValue = computed({
      get(): Array<any> {
        return props.modelValue;
      },
      set(value: Array<any>) {
        emit("update:modelValue", value);
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
    function updateField(value: String[]) {
      internalValue.value = value;
      emit("update:modelValue", value);
    }
    return {
      isDisabled,
      isHidden,
      internalValue,
      deactivateField,
      activateField,
      hideField,
      showField,
      updateField,
    };
  },
};
</script>

<style lang="scss">
@import "@/scss/variables";
.neoChips {
  width: 100%;

  .label {
    display: flex;
    flex: 1;
    flex-direction: row;
    height: 20px;
    max-height: 20px;
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
    // max-height: 50px;
    .p-inputchips {
      width: 100% !important;
      .p-inputchips-input {
        height: 37px !important;
        padding: 4px !important;
        overflow: overlay !important;
        .p-inputchips-chip-item {
          height: 24px !important;
          font-size: 0.9rem !important;
          padding: 0.2rem 0.5rem !important;
          color: white !important;
          background-color: var(--p-primary-color) !important;
          margin: 0.1rem 0 !important;
        }
        .p-inputchips-input-token {
          padding: 0px !important;
        }
      }
    }
  }
}
</style>
