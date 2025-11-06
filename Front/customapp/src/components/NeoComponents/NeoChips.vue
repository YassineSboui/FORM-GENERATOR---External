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
import { computed, reactive, watch } from "vue";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  prefix: string;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  rules: { expression: string }[];
  events: any[];
}
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

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    // Function to update field
    const setValue = (value: Array<any>) => {
      // test if the value is an array before setting
      if (Array.isArray(value)) {
        internalValue.value = value;
        emit("update:modelValue", value);
      }
      // else try to convert to array
      else {
        internalValue.value = [value];
        emit("update:modelValue", [value]);
      }
    };
    const updateField = (value: Array<any>) => {
      internalValue.value = value;
      emit("update:modelValue", value);
    };

    // Function to get current value
    const getValue = () => {
      return props.modelValue;
      //return internalValue.value;
    };

    // Function to update options
    const updateOptions = (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
    };

    // Function to disable field
    const disableField = () => updateOptions({ disabled: true });

    // Function to enable field
    const enableField = () => updateOptions({ disabled: false });

    // Function to hide field
    const hideField = () => updateOptions({ hidden: true });

    // Function to show field
    const showField = () => updateOptions({ hidden: false });

    // Function to set error
    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };

    // Function to remove error
    const clearFieldError = () => {
      errorState.errorMessage = "";
    };
    // Watch for field validity and clear error if valid
    watch(
      () => internalValue.value,
      (newValue) => {
        // If there is an error and the value is now valid, clear the error
        if (errorState.errorMessage) {
          // If required, not empty
          if (newValue && newValue.length > 0) {
            clearFieldError();
          }
        }
      }
    );
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );
    // Error state
    const errorState = reactive({
      errorMessage: "",
    });
    return {
      isDisabled,
      isHidden,
      internalValue,
      hideField,
      showField,
      updateField,
      enableField,
      disableField,
      setValue,
      getValue,
      setFieldError,
      clearFieldError,
      errorState,
    };
  },
};
</script>

<style lang="scss">
@import "@/scss/variables";
.p-inputchips {
  width: 100%;
}
</style>
