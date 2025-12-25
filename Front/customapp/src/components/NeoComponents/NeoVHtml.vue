<template>
  <div class="vhtml" v-show="!isHidden">
    <template v-if="!errorState.errorMessage">
      <div class="input-container" :class="{ 'disabled-wrapper': isDisabled }">
        <div ref="shadowHost" class="vhtml-wrapper"></div>
      </div>
    </template>
    <small class="p-error" id="text-error" v-if="errorState.errorMessage">
      {{ errorState.errorMessage || "\u00A0" }}
    </small>
  </div>
</template>

<script lang="ts">
import {
  computed,
  defineComponent,
  onMounted,
  reactive,
  ref,
  watch,
} from "vue";
import { validateVHtml } from "@/utils/vhtmlValidator";

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
        content: "",
        required: false,
        hidden: false,
        rules: [],
        events: [],
        codeHTML: false,
      }),
    },
  },
  emits: [
    "update:modelValue",
    "update:options",
    "disableField",
    "enableField",
    "hideField",
    "showField",
    "updateField",
  ],
  setup(props, { emit }) {
    const errorState = reactive({ errorMessage: "" });
    const shadowHost = ref<HTMLElement | null>(null);
    let shadowRoot: ShadowRoot | null = null;

    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };
    const clearFieldError = () => {
      errorState.errorMessage = "";
    };

    const internalValue = computed({
      get(): string {
        // Always prioritize options.content over modelValue for vhtml components
        return props.options?.content || props.modelValue || "";
      },
      set(value: string) {
        if (props.options) props.options.content = value;
        emit("update:modelValue", value);
        updateShadowContent();
      },
    });

    // Initialize Shadow DOM for proper encapsulation
    onMounted(() => {
      if (shadowHost.value && !shadowRoot) {
        try {
          shadowRoot = shadowHost.value.attachShadow({ mode: "open" });
          updateShadowContent();
        } catch (e) {
          console.error("[NeoVHtml] Failed to attach shadow DOM:", e);
        }
      }
    });

    // Update Shadow DOM content
    const updateShadowContent = (forceContent?: string) => {
      if (!shadowRoot) {
        console.warn(
          "[NeoVHtml] updateShadowContent called but shadowRoot not available"
        );
        return;
      }

      // Use forced content if provided, otherwise get from internalValue
      const content =
        forceContent !== undefined
          ? forceContent
          : props.options?.content || props.modelValue || "";

      // Create a wrapper with reset styles to prevent inheritance issues
      shadowRoot.innerHTML = `
        <style>
          :host {
            display: block;
            contain: layout style;
            isolation: isolate;
          }
          /* Reset and base styles for the shadow content */
          * {
            box-sizing: border-box;
          }
        </style>
        <div class="shadow-content">${content}</div>
      `;
    };

    const runValidation = (html: string | null | undefined) => {
      const content = html || "";
      const result = validateVHtml(content);
      if (!result.valid)
        setFieldError(
          "Le contenu HTML contient des éléments potentiellement dangereux : " +
            result.errors.join(" | ")
        );
      else {
        if (result.warnings && result.warnings.length)
          console.warn("[NeoVHtml] v-html warnings:", result.warnings);
        clearFieldError();
      }
    };

    watch(
      [() => props.modelValue, () => props.options?.content],
      ([modelValue, optionsContent]) => {
        const valueToValidate = optionsContent ? optionsContent : modelValue;
        runValidation(valueToValidate);
        updateShadowContent();
      },
      { immediate: true }
    );

    const localOptions = reactive<OptionConfig>({
      ...(props.options as OptionConfig),
    });
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const content = ref(localOptions.content || "");
    const isHTML = ref(!!localOptions.codeHTML);
    const disableField = () => updateOptions({ disabled: true });
    const enableField = () => updateOptions({ disabled: false });
    const hideField = () => updateOptions({ hidden: true });
    const showField = () => updateOptions({ hidden: false });
    const setValue = (value: string) => {
      console.log("[NeoVHtml] setValue called with:", value);
      // Update options content first
      if (props.options) {
        props.options.content = value;
        console.log(
          "[NeoVHtml] Updated props.options.content to:",
          props.options.content
        );
      }
      // Emit updates
      emit("update:options", props.options);
      emit("update:modelValue", value);
      // Force update shadow content with the exact value we just set
      updateShadowContent(value);
    };
    const getValue = () => internalValue.value;
    const updateOptions = (newOptions: Partial<OptionConfig>) => {
      Object.assign(localOptions, newOptions);
      emit("update:options", { ...localOptions });
    };

    watch(
      () => props.modelValue,
      (newValue) => {
        internalValue.value = newValue;
      }
    );
    watch(
      [() => props.options?.content, () => props.modelValue],
      ([optionsContent, modelValue]) => {
        if (optionsContent && (!modelValue || modelValue.trim() === ""))
          emit("update:modelValue", optionsContent);
      },
      { immediate: true }
    );
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );

    const computedRules = computed(() => {
      if (Array.isArray(localOptions.rules)) {
        let expression = localOptions.rules
          .map((item) => item.expression)
          .join("|");
        if (localOptions.required)
          expression += expression ? "|required" : "required";
        if (localOptions.hidden || localOptions.disabled) expression = "";
        return expression;
      }
      return "";
    });

    watch(
      () => internalValue.value,
      (newValue) => {
        if (errorState.errorMessage) {
          if (newValue && newValue.trim() !== "") clearFieldError();
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
      // updateField,
      errorState,
      setFieldError,
      clearFieldError,
      setValue,
      getValue,
      updateOptions,
      computedRules,
      shadowHost,
    };
  },
});
</script>

<style lang="scss">
.vhtml-wrapper {
  contain: layout style;
  isolation: isolate;
  overflow: auto;
  position: relative;
  :deep(.vhtml-content) {
    max-width: 100%;
    word-break: break-word;
    overflow-wrap: break-word;
    margin: 0;
    padding: 0;
    img {
      max-width: 100%;
      height: auto;
      display: block;
    }
  }
  :deep(style) {
    display: block !important;
  }
}
.vhtml {
  max-width: 100%;
  word-break: break-word;
  overflow-wrap: break-word;
  white-space: normal;
  img {
    max-width: 100%;
    height: auto;
    display: block;
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
  pointer-events: none;
  opacity: 0.5;
}
</style>
