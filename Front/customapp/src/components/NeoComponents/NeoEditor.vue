<template>
  <div class="neoeditor" v-show="!isHidden" :style="neoEditorStyle">
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
        ></i
      ></label>
    </div>
    <div
      :class="{
        'rtl-editor input-container': isRTL,
        'ltr-editor input-container': !isRTL,
        'disabled-wrapper': isDisabled,
      }"
    >
      <Editor
        class="editor"
        v-model="internalValue"
        :readonly="isDisabled || options.readonly"
        :editorStyle="editorStyle"
      >
      </Editor>
      <small class="p-error" id="text-error" v-if="errorState.errorMessage">
        {{ errorState.errorMessage || "&nbsp;" }}
      </small>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, reactive, ref, watch } from "vue";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  height: string | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
}
export default defineComponent({
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      type: String,
      default: "",
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "Test",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        height: null,
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
    height: {
      type: String,
      default: "150px",
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

    const editorStyle = computed(() => {
      const height = props.options.height || props.height;
      return `
        min-height: ${height};
        max-height: 300px;
        width: 100%;
        font-size: 14.7px;
        line-height: 18.2px;
        font-weight: 400;
        font-family: Trebuchet MS;
        overflow-y: auto;
      `;
    });
    const neoEditorStyle = computed(() => ({
      minHeight: props.options.height || props.height || "150px",
      width: "100%",
      // Remove the height property so it grows with content
    }));
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
    return {
      isDisabled,
      isHidden,
      internalValue,
      errorState,
      editorStyle,
      computedRules,
      neoEditorStyle,
      setValue,
      getValue,
      disableField,
      enableField,
      hideField,
      showField,
      updateField,
      setFieldError,
      clearFieldError,
    };
  },
});
</script>

<style scoped>
.neoeditor {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  min-height: 0;
}

.neoeditor .input-container,
.neoeditor .editor {
  width: 100%;
  min-height: 0;
  height: auto !important;
  box-sizing: border-box;
}

.neoeditor .editor .ql-editor {
  max-height: 250px; /* slightly less than 300px to account for toolbar */
  overflow-y: auto;
}
</style>
