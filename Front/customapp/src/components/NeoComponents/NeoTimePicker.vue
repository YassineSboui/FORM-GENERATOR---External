<template>
  <div class="neoTimePicker" v-show="!isHidden">
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
        height: isParentNeoTable ? '30px' : '60px',
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <!-- rest of the template -->
      <DatePicker
        v-model="internalValue"
        :disabled="isDisabled"
        id="calendar-timeonly"
        timeOnly
        @change="onChange($event)"
        :class="{ 'p-invalid': errorMessage != 'true' }"
        aria-describedby="text-error"
        :minDate="minDate"
        :maxDate="maxDate"
        @focus="$emit('focus', $event)"
        @blur="$emit('blur', $event)"
        @mouseenter="$emit('mouseenter', $event)"
        @mouseleave="$emit('mouseleave', $event)"
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
import { computed, reactive, watch } from "vue";
import { useMyRules, type MyRules } from "@/data/rules";
import { format } from "date-fns";
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
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        rules: [],
        events: [],
        tooltip: "",
      }),
    },
    isParentNeoTable: {
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
    const errorMessage = computed(() => {
      return validateField(internalValue.value);
    });
    const manipulateRequired = () => {
      if (props.options.required) {
        const existingRequiredRule = props.options.rules.find(
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
        const requiredRuleIndex = props.options.rules.findIndex(
          (r: any) => r.code === "required"
        );
        if (requiredRuleIndex !== -1) {
          props.options.rules.splice(requiredRuleIndex, 1);
        }
      }
    };
    const minDate = computed(() => {
      const existingMinDateRule = props.options.rules.find(
        (r: any) => r.code === "timeAfter"
      );
      if (existingMinDateRule) {
        return existingMinDateRule.params[0];
      }
    });
    const maxDate = computed(() => {
      const existingMinDateRule = props.options.rules.find(
        (r: any) => r.code === "timeBefore"
      );
      if (existingMinDateRule) {
        return existingMinDateRule.params[0];
      }
    });
    function validateField(value: any) {
      manipulateRequired();
      let returnMessage = "true";
      props.options.rules.forEach((rule: any) => {
        const rl = rule.code as keyof MyRules;
        const params = rule.params;
        const ruleFunction = useMyRules[rl];
        if (ruleFunction) {
          returnMessage = ruleFunction(value, params);
        }
      });
      return returnMessage;
    }
    const onChange = async (event: any) => {
      internalValue.value = event.target.value;
    };
    const internalValue = computed({
      get: () => {
        if (
          !internalValue.value ||
          internalValue.value == "" ||
          internalValue.value == null ||
          internalValue.value == undefined
        )
          return ""; // Return empty string if value is null or undefined
        return props.modelValue as any;
      },
      set: (value) => {
        emit("update:modelValue", value);
      },
    });

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    // Function to update field
    function setValue(value: any) {
      let date: Date;

      // Check if the value is a string in HH:mm format
      if (typeof value === "string" && value.includes(":")) {
        const [hours, minutes] = value.split(":").map(Number);

        // Check if hours and minutes are valid numbers
        if (!isNaN(hours) && !isNaN(minutes)) {
          date = new Date(); // Create a new Date object for the current date
          date.setHours(hours); // Set hours
          date.setMinutes(minutes); // Set minutes
        } else {
          console.error("Invalid time format provided:", value); // Handle invalid time
          return; // Exit if invalid
        }
      } else {
        const dateStr = typeof value === "string" ? value : value.toISOString(); // Handle potential type mismatch
        date = new Date(dateStr);

        // Check for valid date
        if (isNaN(date.getTime())) {
          console.error("Invalid date provided:", value); // Handle invalid date
          return; // Exit if invalid
        }
      }

      // Emit the updated Date object and set the internal value
      internalValue.value = date; // Set internal value to Date object
      emit("update:modelValue", date); // Emit the updated Date object
    }

    function updateField(value: any) {
      const dateStr = typeof value === "string" ? value : value.toISOString(); // Handle potential type mismatch
      const date = new Date(dateStr);

      if (!isNaN(date.getTime())) {
        // Check for valid date
        date.setHours(date.getHours()); // Access hours correctly (remove accidental extra parentheses)
        date.setMinutes(date.getMinutes());
        internalValue.value = date.toLocaleTimeString(); // Consider using a more robust format
        emit("update:modelValue", date.toLocaleTimeString());
      } else {
        console.error("Invalid date provided:", value); // Handle invalid date
      }
    }

    // Function to get current value
    const getValue = () => {
      const modelValue = props.modelValue as any; // Use your type as necessary

      // If modelValue is a Date object, return formatted time
      if (modelValue instanceof Date) {
        return format(modelValue, "HH:mm"); // Format Date to HH:mm
      }
      // If modelValue is a string and valid time, return it as is
      else if (typeof modelValue === "string" && modelValue.includes(":")) {
        return modelValue; // Return the string time directly
      }

      return ""; // Return an empty string for invalid cases
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

    return {
      isDisabled,
      isHidden,
      internalValue,
      computedRules,
      errorMessage,
      maxDate,
      minDate,
      disableField,
      enableField,
      hideField,
      showField,
      updateField,
      validateField,
      onChange,
      setValue,
      getValue,
      setFieldError,
      clearFieldError,
      errorState,
      updateOptions,
    };
  },
};
</script>
