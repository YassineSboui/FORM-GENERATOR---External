<template>
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
  <div
    :class="
      !isHidden
        ? `neoButton flex flex-column justify-content-${
            options.position === 'center' ? 'center' : options.position
          } mt-5`
        : `neoButton`
    "
    v-show="!isHidden"
    :style="{
      maxWidth: 'fit-content',
      margin:
        options.position === 'center'
          ? '0 auto'
          : options.position === 'start'
          ? isRTL
            ? '0 0 0 auto' // RTL: align start to the right
            : '0 0 0 0' // LTR: no margin for start
          : options.position === 'end'
          ? isRTL
            ? '0 auto 0 0' // RTL: align end to the left
            : '0 0 0 auto' // LTR: align end to the right
          : '0',
    }"
  >
    <Button
      :disabled="isDisabled"
      :hidden="isHidden"
      :raised="options.raised"
      :rounded="options.rounded"
      :text="options.text"
      :outlined="options.outlined"
      :size="options.size"
      :severity="options.severity"
      @click="executeCode"
    >
      <div
        class="right flex align-content-center"
        v-if="options.iconPosition == 'right'"
      >
        <div class="txt align-content-center">
          {{
            language === "FR"
              ? options.label
              : language === "AR"
              ? options.label_AR
              : language === "ENG"
              ? options.label_ENG
              : options.label
          }}

          <i v-if="isLoading" class="pi pi-spin pi-spinner"></i>
        </div>
        <span v-if="options.icon" class="material-icons ml-1">{{
          options.icon
        }}</span>
      </div>
      <div class="left flex align-content-center" v-else>
        <span v-if="options.icon" class="material-icons mr-1">{{
          options.icon
        }}</span>
        <div class="txt align-content-center">
          {{
            language === "FR"
              ? options.label
              : language === "AR"
              ? options.label_AR
              : language === "ENG"
              ? options.label_ENG
              : options.label
          }}
          <i v-if="isLoading" class="pi pi-spin pi-spinner"></i>
        </div>
      </div>
    </Button>
    <small class="p-error mt-1" v-if="errorState.errorMessage">
      {{ errorState.errorMessage || "&nbsp;" }}
    </small>
  </div>
  <br />
</template>

<script lang="ts">
import { computed, defineComponent, reactive, watch } from "vue";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  raised: boolean | null;
  rounded: boolean | null;
  text: boolean | null;
  outlined: boolean | null;
  size: string;
  severity: string;
  position: string;
  action: any[];
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  loading: boolean | null;
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
        label: "Submit",
        required: false,
        disabled: false,
        hidden: false,
        raised: false,
        rounded: false,
        text: false,
        outlined: false,
        loading: false,
        size: "small",
        severity: "primary",
        position: "end",
        action: [],
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
  },
  emits: [
    "update:modelValue",
    "update:options",
    "click",
    "itemSelected",
    "searchItem",
  ],
  setup(props, { emit }) {
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const isLoading = computed(() => localOptions.loading);

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

    const enableLoading = () =>
      updateOptions({ loading: true, disabled: true });
    const disableLoading = () =>
      updateOptions({ loading: false, disabled: false });

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
    const executeCode = () => {
      emit("click");
    };

    return {
      isDisabled,
      isHidden,
      computedRules,
      errorState,
      setFieldError,
      clearFieldError,
      disableField,
      enableField,
      hideField,
      showField,
      executeCode,
      updateOptions,
      isLoading,
      enableLoading,
      disableLoading,
    };
  },
});
</script>

<style lang="scss">
.txt {
  margin-top: 2px;
}
</style>
