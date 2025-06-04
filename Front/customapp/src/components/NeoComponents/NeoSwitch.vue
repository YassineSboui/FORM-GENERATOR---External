<template>
  <div class="neoSwitch" v-show="!isHidden" :dir="isRTL ? 'rtl' : 'ltr'">
    <div class="input-container flex pb-4">
      <div class="flex align-items-center">
        <!-- :inputId="label" -->
        <ToggleSwitch
          :class="{ 'mr-2': isRTL, 'ml-2': !isRTL }"
          v-model="internalValue"
          @change="emitValue(internalValue)"
          :disabled="isDisabled"
          :readonly="options.readonly"
          :hidden="isHidden"
        ></ToggleSwitch>
      </div>
      <label
        v-if="!isParentNeoTable"
        :for="label"
        :class="{ 'mr-2': isRTL, 'ml-2': !isRTL }"
        >{{
          language === "FR"
            ? label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : label
        }}</label
      >
    </div>
  </div>
</template>
<script lang="ts">
import { computed, watchEffect, ref, onMounted, reactive, watch } from "vue";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      default: false,
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        relatedToElise: false,
        rules: [],
        events: [],
      }),
    },
    isRTL: {
      type: Boolean,
      default: false,
    },
    language: {
      type: String,
      default: "FR",
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
  },
  setup(props, { emit }) {
    // Initialize internalValue to modelValue or false if undefined
    const internalValue = ref(
      props.modelValue !== undefined && props.modelValue !== null
        ? props.modelValue && props.modelValue !== ("" as any)
        : false
    );

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    // Function to update field
    const setValue = (value: boolean) => {
      if (typeof value === "boolean") {
        internalValue.value = value;
      } else {
        internalValue.value = value == "true";
      }
      emit("update:modelValue", internalValue.value);
    };
    const updateField = (value: boolean) => {
      if (typeof value === "boolean") {
        internalValue.value = value;
      } else {
        internalValue.value = value == "true";
      }
      emit("update:modelValue", internalValue.value);
    };

    // Function to get current value
    const getValue = () => {
      return internalValue.value;
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

    function emitValue(value: any) {
      internalValue.value = value;
      emit("update:modelValue", value);
    }

    // Watcher for modelValue changes
    watch(
      () => props.modelValue,
      (newValue) => {
        internalValue.value = newValue;
      }
    );

    // Watcher for options changes
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );

    // Watch for changes in modelValue and update internalValue accordingly
    watchEffect(() => {
      internalValue.value =
        props.modelValue !== undefined &&
        props.modelValue !== null &&
        props.modelValue !== ("" as any)
          ? props.modelValue
          : false;
    });

    // Validation rules computation
    const computedRules = computed(() => {
      if (Array.isArray(localOptions.rules)) {
        let expression = localOptions.rules
          .map((item) => item.expression)
          .join("|");

        if (localOptions.required) {
          expression += expression ? "|required" : "required";
        }

        if (localOptions.hidden || localOptions.disabled) {
          expression = "";
        }

        return expression;
      }
      return "";
    });

    // Error state
    const errorState = reactive({
      errorMessage: "",
    });

    // Function to set error
    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };

    // Function to remove error
    const clearFieldError = () => {
      errorState.errorMessage = "";
    };

    // Emit the initial value when the component is mounted
    onMounted(() => {
      emit("update:modelValue", internalValue.value);
    });

    return {
      internalValue,
      isDisabled,
      isHidden,
      computedRules,
      setValue,
      updateField,
      getValue,
      enableField,
      disableField,
      hideField,
      showField,
      emitValue,
      setFieldError,
      clearFieldError,
      updateOptions,
    };
  },
};
</script>
