<template>
  <div class="NeoListDocument" v-show="!isHidden">
    <div class="label">
      <label class="label-container">
        <span>
          {{
            language === "FR"
              ? options.label
              : language === "AR"
              ? options.label_AR
              : language === "ENG"
              ? options.label_ENG
              : options.label
          }}</span
        >
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

    <div class="listDocument">
      <NeoDocument
        v-for="document in documents"
        :key="document.chrono"
        :title="document.nativeObject.subject"
        :prestation="document.nativeObject.customFields"
        :chrono="document.chrono"
        :url="document.mailId"
        v-if="documents.length > 0"
      />
      <div v-else class="docEmpty">
        {{ options.emptyText }}
      </div>
      <small class="p-error" id="text-error" v-if="errorState.errorMessage">
        {{ errorState.errorMessage || "&nbsp;" }}
      </small>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, reactive, ref, watch } from "vue";
import NeoDocument from "./NeoDocument.vue";
import { logger } from "@/api/api";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
}

export default defineComponent({
  components: {
    NeoDocument,
  },
  props: {
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        relatedToElise: false,
        type: "List",
        name: "CF_ListDocument",
        label: "List Document",
        emptyText: "Aucun document",
        hidden: false,
        rules: [],
        events: [],
      }),
    },
    language: {
      type: String,
      default: "FR",
    },
  },
  emits: ["update:options"],
  setup(props, { emit }) {
    const documents = ref([] as any);
    function setDocuments(value: any) {
      if (typeof value === "string") {
        try {
          // Attempt to parse string as JSON
          const parsedValue = JSON.parse(value);
          if (Array.isArray(parsedValue)) {
            documents.value = parsedValue;
          } else {
            console.warn(
              "Parsed value is not an array. Ensure the string is a valid JSON array."
            );
            documents.value = [];
          }
        } catch (error) {
          console.error("Invalid JSON string provided to setValue:", error);
          logger.error(error);
          documents.value = [];
        }
      } else if (Array.isArray(value)) {
        documents.value = value;
      } else {
        console.warn(
          "Invalid value type provided to setValue. Expected string or array."
        );
        documents.value = [];
      }
    }
    // Function to get current value
    const getValue = () => {
      return documents.value;
    };
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Function to update options
    const updateOptions = (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
    };

    // Function to hide field
    const hideField = () => updateOptions({ hidden: true });

    // Function to show field
    const showField = () => updateOptions({ hidden: false });

    const isHidden = computed(() => localOptions.hidden);
    // Validation rules computation
    const computedRules = computed(() => {
      if (Array.isArray(localOptions.rules)) {
        let expression = localOptions.rules
          .map((item) => item.expression)
          .join("|");

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

    // Watcher for options changes
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );
    return {
      documents,

      isHidden,
      computedRules,
      errorState,
      setFieldError,
      clearFieldError,
      setDocuments,
      getValue,
      hideField,
      showField,
      updateOptions,
    };
  },
});
</script>

<style lang="scss">
.NeoListDocument {
  // height: 40px;
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
      padding-top: 5px;
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
      .label .label-container-modified {
        text-align: right;
      }
    }
  }
  .docEmpty {
    background-color: #f9dde0;
    padding: 6px;
    border-radius: 4px;
  }
  .listDocument {
    overflow-y: auto;
    overflow-x: hidden;
    max-height: 200px;
    margin: 5px 0;
  }
}
</style>
