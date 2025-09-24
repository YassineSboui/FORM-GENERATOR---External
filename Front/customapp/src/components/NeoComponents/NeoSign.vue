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
        <span
          v-show="options.required"
          style="color: red; margin-left: 5px; margin-right: 5px"
          >*</span
        >
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
        class="flex gap-2"
        :class="{
          'disabled-wrapper': isDisabled,
          'readOnly-wrapper': readOnly,
        }"
        style="max-width: 100%"
      >
        <div>
          <Vue3Signature
            :w="toPx(options.width, 'w') + 'px'"
            :h="toPx(options.height, 'h') + 'px'"
            ref="signatureCanvas"
            :sigOption="formattedSigOption"
            :disabled="isDisabled"
            class="signature-canvas"
            @mouseup="save('image/jpeg')"
            @touchend="save('image/jpeg')"
          />
        </div>
        <div v-if="!readOnly" class="pt-4">
          <div class="col-12">
            <Button style="margin-left: 10px; margin-top: 20px" @click="clear"
              ><i class="pi pi-eraser"></i
            ></Button>
          </div>
          <div class="col-12">
            <Button style="margin-left: 10px; margin-top: 5px" @click="undo"
              ><i class="pi pi-undo"></i
            ></Button>
          </div>
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
      signatureCanvas.value.fromDataURL(value);
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

    const clear = () => {
      signatureCanvas.value?.clear();
    };

    const undo = () => {
      signatureCanvas.value?.undo();
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
      toPx,
    };
  },
};

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
</script>

<style scoped>
.neoSignField {
  width: 100%;
  .label {
    display: flex;
    flex-direction: row;
    .label-container {
      color: #165c77;
      min-width: 150px;
      align-items: center;
      padding-bottom: 5px;
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
    }
  }
  .input-container {
    width: 100%;
    .signature-canvas {
      border: 1px solid #ccc;
      border-radius: 4px;
      width: 100% !important;
    }
  }
  .button-container {
    margin-top: 10px;
  }
}
.photo-preview-container {
  display: flex;
  gap: 10px;
  margin-top: 10px;
  flex-wrap: wrap;
}
.photo-preview {
  position: relative; /* For positioning the remove button */
  margin-right: 10px; /* Space between photos */
}
.photo-preview {
  position: relative;
  width: 100px;
  height: 100px;
}
.photo-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}
/* Make the cursor a pointer on hover */
.photo-preview img:hover {
  cursor: pointer; /* Change cursor to pointer on hover */
  transform: scale(1.05); /* Slightly enlarge the image on hover */
}

.photo-preview .ml-1 {
  position: absolute;
  top: 5px;
  right: 5px;
}
.photo-preview-box {
  display: flex; /* Align images in a row */
  overflow-x: auto; /* Enable horizontal scrolling */
  background-color: #f8f8f8; /* Light background color */
  border: 2px solid #165c77; /* Border color */
  border-radius: 10px; /* Rounded corners */
  padding: 10px; /* Padding inside the box */
  margin-top: 10px; /* Space above the preview box */
  width: 100%; /* Full width of the dialog */
  max-height: 150px; /* Set a max height for the preview box */
}

/* Media query for screens wider than 450px */
@media (min-width: 450px) {
  .photo-preview-box {
    max-width: 450px; /* Set max width if screen is wider than 450px */
  }
}
.disabled-wrapper {
  pointer-events: none; /* Disable all interactions */
  opacity: 0.5; /* Optional: Make it look visually disabled */
}
.readOnly-wrapper {
  pointer-events: none; /* Disable all interactions */
}
</style>
