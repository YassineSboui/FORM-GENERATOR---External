<template>
  <div
    class="neoTreeSelect"
    v-show="!isHidden"
    :id="myCurrentComponent"
    :dir="isRTL ? 'rtl' : 'ltr'"
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
    <div
      class="input-container"
      :style="{
        height: isParentNeoTable ? '30px' : '60px',
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <Field
        v-model="internalValue"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
      >
        <TreeSelect
          v-model="internalValue"
          filterMode="strict"
          :disabled="isDisabled"
          :options="elements"
          :loading="loading"
          class="w-full neoTreeSelectDropdown flex align-items-center"
          :pt="{
            root: { class: 'w-full ' },
            labelContainer: {
              class: 'labelcontainerMultiSelect',
            },
          }"
          display="comma"
          :empty-message="loading ? ' ' : 'Aucune donnée trouvée'"
          :selectionMode="selectionMode"
          selectedItemsLabel="Vous avez sélectionné le nombre maximum d'éléments"
          :panelStyle="{ direction: isRTL ? 'rtl' : 'ltr' }"
          @click="attachDropdownToParent()"
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
          :class="{ 'p-invalid': errorMessage || errorState.errorMessage }"
          :filter="filter"
          :showClear="options.showClear"
        >
        </TreeSelect>
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
import { useToast } from "primevue/usetoast";
import {
  computed,
  ref,
  onMounted,
  onBeforeUnmount,
  watch,
  nextTick,
  reactive,
} from "vue";
import { useI18n } from "vue-i18n";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  type: string;
  tooltip: string;
  selectedType: string;
  selectedSource: string;
  selectedService: string;
  thesaurusId: string;
  termLimit: number;
  returnLabel: boolean;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  showClear: boolean | null;
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
    items: {
      type: Array,
      required: false,
    },
    label: {
      type: String,
      required: false,
    },
    label_AR: {
      type: String,
      required: false,
    },
    label_ENG: {
      type: String,
      required: false,
    },
    modelValue: {
      required: true,
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "Custom",
        type: "CUSTOM_TREEVIEW",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        relatedToElise: false,
        selectionMode: "single",
        filter: false,
        showClear: false,
        elements: [],
        rules: [],
        events: [],
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
    "focus",
    "blur",
    "mouseleave",
    "mouseenter",
    "update:options",
  ],
  setup(props, { emit }) {
    const toast = useToast();
    const loading = ref(false);
    // Manage item value
    const internalValue = computed({
      get() {
        return props.modelValue;
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });
    const { t } = useI18n();
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });
    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const elements = computed(() => localOptions.elements);
    const selectionMode = computed(() => {
      return localOptions.selectionMode || "single";
    });
    const filter = computed(() => {
      return localOptions.filter || false;
    });
    // Function to get current value
    const getValue = () => {
      if (props.options.selectionMode == "single") {
        console.log("internalValue.value", internalValue.value);
        return Object.keys(internalValue.value as any)[0] ?? ""; // Return the first key
      } else {
        return Object.keys(internalValue.value as any) ?? [];
      }
    };
    // Add a temporary variable to store the initial model value
    const initialModelValue = ref(props.modelValue);
    // Function to update field
    const setValue = (value: any) => {
      if (props.options.selectionMode == "single") {
        if (typeof value === "string") {
          internalValue.value = { [value]: true };
        } else {
          internalValue.value = value;
        }
        console.log("internalValue.value", internalValue.value);
      } else {
        if (Array.isArray(value) && typeof value[0] === "string") {
          internalValue.value = value.map((item: any) => ({ [item]: true }));
        } else {
          internalValue.value = value;
        }
      }
    };
    const updateField = (value: any) => {
      if (props.options.selectionMode == "single") {
        internalValue.value = { [value]: true };
      } else {
        internalValue.value = value.map((item: any) => ({
          [item]: true,
        }));
      }
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
    const selectionItems = computed(() => {
      return props.items;
    });
    const generateRandomString = (length: any) => {
      const characters = "0123456789";
      let randomString = "";

      for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * characters.length);
        randomString += characters[randomIndex];
      }
      return randomString;
    };
    const myCurrentComponent = ref(generateRandomString(10));
    // Handle the dropdown positioning manually
    const handleSelection = () => {
      const dropdown = document.querySelector(
        ".p-treeselect-overlay"
      ) as HTMLElement;
      if (
        dropdown &&
        dropdown.classList.contains("p-connected-overlay-visible")
      ) {
        dropdown.classList.remove("p-connected-overlay-visible");
      }
    };
    const attachDropdownToParent = async () => {
      await nextTick();
      // Find the dropdown and parent within the specific instance
      const dropdown = document.querySelector(
        ".p-treeselect-overlay"
      ) as HTMLElement;

      if (dropdown && parent) {
        dropdown.style.direction = props.isRTL ? "rtl" : "ltr";
      }
    };

    // Watch for changes in TreeItems and set the value when it's fully filled
    watch(
      () => props.options.elements,
      (newValue) => {
        if (
          newValue.length > 0 &&
          initialModelValue.value != "" &&
          initialModelValue.value != null &&
          initialModelValue.value != undefined
        ) {
          setValue(initialModelValue.value);
        }
      },
      { deep: true }
    );
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );
    watch(
      () => props.modelValue,
      (newValue: any) => {
        if (
          newValue &&
          newValue.length > 0 &&
          props.modelValue != "" &&
          props.modelValue != null &&
          props.modelValue != undefined
        ) {
          setValue(props.modelValue);
        }
      },
      { deep: true }
    );
    // Watch for changes in selectionMode and empty the value if it changes
    watch(
      () => localOptions.selectionMode,
      (newSelectionMode, oldSelectedModed) => {
        if (newSelectionMode !== oldSelectedModed) {
          // Reset the value when selection mode changes
          internalValue.value = newSelectionMode === "single" ? {} : [];
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
          if (typeof newValue === "string") {
            isNotEmpty = newValue.trim() !== "";
          } else if (Array.isArray(newValue)) {
            isNotEmpty = newValue.length > 0;
          } else if (typeof newValue === "object" && newValue !== null) {
            isNotEmpty = Object.keys(newValue).length > 0;
          }
          if (isNotEmpty) {
            clearFieldError();
          }
        }
      }
    );

    // Clean up before unmounting
    onBeforeUnmount(() => {
      window.removeEventListener("resize", () => {});
    });
    // On mounted lifecycle hook for managing dropdown
    onMounted(async () => {});

    return {
      t,
      filter,
      loading,
      isHidden,
      elements,
      isDisabled,
      errorState,
      computedRules,
      internalValue,
      selectionItems,
      myCurrentComponent,
      getValue,
      setValue,
      hideField,
      showField,
      updateField,
      enableField,
      disableField,
      setFieldError,
      updateOptions,
      clearFieldError,
      handleSelection,
      selectionMode,
      generateRandomString,
      attachDropdownToParent,
      localOptions,
    };
  },
};
</script>
<style>
.h-35 {
  height: 35px;
}
</style>
