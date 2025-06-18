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
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <!-- :readonly="options.readonly" -->
      <Field
        v-model="internalValue"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
      >
        <DatePicker
          v-model="formatedDate"
          :date-format="formatValue"
          :disabled="isDisabled"
          :readonly="options.readonly"
          :required="options.required"
          :minDate="minDate"
          :maxDate="maxDate"
          :disabledDates="disabledDates"
          :manualInput="isTypeable"
          :inputClass="['customClass', 'datepicker-class']"
          @change="onChange"
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
          :locale="isRTL ? 'ar' : 'fr'"
          style="margin-bottom: 8px"
          showIcon
          fluid
          iconDisplay="input"
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
      </Field>
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

    const toPrimeVueFormat = (format: string): string => {
      console.log("format", format);
      return (
        format
          // Year
          .replace(/yyyy/g, "yy")
          .replace(/yy/g, "y")
          // Month
          .replace(/MMM/g, "M")
          .replace(/MM/g, "mm")
          // Day
          .replace(/dd/g, "dd")
      );
    };

    const formatValue = computed(() => {
      return toPrimeVueFormat(props.options.format);
    });

    const formatedDate = computed({
      get() {
        if (
          !internalValue.value ||
          internalValue.value === "" ||
          internalValue.value === null ||
          internalValue.value === undefined
        ) {
          return null; // Return null for empty value (prevents NaN)
        }
        const date = new Date(internalValue.value);
        if (isNaN(date.getTime())) {
          return null; // Return null for invalid date (prevents NaN)
        }
        return date; // Return Date object for valid value
      },
      set(value: any) {
        let dateString = value;
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
          );
          if (isNaN(parsedDate.getTime())) {
            throw new Error("Invalid date value");
          }
          const date = props.isRules
            ? parsedDate
            : new Date(parsedDate.setDate(parsedDate.getDate() + 1))
                .toISOString()
                .split("T")[0];
          emit("update:modelValue", date);
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
      // Initialisation défensive
      if (!props.options.rules) {
        props.options.rules = [];
      }

      // Recherche de la règle 'required'
      const requiredRuleIndex = props.options.rules.findIndex(
        (r: any) => r.code === "required"
      );
      const hasRequiredRule = requiredRuleIndex !== -1;

      if (props.options.required) {
        // Si la règle existe, on la met à jour
        if (hasRequiredRule) {
          const existingRule = props.options.rules[requiredRuleIndex];
          existingRule.name = "Champ requis";
          existingRule.description =
            "Le champ en cours de validation doit avoir une valeur non vide (requis)";
          existingRule.expression = "required";
        }
        // Sinon, on l'ajoute
        else {
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
        // Si le champ n'est plus requis, on retire la règle
        if (hasRequiredRule) {
          props.options.rules.splice(requiredRuleIndex, 1);
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
</style>
