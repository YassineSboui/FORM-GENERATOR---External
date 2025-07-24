<template>
  <div class="neoAutoComplete" v-show="!isHidden" :dir="isRTL ? 'rtl' : 'ltr'">
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
        ></i>
      </label>
    </div>
    <div
      class="input-container"
      :style="{
        height: isParentNeoTable ? '30px' : '60px',
        'max-height': isParentNeoTable ? '30px' : '60px',
      }"
    >
      <!-- rest of the template -->
      <Field
        v-model="contact"
        :name="options.label"
        :rules="computedRules"
        v-slot="{ field, errorMessage }"
      >
        <AutoComplete
          v-model="contact"
          :disabled="isDisabled"
          :suggestions="items"
          :minLength="options.minLength"
          class="w-full neoAutoCompleteC"
          :class="{
            'rtl-loader': isRTL,
            'p-invalid': errorMessage || errorState.errorMessage,
          }"
          @complete="search"
          @item-select="select"
          :optionLabel="options.optionLabel"
          :invalid="isInvalid"
          @focus="$emit('focus', $event)"
          @blur="$emit('blur', $event)"
          @mouseenter="$emit('mouseenter', $event)"
          @mouseleave="$emit('mouseleave', $event)"
        ></AutoComplete>
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
import { eliseGetContacts } from "@/api/api";
import { useAppStore } from "@/store/app.store";
import _ from "lodash";
import { computed, type Ref, reactive, watch } from "vue";
import { ref } from "vue";

interface OptionConfig {
  label_AR: string;
  label_ENG: string;
  name: string;
  label: string;
  tooltip: string;
  options: string;
  addressBook: string;
  limit: number;
  isOrganization: boolean;
  isPerson: boolean;
  selectedSource: string;
  elements: { code: string; name: string }[];
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
    label: {
      type: String,
      required: false,
    },
    // type: Object,
    modelValue: {
      type: [String, Object],
      default: () => "",
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
        name: "",
        defaultValue: "'c'",
        label: "Test",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        options: "ALL",
        addressBook: "DEFAULT_ADDRESS_BOOK",
        limit: 5,
        isOrganization: true,
        isPerson: true,
        selectedSource: "",
        elements: [{ code: "", name: "" }],
        rules: [],
        events: [],
        description: "",
      }),
    },
    isParentNeoTable: {
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
    "update:options",
    "update:modelValue",
    "errorMessage",
    "itemSelected",
    "searchItem",
    "focus",
    "blur",
    "mouseleave",
    "mouseenter",
  ],
  setup(props, { emit }) {
    // const storeComponent2 = getComponentStore(props.options.name)();
    // const { modelValue, isDisabled, isHidden, items, events } =
    //   storeToRefs(storeComponent);
    // const {
    //   activateField,
    //   deactivateField,
    //   showField,
    //   hideField,
    //   updateItems,
    // } = storeComponent;

    const items: Ref<EliseContactSearch[]> = ref([]);
    //const contact: Ref<any | string> = ref("");
    const store = useAppStore();
    const contact = computed({
      get() {
        return props.modelValue as string | EliseContactSearch | any; //JSON.parse(props.modelValue);
      },
      set(newValue): void {
        emit("update:modelValue", newValue);
      },
    });

    // Function to get current value
    const getValue = () => {
      return contact.value[props.options.optionLabel];
    };

    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });

    // Computed properties for disabled and hidden states
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);
    const description = computed(() => {
      return props.options.description;
    });

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
    const isInvalid = ref(false);

    function makeInvalid(value: boolean) {
      isInvalid.value = value;
    }

    function updateField(value: EliseContactSearch | any) {
      contact.value = value;
      setContactFields(value);
    }
    function updateItems(value: any) {
      items.value = value;
    }

    function clearItem() {
      contact.value = "";
    }
    const search = async () => {
      if (typeof contact.value === "string") {
        const payload = {
          searchFilter: contact.value.replace(/&/g, " ") + "*",
          isPerson: props.options.isPerson,
          isOrganization: props.options.isOrganization,
          limit: props.options.limit,
          addressBook: props.options.addressBook,
          options: props.options.options,
        };
        items.value = await eliseGetContacts(payload);
      }
    };
    function setContactFields(value: EliseContactSearch | any) {
      for (let i = 0; i < props.options.contactPersoField.length; i++) {
        const element = props.options.contactPersoField[i];
        if (element.active) {
          // test if the value is not undefined
          if (_.get(value, element.path) !== undefined) {
            store.Fields[element.value] = _.get(value, element.path) || "";
          } else {
            store.Fields[element.value] = "";
          }

          // store.Fields[element.value] = _.get(value, element.path) || "";
        }
      }
      for (let i = 0; i < props.options.contactOrganField.length; i++) {
        const element = props.options.contactOrganField[i];
        if (element.active) {
          //   // test if the value is not undefined
          if (_.get(value, element.path) !== undefined) {
            store.Fields[element.value] = _.get(value, element.path) || "";
          } else {
            store.Fields[element.value] = "";
          }
        }
        // store.Fields[element.value] = _.get(value, element.path) || "";
      }
    }
    const select = ({ value }: { value: EliseContactSearch }) => {
      setContactFields(value);
      const selectedEvent = props.options.events.find(
        (event: any) => event.rule.code === "select"
      );
      if (selectedEvent) {
        emit("itemSelected", selectedEvent.code);
      }
    };

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

    // Watch for field validity and clear error if valid
    watch(
      () => contact.value,
      (newValue) => {
        // If there is an error and the value is now valid, clear the error
        if (errorState.errorMessage) {
          // If required, not empty
          if (newValue && newValue.trim() !== "") {
            clearFieldError();
          }
        }
      }
    );

    return {
      isDisabled,
      contact,
      items,
      isHidden,
      isInvalid,
      disableField,
      enableField,
      getValue,
      hideField,
      showField,
      updateItems,
      search,
      select,
      makeInvalid,
      updateField,
      setContactFields,
      description,
      computedRules,
      setFieldError,
      clearFieldError,
      errorState,
      updateOptions,
    };
  },
};
</script>

<style lang="scss">
.neoAutoCompleteC.rtl-loader .p-autocomplete-loader {
  right: unset;
  left: 1rem;
}
</style>
