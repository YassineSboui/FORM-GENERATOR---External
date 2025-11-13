<template>
  <div
    class="neonumberfield"
    :class="{ 'mb-3': !isParentNeoTable, 'pt-2': isParentNeoTable }"
    v-show="!isHidden"
  >
    <div class="label" v-if="!isParentNeoTable">
      <label class="label-container">
        <span>{{
          language === "FR"
            ? label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : label
        }}</span>
        <span
          v-show="options.required"
          style="color: red; margin-left: 5px; margin-right: 5px"
        >
          *
        </span>
        <i
          v-if="options.tooltip"
          class="pi pi-info-circle"
          v-tooltip.top="options.tooltip"
          style="
            cursor: pointer;
            font-size: 12px;
            margin-left: 5px;
            margin-right: 5px;
          "
        ></i>
      </label>
    </div>
    <div class="input-container">
      <Field
        v-model="internalValue"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
      >
        <InputNumber
          v-model="internalValue"
          :label="options.label"
          :disabled="isDisabled"
          :required="options.required"
          :readonly="options.readonly"
          :hidden="isHidden"
          :prefix="options.prefix"
          :suffix="options.suffix"
          :mode="options.mode !== undefined ? options.mode : 'decimal'"
          :currency="options.currency"
          :currencyDisplay="options.currencyDisplay"
          :min="options.minFractionDigits"
          :max="options.maxFractionDigits"
          :minFractionDigits="
            options.minFractionDigits !== undefined
              ? options.minFractionDigits
              : 0
          "
          :maxFractionDigits="
            options.maxFractionDigits !== undefined
              ? options.maxFractionDigits
              : 2
          "
          :useGrouping="options.useGrouping"
          :rules="['required']"
          @input="$emit('update:modelValue', internalValue)"
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
          :invalid="errorMessage || false"
        >
        </InputNumber>
        <small
          class="p-error"
          id="text-error"
          v-if="errorMessage || errorState.errorMessage"
        >
          {{ errorMessage || errorState.errorMessage || "&nbsp;" }}
        </small>
      </Field>
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
  mode: "decimal" | "currency" | undefined;
  prefix: string;
  suffix: string;
  currency: string;
  currencyDisplay: string;
  minFractionDigits: number;
  maxFractionDigits: number;
  useGrouping: boolean;
  required: boolean;
  readonly: boolean;
  disabled: boolean;
  hidden: boolean;
  relatedToElise: boolean;
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      default: 0,
    },
    options: {
      type: Object as () => OptionConfig,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "",
        tooltip: "",
        mode: "decimal",
        prefix: null,
        suffix: null,
        currency: "EUR",
        currencyDisplay: "code",
        minFractionDigits: -999999999999999999999,
        maxFractionDigits: 999999999999999999999,
        useGrouping: false,
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        relatedToElise: false,
        rules: [],
        events: [],
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    language: {
      type: String,
      default: "FR",
    },
  },
  emits: [
    "update:options",
    "update:modelValue",
    "focus",
    "blur",
    "mouseleave",
    "mouseenter",
  ],
  setup(props, { emit }) {
    // Internal value computed property
    const internalValue = computed({
      get() {
        return props.modelValue;
      },
      set(value) {
        emit("update:modelValue", value);
      },
    });

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    // Function to update field
    const setValue = (value: number) => {
      internalValue.value = value;
      emit("update:modelValue", value);
    };
    const updateField = (value: number) => {
      internalValue.value = value;
      emit("update:modelValue", value);
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

    // Validation rules computation
    const computedRules = computed(() => {
      if (Array.isArray(localOptions.rules)) {
        let expression = localOptions.rules
          .map((item) => item.expression)
          .join("|");

        if (localOptions.required) {
          expression += expression ? "|required" : "required";
        }

        if (localOptions.hidden) {
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

    // Watch for field validity and clear error if valid
    watch(
      () => internalValue.value,
      (newValue) => {
        // If there is an error and the value is now valid, clear the error
        if (errorState.errorMessage) {
          let isNotEmpty = false;
          if (typeof newValue === "number") {
            isNotEmpty = !isNaN(newValue);
          } else if (typeof newValue === "string") {
            isNotEmpty = (newValue as string).trim() !== "";
          }
          if (isNotEmpty) {
            clearFieldError();
          }
        }
      }
    );

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
      setFieldError,
      clearFieldError,
      errorState,
      updateOptions,
    };
  },
};
</script>
<style scoped lang="scss"></style>
