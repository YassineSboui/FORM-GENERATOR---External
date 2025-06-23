<template>
  <div class="neoOptionsGroup mb-3" v-show="!isHidden">
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
    <div class="input-container ml-3">
      <div class="vertical" v-if="!options.horizontal">
        <div
          class="flex align-items-center mb-2"
          v-for="element in options.elements"
          :key="element"
        >
          <neo-option
            v-model="internalValue"
            :label="element.label"
            :options="{ disabled: isDisabled }"
          ></neo-option>
        </div>
      </div>
      <div class="horizontal grid mt-1" v-else>
        <div class="col" v-for="element in options.elements" :key="element">
          <neo-option
            v-model="internalValue"
            :label="element.label"
            :options="{ disabled: isDisabled }"
            :isRTL="isRTL"
          ></neo-option>
          <!-- @input="$emit('update:modelValue', internalValue)" -->
        </div>
      </div>
      <small class="p-error" id="text-error" v-if="errorState.errorMessage">
        {{ errorState.errorMessage || "&nbsp;" }}
      </small>
      <!-- <div class="error-container"></div> -->
    </div>
  </div>
</template>

<script lang="ts">
import { computed, reactive, watch } from "vue";
import { logger } from "@/api/api";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  required: boolean | null;
  horizontal: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
  elements: any[];
}

export default {
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      // type: String,
      default: "",
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
        elements: [],
        horizontal: false,
        rules: [],
        events: [],
        tooltip: "",
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
      return internalValue.value;
    };

    const updateItems = (newElements: string | any[]) => {
      if (newElements === undefined || newElements === null) {
        console.warn("New elements are undefined or null, skipping update.");
        return;
      }
      let parsedElements: any[] = [];

      // Check if newElements is a string, and try to parse it
      if (typeof newElements === "string") {
        try {
          parsedElements = JSON.parse(newElements);
        } catch (error) {
          console.error(
            "Failed to parse elements. Invalid JSON string:",
            error
          );
          logger.error(error);
          return; // Exit if parsing fails
        }
      } else {
        // If already an array, assign it directly
        parsedElements = newElements;
      }

      // Map the parsed elements based on their type
      const formattedElements = parsedElements.map((item) => {
        if (typeof item === "object" && item !== null) {
          return {
            value: item.value || item.code,
            label: item.label || item.name,
          };
        } else {
          return {
            value: item,
            label: item,
          };
        }
      });

      props.options.elements = formattedElements;
      // Update the options with the formatted elements
      updateOptions({ elements: formattedElements });
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

    return {
      internalValue,
      isDisabled,
      isHidden,
      getValue,
      setValue,
      disableField,
      enableField,
      setFieldError,
      clearFieldError,
      hideField,
      showField,
      emitValue,
      updateField,
      errorState,
      computedRules,
      updateItems,
      localOptions,
      updateOptions,
    };
  },
};
</script>
