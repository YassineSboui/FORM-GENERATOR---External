<template>
  <div v-if="loading">
    <Loader style="height: 70vh !important" />
  </div>
  <div v-else class="componentForm" :dir="isRTL ? 'rtl' : 'ltr'">
    <!-- Stepper logic removed: only flat form rendering below -->
    <!-- pa-5 -->
    <div class="zone-page-header-parent">
      <div v-for="(item, itemIndex) in itemsFormCopy" :key="itemIndex">
        <zone-component-table
          :isRTL="isRTL"
          @AppRefs="handleRefs($event)"
          v-model="itemsFormCopy[itemIndex]"
          v-model:fields="Fields"
          @handleInputChange="handleInputChange($event)"
          @executeFun="executeFun($event)"
          @itemSelected="handleCodeselected($event)"
          @searchItem="searchItemFunc($event)"
          @focus="handleFocus($event)"
          @blur="handleBlur($event)"
          @mouseenter="handleMouseenter($event)"
          @mouseleave="handleMouseleave($event)"
        ></zone-component-table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import {
  computed,
  ref,
  onMounted,
  watch,
  getCurrentInstance,
  onUnmounted,
  type Ref,
  nextTick,
} from "vue";
import { validateByRule } from "@/utils/fieldValidator";
import { fetchDataByTableGuid, logger, logBlockly } from "@/api/api";
import { useToast } from "primevue/usetoast";
import { useAppStore } from "@/store/app.store";
import ZoneComponentTable from "../Zone/ZoneComponentTable.vue";
import { useHttpRequest } from "@/store/httpRequest.store";
import { storeToRefs } from "pinia";
import { localize } from "@vee-validate/i18n";
import { useI18n } from "vue-i18n";
import { executeCodeAsync } from "@/utils/codeExecutor";
import {
  fieldUtility,
  stringUtility,
  mathUtility,
  arrayUtility,
  eliseUtility,
  storeUtility,
  formUtility,
  initializeBlocklyUtilities,
  systemUtility,
} from "@/utils/blocklyUtilities";

// export default {
const props = defineProps({
  modelValue: {
    default: [] as any,
    required: true,
  },
  isFormDisplay: {
    type: Object,
    required: false,
    default: {
      value: false,
      objectId: "",
    },
  },
  isSubmit: {
    type: Boolean,
    required: false,
    default: false,
  },
  myWatchedVariable: Boolean,
  isGenerateModel: Boolean,
  configForm: {
    type: Object,
    required: false,
  },
  isEdit: {
    type: Boolean,
    required: false,
    default: false,
  },
  isRTL: {
    type: Boolean,
    required: false,
    default: false,
  },
  clearFields: {
    type: Boolean,
    required: false,
    default: false,
  },
  tableFields: {
    type: Object,
    required: false,
    default: () => ({}),
  },
});
// },
const emit = defineEmits([
  "update:modelValue",
  "update:myWatchedVariable",
  "fieldsValueChanged",
  "update:isSubmit",
  "emitXml",
  "done",
]);
const { t } = useI18n();
const store = useAppStore();
const { Fields } = storeToRefs(store);
const win = window;
const app = getCurrentInstance() as any;
const panelCollapsed = ref(false);
const ArrayRef = ref([] as any);
const secondArray = ref([] as any);
const duplicateClick = ref(0);
const toast = useToast();
const internalFormConfig = ref({} as any);
const Variables = ref({} as any);
const loading = ref(false);

const User = ref({ displayName: "" } as any);
const Version = ref({} as any);
const QueryParameters = ref({} as any);
const GlobalVariables = ref({} as any);

const itemsForm = computed({
  get() {
    return props.modelValue;
  },
  set(newValue: any) {
    emit("update:modelValue", newValue);
  },
});

const itemsFormCopy = ref(itemsForm.value);
const myWatchedVariable = computed(() => props.myWatchedVariable); // Use computed property for reactivity

watch(myWatchedVariable, (newVal) => {
  if (newVal) {
    emit(
      "fieldsValueChanged",
      Object.keys(localFields.value).reduce((a, k) => {
        a[k] = Fields.value[k];
        return a;
      }, {} as any)
    );
  }
});

const uuidv4 = () => {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0,
      v = c == "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
};

const requiredFieldsNotEmpty = () => {
  for (let element in app.refs) {
    const isRequired = app.refs[element][0]?.options?.required === true;
    if (
      isRequired &&
      (!Fields.value[element] || Fields.value[element] == "[]")
    ) {
      return false;
    }
  }
  return true;
};
const notValidFieldsExists = () => {
  let formHasError = store.formHasError.length > 0;
  if (formHasError) {
    return true;
  }
  return false;
};

// Create execution context with all necessary variables and functions
const createExecutionContext = () => ({
  uuidv4,
  isEmpty,
  requiredFieldsNotEmpty,
  notValidFieldsExists,
  submit,
  Fields,
  User,
  Version,
  Variables,
  GlobalVariables,
  QueryParameters,
  app,
  store,
  toast,
  t,
  fieldUtility,
  stringUtility,
  mathUtility,
  arrayUtility,
  eliseUtility,
  storeUtility,
  formUtility,
  systemUtility,
});

const submit = async () => {
  const valid = validateFieldsBeforeSubmit();
  if (valid === false) {
    toast.add({
      severity: "error",
      summary: t("ComponentForm.missingInformationsHeader"),
      detail: t("ComponentForm.missingInformationsMessage"),
      life: 3000,
    });
    emit("done", 3);
    return;
  } else if (notValidFieldsExists()) {
    toast.add({
      severity: "error",
      summary: t("ComponentForm.invalidInformationsHeader"),
      detail: t("ComponentForm.invalidInformationsMessage"),
      life: 3000,
    });
    emit("done", 3);
    return;
  } else {
    // No Notice logic, just emit done
    emit("done", 1);
  }
};
const submitNow = computed({
  get: () => props.isSubmit,
  set: (value) => emit("update:isSubmit", value),
});
watch(submitNow, (newVal) => {
  if (newVal) {
    submit();
    submitNow.value = false;
  }
});

const handleFieldSetting = (pageItem: any, clear = false) => {
  if (pageItem.zone === "ZR") {
    localFields.value[pageItem.code] ??= [];
  } else {
    const columnNames = ["column1", "column2", "column3", "column4"];
    columnNames.forEach((columnName) => {
      for (let z = 0; z < pageItem.rows[columnName].length; z++) {
        const options = pageItem.rows[columnName][z]?.options;
        if (options) {
          if (clear) {
            localFields.value[options.name] = "";
          } else {
            localFields.value[options.name] ??= "";
          }
        }
      }
    });
  }
};
const processPage = (pageItem: any, clear = false) => {
  handleFieldSetting(pageItem, clear);
};

const localFields: Ref<any> = ref({});
const setFields = (item: any, div: number) => {
  localFields.value ??= {};
  item.forEach((singleItem: any) => {
    processPage(singleItem);
  });
  if (props.tableFields && Object.keys(props.tableFields).length > 0) {
    Fields.value = { ...Fields.value, ...props.tableFields };
  }
  store.Fields = { ...localFields.value, ...store.Fields };
};

const clearFieldsFunc = (item: any, div: number) => {
  localFields.value ??= {};
  item.forEach((singleItem: any) => {
    processPage(singleItem, true);
  });
  Object.keys(localFields.value).forEach((key) => delete Fields.value[key]);
};

onUnmounted(() => {
  clearFieldsFunc(itemsFormCopy.value, 0);
});

const isLoadingComponent = ref(true);
const setLocale = () => {
  props.isRTL ? localize("ar") : localize("fr");
};

const HeaderHeight = ref([] as any);

onMounted(async () => {
  logBlockly.info("onMounted");
  initializeBlocklyUtilities({ app, store });
  setLocale();
  setFields(itemsFormCopy.value, 0);
  internalFormConfig.value = props.configForm;
  isLoadingComponent.value = true;
  try {
    // simlate a virtual delay like an API call
    loading.value = true;
    await new Promise((resolve) => setTimeout(resolve, 500));
    loading.value = false;
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  }

  internalFormConfig.value?.variables?.forEach((element: any) => {
    Variables.value[element.key] = element.value;
  });
  const afterLoad = ref("" as any);
  internalFormConfig.value?.events?.forEach(async (evnt: any) => {
    if (evnt.code != "" && evnt.rule.code == "beforeLoad") {
      try {
        await executeCodeAsync(evnt.code, createExecutionContext());
      } catch (error) {
        console.error("error", error);
        logger.error(error);
      }
    }
    if (evnt.code != "" && evnt.rule.code == "afterLoad") {
      afterLoad.value = evnt;
    }
  });
  isLoadingComponent.value = false;
  loading.value = false;
  if (afterLoad.value) {
    try {
      await executeCodeAsync(afterLoad.value.code, createExecutionContext());
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
  const headerElement = document.querySelector(
    ".zone-page-header"
  ) as HTMLElement;
  if (headerElement) {
    HeaderHeight.value = headerElement.offsetHeight;
    await nextTick();
    HeaderHeight.value = headerElement.offsetHeight;
  } else {
    console.warn("Header element not found");
  }
});

const handleCollapsed = (event: any) => {
  panelCollapsed.value = event;
};

const handleInputChange = async (item: any) => {
  if (!item) return;
  await nextTick();
  const selectedEvent = item.find((event: any) => event.rule.code === "change");
  if (selectedEvent) {
    try {
      await executeCodeAsync(selectedEvent.code, createExecutionContext());
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
};

const handleFocus = async (item: any) => {
  if (!item) return;
  await nextTick();
  const selectedEvent = item.find((event: any) => event.rule.code === "focus");
  if (selectedEvent) {
    try {
      await executeCodeAsync(selectedEvent.code, createExecutionContext());
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
};

const handleBlur = async (item: any) => {
  if (!item) return;
  await nextTick();
  const selectedEvent = item.find((event: any) => event.rule.code === "blur");
  if (selectedEvent) {
    try {
      await executeCodeAsync(selectedEvent.code, createExecutionContext());
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
};

const handleMouseenter = async (item: any) => {
  if (!item) return;
  await nextTick();
  const selectedEvent = item.find(
    (event: any) => event.rule.code === "mouseenter"
  );
  if (selectedEvent) {
    try {
      await executeCodeAsync(selectedEvent.code, createExecutionContext());
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
};

const handleMouseleave = async (item: any) => {
  if (!item) return;
  await nextTick();
  const selectedEvent = item.find(
    (event: any) => event.rule.code === "mouseleave"
  );
  if (selectedEvent) {
    try {
      await executeCodeAsync(selectedEvent.code, createExecutionContext());
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
};

const handleRefs = (event: any) => {
  ArrayRef.value.push(event);
  ArrayRef.value.forEach((element: any) => {
    const exist = secondArray.value.find((el: any) => el == element);
    if (!exist) {
      secondArray.value.push(element);
    }
  });

  const mergedObject = secondArray.value.reduce((result: any, obj: any) => {
    Object.keys(obj).forEach((key) => {
      // If key doesn't exist, initialize it
      if (!result[key]) {
        result[key] = [];
      }

      // Push all items from arrays
      result[key].push(...obj[key]);
    });

    return result;
  }, {});
  app.refs = mergedObject;
};

const executeFun = async (code: string) => {
  try {
    await executeCodeAsync(code, createExecutionContext());
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  }
};

const handleCodeselected = async (code: string) => {
  try {
    await executeCodeAsync(code, createExecutionContext());
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  }
};

const searchItemFunc = async (code: string) => {
  try {
    await executeCodeAsync(code, createExecutionContext());
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  }
};

const isEmpty = (value: any): boolean => {
  // Handle null and undefined
  if (value === null || value === undefined) return true;

  // Handle strings (including whitespace-only and JSON array strings)
  if (typeof value === "string") {
    const trimmed = value.trim();
    return trimmed === "" || trimmed === "[]" || trimmed === "{}";
  }

  // Handle numbers (0 is not considered empty, but NaN is)
  if (typeof value === "number") {
    return Number.isNaN(value);
  }

  // Handle booleans (both true and false are not considered empty)
  if (typeof value === "boolean") {
    return false;
  }

  // Handle arrays
  if (Array.isArray(value)) {
    return value.length === 0;
  }

  // Handle objects (including Date, but exclude functions)
  if (typeof value === "object") {
    // Handle Date objects - empty if invalid date
    if (value instanceof Date) {
      return Number.isNaN(value.getTime());
    }

    // Handle regular objects
    return Object.keys(value).length === 0;
  }

  // Handle functions (not considered empty)
  if (typeof value === "function") {
    return false;
  }

  // Handle symbols (not considered empty)
  if (typeof value === "symbol") {
    return false;
  }

  // Handle BigInt (0n is not considered empty)
  if (typeof value === "bigint") {
    return false;
  }

  // Default case - not empty
  return false;
};

const validateField = (pageItem: any, columnName: string, valid: boolean) => {
  for (let z = 0; z < pageItem.rows[columnName].length; z++) {
    const field = pageItem.rows[columnName][z];
    const options = field?.options;
    if (options && options.rules && Array.isArray(options.rules)) {
      for (const rule of options.rules) {
        const result = validateByRule(
          rule,
          Fields.value[options.name],
          options.label,
          props.isRTL ? "ar" : "fr"
        );
        if (!result.valid) {
          valid = false;
          break;
        }
      }
    } else if (
      options &&
      options.required &&
      options.hidden !== true &&
      isEmpty(Fields.value[options.name])
    ) {
      valid = false;
    }
  }
  return valid;
};

const initFields = () => {
  Object.keys(Fields.value).forEach((key) => {
    Fields.value[key] = "";
  });
};

const fetchTableData = async (code: string) => {
  return await fetchDataByTableGuid(code);
};

const validateFieldsBeforeSubmit = () => {
  let valid = true;
  itemsFormCopy.value.some((section: any) => {
    const columnNames = ["column1", "column2", "column3", "column4"];
    columnNames.forEach((columnName) => {
      valid = validateField(section, columnName, valid);
    });
  });
  return valid;
};

defineExpose({
  itemsForm,
  Fields,
  win,
  panelCollapsed,
  itemsFormCopy,
  fetchTableData,
  submit,
  handleInputChange,
  executeFun,
  handleCollapsed,
  handleRefs,
  // Removed showSection and hideSection from export
  // Removed validatePageFields from export
  initFields,
});
</script>
<style lang="scss">
.componentForm {
  width: 99%;
  height: 100%;
}
.head {
  padding: 2px !important;
  font-family: Trebuchet MS, sans-serif;
  color: #4a4a4a;
  font-size: 16px;
  line-height: 18.2px;
  font-weight: 400;
  .haedDivider {
    width: 3px;
    height: 20px;
    background-color: #b6cbd4;
  }
}
// .p-divider.p-divider-horizontal {
//   margin: 0 !important;
// }
.p-divider.p-divider-vertical {
  margin: 0 !important;
  padding: 0 !important;
}

.duplicatable-zone-footer {
  .p-button.p-button-icon-only {
    width: 2.3rem;
    height: 2.3rem;
  }
}
Z .visibility-hidden {
  visibility: hidden;
}
::-webkit-scrollbar-track {
  margin-top: var(--scrollbar-margin-top);
}
</style>
