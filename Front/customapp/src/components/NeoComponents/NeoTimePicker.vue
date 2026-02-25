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
        :time-only="true"
        :hour-format="'24'"
        :update-model-type="'string'"
        @update:model-value="onChange($event)"
        :class="{ 'p-invalid': errorMessage || errorState.errorMessage }"
        aria-describedby="text-error"
        :min-date="minDate"
        :max-date="maxDate"
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
import { computed, reactive, watch, ref } from "vue";
import { useMyRules, type MyRules } from "@/data/rules";
import { useI18n } from "vue-i18n";
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
    const { t } = useI18n(); // <-- Add this line
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
        (r: any) => r.code === "required",
      );
      const hasRequiredRule = requiredRuleIndex !== -1;

      if (props.options.required) {
        // Si la règle existe, on la met à jour
        if (hasRequiredRule) {
          const existingRule = props.options.rules[requiredRuleIndex];
          existingRule.name = t("rules.requiredName");
          existingRule.description = t("rules.requiredDescription");
          existingRule.expression = "required";
        }
        // Sinon, on l'ajoute
        else {
          props.options.rules.push({
            code: "required",
            name: t("rules.requiredName"),
            description: t("rules.requiredDescription"),
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
      const existingMinDateRule = props.options.rules.find(
        (r: any) => r.code === "timeAfter",
      );
      if (existingMinDateRule) {
        return existingMinDateRule.params[0];
      }
    });
    const maxDate = computed(() => {
      const existingMinDateRule = props.options.rules.find(
        (r: any) => r.code === "timeBefore",
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
        const rules = useMyRules;
        const ruleFunction = rules[rl];
        if (ruleFunction) {
          returnMessage = ruleFunction(value, params);
        }
      });
      return returnMessage;
    }
    const onChange = async (value: any) => {
      internalValue.value = value;
    };
    // Initialize internalValue with prop value
    const initialValue = props.modelValue;
    const internalValue = ref(initialValue) as any;

    // Sync internalValue with modelValue prop
    watch(
      () => props.modelValue,
      (newValue) => {
        internalValue.value = newValue;
      },
    );

    // When internalValue changes, emit update
    watch(internalValue, (newValue) => {
      emit("update:modelValue", newValue);
    });

    // Update setValue
    const setValue = (value: any) => {
      internalValue.value = value;
      emit("update:modelValue", value);
    };
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });
    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    // Function to update field
    const updateField = (value: any) => {
      internalValue.value = value;
      emit("update:modelValue", value);
    };

    // Function to get current value
    const getValue = () => {
      const modelValue = props.modelValue as any; // Use your type as necessary

      return modelValue; // Return the modelValue directly
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

    // Watcher for options changes
    watch(
      () => props.options,
      (newOptions) => {
        Object.assign(localOptions, newOptions);
      },
      { deep: true },
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
          // If required, not empty
          if (newValue && newValue.trim() !== "") {
            clearFieldError();
          }
        }
      },
    );

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
      manipulateRequired,
      t, // Expose the translation function
    };
  },
};
</script>
