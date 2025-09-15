<template>
  <div class="vhtml" v-show="!isHidden">
    <!-- <div class="label" v-if="label">
      <label class="label-container">
        <span>{{ label }}</span>
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
    </div> -->
    <div class="input-container" :class="{ 'disabled-wrapper': isDisabled }">
      <div v-html="internalValue" class="vhtml-wrapper"></div>
    </div>
    <small class="p-error" id="text-error" v-if="errorState.errorMessage">
      {{ errorState.errorMessage || "&nbsp;" }}
    </small>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, reactive, ref, watch } from "vue";

interface OptionConfig {
  name: string;
  label: string;
  tooltip: string;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
  codeHTML: boolean | null;
  content: string | null;
}

export default defineComponent({
  props: {
    label: String,
    modelValue: {
      type: String,
      default: "",
    },
    options: {
      type: Object,
      default: () => ({
        name: "",
        label: "Test",
        disabled: false,
        content: false,
        required: false,
        hidden: false,
        rules: [],
        events: [],
        codeHTML: false,
      }),
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
    // Function to process HTML and extract safe content
    const processHtmlContent = (htmlContent: string): string => {
      if (!htmlContent) return "";

      // Create a temporary DOM element to parse the HTML
      const parser = new DOMParser();
      const doc = parser.parseFromString(htmlContent, "text/html");

      // Extract styles from head
      const styles = Array.from(doc.querySelectorAll("style"))
        .map((style) => style.textContent)
        .join("\n");

      // Extract body content
      const bodyContent = doc.body ? doc.body.innerHTML : htmlContent;

      // Create scoped styles by prefixing with .vhtml-content
      const scopedStyles = styles.replace(/([^{}]+){/g, (match, selector) => {
        // Skip @media, @keyframes, etc.
        if (selector.trim().startsWith("@")) return match;

        // Replace body selector with .vhtml-content
        if (selector.includes("body")) {
          return selector.replace(/body/g, ".vhtml-content") + "{";
        }

        // Scope other selectors
        return ".vhtml-content " + selector.trim() + "{";
      });

      // Return processed content
      return `<style scoped>${scopedStyles}</style><div class="vhtml-content">${bodyContent}</div>`;
    };

    const internalValue = computed({
      get(): string {
        const rawContent = props.options.content
          ? props.options.content
          : props.modelValue;
        return processHtmlContent(rawContent);
      },
      set(value: string) {
        props.options.content = value;
        emit("update:modelValue", value);
      },
    });

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    const content = computed({
      get(): string {
        return props.options.content;
      },
      set(value: string) {
        props.options.content = value;
        emit("update:options", props.options);
      },
    });
    const isHTML = ref(
      props.options.codeHTML !== null &&
        props.options.codeHTML !== undefined &&
        props.options.codeHTML !== ""
        ? props.options.codeHTML
        : false
    );
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

    // Function to update field
    const setValue = (value: string) => {
      internalValue.value = value;
      emit("update:modelValue", value);
    };
    const updateField = (value: string) => {
      internalValue.value = value;
      props.options.content = value;
      emit("update:options", props.options);
      emit("update:modelValue", value);
    };

    // Function to get current value
    const getValue = () => {
      return internalValue.value;
    };

    // Watcher for modelValue changes
    watch(
      () => props.modelValue,
      (newValue) => {
        internalValue.value = newValue;
      }
    );

    // Check if options.content is not empty and modelValue is empty, then set content to modelValue
    watch(
      [() => props.options.content, () => props.modelValue],
      ([optionsContent, modelValue]) => {
        console.log("Watcher triggered:", {
          optionsContent,
          modelValue,
        });
        if (optionsContent && (!modelValue || modelValue.trim() === "")) {
          emit("update:modelValue", optionsContent);
        }
      },
      { immediate: true }
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
      isDisabled,
      isHidden,
      internalValue,
      content,
      isHTML,
      disableField,
      enableField,
      hideField,
      showField,
      updateField,
      errorState,
      setFieldError,
      clearFieldError,
      setValue,
      getValue,
      updateOptions,
      computedRules,
    };
  },
});
</script>

<style lang="scss">
.vhtml-wrapper {
  // Create isolation boundary
  contain: layout style;
  isolation: isolate;
  overflow: auto;
  position: relative;

  // Default styling for content
  :deep(.vhtml-content) {
    max-width: 100%;
    word-break: break-word;
    overflow-wrap: break-word;

    // Reset any inherited styles that might interfere
    margin: 0;
    padding: 0;

    // Ensure images behave properly
    img {
      max-width: 100%;
      height: auto;
      display: block;
    }
  }

  // Hide any style tags that might be injected
  :deep(style) {
    display: none !important;
  }
}

.vhtml {
  max-width: 100%;
  word-break: break-word;
  overflow-wrap: break-word;
  white-space: normal;

  // Ensures images do not overflow the container
  img {
    max-width: 100%; // Restrict image width to the container's width
    height: auto; // Maintain aspect ratio
    display: block; // Prevent any unwanted spacing under the image
  }
}
.neoeditor {
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
    height: 100%;
  }
}
.ql-align-center {
  text-align: center !important;
}
.ql-size-large {
  font-size: 16px !important;
  line-height: 20.8px !important;
  font-weight: 400 !important;
  font-family: "Trebuchet MS", TrebuchetMS, -apple-system, BlinkMacSystemFont,
    "Segoe UI", Roboto, Oxygen-Sans, Ubuntu, Cantarell, "Helvetica Neue",
    sans-serif;
}
.ql-size-small {
  font-size: 12.8px !important;
  line-height: 16.6px !important;
  font-weight: 400 !important;

  font-family: "Trebuchet MS", TrebuchetMS, -apple-system, BlinkMacSystemFont,
    "Segoe UI", Roboto, Oxygen-Sans, Ubuntu, Cantarell, "Helvetica Neue",
    sans-serif;
}
.ql-snow .ql-editor h1 {
  font-size: 20px !important;
  line-height: 26px !important;
  font-weight: 700 !important;

  font-family: "Trebuchet MS", TrebuchetMS, -apple-system, BlinkMacSystemFont,
    "Segoe UI", Roboto, Oxygen-Sans, Ubuntu, Cantarell, "Helvetica Neue",
    sans-serif;
}
.ql-snow .ql-editor h2 {
  font-size: 14px !important;
  line-height: 18.2px !important;
  font-weight: 700 !important;

  font-family: "Trebuchet MS", TrebuchetMS, -apple-system, BlinkMacSystemFont,
    "Segoe UI", Roboto, Oxygen-Sans, Ubuntu, Cantarell, "Helvetica Neue",
    sans-serif;
}
.ql-snow .ql-editor h3 {
  font-size: 12.8px !important;
  line-height: 16.6px !important;
  font-weight: 700 !important;

  font-family: "Trebuchet MS", TrebuchetMS, -apple-system, BlinkMacSystemFont,
    "Segoe UI", Roboto, Oxygen-Sans, Ubuntu, Cantarell, "Helvetica Neue",
    sans-serif;
}
.ql-snow .ql-picker.ql-size .ql-picker-item[data-value="huge"]::before {
  font-size: 10px !important;
  font-family: "Trebuchet MS", TrebuchetMS, -apple-system, BlinkMacSystemFont,
    "Segoe UI", Roboto, Oxygen-Sans, Ubuntu, Cantarell, "Helvetica Neue",
    sans-serif;
}
.disabled-wrapper {
  pointer-events: none; /* Disable all interactions */
  opacity: 0.5; /* Optional: Make it look visually disabled */
}
</style>
