<template>
  <div class="neoAutoComplete" v-show="!isHidden" :dir="isRTL ? 'rtl' : 'ltr'">
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
        >
        </i>
      </label>
    </div>
    <div
      class="input-container"
      :style="{
        height: isParentNeoTable ? '30px' : '60px',
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <!-- append-to="self" -->
      <Field
        v-model="internalValue"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
      >
        <div class="autocomplete-wrapper">
          <AutoComplete
            v-model="internalValue"
            :disabled="isDisabled"
            :suggestions="items"
            :minLength="options.minLength"
            :placeholder="searchPlaceholder + (loading ? '...' : '')"
            class="w-full neoFlowChartC"
            :class="{
              'rtl-loader': isRTL,
              'p-invalid': errorMessage || errorState.errorMessage,
            }"
            @complete="handleComplete"
            @item-select="select"
            optionLabel="name"
            data-key="id"
            forceSelection
            :invalid="isInvalid"
            @focus="$emit('focus', $event)"
            @blur="$emit('blur', $event)"
            @mouseenter="$emit('mouseenter', $event)"
            @mouseleave="$emit('mouseleave', $event)"
          >
            <template #option="slotProps">
              <div class="flex items-center">
                <i
                  v-if="options.itemType === 'User'"
                  class="pi pi-user mr-2"
                  style="color: #6366f1"
                ></i>
                <i
                  v-else-if="options.itemType === 'Service'"
                  class="pi pi-building mr-2"
                  style="color: #10b981"
                ></i>
                <i
                  v-else-if="options.itemType === 'All'"
                  class="pi pi-users mr-2"
                  style="color: #f59e0b"
                ></i>
                <div>{{ slotProps.option.name }}</div>
              </div>
            </template>
          </AutoComplete>
          <i
            :class="[
              dynamicIconClass,
              'search-icon',
              { 'search-icon-rtl': isRTL },
            ]"
            :style="{ color: dynamicIconColor }"
          ></i>
        </div>
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
import { computed, onMounted, reactive, watch } from "vue";
import { searchFlowChart } from "@/api/api";
import { ref } from "vue";
import { useHttpRequest } from "@/store/httpRequest.store";
import { useAppStore } from "@/store/app.store";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  defaultValue: "";
  relatedToElise: boolean;
  type: string;
  name: string;
  label: string;
  required: boolean;
  readonly: boolean;
  disabled: boolean;
  hidden: boolean;
  itemType: string;
  searchType: string;
  tooltip: "";
  fullService: boolean;
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
    items: {
      type: Array,
      // required: true,
    },
    label: {
      type: String,
      required: false,
    },
    // type: Object,
    modelValue: {
      // type: Object,
      required: true,
    },
    returnObject: {
      type: Boolean,
      default: false,
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "CF_FLOWCHART",
        label: "FlowChart",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        elements: [],
        rules: [],
        events: [],
        relatedToElise: false,
        type: "FLOWCHART",
        itemType: "User",
        searchType: "Name",
        tooltip: "",
        fullService: false,
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    forService: {
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
    "update:modelValue",
    "update:options",
    "itemSelected",
    "focus",
    "blur",
    "mouseenter",
    "mouseleave",
    "errorMessage",
    "searchItem",
  ],
  setup(props, { emit }) {
    const items = ref<any[]>([]);
    const isInvalid = ref(false);
    const store = useHttpRequest();
    const appStore = useAppStore();
    const loading = ref(true);
    const searchTimeout = ref<ReturnType<typeof setTimeout> | null>(null);
    const isUserTyping = ref(false); // Flag to track if user is actively searching

    const internalValue = computed({
      get() {
        return props.modelValue as string | any;
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });

    // Watch for modelValue changes to fetch data when it's a string ID
    // Skip this when user is actively typing/searching
    watch(
      () => props.modelValue,
      async (newValue) => {
        // Skip if user is actively typing
        if (isUserTyping.value) {
          return;
        }

        if (typeof newValue === "string" && newValue.trim() !== "") {
          const payload = {
            searchTerm: newValue,
            fullService: localOptions.fullService,
            ignoredElements: [],
            itemType: localOptions.itemType,
            searchType: isLexiconOrGuid(newValue)
              ? "Id"
              : localOptions.searchType,
            ldapAttribute: "",
          };

          let response = await searchFlowChart(payload);
          const key = isLexiconOrGuid(newValue)
            ? "id"
            : localOptions.searchType.toLowerCase();

          if (
            response.length > 0 &&
            (response[0] as Record<string, any>)[key] === newValue
          ) {
            // Only update if the current modelValue is still the same string ID
            // This prevents setting an object when we expect to keep the string ID
            if (
              props.modelValue === newValue &&
              typeof props.modelValue === "string"
            ) {
              emit("update:modelValue", response[0]);
            }
          }
        }
      }
    );

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    // Computed property for search placeholder
    const searchPlaceholder = computed(() => {
      const itemType = localOptions.itemType;
      if (itemType === "User") {
        return "Rechercher un utilisateur";
      } else if (itemType === "Service") {
        return "Rechercher un service";
      } else if (itemType === "All") {
        return "Rechercher un utilisateur ou service";
      } else {
        return "Rechercher...";
      }
    });

    // Computed property for dynamic icon class
    const dynamicIconClass = computed(() => {
      if (internalValue.value && typeof internalValue.value === "object") {
        if (internalValue.value.id.startsWith("LEXICON")) {
          return "pi pi-building";
        } else {
          return "pi pi-user";
        }
      }
      return "pi pi-search"; // Default search icon
    });

    // Computed property for dynamic icon color
    const dynamicIconColor = computed(() => {
      if (
        internalValue.value &&
        typeof internalValue.value === "object" &&
        internalValue.value.types
      ) {
        const types = internalValue.value.types;
        if (types.includes(1) && !types.includes(2)) {
          return "#6366f1"; // User color (blue)
        } else if (types.includes(2) && !types.includes(1)) {
          return "#10b981"; // Service color (green)
        } else if (types.includes(1) && types.includes(2)) {
          return "#f59e0b"; // Mixed color (orange)
        }
      }
      return "#165c77"; // Default search icon color
    });

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

    // // Watcher for modelValue changes
    // watch(
    //   () => props.modelValue,
    //   (newValue) => {
    //     internalValue.value = newValue;
    //   }
    // );

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

    function makeInvalid(value: boolean) {
      isInvalid.value = value;
    }

    // Debounced search function
    const debouncedSearch = async (strict: boolean) => {
      if (typeof internalValue.value === "string") {
        console.log(
          "[NeoFlowchart_V2] Executing debounced search:",
          internalValue.value
        );
        const payload = {
          searchTerm: strict
            ? internalValue.value
            : internalValue.value.replace(/&/g, " ") + "*",
          fullService: props.options.fullService,
          ignoredElements: [],
          itemType: props.options.itemType,
          searchType: props.options.searchType,
          ldapAttribute: "",
        };
        items.value = await searchFlowChart(payload);
      }
    };

    // Handle complete event from AutoComplete with debouncing
    const handleComplete = () => {
      console.log("[NeoFlowchart_V2] Complete event triggered, debouncing...");

      // Set flag to prevent modelValue watcher from triggering
      isUserTyping.value = true;

      // Clear any existing timeout
      if (searchTimeout.value) {
        clearTimeout(searchTimeout.value);
      }

      // Debounce: wait 1500ms after user stops typing
      searchTimeout.value = setTimeout(() => {
        console.log("[NeoFlowchart_V2] User stopped typing, executing search");
        debouncedSearch(false);
      }, 1500);
    };

    const search = (strict: boolean) => {
      console.log(
        "[NeoFlowchart_V2] Search triggered, value type:",
        typeof internalValue.value
      );

      // Clear any existing timeout
      if (searchTimeout.value) {
        clearTimeout(searchTimeout.value);
      }

      // Debounce: wait 1500ms after user stops typing
      searchTimeout.value = setTimeout(() => {
        debouncedSearch(strict);
      }, 1500);
    };

    const select = () => {
      // Clear the typing flag when user selects an item
      isUserTyping.value = false;

      const selectedEvent = props.options.events.find(
        (event: any) => event.rule.code === "select"
      );
      if (selectedEvent) {
        emit("itemSelected", selectedEvent.code);
      }
    };

    function manageProperties(opt: any) {
      if (opt) {
        updateOptions(opt);
      }
    }

    // Function to check if value contains LEXICON_ or is a GUID
    function isLexiconOrGuid(value: any): boolean {
      if (!value || typeof value !== "string") {
        return false;
      }

      // Check if contains LEXICON_
      if (value.includes("LEXICON_")) {
        return true;
      }

      // Check if it's a GUID format (32 hexadecimal characters)
      const guidRegex = /^[0-9a-f]{32}$/i;
      return guidRegex.test(value);
    }

    watch(
      () => store.loading,
      async (newValue, oldValue) => {
        console.log(
          "[NeoFlowchart_V2] Store loading changed:",
          oldValue,
          "→",
          newValue
        );
        if (newValue === false) {
          loading.value = true;
          await search(true);
          console.log(
            "[NeoFlowchart_V2] Search completed, items found:",
            items.value.length
          );
          if (items.value.length > 0) {
            internalValue.value = items.value[0];
          }
          loading.value = false;
        }
      }
    );

    // watch(
    //   () => props.modelValue,
    //   async (newValue) => {
    //     if (typeof newValue === "string" && newValue.startsWith("LEXICON")) {
    //       console.log("updating internalValue");
    //       internalValue.value = await appStore.getObjectByLexicon(newValue);
    //     }
    //   }
    // );

    onMounted(async () => {
      if (
        props.modelValue !== undefined &&
        props.modelValue !== null &&
        props.modelValue !== ""
      ) {
        if (typeof props.modelValue === "string") {
          const payload = {
            searchTerm: props.modelValue,
            fullService: localOptions.fullService,
            ignoredElements: [],
            itemType: localOptions.itemType,
            searchType: "Id",
            ldapAttribute: "",
          };
          let response = await searchFlowChart(payload);
          const key = localOptions.searchType
            ? localOptions.searchType.toLowerCase()
            : "";
          if (
            response.length !== 0 &&
            (response[0] as Record<string, any>)[key] === props.modelValue
          ) {
            internalValue.value = response.length > 0 ? response[0] : null;
          }
        }
      }
    });

    return {
      loading,
      search,
      handleComplete,
      select,
      isDisabled,
      isHidden,
      internalValue,
      items,
      isInvalid,
      computedRules,
      errorState,
      searchPlaceholder,
      dynamicIconClass,
      dynamicIconColor,
      disableField,
      enableField,
      hideField,
      showField,
      setFieldError,
      getValue,
      setValue,
      clearFieldError,
      makeInvalid,
      updateField,
      manageProperties,
      isLexiconOrGuid,
    };
  },
};
</script>

<style lang="scss">
.p-autocomplete .p-autocomplete-label {
  padding: 0 0.5rem !important;
}
/* Styles pour NeoFlowchart AutoComplete */
.autocomplete-wrapper {
  position: relative;

  .search-icon {
    position: absolute;
    left: 0.75rem;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    z-index: 1;
    font-size: 1.1rem;
  }

  .search-icon-rtl {
    left: auto;
    right: 0.75rem;
  }
}
.neoFlowChartC {
  position: relative;

  .p-autocomplete-input {
    padding-left: 2.5rem !important;
  }

  /* RTL support for search icon */
  &.rtl-loader .p-autocomplete-input {
    padding-left: 0.75rem !important;
    padding-right: 2.5rem !important;
  }

  .p-autocomplete-panel .p-autocomplete-items .p-autocomplete-item {
    padding: 0.5rem 0.75rem;

    .flex.items-center {
      display: flex;
      align-items: center;
      gap: 0.5rem;

      i {
        font-size: 1rem;
        min-width: 1rem;
      }
    }
  }
}
</style>
