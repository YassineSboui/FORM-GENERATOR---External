<template>
  <div class="vhtml" v-show="!isHidden">
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
    "update:modelValue",
    "update:options",
    "disableField",
    "enableField",
    "hideField",
    "showField",
    "updateField",
  ],
  setup(props, { emit }) {
    // =========================
    // ÉTAT D'ERREUR GLOBAL
    // =========================
    const errorState = reactive({
      errorMessage: "",
    });

    // Fonction pour définir un message d'erreur
    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };

    // Fonction pour effacer le message d'erreur
    const clearFieldError = () => {
      errorState.errorMessage = "";
    };

    // =========================
    // TRAITEMENT + VALIDATION DU HTML
    // =========================
    // Function to process HTML and extract safe content
    const processHtmlContent = (htmlContent: string): string => {
      // Étape 1 : rien à valider si contenu vide
      if (!htmlContent || !htmlContent.trim()) {
        // On s'assure aussi d'effacer un ancien message d'erreur
        if (errorState && errorState.errorMessage) {
          clearFieldError();
        }
        return "";
      }

      // Étape 2 : validation de sécurité avant tout rendu via v-html
      const validationResult = validateVHtml(htmlContent);

      if (!validationResult.valid) {
        // On bloque l'affichage du HTML potentiellement dangereux
        const message =
          "Le contenu HTML contient des éléments potentiellement dangereux : " +
          validationResult.errors.join(" | ");
        setFieldError(message);
        // On retourne une chaîne vide pour éviter toute exécution via v-html
        return "";
      }

      // On peut éventuellement journaliser les avertissements dans la console
      if (validationResult.warnings && validationResult.warnings.length > 0) {
        console.warn(
          "[NeoVHtml] Avertissements pour le contenu v-html :",
          validationResult.warnings
        );
      }

      // Contenu considéré comme sûr côté client → on efface un éventuel ancien message
      if (errorState && errorState.errorMessage) {
        clearFieldError();
      }

      // Étape 3 : logique existante de traitement / scoping du HTML

      // If it's a full HTML document, extract and process it
      if (
        htmlContent.trim().toLowerCase().startsWith("<!doctype") ||
        htmlContent.trim().toLowerCase().startsWith("<html")
      ) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(htmlContent, "text/html");

        // Extract all styles from head
        const styles = Array.from(doc.querySelectorAll("style"))
          .map((style) => style.textContent)
          .join("\n");

        // Extract body content
        const bodyContent = doc.body.innerHTML;

        // Scope styles to the vhtml-content container
        const scopedStyles = styles
          .split("}")
          .map((rule) => {
            if (!rule.trim()) return "";
            // Scope to .vhtml-content
            return `.vhtml-content ${rule}}`;
          })
          .join("\n");

        const wrapperStyles = ` style="all: initial; font-family: inherit; font-size: inherit; line-height: inherit;"`;

        // Return scoped content
        return `<style>${scopedStyles}</style><div class="vhtml-content"${wrapperStyles}>${bodyContent}</div>`;
      }

      // For non-full HTML documents, we still scope styles if there are <style> tags
      const parser = new DOMParser();
      const doc = parser.parseFromString(htmlContent, "text/html");

      const styles = Array.from(doc.querySelectorAll("style"))
        .map((style) => style.textContent)
        .join("\n");

      let bodyContent = htmlContent;
      if (styles) {
        // Remove style tags from original HTML
        Array.from(doc.querySelectorAll("style")).forEach((style) =>
          style.remove()
        );
        bodyContent = doc.body.innerHTML;
      }

      if (styles) {
        const scopedStyles = styles
          .split("}")
          .map((rule) => {
            if (!rule.trim()) return "";
            return `.vhtml-content ${rule}}`;
          })
          .join("\n");

        const wrapperStyles = ` style="all: initial; font-family: inherit; font-size: inherit; line-height: inherit;"`;

        return `<style>${scopedStyles}</style><div class="vhtml-content"${wrapperStyles}>${bodyContent}</div>`;
      }

      // For non-full HTML documents without separate styles, return as-is
      return htmlContent;
    };

    const internalValue = computed({
      get(): string {
        const rawContent = props.options?.content
          ? props.options.content
          : props.modelValue;
        return processHtmlContent(rawContent);
      },
      set(value: string) {
        if (props.options) {
          props.options.content = value;
        }
        emit("update:modelValue", value);
      },
    });

    const isDisabled = computed(() => !!props.options?.disabled);
    const isHidden = computed(() => !!props.options?.hidden);

    const localOptions = reactive<OptionConfig>({
      ...(props.options as OptionConfig),
    });

    const content = ref(localOptions.content || "");
    const isHTML = ref(!!localOptions.codeHTML);

    const updateField = () => {
      emit("updateField", {
        name: localOptions.name,
        value: internalValue.value,
      });
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
    };

    const getValue = () => {
      return internalValue.value;
    };

    const updateOptions = (newOptions: Partial<OptionConfig>) => {
      Object.assign(localOptions, newOptions);
      emit("update:options", { ...localOptions });
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
      [() => props.options?.content, () => props.modelValue],
      ([optionsContent, modelValue]) => {
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

    // Validation rules computation (déjà existant)
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

    // Watch for field validity and clear error if valid (existant)
    watch(
      () => internalValue.value,
      (newValue) => {
        // Si on avait une erreur "contenu vide" et que maintenant le champ n'est plus vide,
        // on la nettoie (ça ne gère pas uniquement la sécurité, mais aussi le "required").
        if (errorState.errorMessage) {
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

  // Allow style tags for animations and custom styles
  :deep(style) {
    display: block !important;
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
