<template>
  <div class="neoqrcode" v-show="!isHidden">
    <div class="label" v-if="label && showLabel">
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
    </div>
    <div
      class="input-container"
      v-if="!isHidden"
      :class="{ 'disabled-wrapper': isDisabled }"
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
      <div ref="qrcodeContainer">
        <qrcode-vue
          ref="qrCodeRef"
          :value="internalValue"
          :size="options.size"
          :level="options.errorCorrectionLevel"
          :render-as="options.renderAs"
          :background="options.background"
          :foreground="options.foreground"
          :margin="options.margin"
        />
      </div>
    </div>
    <small class="p-error" id="text-error" v-if="errorState.errorMessage">
      {{ errorState.errorMessage || "&nbsp;" }}
    </small>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, nextTick, reactive, ref, watch } from "vue";

interface OptionConfig {
  name: string;
  label: string;
  tooltip: string;
  required: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  events: any[];
  position: string;
  size: number; // Added size property
  errorCorrectionLevel: string; // Added error correction level property
  renderAs: string; // Added render type property
  background: string; // Added background color property
  foreground: string; // Added foreground color property
  margin: number; // Added margin property
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
        required: false,
        hidden: false,
        readonly: false,
        rules: [],
        events: [],
        position: "center",
        size: 150, // Default size
        errorCorrectionLevel: "M", // Default error correction level
        renderAs: "canvas", // Default render type
        background: "#ffffff", // Default background color
        foreground: "#000000", // Default foreground color
        margin: 0, // Default margin
      }),
    },
    showLabel: {
      type: Boolean,
      default: true,
    },
    isRTL: {
      type: Boolean,
      default: false,
    },
  },
  setup(props, { emit }) {
    const internalValue = computed({
      get(): string {
        return props.options.value ? props.options.value : props.modelValue;
      },
      set(value: string) {
        props.options.value = value;
        emit("update:modelValue", value);
      },
    });
    // Ref for qrcode container and qrcode component
    const qrcodeContainer = ref(null as any);
    const qrCodeRef = ref(null as any);
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    // Function to update options
    const updateOptions = (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
    };

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
      emit("update:modelValue", value);
    };

    // Function to get current value
    const getValue = () => {
      return internalValue.value;
    };

    const getImage = () => {
      // Access the canvas element within qrcodeContainer, assuming it's the only element inside
      const canvas = qrcodeContainer.value?.querySelector("canvas");
      if (canvas && canvas instanceof HTMLCanvasElement) {
        // Convert the canvas content to a Base64-encoded string (without the data URL prefix)
        const dataUrl = canvas.toDataURL("image/png");
        const base64 = dataUrl.replace(/^data:image\/png;base64,/, "");
        return base64;
      } else {
        console.warn("Canvas element not found in qrcodeContainer.");
        return null;
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

    return {
      isDisabled,
      isHidden,
      internalValue,
      computedRules,
      errorState,
      qrcodeContainer,
      qrCodeRef,
      setFieldError,
      clearFieldError,
      setValue,
      getValue,
      updateOptions,
      hideField,
      showField,
      updateField,
      getImage,
    };
  },
});
</script>

<style lang="scss">
.neoqrcode {
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

.disabled-wrapper {
  pointer-events: none; /* Disable all interactions */
  opacity: 0.5; /* Optional: Make it look visually disabled */
}
</style>
