<template>
  <div class="neoTableComponent mb-4" v-show="!isHidden">
    <div class="label" v-if="label">
      <label class="label-container">
        <span> {{ table?.objectConfig?.formConfig?.description }}</span>

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

    <div class="input-container" :class="{ 'disabled-wrapper': isDisabled }">
      <neoTable
        v-if="Template.length > 0"
        v-model="objects"
        :config="table"
        :isRTL="isRTL"
        :params="options.params"
        :readonly="options.readonly"
        ref="TableRef"
        @selectedObjects="getSelectedObjects($event)"
      />
      <small class="p-error" id="text-error" v-if="errorState.errorMessage">
        {{ errorState.errorMessage || "&nbsp;" }}
      </small>
    </div>
  </div>
</template>

<script lang="ts">
import { fetchObjectByGuid } from "@/api/api";
import type { Ref } from "vue";
import {
  onBeforeMount,
  onMounted,
  computed,
  defineComponent,
  ref,
  watch,
  reactive,
  getCurrentInstance,
  defineExpose,
  readonly,
} from "vue";
import {} from "vue";
import { useRoute } from "vue-router";
import { logger } from "@/api/api";
interface OptionConfig {
  name: string;
  label: string;
  id: number;
  tooltip: string;
  required: boolean | null;
  readonly: boolean | null;
  disabled: boolean | null;
  hidden: boolean | null;
  relatedToElise: boolean | null;
  rules: { expression: string }[];
  params: any[];
  events: any[];
}
export default defineComponent({
  props: {
    label: String,
    modelValue: {
      // type: Array,
      default: [],
    },
    options: {
      type: Object,
      default: () => ({
        name: "",
        label: "Test",
        id: 0,
        tooltip: "",
        required: false,
        readonly: false,
        disabled: false,
        relatedToElise: false,
        hidden: false,
        rules: [],
        events: [],
        params: [],
      }),
    },
    isRTL: {
      type: Boolean,
      default: false,
    },
  },
  emits: ["update:options", "update:modelValue"],
  setup(props, { emit }) {
    const convertData = (data: any) => {
      if (data != "") {
        const outputData = {
          row: data,
        };
        return outputData;
      } else return data;
    };
    const revertData: (data: any) => any[] = (data: any) => {
      return data.row ?? data;
    };

    const objects: Ref<any> = ref([]);
    const object: Ref<ObjectModel | null | any> = ref(null);
    const route = useRoute();
    const internalValue = computed({
      get() {
        return props.modelValue;
      },
      set(newValue: any) {
        emit("update:modelValue", convertData(newValue));
      },
    });
    const params = computed({
      get() {
        return props.options.params;
      },
      set(newValue: any) {
        props.options.params = newValue;
        emit("update:options", props.options);
      },
    });
    // Create a local copy of options to manage mutability
    const localOptions = reactive({ ...props.options });
    const isDisabled = computed(() => localOptions.disabled);
    const isHidden = computed(() => localOptions.hidden);

    const table: Ref<any> = computed(() => {
      return object.value
        ? {
            ...JSON.parse(object.value.objectJson),
            id: object.value.id,
          }
        : {};
    });
    const Template: Ref<any[]> = computed(() => {
      return object.value
        ? JSON.parse(object.value.objectJson).objectConfig.formTemplate
        : [];
    });
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

    const setValue = (value: any) => {
      if (typeof value === "string") {
        try {
          value = JSON.parse(value);
        } catch (error) {
          console.error("Invalid JSON string:", error);
          logger.error(error);
          return;
        }
      }
      internalValue.value = value;
      emit("update:modelValue", value);
    };

    const addValues = (values: any) => {
      if (typeof values === "string") {
        try {
          values = JSON.parse(values);
        } catch (error) {
          console.error("Invalid JSON string:", error);
          logger.error(error);
          return;
        }
      }
      internalValue.value = [...internalValue.value, ...values];
      emit("update:modelValue", internalValue.value);
    };

    const updateField = (value: any) => {
      if (typeof value === "string") {
        try {
          value = JSON.parse(value);
        } catch (error) {
          console.error("Invalid JSON string:", error);
          logger.error(error);
          return;
        }
      }
      if (Array.isArray(value)) {
        value = value.map((item) => ({
          ...item,
          id:
            item.id ??
            Math.floor(
              100000000000000 + Math.random() * 900000000000000
            ).toString(),
        }));
      }
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

    watch(
      () => objects,
      () => {
        emit("update:modelValue", convertData(objects.value));
      },
      { deep: true }
    );
    watch(
      () => internalValue.value,
      () => {
        objects.value = revertData(internalValue.value);
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

    watch(
      () => props.options.id,
      async (value: any) => {
        object.value = null;
        objects.value = [];
        object.value = await fetchObjectByGuid(value);
        params.value = table.value?.objectConfig?.formConfig?.variables;
      }
    );

    // Function to set error
    const setFieldError = (errorMessage: string) => {
      errorState.errorMessage = errorMessage;
    };

    // Function to remove error
    const clearFieldError = () => {
      errorState.errorMessage = "";
    };

    const handleFocus = (event: any) => {
      // event.target.blur();
    };
    onBeforeMount(async () => {
      if (internalValue.value.length > 0) {
        objects.value = internalValue.value;
      } else if (props.options.id != 0) {
        object.value = await fetchObjectByGuid(props.options.id);
      }
    });
    const app = getCurrentInstance() as any;
    // const TableRef = ref({} as any);
    // Object to store dynamic refs
    // onMounted(() => {
    //   const tablesComponents = document.querySelectorAll(".neoTableComponent");

    //   tablesComponents.forEach((tableComponent) => {
    //     tableComponent.addEventListener("hover", () => {
    //       tableComponent.scrollIntoView();
    //     });
    //     // (tableComponent as HTMLElement).focus();
    //   });
    //   // TableRef.value[props.options.name] = null;
    // });
    const TableRef = ref(null);
    const selectedObjects = ref([]);
    const getSelectedObjects = (event: any) => {
      console.log("Selected Objects:", event);
      selectedObjects.value = event;
    };
    const exposeSelectedObjects = () => {
      return selectedObjects.value;
    };
    // defineExpose({
    //   TableRef,
    //   getSelectedObjects,
    //   exposeSelectedObjects,
    // });

    return {
      isHidden,
      isDisabled,
      objects,
      table,
      Template,
      internalValue,
      computedRules,
      errorState,
      TableRef,
      localOptions,
      selectedObjects,
      handleFocus,
      setValue,
      addValues,
      updateField,
      getValue,
      disableField,
      enableField,
      hideField,
      showField,
      setFieldError,
      clearFieldError,
      updateOptions,
      exposeSelectedObjects,
      getSelectedObjects,
    };
  },
});
</script>

<style lang="scss">
.neoTableComponent {
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
      padding-top: 5px;
      .label .label-container-modified {
        text-align: right;
      }
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
    }
  }
  .p-datatable-wrapper {
    // remove the position
    position: unset !important;
  }

  .p-datatable-frozen-tbody {
    position: unset !important;
  }
  .disabled-wrapper {
    pointer-events: none; /* Disable all interactions */
    opacity: 0.5; /* Optional: Make it look visually disabled */
  }
}
</style>
