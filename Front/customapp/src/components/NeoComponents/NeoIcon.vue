<template>
  <div class="neoIcon" v-show="!isHidden">
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
        <Select
          :disabled="isDisabled"
          :readonly="options.readonly"
          v-model="internalValue"
          :options="allIcons"
          class="w-full neoIconDropdown"
          :class="{ 'p-invalid': errorMessage || errorState.errorMessage }"
          :panelStyle="{ direction: isRTL ? 'rtl' : 'ltr' }"
          @click.stop
          filter
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
        >
          <template #option="slotProps">
            <div class="flex align-items-center" style="gap: 8px">
              <span class="material-icons">{{ slotProps.option }}</span>
              <div>{{ slotProps.option }}</div>
            </div>
          </template>
          <template #value="slotProps">
            <div
              class="flex align-items-center"
              v-if="slotProps.value"
              style="gap: 8px"
            >
              <span class="material-icons">{{ slotProps.value }}</span>
              <div>{{ slotProps.value }}</div>
            </div>
          </template>
        </Select>
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
import { computed, onBeforeMount, reactive, ref, watch } from "vue";
// Update the path below if your icons file is located elsewhere
import { icons } from "../../data/icons";
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
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: {
      type: String,
      default: "",
    },
    options: {
      type: Object as () => OptionConfig,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "",
        tooltip: "",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        relatedToElise: false,
        rules: [],
        events: [],
      }),
    },
    language: {
      type: String,
      default: "FR",
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    isRTL: {
      type: Boolean,
      default: false,
    },
  },
  emits: [
    "update:options",
    "update:modelValue",
    "focus",
    "blur",
    "mouseenter",
    "mouseleave",
  ],
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
    const allIcons = ref([] as any[]);
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled) as any;
    const isHidden = computed(() => localOptions.hidden) as any;

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

    const primeIcons = ref(icons as string[]);
    onBeforeMount(() => {
      primeIcons.value.forEach((data) => {
        allIcons.value.push(data);
      });
    });
    return {
      internalValue,
      isDisabled,
      isHidden,
      computedRules,
      allIcons,
      setValue,
      updateField,
      getValue,
      enableField,
      disableField,
      hideField,
      showField,
      setFieldError,
      clearFieldError,
      errorState,
      updateOptions,
    };
  },
};
</script>

<style lang="scss">
@import "@/scss/variables";
.neoIcon {
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
      padding-top: 5px;
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
      .label .label-container-modified {
        text-align: right;
      }
    }
  }
  .input-container {
    width: 100%;
    .neoIconDropdown {
      background-color: #f3f8f9;
      span {
        display: flex;
        align-items: center;
      }
      ::v-deep .p-dropdown-filter {
        direction: ltr;
      }
      &.rtl ::v-deep .p-dropdown-filter {
        direction: rtl;
        text-align: right;
      }
    }
  }
}
</style>
