<template>
  <div class="neoSignField mb-3" v-show="!isHidden">
    <div class="label" v-if="options.label">
      <label class="label-container">
        {{
          language === "FR"
            ? options.label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : options.label
        }}
        <span v-show="options.required" class="required-asterisk">*</span>
        <i
          v-if="options.tooltip"
          class="pi pi-info-circle tooltip-icon"
          v-tooltip.top="options.tooltip"
        ></i>
      </label>
    </div>
    <div
      class="input-container"
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
      <div
        class="signature-container"
        :class="{
          'disabled-wrapper': isDisabled,
          'readOnly-wrapper': readOnly,
        }"
        style="max-width: 100%"
      >
        <div class="signature-field-wrapper">
          <Vue3Signature
            :w="toPx(options.width, 'w') + 'px'"
            :h="toPx(options.height, 'h') + 'px'"
            ref="signatureCanvas"
            :sigOption="formattedSigOption"
            :disabled="isDisabled"
            class="signature-canvas"
            @mouseup="save('image/jpeg')"
            @touchend="save('image/jpeg')"
            @touchmove="onTouchMove"
          />
        </div>
        <div v-if="!readOnly" class="signature-actions">
          <Button
            severity="secondary"
            outlined
            size="small"
            class="action-button"
            @click="clear"
          >
            <i class="pi pi-eraser"></i>
          </Button>
          <Button
            severity="secondary"
            outlined
            size="small"
            class="action-button"
            @click="undo"
          >
            <i class="pi pi-undo"></i>
          </Button>
        </div>
      </div>
      <small class="p-error" v-if="errorState.errorMessage">{{
        errorState.errorMessage
      }}</small>
    </div>
  </div>
</template>

<script lang="ts">
import { ref, computed, reactive, watch, onMounted } from "vue";
import { useHttpRequest } from "@/store/httpRequest.store";
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
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        sigOption: {
          penColor: { r: 0, g: 0, b: 2505 },
          backgroundColor: { r: 255, g: 255, b: 255 },
        },
        width: 400,
        height: 200,
        position: "center",
        required: false,
        disabled: false,
        hidden: false,
        tooltip: "",
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
  emits: ["update:modelValue", "update:options"],
  setup(props, { emit }) {
    const internalValue = ref(props.modelValue as any);
    const localOptions = reactive({ ...props.options });
    const isDisabled = computed(() => localOptions.disabled);
    const readOnly = computed(() => localOptions.readonly);
    const isHidden = computed(() => localOptions.hidden);
    const signatureCanvas = ref<InstanceType<any> | null>(null);
    const signatures = ref([] as any);
    const showEliseSignatures = ref(false);

    const getValue = () => {
      return internalValue.value;
    };

    const setValue = (value: any) => {
      if (!value) return;
      // Check if value already has data URL prefix
      const dataUrl = value.startsWith("data:")
        ? value
        : `data:image/jpeg;base64,${value}`;
      signatureCanvas.value.fromDataURL(dataUrl);
    };

    const updateOptions = (updates: Partial<OptionConfig>) => {
      Object.assign(localOptions, updates);
      emit("update:options", localOptions);
    };

    const disableField = () => updateOptions({ disabled: true });
    const enableField = () => updateOptions({ disabled: false });
    const hideField = () => updateOptions({ hidden: true });
    const showField = () => updateOptions({ hidden: false });

    const formattedSigOption = computed(() => {
      const { penColor, backgroundColor } = localOptions.sigOption;

      // Check if penColor and backgroundColor are defined
      const penColorString =
        penColor &&
        penColor.r !== undefined &&
        penColor.g !== undefined &&
        penColor.b !== undefined
          ? `rgb(${penColor.r}, ${penColor.g}, ${penColor.b})`
          : `rgb(0, 0, 0)`; // Default to black if not defined

      const backgroundColorString =
        backgroundColor &&
        backgroundColor.r !== undefined &&
        backgroundColor.g !== undefined &&
        backgroundColor.b !== undefined
          ? `rgb(${backgroundColor.r}, ${backgroundColor.g}, ${backgroundColor.b})`
          : `rgb(255, 255, 255)`; // Default to white if not defined

      return {
        penColor: penColorString,
        backgroundColor: backgroundColorString,
      };
    });

    watch(
      () => props.modelValue,
      (newValue) => {
        internalValue.value = newValue;
      }
    );

    watch(
      () => props.options.sigOption,
      (newSigOption) => {
        localOptions.sigOption = newSigOption;
      },
      { deep: true }
    );
    // Watcher for options changes
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true }
    );

    const save = (format: any) => {
      if (signatureCanvas.value) {
        const imageUrl = signatureCanvas.value.save(format);
        internalValue.value = imageUrl;
        emit("update:modelValue", imageUrl.split(";base64,")[1]);
      }
    };

    // Add debounced save for better mobile performance
    let saveTimeout: any = null;
    const debouncedSave = (format: any) => {
      if (saveTimeout) {
        clearTimeout(saveTimeout);
      }
      saveTimeout = setTimeout(() => {
        save(format);
      }, 100); // 100ms debounce
    };

    const onTouchMove = () => {
      // Save on touch move to ensure signature is captured during drawing on mobile
      debouncedSave("image/jpeg");
    };

    const clear = () => {
      signatureCanvas.value?.clear();
      internalValue.value = "";
      emit("update:modelValue", "");
    };

    const undo = () => {
      signatureCanvas.value?.undo();
      // Save after undo to update the model value
      setTimeout(() => {
        save("image/jpeg");
      }, 50);
    };

    const addWaterMark = () => {
      signatureCanvas.value?.addWaterMark({
        text: "Sample Watermark",
        font: "20px Arial",
        style: "all",
        fillStyle: "red",
        strokeStyle: "blue",
        x: 100,
        y: 200,
        sx: 100,
        sy: 200,
      });
    };

    const fromElise = async () => {
      showEliseSignatures.value = !showEliseSignatures.value;
    };

    const errorState = reactive({
      errorMessage: "",
    });

    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };

    const clearFieldError = () => {
      errorState.errorMessage = "";
    };
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

    // Fetch user and their signatures
    onMounted(async () => {});

    function toPx(value: string | number, axis: "w" | "h" = "w"): number {
      if (typeof value === "number") return value;
      if (typeof value === "string") {
        if (value.endsWith("px")) return parseInt(value, 10);
        if (value.endsWith("vw")) {
          const vw = parseFloat(value);
          return Math.round((window.innerWidth * vw) / 100);
        }
        if (value.endsWith("vh")) {
          const vh = parseFloat(value);
          return Math.round((window.innerHeight * vh) / 100);
        }
        if (value.endsWith("%")) {
          // Optional: handle % if you want, or fallback to a default
          return axis === "w" ? window.innerWidth : window.innerHeight;
        }
        // fallback: try to parse as number
        return parseInt(value, 10);
      }
      return 400; // fallback default
    }

    return {
      internalValue,
      localOptions,
      isDisabled,
      readOnly,
      isHidden,
      signatureCanvas,
      errorState,
      save,
      clear,
      computedRules,
      undo,
      addWaterMark,
      fromElise,
      setFieldError,
      clearFieldError,
      getValue,
      setValue,
      updateOptions,
      hideField,
      showField,
      disableField,
      enableField,
      formattedSigOption,
      signatures,
      showEliseSignatures,
      onTouchMove,
      toPx,
    };
  },
};
</script>

<style scoped>
.neoSignField {
  width: 100%;

  .label {
    display: flex;
    flex-direction: row;
    margin-bottom: 0.5rem;

    .label-container {
      min-width: 150px;
      align-items: center;
      display: flex;
      padding-bottom: 5px;
      padding-top: 5px;

      .required-asterisk {
        color: var(--p-primary-color);
        font-weight: 600;
      }

      .tooltip-icon {
        cursor: pointer;
        font-size: 12px;
        color: #6c757d;
        transition: color 0.2s ease-in-out;

        &:hover {
          color: var(--p-primary-color);
        }
      }
    }
  }

  .input-container {
    width: 100%;

    .signature-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;

      .signature-field-wrapper {
        position: relative;
        border-radius: 6px;
        overflow: hidden;
        box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1),
          0 1px 2px -1px rgba(0, 0, 0, 0.1);
        transition: all 0.2s ease-in-out;
        border: 1px solid #d1d5db;

        &:hover {
          border-color: var(--p-primary-color);
          box-shadow: 0 0 0 0.2rem var(--p-primary-color-20);
        }

        &:focus-within {
          border-color: var(--p-primary-color);
          box-shadow: 0 0 0 0.2rem var(--p-primary-color-20);
        }

        .signature-canvas {
          border: none !important;
          border-radius: 0 !important;
          width: 100% !important;
          display: block;
          background-color: #ffffff;
        }
      }

      .signature-actions {
        display: flex;
        justify-content: center;
        gap: 0.75rem;

        .action-button {
          border-radius: 6px;
          padding: 0.5rem 1rem;
          font-size: 14px;
          font-weight: 500;
          transition: all 0.2s ease-in-out;
          border: 1px solid #d1d5db;
          background: #ffffff;
          color: #374151;

          &:hover {
            border-color: var(--p-primary-color);
            background-color: #f8fafc;
            color: var(--p-primary-color);
            transform: translateY(-1px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.12);
          }

          &:focus {
            outline: none;
            border-color: var(--p-primary-color);
            box-shadow: 0 0 0 0.2rem var(--p-primary-color-20);
          }

          &:active {
            transform: translateY(0);
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.12);
          }

          i {
            font-size: 16px;
          }
        }
      }
    }
  }

  .p-error {
    color: var(--p-primary-color);
    font-size: 12px;
    margin-top: 0.25rem;
    font-weight: 400;
  }
}

.photo-preview-container {
  display: flex;
  gap: 10px;
  margin-top: 10px;
  flex-wrap: wrap;
}

.photo-preview {
  position: relative;
  width: 100px;
  height: 100px;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    transition: all 0.3s ease;

    &:hover {
      cursor: pointer;
      transform: scale(1.05);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.2);
    }
  }

  .ml-1 {
    position: absolute;
    top: 5px;
    right: 5px;
  }
}

.photo-preview-box {
  display: flex;
  overflow-x: auto;
  background-color: #f8fafc;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  padding: 1rem;
  margin-top: 0.75rem;
  width: 100%;
  max-height: 150px;
  transition: border-color 0.2s ease-in-out;

  &:hover {
    border-color: var(--p-primary-color);
  }

  @media (min-width: 450px) {
    max-width: 450px;
  }
}

.disabled-wrapper {
  pointer-events: none;
  opacity: 0.6;
  filter: grayscale(0.3);

  .signature-field-wrapper {
    background-color: #f9fafb;
    border-color: #e5e7eb !important;

    &:hover {
      border-color: #e5e7eb !important;
      box-shadow: none !important;
    }
  }
}

.readOnly-wrapper {
  pointer-events: none;

  .signature-field-wrapper {
    background-color: #f9fafb;
    border-color: #e5e7eb;

    &::after {
      content: "";
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: linear-gradient(
        45deg,
        transparent 40%,
        rgba(0, 0, 0, 0.05) 50%,
        transparent 60%
      );
      pointer-events: none;
    }
  }

  .signature-actions {
    display: none;
  }
}

/* Animation for button interactions */
@keyframes buttonPress {
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(0.98);
  }
  100% {
    transform: scale(1);
  }
}

.action-button:active {
  animation: buttonPress 0.1s ease-in-out;
}

/* Focus styles for accessibility */
*:focus-visible {
  outline: 2px solid var(--p-primary-color);
  outline-offset: 2px;
}
</style>
