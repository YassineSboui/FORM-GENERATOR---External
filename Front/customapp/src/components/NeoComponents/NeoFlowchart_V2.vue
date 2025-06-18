<template>
  <div class="neoAutoComplete" v-show="!isHidden" :dir="isRTL ? 'rtl' : 'ltr'">
    <div class="label" v-if="!isParentNeoTable">
      <label class="label-container">
        {{
          language === "FR"
            ? label
            : language === "AR"
            ? options.label_AR
            : language === "ENG"
            ? options.label_ENG
            : label
        }}
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
        >
        </i>
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
        <!-- append-to="self" -->
        <AutoComplete
          v-model="internalValue"
          :disabled="isDisabled"
          :suggestions="items"
          :minLength="options.minLength"
          class="w-full neoAutoCompleteC"
          :class="{
            'rtl-loader': isRTL,
            'p-invalid': errorMessage || errorState.errorMessage,
          }"
          @complete="search(false)"
          @item-select="select"
          optionLabel="name"
          data-key="id"
          forceSelection
          :invalid="isInvalid"
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
        >
          <template #option="slotProps">
            <div class="flex items-center">
              <div>{{ slotProps.option.name }}</div>
            </div>
          </template>
        </AutoComplete>
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
import { computed, onMounted, reactive, watch } from "vue";
import { searchFlowChart } from "@/api/api";
import { ref } from "vue";
import { useHttpRequest } from "@/store/httpRequest.store";
import { useAppStore } from "@/store/app.store";
import app from "@/main";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  defaultValue: "";
  relatedToElise: boolean;
  type: string;
  name: string;
  label: string;
  required: boolean;
  readonly: boolean;
  disabled: boolean;
  hidden: boolean;
  itemType: string;
  searchType: string;
  tooltip: "";
  fullService: boolean;
  rules: { expression: string }[];
  events: any[];
}

export default {
  props: {
    items: {
      type: Array,
      // required: true,
    },
    label: {
      type: String,
      required: false,
    },
    // type: Object,
    modelValue: {
      // type: Object,
      required: true,
    },
    returnObject: {
      type: Boolean,
      default: false,
    },
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "CF_FLOWCHART",
        label: "FlowChart",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        elements: [],
        rules: [],
        events: [],
        relatedToElise: false,
        type: "FLOWCHART",
        itemType: "User",
        searchType: "Name",
        tooltip: "",
        fullService: false,
      }),
    },
    isParentNeoTable: {
      type: Boolean,
      default: false,
    },
    forService: {
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
    "update:options",
    "itemSelected",
    "focus",
    "blur",
    "mouseenter",
    "mouseleave",
    "errorMessage",
    "searchItem",
  ],
  setup(props, { emit }) {
    const items = ref<any[]>([]);
    const isInvalid = ref(false);
    const store = useHttpRequest();
    const appStore = useAppStore();
    const loading = ref(true);

    const internalValue = computed({
      get() {
        return props.modelValue as string | any;
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

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

    function makeInvalid(value: boolean) {
      isInvalid.value = value;
    }
    const search = async (strict: boolean) => {
      console.log("internalValue", internalValue.value);
      console.log("typeof internalValue.value", typeof internalValue.value);
      if (typeof internalValue.value === "string") {
        const payload = {
          searchTerm: strict
            ? internalValue.value
            : internalValue.value.replace(/&/g, " ") + "*",
          fullService: props.options.fullService,
          ignoredElements: [],
          itemType: props.options.itemType,
          searchType: props.options.searchType,
          ldapAttribute: "",
        };
        items.value = await searchFlowChart(payload);
      }
    };

    const select = () => {
      const selectedEvent = props.options.events.find(
        (event: any) => event.rule.code === "select"
      );
      if (selectedEvent) {
        emit("itemSelected", selectedEvent.code);
      }
    };

    function manageProperties(opt: any) {
      if (opt) {
        updateOptions(opt);
      }
    }

    watch(
      () => store.loading,
      async (newValue, oldValue) => {
        console.log("newValue", newValue);
        console.log("oldValue", oldValue);
        if (newValue === false) {
          loading.value = true;
          await search(true);
          console.log("items", items.value);
          if (items.value.length > 0) {
            internalValue.value = items.value[0];
          }
          loading.value = false;
        }
      }
    );

    watch(
      () => props.modelValue,
      async (newValue) => {
        if (typeof newValue === "string" && newValue.startsWith("LEXICON")) {
          console.log("updating internalValue");
          internalValue.value = await appStore.getObjectByLexicon(newValue);
        }
      }
    );

    return {
      loading,
      search,
      select,
      isDisabled,
      isHidden,
      internalValue,
      items,
      isInvalid,
      computedRules,
      errorState,
      disableField,
      enableField,
      hideField,
      showField,
      setFieldError,
      getValue,
      setValue,
      clearFieldError,
      makeInvalid,
      updateField,
      manageProperties,
    };
  },
};
</script>
