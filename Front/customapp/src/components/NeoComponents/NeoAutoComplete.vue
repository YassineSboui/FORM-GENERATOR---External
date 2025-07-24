<template>
  <div class="neoAutoComplete" v-show="!isHidden" :dir="isRTL ? 'rtl' : 'ltr'">
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
        <!-- rest of the template -->
        <AutoComplete
          v-model="internalValue"
          :disabled="isDisabled"
          :suggestions="items"
          :minLength="options.minLength"
          class="w-full neoAutoCompleteC"
          :class="{
            'rtl-loader': isRTL,
            'p-invalid': errorMessage || errorState.errorMessage,
          }"
          @complete="search"
          @item-select="select"
          :optionLabel="options.optionLabel"
          :optionValue="options.optionValue"
          :invalid="isInvalid"
          :readonly="options.readonly"
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
        ></AutoComplete>
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
import { computed, reactive, watch } from "vue";
import { ref } from "vue";
import { logger } from "@/api/api";
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
  relatedToElise: boolean | null;
  selectedSource: string;
  elements: any[];
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
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
        name: "",
        defaultValue: "",
        label: "Test",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        selectedSource: "",
        elements: [{ code: "", name: "" }],
        rules: [],
        events: [],
      }),
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
    "errorMessage",
    "itemSelected",
    "searchItem",
    "focus",
    "blur",
    "mouseenter",
    "mouseleave",
  ],
  setup(props, { emit }) {
    // const storeComponent2 = getComponentStore(props.options.name)();
    // const { modelValue, isDisabled, isHidden, items, events } =
    //   storeToRefs(storeComponent);
    // const {
    //   activateField,
    //   deactivateField,
    //   showField,
    //   hideField,
    //   updateItems,
    // } = storeComponent;
    const items = ref([] as any[]);
    const internalValue = computed({
      get() {
        return props.modelValue;
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
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

      props.options.elements = parsedElements;
      items.value = parsedElements;
      // Update the options with the formatted elements
      updateOptions({ elements: parsedElements });
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
    const isInvalid = ref(false);

    function makeInvalid(value: boolean) {
      isInvalid.value = value;
    }

    function setElements(value: any) {
      items.value = value;
    }
    const search = async () => {
      const selectedEvent = props.options.events.find(
        (event: any) => event.rule.code === "search"
      );
      if (selectedEvent) {
        emit("searchItem", selectedEvent.code);
      }
    };
    const select = (value: any) => {
      const selectedEvent = props.options.events.find(
        (event: any) => event.rule.code === "select"
      );
      if (selectedEvent) {
        emit("itemSelected", selectedEvent.code);
      }
    };

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
    // if (isRtl.value) {
    //   (
    //     document.querySelector(".p-autocomplete-loader") as HTMLElement
    //   )?.style.setProperty("right", "96%");
    // }
    // watch(
    //   () => isRtl.value,
    //   (newValue) => {
    //     (document.querySelector(".p-autocomplete-loader")as HTMLElement)?.style.setProperty("right", newValue ? "97%" : "1rem");
    //   }
    // );

    return {
      isDisabled,
      internalValue,
      items,
      isHidden,
      isInvalid,
      disableField,
      enableField,
      hideField,
      showField,
      updateItems,
      search,
      select,
      makeInvalid,
      updateField,
      computedRules,
      setFieldError,
      clearFieldError,
      errorState,
      getValue,
      setValue,
      setElements,
    };
  },
};
</script>

<style lang="scss">
.neoAutoCompleteC.rtl-loader .p-autocomplete-loader {
  right: unset;
  left: 1rem;
}
</style>
