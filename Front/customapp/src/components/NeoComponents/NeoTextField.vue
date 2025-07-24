<template>
  <div
    class="neotextfield"
    :class="{ 'mb-3': !isParentNeoTable, 'pt-2': isParentNeoTable }"
    v-show="!isHidden"
  >
    <!-- Label section -->
    <div class="label" v-if="!isParentNeoTable">
      <label class="label-container">
        {{
          language === "FR"
            ? label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : label
        }}

        <!-- Required indicator -->
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
          style="cursor: pointer; font-size: 12px"
        ></i>
      </label>
    </div>

    <!-- Input field container -->
    <div class="input-container relative">
      <Field
        v-model="internalValue"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
      >
        <div class="flex">
          <!-- Prefix, if provided -->
          <span
            :class="[' mr-1', isRTL ? 'inverted-prefix' : 'prefix']"
            v-if="options.prefix"
          >
            {{ options.prefix }}
          </span>
          <span
            :class="[' mr-1', isRTL ? 'inverted-prefix' : 'prefix']"
            v-else-if="prefix"
          >
            {{ prefix }}
          </span>
          <!-- Input field -->

          <InputText
            v-bind="field"
            :label="options.label"
            :disabled="isDisabled"
            :required="options.required"
            :readonly="options.readonly"
            :hidden="options.hidden"
            class="w-full"
            :class="{ 'p-invalid': errorMessage || errorState.errorMessage }"
            :type="isPasswordVisible ? 'password' : 'text'"
            @focus="$emit('focus', $event)"
            @blur="$emit('blur', $event)"
            @mouseenter="$emit('mouseenter', $event)"
            @mouseleave="$emit('mouseleave', $event)"
          />
          <!-- Eye icon -->
          <i
            v-if="options.typePassword"
            class="pi"
            :class="isPasswordVisible ? 'pi-eye-slash' : 'pi-eye'"
            @click="togglePasswordVisibility"
            :style="eyeIconStyle"
          ></i>
        </div>
        <!-- Error message display -->
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
import { computed, reactive, ref, watch } from "vue";
import type { CSSProperties } from "vue";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  prefix: string;
  typePassword: boolean | null;
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
      type: String,
      default: "",
    },
    options: {
      type: Object as () => OptionConfig,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "",
        prefix: "",
        tooltip: "",
        typePassword: false,
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        relatedToElise: false,
        rules: [],
        events: [],
      }),
    },
    prefix: {
      type: String,
      default: null,
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    isRTL: {
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
        // Remove the prefix from modelValue when displaying in the input
        if (
          props.options.prefix &&
          props.modelValue.startsWith(props.options.prefix)
        ) {
          return props.modelValue.substring(props.options.prefix.length);
        }
        return props.modelValue;
      },
      set(value) {
        // Ensure the prefix is not added multiple times
        let updatedValue = value;
        if (props.options.prefix && props.options.prefix !== "") {
          if (!value.startsWith(props.options.prefix)) {
            updatedValue = props.options.prefix + value;
          }
        }
        emit("update:modelValue", updatedValue);
      },
    });

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const isPasswordVisible = ref(localOptions.typePassword || false);

    // Toggle password visibility
    const togglePasswordVisibility = () => {
      isPasswordVisible.value = !isPasswordVisible.value;
    };
    // Function to update field
    const setValue = (value: string) => {
      internalValue.value = value;
      emit("update:modelValue", value);
    };
    const updateField = (value: string) => {
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
          if (newValue && newValue.trim() !== "") {
            clearFieldError();
          }
        }
      }
    );

    const eyeIconStyle = computed<CSSProperties>(() => ({
      cursor: "pointer",
      position: "absolute",
      top: "50%",
      transform: "translateY(-50%)",
      [props.isRTL ? "left" : "right"]: "10px", // Use computed property value
      fontSize: "16px",
      color: "#0a6e89 ",
    }));
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

    watch(
      () => props.options.typePassword,
      (newValue: any) => {
        isPasswordVisible.value = newValue;
      }
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

    return {
      internalValue,
      isDisabled,
      isHidden,
      computedRules,
      isPasswordVisible,
      eyeIconStyle,
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
      togglePasswordVisibility,
    };
  },
};
</script>
<style scoped lang="scss"></style>
