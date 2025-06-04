<template>
  <div class="neodatepickerExternal" v-show="!isHidden">
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
      class="input-container"
      :style="{
        position: 'unset',
        height: isParentNeoTable ? '30px' : '60px',
        'min-width': isAbsolute && isParentNeoTable ? '130px' : 'unset',
        'max-height': isParentNeoTable ? '30px' : '60px',
        'max-width': isParentNeoTable ? maxWidth : 'unset',
        'margin-top': isAbsolute && isParentNeoTable ? '-15px' : 'unset',
      }"
    >
      <!-- :readonly="options.readonly" -->
      <vue3-datepicker
        ref="datepicker"
        input-class="customClass datepicker-class p-component"
        :format="formatValue"
        v-model="formatedDate"
        :disabled="isDisabled"
        :required="options.required"
        :hidden="isHidden"
        :typeable="isTypeable"
        class="customClass datepicker-class"
        @selected="onChange"
        :class="{ 'p-invalid': errorMessage != 'true' }"
        :disabled-dates="{
          from: maxDate,
          to: minDate,
          dates: disabledDates,
          ranges: rangeDate,
          days: days,
          daysOfMonth: daysOfMonth,
        }"
        @click="
          () => {
            isAbsolute = true;
          }
        "
        @closed="
          () => {
            isAbsolute = false;
          }
        "
        @focus="$emit('focus', $event)"
        @blur="$emit('blur', $event)"
        @mouseenter="$emit('mouseenter', $event)"
        @mouseleave="$emit('mouseleave', $event)"
        monday-first="true"
        :language="isRTL ? 'arTn' : 'fr'"
      />

      <small
        class="p-error"
        id="text-error"
        v-if="
          (errorMessage !== 'true' && errorMessage) || errorState.errorMessage
        "
      >
        {{
          errorMessage !== "true"
            ? errorMessage
            : errorState.errorMessage || "&nbsp;"
        }}
      </small>
    </div>
  </div>
</template>

<script lang="ts">
import { computed, onMounted, reactive, ref, watch } from "vue";
import { useMyRules, type MyRules } from "@/data/rules";
import { format, parse } from "date-fns";
import { useAppStore } from "@/store/app.store";
import { logger } from "@/api/api";
interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  Typeable: boolean | null;
  format: string;
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
      type: String || Date,
      default: "",
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "Test",
        format: "dd MMM yyyy",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        Typeable: true,
        rules: [],
        events: [],
        tooltip: "",
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    isRules: {
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
    "mouseenter",
    "mouseleave",
    "update:options",
  ],
  setup(props, { emit }) {
    onMounted(() => {
      maxWidth.value =
        (document.getElementsByClassName("neodatepicker")[0] as HTMLElement)
          ?.offsetWidth + "px";

      if (props.options.required) {
        const existingRequiredRule = props.options.rules?.find(
          (r: any) => r.code === "required"
        );
        if (existingRequiredRule) {
          existingRequiredRule.name = "Champ requis";
          existingRequiredRule.description =
            "Le champ en cours de validation doit avoir une valeur non vide (requis)";
          existingRequiredRule.expression = "required";
        } else {
          props.options.rules.push({
            code: "required",
            name: "Champ requis",
            description:
              "Le champ en cours de validation doit avoir une valeur non vide (requis)",
            params: [],
            expression: "required",
          });
        }
      }
    });
    const maxWidth = ref("");
    const datepicker = ref<any>(null);
    const internalValue = computed({
      get(): string {
        return props.modelValue;
      },
      set(value: string) {
        emit("update:modelValue", value);
      },
    });
    watch(
      () => props.modelValue,
      () => {
        errorHandled.value = false;
        // store.removeFormHasError(props.options.name);
      }
    );
    const formatValue = computed(() => {
      return props.options.format;
    });

    const formatedDate = computed({
      get() {
        if (
          !internalValue.value ||
          internalValue.value == "" ||
          internalValue.value == null ||
          internalValue.value == undefined
        )
          return ""; // Return empty string if value is null or undefined
        const date = new Date(internalValue.value);

        return format(date, props.options.format); // Format date using date-fns
      },
      set(value: any) {
        let dateString = value;

        // If the value is a Date object, convert it to a string first
        if (value instanceof Date) {
          dateString = format(value, props.options.format);
        } else if (typeof value !== "string") {
          console.error(
            "Expected a string or Date object for date parsing, but received:",
            typeof value
          );
          return;
        }

        try {
          const parsedDate = parse(
            dateString,
            props.options.format,
            new Date()
          ); // Parse the string or formatted date

          // Check if parsedDate is valid
          if (isNaN(parsedDate.getTime())) {
            throw new Error("Invalid date value");
          }

          // Format the parsed date correctly or convert it to ISO string if needed
          const date = props.isRules
            ? parsedDate
            : new Date(parsedDate.setDate(parsedDate.getDate() + 1))
                .toISOString()
                .split("T")[0];
          console.log("date", date);
          emit("update:modelValue", date); // Emit the formatted date
        } catch (error) {
          console.error("Error parsing the date:", error);
          logger.error(error);
        }
      },
    });

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const isTypeable = computed({
      get(): boolean {
        return localOptions.Typeable;
      },
      set(value: boolean) {
        localOptions.Typeable = value;
        emit("update:options", props.options);
      },
    });

    // Function to update field
    const setValue = (date: Date) => {
      if (typeof date === "string") {
        date = new Date(date);
      }
      internalValue.value = date.toISOString();
      emit("update:modelValue", date.toISOString());
    };

    const updateField = (date: Date | null) => {
      if (date == null) {
        internalValue.value = "";
        emit("update:modelValue", "");
        return;
      }
      if (typeof date === "string") {
        date = new Date(date);
      }
      internalValue.value = date.toISOString();
      emit("update:modelValue", date.toISOString());
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

    const onChange = async (event: any) => {
      internalValue.value = event;

      // Access the input element using the existing ref
      const datepickerInput = datepicker.value?.$el.querySelector(
        ".vuejs3-datepicker__inputvalue"
      ) as HTMLElement;

      if (datepickerInput) {
        datepickerInput.blur();
      }
    };
    const errorMessage = computed(() => {
      return validateField(internalValue.value);
    });
    const manipulateRequired = () => {
      if (props.options.required) {
        const existingRequiredRule = props.options.rules?.find(
          (r: any) => r.code === "required"
        );
        if (existingRequiredRule) {
          existingRequiredRule.name = "Champ requis";
          existingRequiredRule.description =
            "Le champ en cours de validation doit avoir une valeur non vide (requis)";
          existingRequiredRule.expression = "required";
        } else {
          props.options.rules.push({
            code: "required",
            name: "Champ requis",
            description:
              "Le champ en cours de validation doit avoir une valeur non vide (requis)",
            params: [],
            expression: "required",
          });
        }
      } else {
        const requiredRuleIndex = props.options.rules?.findIndex(
          (r: any) => r.code === "required"
        );
        if (requiredRuleIndex !== -1) {
          props.options.rules?.splice(requiredRuleIndex, 1);
        }
      }
    };
    const minDate = computed(() => {
      const existingMinDateRule = props.options.rules?.find(
        (r: any) => r.code === "dateAfter" || r.code === "dateAfterToday"
      );
      const existingMinDateBetweenRule = props.options.rules?.find(
        (r: any) => r.code === "dateBetween"
      );
      if (existingMinDateRule) {
        isTypeable.value = false;
        return existingMinDateRule.params[0] ?? new Date();
      } else if (existingMinDateBetweenRule) {
        isTypeable.value = false;
        return existingMinDateBetweenRule.params[0];
      }
    });
    const maxDate = computed(() => {
      const existingMaxDateRule = props.options.rules?.find(
        (r: any) => r.code === "dateBefore" || r.code === "dateBeforeToday"
      );
      const existingMaxDateBetweenRule = props.options.rules?.find(
        (r: any) => r.code === "dateBetween"
      );
      if (existingMaxDateRule) {
        isTypeable.value = false;
        return existingMaxDateRule.params[0] ?? new Date();
      } else if (existingMaxDateBetweenRule) {
        isTypeable.value = false;
        return existingMaxDateBetweenRule.params[1];
      }
    });
    const disabledDates = computed(() => {
      const existingDateRule = props.options.rules?.find(
        (r: any) => r.code === "dateIsNot"
      );
      if (existingDateRule) {
        isTypeable.value = false;
        return existingDateRule.params;
      }
    });
    const rangeDate = computed(() => {
      const existingDateRule = props.options.rules?.find(
        (r: any) => r.code === "disabledDateRange"
      );
      if (existingDateRule) {
        isTypeable.value = false;

        return [
          {
            from: existingDateRule.params[0],
            to: existingDateRule.params[1],
          },
        ];
      }
    });
    function validateField(value: any) {
      manipulateRequired();
      let returnMessage = "true";
      props.options.rules?.forEach((rule: any) => {
        const rl = rule.code as keyof MyRules;
        const params = rule.params;
        const ruleFunction = useMyRules[rl];
        if (ruleFunction) {
          returnMessage = ruleFunction(value, params);
        }
      });
      return returnMessage;
    }
    const days = computed(() => {
      const existingDateRule = props.options.rules?.find(
        (r: any) => r.code === "disabledWeekDays"
      );
      if (existingDateRule) {
        isTypeable.value = false;

        return existingDateRule.params;
      }
    });
    const daysOfMonth = computed(() => {
      const existingDateRule = props.options.rules?.find(
        (r: any) => r.code === "disabledMonthDays"
      );
      if (existingDateRule) {
        isTypeable.value = false;
        return existingDateRule.params;
      }
    });
    const isAbsolute = ref(false);
    const store = useAppStore();
    const errorHandled = ref(false);

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
    watch(
      () => errorMessage.value,
      (newValue, oldValue) => {
        // handleErrorMessage(newValue);
      }
    );
    const handleErrorMessage = (errorMessage: string) => {
      if (errorMessage == "true" && !errorHandled.value) {
        // store.addFormHasError(props.options.name);
        errorHandled.value = true;
        return true;
      } else if (errorMessage == "false" && errorHandled.value) {
        // store.removeFormHasError(props.options.name);
        return false;
      }
    };

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
      // Variables
      days,
      isDisabled,
      isHidden,
      minDate,
      maxDate,
      maxWidth,
      isAbsolute,
      daysOfMonth,
      internalValue,
      formatValue,
      errorMessage,
      formatedDate,
      disabledDates,
      rangeDate,
      isTypeable,
      computedRules,
      datepicker,
      // Functions
      onChange,
      hideField,
      showField,
      enableField,
      disableField,
      errorState,
      getValue,
      setValue,
      updateField,
      handleErrorMessage,
      setFieldError,
      clearFieldError,
      updateOptions,
    };
  },
};
</script>

<style lang="scss">
@import "@/scss/variables";

$label-container-min-width: 150px;
$text-input-height: 30px;
$write-mode-input-height: 28px;
$color-invalid-input: #faa19b;
$color-datepicker-calendar: #4bd;

.neodatepickerExternal {
  width: 100%;
  .label {
    display: flex;
    flex: 1;
    flex-direction: row;
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
    height: 60px;
    max-height: 60px;
    position: relative !important;
    // position: absolute;
    .datepicker-class {
      width: 100%;
      background-color: #ffffff;
      border-radius: 4px;
      border: 1px solid #eaecee;
      height: 36px;
      .vuejs3-datepicker__calendar {
        top: 110%;
        position: absolute !important;
        background-color: #ffffff;
        border-radius: 5px;
        border: 1px solid $color-blue-dark;
        z-index: 9999 !important;
      }
      .vuejs3-datepicker__calendar-topbar {
        display: none;
      }
      .vuejs3-datepicker__inputvalue {
        width: 100%;
        border-radius: 4px;
        border: 1px solid #cbd5e1;
        border-style: solid !important;
        background-color: #ffffff;
        color: $color-grey-tundora;
        height: 36px;
        min-height: 36px;
        outline: none;
        min-width: 130px !important;
      }
      .vuejs3-datepicker__typeablecalendar {
        position: absolute;
        top: 8px;
        left: 10px;
      }
      .vuejs3-datepicker__inputvalue:focus {
        border: 1px solid $color-blue-dark !important;
      }
      input:focus {
        border: 1px solid $color-blue-dark !important;
      }
      input:hover {
        border: 1px solid $color-blue-jungle-mist;
      }
      .vuejs3-datepicker__calendar .cell {
        &.selected {
          background: $color-datepicker-calendar !important;
          color: unset !important;
        }

        &:not(.blank):not(.disabled):hover {
          border: 1px solid $color-datepicker-calendar !important;
        }
      }
    }
  }
  .vuejs3-datepicker__value {
    width: 100%;
    border-radius: 4px;
    border: 1px solid #cbd5e1;
    border-style: solid !important;
    background-color: #ffffff;
    color: $color-grey-tundora;
    height: 36px;
    min-height: 36px;
    outline: none;
    display: flex;
    align-items: center;
  }
  .error-container {
    width: 100%;
  }
}
.neodatepicker .input-container .datepicker-class:disabled {
  border: 1px solid #d9dde0 !important;
  background-color: white !important;
  color: #959fa7 !important;
  cursor: not-allowed;
  box-shadow: none !important;
  outline: none !important;
}
.vuejs3-datepicker__calendar .flex-rtl {
  display: block !important;
}
</style>
