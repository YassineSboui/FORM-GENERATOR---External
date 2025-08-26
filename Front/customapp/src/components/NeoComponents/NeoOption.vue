<template>
  <div class="neoRadio">
    <div class="flex align-items-center gap-2" style="height: 25px !important">
      <div>
        <RadioButton
          v-model="internalValue"
          :inputId="label + Math.random().toString(36).slice(2, 12)"
          name="label"
          :value="value"
          :disabled="isDisabled"
        />
      </div>
      <div>
        <label>{{ label }}</label>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { computed } from "vue";

export default {
  props: {
    label: String,
    value: String,
    modelValue: {
      type: String,
      default: "",
    },
    index: {
      type: Number,
      default: 0,
    },
    options: {
      type: Object,
      default: () => ({
        name: "",
        label: "",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        elements: [],
        rules: [],
        events: [],
      }),
    },
    isRTL: {
      type: Boolean,
      default: false,
    },
  },
  setup(props, { emit }) {
    const internalValue = computed({
      get(): String {
        return props.modelValue;
      },
      set(value: String) {
        emit("update:modelValue", value);
      },
    });
    // watch(() => props.modelValue, (value : string) => {
    //   internalValue.value = value;
    // });
    // watch(() => internalValue.value, (value : string) => {
    //   emit("update:modelValue", value);
    // });
    const isDisabled = computed({
      get(): boolean {
        return props.options.disabled;
      },
      set(value: boolean) {
        emit("update:options", (props.options.disabled = value));
      },
    });
    function deactivateField() {
      isDisabled.value = true;
    }
    function emitValue(value: any) {
      emit("update:modelValue", value);
    }
    return {
      isDisabled,
      internalValue,
      deactivateField,
      emitValue,
    };
  },
};
</script>
