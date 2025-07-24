<template>
  <div
    class="neoRecap"
    :class="
      !options.lineaire
        ? 'flex flex-column'
        : 'flex flex-row align-items-center'
    "
    v-if="!isHidden"
  >
    <!-- <div class="label"> -->
    <span class="neoRecap-label-container">
      {{
        language === "FR"
          ? options.label
          : language === "AR"
          ? options.label_AR
          : language === "ENG"
          ? options.label_ENG
          : options.label
      }}
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
      ></i
      >:</span
    >
    <!-- </div> -->
    <div class="recap ml-2">
      {{ internalValue }}
    </div>
    <small class="p-error" id="text-error" v-if="errorState.errorMessage">
      {{ errorState.errorMessage || "&nbsp;" }}
    </small>
  </div>
</template>

<script lang="ts">
import { useAppStore } from "@/store/app.store";
import { computed, defineComponent, reactive, watch } from "vue";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  lineaire: boolean | null;
  type: string;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
}

export default defineComponent({
  props: {
    modelValue: {
      type: String,
      default: "",
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        type: "Nom",
        name: "CF_Recap",
        label: "Recap",
        lineaire: false,
        hidden: false,
      }),
    },
    language: {
      type: String,
      default: "FR",
    },
  },
  emits: ["update:options", "update:modelValue"],
  setup(props, { emit }) {
    const store = useAppStore();
    const internalValue = computed({
      get(): string {
        return props.modelValue;
      },
      set(value: string) {
        emit("update:modelValue", value);
      },
    });
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
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

    // Function to update options
    const updateOptions = (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
    };

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

    return {
      internalValue,
      isHidden,
      updateField,
      setValue,
      getValue,
      computedRules,
      setFieldError,
      clearFieldError,
      errorState,
      hideField,
      showField,
      updateOptions,
    };
  },
});
</script>

<style lang="scss">
.neoRecap {
  .neoRecap-label-container {
    color: #165c77;
    align-items: center;
    display: flex;
    font-size: 14px;
    height: 30px;
  }
  .recap {
    font-size: 14px;
    display: inline-block;
    overflow-wrap: break-word;
  }
}
</style>
