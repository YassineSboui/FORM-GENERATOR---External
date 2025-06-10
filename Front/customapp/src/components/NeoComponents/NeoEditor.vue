<template>
  <div class="neoeditor" v-show="!isHidden" :style="neoEditorStyle">
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
        ></i
      ></label>
    </div>
    <div
      :class="{
        'rtl-editor input-container': isRTL,
        'ltr-editor input-container': !isRTL,
        'disabled-wrapper': isDisabled,
      }"
    >
      <Editor
        class="editor"
        v-model="internalValue"
        :readonly="isDisabled || options.readonly"
        :style="editorStyle"
      >
        <template #toolbar>
          <span class="ql-formats">
            <select class="ql-size">
              <option value="small"></option>
              <option selected></option>
              <option value="large"></option>
            </select>
            <select class="ql-header">
              <option value="1"></option>
              <option value="2"></option>
              <option value="3"></option>
              <option selected></option>
            </select>
            <select class="ql-font">
              <option value="serif"></option>
              <option value="monospace"></option>
            </select>
            <button class="ql-bold"></button>
            <button class="ql-italic"></button>
            <button class="ql-underline"></button>
            <button class="ql-strike"></button>
            <button class="ql-blockquote"></button>
            <button class="ql-code-block"></button>
            <button class="ql-list" value="ordered"></button>
            <button class="ql-list" value="bullet"></button>
            <button class="ql-script" value="sub"></button>
            <button class="ql-script" value="super"></button>
            <button class="ql-indent" value="-1"></button>
            <button class="ql-indent" value="+1"></button>
            <button class="ql-direction"></button>
            <select class="ql-color">
              <!-- Grey -->
              <option value="#101316"></option>
              <option value="#414E59"></option>
              <option value="#73808B"></option>
              <option value="#959FA7"></option>
              <!-- Brand -->
              <option value="#064252"></option>
              <option value="#0A6E89"></option>
              <option value="#398AA0"></option>
              <option value="#68A7B8"></option>
              <!-- Purple -->
              <option value="#6A1840"></option>
              <option value="#8E2055"></option>
              <option value="#C05086"></option>
              <option value="#CF78A2"></option>
              <!-- Red -->
              <option value="#941222"></option>
              <option value="#E71D36"></option>
              <option value="#EC475C"></option>
              <option value="#F07282"></option>
              <!-- Yellow -->
              <option value="#947900"></option>
              <option value="#F6C900"></option>
              <option value="#F8D32F"></option>
              <option value="#FADD5E"></option>
              <!-- Green -->
              <option value="#4F6F11"></option>
              <option value="#84B91C"></option>
              <option value="#9BC646"></option>
              <option value="#B2D46F"></option>
            </select>
            <select class="ql-background">
              <!-- Grey -->
              <option value="#101316"></option>
              <option value="#414E59"></option>
              <option value="#73808B"></option>
              <option value="#959FA7"></option>
              <!-- Brand -->
              <option value="#064252"></option>
              <option value="#0A6E89"></option>
              <option value="#398AA0"></option>
              <option value="#68A7B8"></option>
              <!-- Purple -->
              <option value="#6A1840"></option>
              <option value="#8E2055"></option>
              <option value="#C05086"></option>
              <option value="#CF78A2"></option>
              <!-- Red -->
              <option value="#941222"></option>
              <option value="#E71D36"></option>
              <option value="#EC475C"></option>
              <option value="#F07282"></option>
              <!-- Yellow -->
              <option value="#947900"></option>
              <option value="#F6C900"></option>
              <option value="#F8D32F"></option>
              <option value="#FADD5E"></option>
              <!-- Green -->
              <option value="#4F6F11"></option>
              <option value="#84B91C"></option>
              <option value="#9BC646"></option>
              <option value="#B2D46F"></option>
            </select>
            <select class="ql-align">
              <option value="justify"></option>
              <option value="center"></option>
              <option value="right"></option>
            </select>
            <button class="ql-clean"></button>
            <button class="ql-link"></button>
            <button class="ql-image"></button>
          </span>
        </template>
      </Editor>
      <small class="p-error" id="text-error" v-if="errorState.errorMessage">
        {{ errorState.errorMessage || "&nbsp;" }}
      </small>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, defineComponent, reactive, ref, watch } from "vue";

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
  height: string | null;
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
        label: "Test",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        height: null,
        rules: [],
        events: [],
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
    height: {
      type: String,
      default: "150px",
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
  },
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

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    const editorStyle = computed(() => {
      const height = props.options.height || props.height;
      return `height: ${height}; font-size: 14.7px; line-height: 18.2px; font-weight: 400; font-family: Trebuchet MS`;
    });
    const neoEditorStyle = computed(() => {
      const height = props.options.height || props.height;
      // If height is set, use calc to add 50px, otherwise fallback to 200px
      return {
        height: height ? `calc(${height} + 75px)` : "200px",
      };
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
    return {
      isDisabled,
      isHidden,
      internalValue,
      errorState,
      editorStyle,
      computedRules,
      neoEditorStyle,
      setValue,
      getValue,
      disableField,
      enableField,
      hideField,
      showField,
      updateField,
      setFieldError,
      clearFieldError,
    };
  },
});
</script>
<style scoped></style>
