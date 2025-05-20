<template>
  <div
    class="componentForm"
    :dir="isRTL ? 'rtl' : 'ltr'"
    :style="{ '--scrollbar-margin-top': marginTop }"
  >
    <div class="stepper" v-if="stepper.isStepper">
      <div
        class="ZSTNavigation mb-3"
        style="width: 100%"
        v-if="!stepper.showPageNames"
      >
        <div class="grid">
          <div class="col flex justify-content-start" v-if="isEdit">
            <Button
              v-show="showPageNum > 1"
              @click="navigatePrevious(showPageNum)"
            >
              Précédent
            </Button>
          </div>
          <div class="col flex justify-content-end" v-if="isEdit">
            <Button
              v-show="showPageNum < stepper.steps"
              @click="navigateNext(showPageNum)"
            >
              Suivant
            </Button>
          </div>
        </div>
      </div>
      <div class="flex justify-content-between mb-2 pages-headers" v-else>
        <div class="NeoTabView">
          <div class="NeoTabHeaders">
            <div
              v-for="i in stepper.steps"
              :key="i"
              class="NeoTabHeader"
              :class="{
                'NeoTabHeader--active': showPageNum === i,
              }"
              style="cursor: pointer"
              @click="showPageNum = i"
            >
              <span class="NeoTabHeader__label">{{
                stepper.names[`page${i}`]
              }}</span>
            </div>
          </div>
        </div>
        <!-- <Button
            v-if="showPageNum == stepper.steps && !isEdit"
            @click="submitStepper()"
            class="ml-2"
            >Valider</Button
          > -->
      </div>
      <div
        v-for="pg in stepper.steps"
        :style="props.isEdit ? {} : computePageStyle(pg)"
        class="zone-page-header-parent"
      >
        <div
          v-for="i in Math.max(itemsFormCopy[0].pages[`page${pg}`].length || 0)"
          :key="i"
          :class="stickyHeaderClass(pg, i)"
          :style="{
            display: pg === showPageNum ? '' : 'none !important',
          }"
        >
          <div
            v-if="itemsFormCopy[0].pages[`page${pg}`][i - 1]?.zone === 'ZR'"
            v-show="itemsFormCopy[0].pages[`page${pg}`][i - 1]?.show !== false"
            :class="{
              'm-2':
                !itemsFormCopy[0].pages[`page${pg}`][i - 1].code?.includes(
                  'FIXED_HEADER'
                ),
            }"
            ref="itemRefs"
            :data-key="itemsFormCopy[0].pages[`page${pg}`][i - 1].code"
          >
            <P-ZR
              :isRTL="isRTL"
              v-model="itemsFormCopy[0].pages[`page${pg}`][i - 1]"
              :repeatableZoneChildrens="repeatableZoneChildrens"
              :Fields="Fields"
              @inputChange="handleInputChange($event)"
              @executeFun="executeFun($event)"
              @handleRefs="handleRefs($event)"
              @duplicate="duplicate($event)"
              @focus="handleFocus($event)"
              @blur="handleBlur($event)"
              @mouseenter="handleMouseenter($event)"
              @mouseleave="handleMouseleave($event)"
              :language="language"
            ></P-ZR>
          </div>
          <div
            v-if="itemsFormCopy[0].pages[`page${pg}`][i - 1]?.zone === 'ZS'"
            v-show="itemsFormCopy[0].pages[`page${pg}`][i - 1]?.show !== false"
            class="grid"
            style="width: 100%"
          >
            <div
              class="ml-3 mr-2"
              :style="{
                width:
                  itemsFormCopy[0].pages[`page${pg}`][i - 1].sizes[0] - 1 + '%',
              }"
            >
              <div
                v-for="j in Math.max(
                  itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column1
                    .length || 0
                )"
                :key="j"
              >
                <div
                  v-if="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column1[
                      j - 1
                    ]?.zone === 'ZR'
                  "
                  v-show="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column1[
                      j - 1
                    ]?.show !== false
                  "
                  class="m-2"
                  ref="itemRefs"
                  :data-key="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column1[
                      j - 1
                    ].code
                  "
                >
                  <P-ZR
                    :isRTL="isRTL"
                    v-model="
                      itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column1[
                        j - 1
                      ]
                    "
                    :repeatableZoneChildrens="repeatableZoneChildrens"
                    :Fields="Fields"
                    @inputChange="handleInputChange($event)"
                    @executeFun="executeFun($event)"
                    @handleRefs="handleRefs($event)"
                    @duplicate="duplicate($event)"
                    @focus="handleFocus($event)"
                    @blur="handleBlur($event)"
                    @mouseenter="handleMouseenter($event)"
                    @mouseleave="handleMouseleave($event)"
                    :language="language"
                  ></P-ZR>
                </div>

                <zone-component
                  v-else
                  :isRTL="isRTL"
                  @AppRefs="handleRefs($event)"
                  v-model="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column1[
                      j - 1
                    ]
                  "
                  v-model:fields="Fields"
                  @handleInputChange="handleInputChange($event)"
                  @executeFun="executeFun($event)"
                  @itemSelected="handleCodeselected($event)"
                  @searchItem="searchItemFunc($event)"
                  @focus="handleFocus($event)"
                  @blur="handleBlur($event)"
                  @mouseenter="handleMouseenter($event)"
                  @mouseleave="handleMouseleave($event)"
                  style="min-height: 60px"
                  :language="language"
                ></zone-component>
              </div>
            </div>
            <Divider layout="vertical" />
            <div
              class="ml-2"
              :style="{
                width:
                  itemsFormCopy[0].pages[`page${pg}`][i - 1].sizes[1] - 1 + '%',
              }"
            >
              <div
                v-for="j in Math.max(
                  itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column2
                    .length || 0
                )"
                :key="j"
              >
                <div
                  v-if="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column2[
                      j - 1
                    ]?.zone === 'ZR'
                  "
                  v-show="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column2[
                      j - 1
                    ]?.show !== false
                  "
                  class="m-2"
                  ref="itemRefs"
                  :data-key="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column2[
                      j - 1
                    ].code
                  "
                >
                  <P-ZR
                    :isRTL="isRTL"
                    v-model="
                      itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column2[
                        j - 1
                      ]
                    "
                    :repeatableZoneChildrens="repeatableZoneChildrens"
                    :Fields="Fields"
                    @inputChange="handleInputChange($event)"
                    @executeFun="executeFun($event)"
                    @handleRefs="handleRefs($event)"
                    @duplicate="duplicate($event)"
                    @focus="handleFocus($event)"
                    @blur="handleBlur($event)"
                    @mouseenter="handleMouseenter($event)"
                    @mouseleave="handleMouseleave($event)"
                    :language="language"
                  ></P-ZR>
                </div>
                <zone-component
                  v-else
                  :isRTL="isRTL"
                  @AppRefs="handleRefs($event)"
                  v-model="
                    itemsFormCopy[0].pages[`page${pg}`][i - 1].rows.column2[
                      j - 1
                    ]
                  "
                  v-model:fields="Fields"
                  @handleInputChange="handleInputChange($event)"
                  @executeFun="executeFun($event)"
                  @itemSelected="handleCodeselected($event)"
                  @searchItem="searchItemFunc($event)"
                  style="min-height: 60px"
                  @focus="handleFocus($event)"
                  @blur="handleBlur($event)"
                  @mouseenter="handleMouseenter($event)"
                  @mouseleave="handleMouseleave($event)"
                  :language="language"
                ></zone-component>
              </div>
            </div>
          </div>
          <zone-component
            :isRTL="isRTL"
            @AppRefs="handleRefs($event)"
            v-model="itemsFormCopy[0].pages[`page${pg}`][i - 1]"
            v-model:fields="Fields"
            @handleInputChange="handleInputChange($event)"
            @executeFun="executeFun($event)"
            @itemSelected="handleCodeselected($event)"
            @searchItem="searchItemFunc($event)"
            @focus="handleFocus($event)"
            @blur="handleBlur($event)"
            @mouseenter="handleMouseenter($event)"
            @mouseleave="handleMouseleave($event)"
            :language="language"
          ></zone-component>
        </div>
      </div>
    </div>
    <!-- pa-5 -->
    <div v-else class="zone-page-header-parent">
      <div
        v-for="(item, itemIndex) in itemsFormCopy"
        :key="itemIndex"
        :class="item.code == 'FIXED_HEADER' ? 'zone-page-sticky-header' : ''"
        :style="
          item.code == 'FIXED_HEADER'
            ? {
                margin: '0 0 ' + HeaderHeight + 'px ' + ' 0 ',
              }
            : null
        "
      >
        <div
          v-if="item?.zone === 'ZR'"
          v-show="item?.show !== false"
          :class="item.code == 'FIXED_HEADER' ? '' : 'm-2'"
          ref="itemRefs"
          :data-key="item.code"
        >
          <P-ZR
            :isRTL="isRTL"
            v-model="itemsFormCopy[itemIndex]"
            :repeatableZoneChildrens="repeatableZoneChildrens"
            :Fields="Fields"
            @inputChange="handleInputChange($event)"
            @executeFun="executeFun($event)"
            @handleRefs="handleRefs($event)"
            @duplicate="duplicate($event)"
            @focus="handleFocus($event)"
            @blur="handleBlur($event)"
            @mouseenter="handleMouseenter($event)"
            @mouseleave="handleMouseleave($event)"
            :language="language"
          ></P-ZR>
        </div>
        <div
          v-if="item?.zone === 'ZS'"
          v-show="item?.show !== false"
          class="grid"
          style="width: 100%"
        >
          <div class="ml-3 mr-2" :style="{ width: item.sizes[0] - 1 + '%' }">
            <div v-for="i in Math.max(item.rows.column1.length || 0)" :key="i">
              <div
                v-if="item.rows.column1[i - 1]?.zone === 'ZR'"
                v-show="item.rows.column1[i - 1]?.show !== false"
                class="m-2"
                ref="itemRefs"
                :data-key="item.rows.column1[i - 1].code"
              >
                <P-ZR
                  :isRTL="isRTL"
                  v-model="item.rows.column1[i - 1]"
                  :repeatableZoneChildrens="repeatableZoneChildrens"
                  :Fields="Fields"
                  @inputChange="handleInputChange($event)"
                  @executeFun="executeFun($event)"
                  @handleRefs="handleRefs($event)"
                  @duplicate="duplicate($event)"
                  @focus="handleFocus($event)"
                  @blur="handleBlur($event)"
                  @mouseenter="handleMouseenter($event)"
                  @mouseleave="handleMouseleave($event)"
                  :language="language"
                ></P-ZR>
              </div>

              <zone-component
                v-else
                :isRTL="isRTL"
                @AppRefs="handleRefs($event)"
                v-model="item.rows.column1[i - 1]"
                v-model:fields="Fields"
                @handleInputChange="handleInputChange($event)"
                @executeFun="executeFun($event)"
                @itemSelected="handleCodeselected($event)"
                @searchItem="searchItemFunc($event)"
                @focus="handleFocus($event)"
                @blur="handleBlur($event)"
                @mouseenter="handleMouseenter($event)"
                @mouseleave="handleMouseleave($event)"
                style="min-height: 60px"
                :language="language"
              ></zone-component>
            </div>
          </div>
          <Divider layout="vertical" />
          <div class="ml-2" :style="{ width: item.sizes[1] - 1 + '%' }">
            <div v-for="i in Math.max(item.rows.column2.length || 0)" :key="i">
              <div
                v-if="item.rows.column2[i - 1]?.zone === 'ZR'"
                v-show="item.rows.column2[i - 1]?.show !== false"
                class="m-2"
                ref="itemRefs"
                :data-key="item.rows.column2[i - 1].code"
              >
                <P-ZR
                  :isRTL="isRTL"
                  v-model="item.rows.column2[i - 1]"
                  :repeatableZoneChildrens="repeatableZoneChildrens"
                  :Fields="Fields"
                  @inputChange="handleInputChange($event)"
                  @executeFun="executeFun($event)"
                  @handleRefs="handleRefs($event)"
                  @duplicate="duplicate($event)"
                  @focus="handleFocus($event)"
                  @blur="handleBlur($event)"
                  @mouseenter="handleMouseenter($event)"
                  @mouseleave="handleMouseleave($event)"
                  :language="language"
                ></P-ZR>
              </div>
              <zone-component
                v-else
                :isRTL="isRTL"
                @AppRefs="handleRefs($event)"
                v-model="item.rows.column2[i - 1]"
                v-model:fields="Fields"
                @handleInputChange="handleInputChange($event)"
                @executeFun="executeFun($event)"
                @itemSelected="handleCodeselected($event)"
                @searchItem="searchItemFunc($event)"
                @focus="handleFocus($event)"
                @blur="handleBlur($event)"
                @mouseenter="handleMouseenter($event)"
                @mouseleave="handleMouseleave($event)"
                style="min-height: 60px"
                :language="language"
              ></zone-component>
            </div>
          </div>
        </div>
        <zone-component
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
          :language="language"
        ></zone-component>
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
  onBeforeMount,
  onUnmounted,
  type Ref,
  nextTick,
} from "vue";
import {
  saveNotice,
  fileUpload,
  generateXMLModel,
  generateModel,
  fetchNotice,
  fetchMetadata,
  updateNotice,
  fetchDataByTableGuid,
  logger,
  callEliseWebService,
} from "@/api/api";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import { useAppStore } from "@/store/app.store";
import ZoneComponent from "./ZoneComponent.vue";
import { useRoute, useRouter } from "vue-router";
import { useHttpRequest } from "@/store/httpRequest.store";
import { AifileUpload } from "@/api/api";
// import { useComponentStore } from "@/store/component.store";
// import storeMap from "@/main";
import { storeToRefs } from "pinia";
import { localize } from "@vee-validate/i18n";

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
  stepper: {
    type: Object,
    required: false,
    default: {
      isStepper: false,
      steps: 1,
      showPageNames: false,
      names: {
        page1: "Page 1",
      },
    },
  },
  clearFields: {
    type: Boolean,
    required: false,
    default: false,
  },
  showPageNum: {
    type: Number,
    required: false,
    default: 1,
  },
  executeNavigateNext: {
    type: Boolean,
    required: false,
    default: false,
  },
  tableFields: {
    type: Object,
    required: false,
    default: () => ({
      label_AR: "",
      label_ENG: "",
    }),
  },
  language: {
    type: String,
    required: false,
    default: "FR",
  },
  showLoader: {
    type: Boolean,
    required: false,
    default: true,
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
  "update:showPageNum",
  "update:executeNavigateNext",
]);
// setup(props, { emit }) {
const store = useAppStore();

const { Fields } = storeToRefs(store);
const confirm = useConfirm();
const win = window;
const itemRefs = ref([] as any[]);
const app = getCurrentInstance() as any;
//const Fields = ref({} as any);
const Notice = ref([] as any);
const NoticeMapping = ref({} as any);
const NoticeData = ref({} as any);
const NoticeHtml = ref({} as any);
const NoticeAttachements = ref([{}] as any);
const panelCollapsed = ref(false);
const ArrayRef = ref([] as any);
const secondArray = ref([] as any);
const fiel = ref({} as any);
const duplicateClick = ref(0);
const copy = ref({ copy: [] } as any);
const toast = useToast();
const internalFormConfig = ref({} as any);
const Variables = ref({} as any);
// const showPageNum = ref(1);

const showPageNum = computed({
  get() {
    return props.showPageNum;
  },
  set(newValue: any) {
    emit("update:showPageNum", newValue);
  },
});
const executeNavigateNext = computed({
  get() {
    return props.executeNavigateNext;
  },
  async set(newValue: any) {
    emit("update:executeNavigateNext", newValue);
  },
});
watch(executeNavigateNext, (newVal) => {
  if (newVal) {
    navigateNext(showPageNum.value);
    // calculatePagesStyle();
  }
});
watch(showPageNum, (newVal) => {
  calculatePagesStyle();
});
const router = useRouter();

const cancel = () => {
  if (window.self === window.top) {
    router.go(-1);
  } else {
    window.parent.postMessage("EliseCustomActionDone", "*");
    parent.location.reload();
  }
};
const User = ref({ displayName: "" } as any);
const Version = ref({} as any);
const GlobalVariables = ref({} as any);
const QueryParameters = ref({} as any);
const disabledNextButton = ref(false);
const disabledPreviousButton = ref(false);
const newNotice = {
  Lang: "fr",
  mapping: {},
  data: {},
  html: {},
  files: [],
  Attachements: [
    {
      guid: "",
      isLinked: false,
    },
  ],
  models: [] as string[],
  uploadTable: {},
};

function useModel(modelGuid: string) {
  if (!newNotice.models.includes(modelGuid)) {
    newNotice.models.push(modelGuid);
  }
}
const toggleVisibility = (section: any, sectionCode: any, show: boolean) => {
  const { column1, column2 } = section.rows;
  const column =
    column1.find((c: any) => c.code === sectionCode) ||
    column2.find((c: any) => c.code === sectionCode);
  if (column) {
    column.show = show;
    return true;
  }
  return false;
};

function showSection(sectionCode: string) {
  if (props.stepper.isStepper) {
    for (let i = 1; i <= props.stepper.steps; i++) {
      const page = itemsFormCopy.value[0].pages[`page${i}`];
      const sect = page.find((pageItem: any) =>
        pageItem.zone === "ZS" && pageItem.code !== sectionCode
          ? toggleVisibility(pageItem, sectionCode, true)
          : pageItem.code === sectionCode
      );
      if (sect) {
        sect.show = true;
        return;
      }
    }
  } else {
    itemsFormCopy.value.some((section: any) =>
      section.code === "ZS" && section.code !== sectionCode
        ? toggleVisibility(section, sectionCode, true)
        : section.code === sectionCode && (section.show = true)
    );
  }
}

function hideSection(sectionCode: string) {
  if (props.stepper.isStepper) {
    for (let i = 1; i <= props.stepper.steps; i++) {
      const page = itemsFormCopy.value[0].pages[`page${i}`];
      const sect = page.find((pageItem: any) =>
        pageItem.zone === "ZS" && pageItem.code !== sectionCode
          ? toggleVisibility(pageItem, sectionCode, false)
          : pageItem.code === sectionCode
      );
      if (sect) {
        sect.show = false;
        return;
      }
    }
  } else {
    itemsFormCopy.value.some((section: any) =>
      section.code === "ZS" && section.code !== sectionCode
        ? toggleVisibility(section, sectionCode, false)
        : section.code === sectionCode && (section.show = false)
    );
  }
}
function disableNextButtonFunction() {
  disabledNextButton.value = true;
}
function disablePreviousButtonFunction() {
  disabledPreviousButton.value = true;
}
function enableNextButtonFunction() {
  disabledNextButton.value = false;
}
function enablePreviousButtonFunction() {
  disabledPreviousButton.value = false;
}

const itemsForm = computed({
  get() {
    return props.modelValue;
  },
  set(newValue: any) {
    emit("update:modelValue", newValue);
  },
});

const itemsFormCopy = ref(itemsForm.value);
const fixedHeadersHeights = ref([] as any);

itemsForm.value.forEach((item: any) => {
  fixedHeadersHeights.value.push(0);
});
const myWatchedVariable = computed(() => props.myWatchedVariable); // Use computed property for reactivity

watch(myWatchedVariable, (newVal) => {
  if (newVal) {
    emit(
      "fieldsValueChanged",
      Object.keys(localFields.value).reduce((a, k) => {
        a[k] = Fields.value[k];
        return a;
      }, {} as any)
    ); // Emit Fields.value to the parent
    // Fields.value = {};
    // setFields(itemsFormCopy.value, 0)
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

const submitStepper = () => {
  submitNow.value = true;
};
const NoticeFiles = ref();
const NoticeUploadTable = ref();
const submitNotice = async () => {
  Notice.value = [];
  NoticeAttachements.value = [];
  NoticeFiles.value = [];
  NoticeUploadTable.value = [];
  for (let elem of itemRefs.value) {
    const key = elem.dataset.key;
    NoticeData.value[key] = Fields.value[key] ?? "";
  }

  console.log("app.refs", app.refs);
  for (let element in app.refs) {
    const options = app.refs[element][0]?.options;
    const related = options?.relatedToElise;
    const isEditor = options?.type == "Editor";
    const isFile = options?.type == "Upload";
    const isPhoto = options?.type == "PHOTO";
    const isTable = options?.type == "Table";
    const isFlowchart = options?.type == "FLOWCHART";
    const isTreewiew = options?.type == "TREEVIEW";
    const isUploadTable = isTable && options?.isUploadTable;
    const isVHTML = options?.type == "HTML";
    if (isFile) {
      if (!options.useAILise) {
        for (const elem of Fields.value[element]) {
          // const result = await fileUpload(elem);
          NoticeAttachements.value.push({
            guid: elem.guid,
            // isLinked: elem.isLinked,
            fileName: elem.fileName,
          });
        }
      }
    } else if (isEditor) {
      NoticeHtml.value[element] = Fields.value[element] ?? "";
    } else if (isTreewiew) {
      if (options.selectedType == "Organigramme") {
        NoticeMapping.value[element] =
          Object.keys(Fields.value[element])[0] ?? "";
        NoticeData.value[element] = Object.keys(Fields.value[element])[0] ?? "";
      } else {
        NoticeMapping.value[element] = Object.keys(Fields.value[element]) ?? [];
        NoticeData.value[element] = Object.keys(Fields.value[element]) ?? [];
      }
    } else if (related && !isFlowchart) {
      NoticeMapping.value[element] = Fields.value[element] ?? "";
      NoticeData.value[element] = Fields.value[element] ?? "";
    } else if (isFlowchart && related) {
      NoticeMapping.value[element] = Fields.value[element].id ?? "";
      NoticeData.value[element] = Fields.value[element] ?? "";
    } else if (isUploadTable) {
      NoticeUploadTable.value[element] = Fields.value[element] ?? "";
      NoticeData.value[element] = Fields.value[element] ?? "";
    } else if (isPhoto) {
      NoticeFiles.value = mapFileField(Fields.value[element]);
    } else if (!isVHTML && options !== undefined) {
      NoticeData.value[element] = Fields.value[element] ?? "";
    }
  }
  newNotice.Attachements = NoticeAttachements.value;
  newNotice.data = {
    ...store.currentNotice?.noticeJson?.data,
    ...NoticeData.value,
  };
  newNotice.html = NoticeHtml.value;

  newNotice.mapping = {
    ...store.currentNotice?.noticeJson?.mapping,
    ...NoticeMapping.value,
  };
  newNotice.uploadTable = {
    ...store.currentNotice?.noticeJson?.uploadTable,
    ...NoticeUploadTable.value,
  };
  newNotice.files = NoticeFiles.value;

  confirm.require({
    message: props.isRTL
      ? "هل أنت متأكد أنك تريد تأكيد هذا النموذج؟"
      : "Êtes-vous sûr de vouloir valider ce formulaire ?",
    header: props.isRTL ? "تأكيد" : "Confirmation",
    rejectLabel: props.isRTL ? "لا" : "Non",
    rejectClass: "p-button-danger",
    acceptLabel: props.isRTL ? "نعم" : "Oui",
    accept: async () => {
      httpRequest.setLoading(true);
      const beforeSaveCode = internalFormConfig.value.events.find(
        (evnt: any) => evnt.rule.code == "beforeSave"
      )?.code;
      if (beforeSaveCode) {
        try {
          await eval(
            "(async () => { const store = useAppStore(); " +
              beforeSaveCode +
              "})()"
          );
        } catch (error) {
          console.error("error", error);
          logger.error(error);
        }
      }
      if (props.isGenerateModel) {
        const ob = await generateXMLModel({
          Data: newNotice.data,
          Mapping: newNotice.mapping,
          Html: newNotice.html,
          Attachements: newNotice.Attachements,
        });
        await generateModel({
          Data: newNotice.data,
          Mapping: newNotice.mapping,
          Html: newNotice.html,
          Attachements: newNotice.Attachements,
        });
        if (ob) {
          emit("emitXml", ob);
          emit("done", true);
        }
      } else {
        emit("done", false);
        let obj;
        // new document to be created
        if (!store.currentNotice) {
          obj = await saveNotice({
            objectId: props.isFormDisplay.objectId,
            noticeJson: newNotice,
            newDoc: !!route.query.newDoc,
            disableNotice: Boolean(route.query.disableNotice),
          });
          if (obj) {
            const afterSaveCode = internalFormConfig.value.events.find(
              (evnt: any) => evnt.rule.code == "afterSave"
            )?.code;
            if (afterSaveCode) {
              store.setNotice(obj);
              try {
                await eval(
                  "(async () => { const store = useAppStore(); " +
                    afterSaveCode +
                    "})()"
                );
              } catch (error) {
                console.error("error", error);
                logger.error(error);
              }
            }

            emit("done", true);
            // appStore.setLoading(false);
            if (window.self === window.top) {
              location.replace(obj.url);
            } else {
              if (!!route.query.newDoc) {
                parent.location.replace(obj.url);
                return;
              }
              window.parent.postMessage("EliseCustomActionDone", "*");
              parent.location.reload();
            }
          }
        } else {
          obj = await updateNotice({
            objectId: props.isFormDisplay.objectId,
            noticeId: store.currentNotice.id,
            noticeJson: newNotice,
            newDoc: !!route.query.newDoc,
            disableNotice: Boolean(route.query.disableNotice),
          });
          if (obj) {
            const afterSaveCode = internalFormConfig.value.events.find(
              (evnt: any) => evnt.rule.code == "afterSave"
            )?.code;
            if (afterSaveCode) {
              store.setNotice(obj);
              try {
                await eval(
                  "(async () => { const store = useAppStore(); " +
                    afterSaveCode +
                    "})()"
                );
              } catch (error) {
                console.error("error", error);
                logger.error(error);
              }
            }
            emit("done", true);
            //appStore.setLoading(false);
            // httpRequest.setLoading(false);
            if (window.self === window.top) {
              location.replace(obj.url);
              parent.location.reload();
            } else {
              window.parent.postMessage("EliseCustomActionDone", "*");
              parent.location.reload();
            }
          }
        }
        // }
      }
    },
    reject: () => {
      emit("done", false);
    },
    onHide: () => {
      emit("done", false);
    },
  });
};
const httpRequest = useHttpRequest();
const submit = async () => {
  const valid = validateFieldsBeforeSubmit();
  if (valid === false) {
    toast.add({
      severity: "error",
      summary: "Informations manquantes",
      detail: "Veuillez remplir tous les champs obligatoires",
      life: 3000,
    });
    emit("done", 3);
    return;
  } else if (notValidFieldsExists()) {
    toast.add({
      severity: "error",
      summary: "Informations invalides",
      detail: "Veuillez corriger les champs invalides",
      life: 3000,
    });
    emit("done", 3);
    return;
  } else {
    await submitNotice();
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
// function to map the file field to the notice object
const mapFileField = (field: any) => {
  if (!field) {
    return [];
  }
  const mappedFiles = [];
  console.log("field", field);
  for (let i = 0; i < field.length; i++) {
    const file = field[i];
    mappedFiles.push({
      FileB64: "",
      Guid: file.guid,
      fileName: file.fileName,
    });
  }
  console.log("mappedFiles", mappedFiles);
  return mappedFiles;
};

const handleFieldSettingSplitterZone = (pageItem: any) => {
  const columnNames = ["column1", "column2", "column3", "column4"];
  columnNames.forEach((columnName) => {
    for (let z = 0; z < pageItem.rows[columnName].length; z++) {
      const row = pageItem.rows[columnName][z];
      if (row.zone === "ZR" && row.isSection === false) {
        localFields.value[row.code] ??= [];
        handleFieldSettingRepeatableZone(row);
      } else if (row.zone === "ZR" && row.isSection) {
        const columns = ["column1", "column2", "column3", "column4"];
        const item = row.rows["column1"];
        for (let k = 0; k < item.length; k++) {
          columns.forEach((col) => {
            for (let d = 0; d < item[k].rows[col].length; d++) {
              const options = item[k].rows[col][d]?.options;
              if (options) {
                localFields.value[options.name] ??= "";
              }
            }
            // const options = item[col][k]?.options;
            // if (options) {
            //   localFields.value[options.name] ??= "";
            // }
          });
        }
      }
      Object.values(row.rows).forEach((col: any) => {
        Object.values(col).forEach((field: any) => {
          const options = field.options;
          if (options) {
            localFields.value[options.name] ??= "";
          }
        });
      });
    }
  });
};
const handleFieldSettingRepeatableZone = (pageItem: any) => {
  const itemCol = pageItem.rows.column1;
  for (let k = 0; k < itemCol.length; k++) {
    const columns = ["column1", "column2", "column3", "column4"];
    columns.forEach((col) => {
      for (let z = 0; z < itemCol[k].rows[col].length; z++) {
        const options = itemCol[k].rows[col][z]?.options;
        if (options) {
          fiel.value[options.name] ??= "";
        }
      }
    });
  }

  if (Object.keys(fiel.value).length !== 0) {
    localFields.value[pageItem.code].push(Object.assign({}, fiel.value));
    fiel.value = {};
  }
};

const handleFieldSetting = (pageItem: any, clear = false) => {
  if (pageItem.zone === "ZS") {
    handleFieldSettingSplitterZone(pageItem);
  } else if (pageItem.zone === "ZR" && pageItem.isSection === false) {
    localFields.value[pageItem.code] ??= [];
    handleFieldSettingRepeatableZone(pageItem);
  } else if (pageItem.zone === "ZR" && pageItem.isSection) {
    const columnNames = ["column1", "column2", "column3", "column4"];
    const itemZone = pageItem.rows["column1"];
    for (let k = 0; k < itemZone.length; k++) {
      columnNames.forEach((columnName) => {
        for (let z = 0; z < itemZone[k].rows[columnName].length; z++) {
          const options = itemZone[k].rows[columnName][z]?.options;
          if (options) {
            localFields.value[options.name] ??= "";
          }
        }
      });
    }
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
  // const columnNames = ["column1", "column2", "column3", "column4"];
  // columnNames.forEach((columnName) =>
  handleFieldSetting(pageItem, clear);
  // );
};

const localFields: Ref<any> = ref({});
const setFields = (item: any, div: number) => {
  localFields.value ??= {};
  if (props.stepper.isStepper) {
    for (let t = 1; t <= props.stepper.steps; t++) {
      const pages = item[0].pages[`page${t}`];
      for (let d = 0; d < pages.length; d++) {
        processPage(pages[d]);
      }
    }
  } else {
    item.forEach((singleItem: any) => {
      processPage(singleItem);
    });
  }
  if (props.tableFields && Object.keys(props.tableFields).length > 0) {
    Fields.value = { ...Fields.value, ...props.tableFields };
  }
  store.Fields = { ...localFields.value, ...store.Fields };
};

const clearFieldsFunc = (item: any, div: number) => {
  localFields.value ??= {};
  if (props.stepper.isStepper) {
    for (let t = 1; t <= props.stepper.steps; t++) {
      const pages = item[0].pages[`page${t}`];
      for (let d = 0; d < pages.length; d++) {
        processPage(pages[d], true);
      }
    }
  } else {
    item.forEach((singleItem: any) => {
      processPage(singleItem, true);
    });
  }
  Object.keys(localFields.value).forEach((key) => delete Fields.value[key]);
};

const route = useRoute();
const appStore = useHttpRequest();
onUnmounted(() => {
  clearFieldsFunc(itemsFormCopy.value, 0);
});
const isLoadingComponent = ref(true);
const setLocale = () => {
  props.isRTL ? localize("ar") : localize("fr");
};
const HeaderHeight = ref([] as any);
onMounted(async () => {
  GlobalVariables.value = {};
  console.log("onMounted");
  setLocale();
  if (props.showLoader) {
    useHttpRequest().setLoading(true);
  }
  setFields(itemsFormCopy.value, 0);
  internalFormConfig.value = props.configForm;
  QueryParameters.value = { ...route.query };
  // i want to simulate a delay of an await function
  await new Promise((resolve) => setTimeout(resolve, 100));

  internalFormConfig.value?.variables?.forEach((element: any) => {
    Variables.value[element.key] = element.value;
  });
  const afterLoad = ref("" as any);
  internalFormConfig.value?.events?.forEach(async (evnt: any) => {
    if (evnt.code != "" && evnt.rule.code == "beforeLoad") {
      try {
        await eval(
          "(async () => { const store = useAppStore(); " + evnt.code + "})()"
        );
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
  useHttpRequest().setLoading(false);
  if (afterLoad.value) {
    try {
      await eval(
        "(async () => { const store = useAppStore();" +
          afterLoad.value.code +
          "})()"
      );
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
  const headerElement = document.querySelector(
    ".zone-page-header"
  ) as HTMLElement;

  if (headerElement) {
    // Use offsetHeight to get the height of the element
    HeaderHeight.value = headerElement.offsetHeight;
    await nextTick();
    HeaderHeight.value = headerElement.offsetHeight;
  } else {
    console.warn("Header element not found");
  }

  calculatePagesStyle();
});

const handleCollapsed = (event: any) => {
  panelCollapsed.value = event;
};
function arraysEqual(value_array: any, value_to: any) {
  if (value_array.length !== value_to.length) {
    return false;
  }
  for (let i = 0; i < value_array.length; i++) {
    if (value_array[i] !== value_to[i]) {
      return false;
    }
  }
  return true;
}
const handleInputChange = async (item: any) => {
  if (!item) return;
  await nextTick();
  const selectedEvent = item.find((event: any) => event.rule.code === "change");
  if (selectedEvent) {
    try {
      const store = useAppStore();
      await eval(
        "(async () => { const store = useAppStore(); " +
          selectedEvent.code +
          "})()"
      );
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
      const store = useAppStore();
      await eval(
        "(async () => { const store = useAppStore(); " +
          selectedEvent.code +
          "})()"
      );
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
      const store = useAppStore();
      await eval(
        "(async () => { const store = useAppStore(); " +
          selectedEvent.code +
          "})()"
      );
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
      const store = useAppStore();
      await eval(
        "(async () => { const store = useAppStore(); " +
          selectedEvent.code +
          "})()"
      );
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
      const store = useAppStore();
      await eval(
        "(async () => { const store = useAppStore(); " +
          selectedEvent.code +
          "})()"
      );
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
    for (const key in obj) {
      if (Object.prototype.hasOwnProperty.call(obj, key)) {
        result[key] = obj[key];
      }
    }
    return result;
  }, {});
  app.refs = mergedObject;
};
const executeFun = async (event: any) => {
  console.log("event.id", event.id);
  console.log("app.refs", app.refs);
  console.log(" app.refs[event.id]", app.refs[event.id]);
  app.refs[event.id][0].enableLoading();
  try {
    await eval(
      "(async () => { const store = useAppStore(); " + event.code + "})()"
    );
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  } finally {
    app.refs[event.id][0].disableLoading();
  }
};
const handleCodeselected = async (code: string) => {
  try {
    await eval("(async () => { const store = useAppStore(); " + code + "})()");
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  }
};
const searchItemFunc = async (code: string) => {
  try {
    await eval("(async () => { const store = useAppStore(); " + code + "})()");
  } catch (error) {
    console.error("error", error);
    logger.error(error);
  }
};
const repeatableZoneChildrens = ref({} as any);

const duplicate = (event: any) => {
  // repeatableZoneChildrens.value = {
  //   ...repeatableZoneChildrens.value,
  //   [elem.code]: (repeatableZoneChildrens.value[elem.code] || 0) + 1,
  // };
  repeatableZoneChildrens.value = event;
  setFields(itemsFormCopy.value, duplicateClick.value);
};
const deleteDuplicated = (elem: any, index: any) => {
  Fields.value[elem].splice(index, 1);
  repeatableZoneChildrens.value[elem] = repeatableZoneChildrens.value[elem] - 1;
};

const navigatePrevious = (page: any) => {
  console.log("navigatePrevious");
  if (showPageNum.value > 1) {
    showPageNum.value--;
    calculatePagesStyle();
  }
};
const navigateNext = (page: any) => {
  const codeBefore = itemsFormCopy.value[0].config.events[`page${page}`].find(
    (evnt: any) => evnt.rule.code == "beforeFollowing"
  )?.code;
  calculatePagesStyle();
  if (codeBefore) {
    try {
      eval(
        "(async () => { const store = useAppStore(); " + codeBefore + "})()"
      );
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  } else {
    showPageNum.value < props.stepper.steps
      ? showPageNum.value++
      : showPageNum.value;
  }
  executeNavigateNext.value = false;
  // showPageNum.value = page;
};
const navigate = () => {
  showPageNum.value < props.stepper.steps
    ? showPageNum.value++
    : showPageNum.value;
};
function validatePageFields(page: any) {
  let valid = true;
  const pages = itemsFormCopy.value[0].pages[`page${page}`];
  for (let d = 0; d < pages.length; d++) {
    const columnNames = ["column1", "column2", "column3", "column4"];
    columnNames.forEach((columnName) => {
      valid = validateField(pages[d], columnName, valid);
    });
    if (!valid) {
      return false;
    }
  }
  if (!valid) {
    toast.add({
      severity: "error",
      summary: "Informations manquantes ou invalides",
      detail: "Veuillez remplir tous les champs obligatoires",
      life: 3000,
    });
  }
  return valid;
}

const isEmpty = (value: any) => {
  if (value === null || value === undefined) return true; // Null or undefined
  if (
    typeof value === "string" &&
    (value.trim() === "" || value.trim() === "[]")
  )
    return true; // Empty string
  if (Array.isArray(value) && value.length === 0) return true; // Empty array
  if (
    typeof value === "object" &&
    !Array.isArray(value) &&
    Object.keys(value).length === 0
  )
    return true; // Empty object
  return false; // Otherwise, not empty
};

const validateField = (pageItem: any, columnName: string, valid: boolean) => {
  if (pageItem.zone === "ZS") {
    return validateFieldSplitterZone(pageItem, columnName, valid);
  } else if (pageItem.zone === "ZR") {
    Fields.value[pageItem.code] ??= [];
    return validateFieldRepeatableZone(pageItem, columnName, valid);
  } else {
    for (let z = 0; z < pageItem.rows[columnName].length; z++) {
      const options = pageItem.rows[columnName][z]?.options;
      if (options && options.type === "HTML") {
        if (options && options.required && options.hidden !== true) {
          return false;
        }
      } else {
        if (
          options &&
          options.required &&
          options.hidden !== true &&
          isEmpty(Fields.value[options.name])
        ) {
          return false;
        }
      }
    }
  }
  return valid; // Return valid if no changes
};

const validateFieldRepeatableZone = (
  pageItem: any,
  columnName: string,
  valid: boolean
) => {
  const itemCol = pageItem.rows.column1;
  if (pageItem.show === false) {
    return true;
  } else {
    for (let k = 0; k < itemCol.length; k++) {
      for (let z = 0; z < pageItem.rows[columnName].length; z++) {
        const columnNames = ["column1", "column2", "column3", "column4"];
        columnNames.forEach((columnNameZ) => {
          Object.values(pageItem.rows[columnName][z].rows[columnNameZ]).forEach(
            (field: any) => {
              const options = field.options;
              if (
                options &&
                options.required &&
                options.hidden !== true &&
                isEmpty(Fields.value[options.name])
              ) {
                valid = false;
              }
            }
          );
        });
        if (!valid) {
          break;
        }
      }
      if (!valid) {
        break;
      }
    }
    return valid;
  }
};

const validateFieldSplitterZone = (
  pageItem: any,
  columnName: string,
  valid: boolean
) => {
  for (let z = 0; z < pageItem.rows[columnName].length; z++) {
    const row = pageItem.rows[columnName][z];
    if (row.zone === "ZR") {
      Fields.value[row.code] ??= [];
      valid = validateFieldRepeatableZone(row, columnName, valid);
    }
    Object.values(row.rows).forEach((col: any) => {
      Object.values(col).forEach((field: any) => {
        const options = field.options;
        if (
          options &&
          options.required &&
          options.hidden !== true &&
          isEmpty(Fields.value[options.name])
        ) {
          valid = false;
        }
      });
    });
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

const toggleSectionStatus = (
  section: any,
  sectionCode: any,
  toggled: boolean
) => {
  const { column1, column2 } = section.rows;
  const column =
    column1.find((c: any) => c.code === sectionCode) ||
    column2.find((c: any) => c.code === sectionCode);
  if (column) {
    column.toggled = toggled;
    return true;
  }
  return false;
};
function toggleSection(sectionCode: string, toggled: boolean) {
  if (props.stepper.isStepper) {
    for (let i = 1; i <= props.stepper.steps; i++) {
      const page = itemsFormCopy.value[0].pages[`page${i}`];
      const sect = page.find((pageItem: any) =>
        pageItem.zone === "ZS" && pageItem.code !== sectionCode
          ? toggleSectionStatus(pageItem, sectionCode, toggled)
          : pageItem.code === sectionCode
      );
      if (sect) {
        sect.toggled = toggled;
        return;
      }
    }
  } else {
    itemsFormCopy.value.some((section: any) =>
      section.code === "ZS" && section.code !== sectionCode
        ? toggleSectionStatus(section, sectionCode, toggled)
        : section.code === sectionCode && (section.toggled = toggled)
    );
  }
}

const validateFieldsBeforeSubmit = () => {
  let valid = true;
  if (props.stepper.isStepper) {
    for (let i = 1; i <= props.stepper.steps; i++) {
      valid = validatePageFields(i);
      if (!valid) {
        return false;
      }
    }
  } else {
    itemsFormCopy.value.some((section: any) => {
      const columnNames = ["column1", "column2", "column3", "column4"];
      columnNames.forEach((columnName) => {
        valid = validateField(section, columnName, valid);
      });
    });
  }
  return valid;
};

const computePageStyle = computed(() => (pg: any) => {
  return {
    width: "100%",
    marginTop: props.stepper.showPageNames
      ? fixedHeadersHeights.value[pg] + 40 + "px"
      : fixedHeadersHeights.value[pg] + "px",
  };
});
const marginTop = computed(() => {
  console.log(
    "fixedHeadersHeights.value[showPageNum.value]",
    fixedHeadersHeights.value
  );
  return props.stepper.showPageNames
    ? fixedHeadersHeights.value[showPageNum.value] + 40 + "px"
    : calculateHeight("FIXED_HEADER") + "px";
});

async function calculatePagesStyle() {
  for (let pg = 1; pg <= props.stepper.steps; pg++) {
    const page = itemsFormCopy.value[0]?.pages
      ? itemsFormCopy.value[0].pages[`page${pg}`] ?? null
      : null;
    if (!page) {
      return;
    }
    const zones = page.filter((zone: any) => zone.zone === "ZR");
    const zonesWithFixedHeader = zones.filter((zone: any) =>
      zone.code.includes("FIXED_HEADER")
    );
    await nextTick();
    const height = zonesWithFixedHeader.reduce((acc: number, zone: any) => {
      return acc + calculateHeight(zone.code);
    }, 0);
    fixedHeadersHeights.value[pg] = height;
  }
}

const calculateHeight = (zoneCode: string) => {
  const zone = document.querySelector(
    `[data-key="${zoneCode}"]`
  ) as HTMLElement;
  if (zone) {
    (zone.firstChild as HTMLElement).style.top = props.stepper.showPageNames
      ? "40px"
      : "0px";
    return (zone.firstChild as HTMLElement)?.offsetHeight;
  }
  return 0;
};

const stickyHeaderClass = computed(() => {
  return (pg: any, i: any) => {
    const item = itemsFormCopy.value[0].pages[`page${pg}`][i - 1];
    return item?.code?.includes("FIXED_HEADER")
      ? "zone-page-sticky-header"
      : "";
  };
});
async function executeWebService(webServiceName: String, parameters: any) {
  const obj = {
    eliseWsInputType: webServiceName,
    objet: parameters,
  };
  console.log("obj", obj);
  try {
    const result = await callEliseWebService(obj);
    return result;
  } catch (error) {
    console.error("error", error);
    logger.error(error);
    return error;
  }
}
defineExpose({
  itemsForm,
  Fields,
  win,
  panelCollapsed,
  itemsFormCopy,
  repeatableZoneChildrens,
  showPageNum,
  disabledNextButton,
  disabledPreviousButton,
  fetchTableData,
  enablePreviousButtonFunction,
  enableNextButtonFunction,
  disableNextButtonFunction,
  disablePreviousButtonFunction,
  submit,
  handleInputChange,
  executeFun,
  handleCollapsed,
  handleRefs,
  duplicate,
  deleteDuplicated,
  useModel,
  showSection,
  hideSection,
  validatePageFields,
  navigate,
  toggleSection,
  initFields,
  executeWebService,
});
</script>
<style lang="scss">
.componentForm {
  width: 99%;
  height: 100%;
  min-height: 76vh;
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
.ZSTNavigation {
  position: sticky;
  top: 0;
  // height: 7rem;
  // padding: 0 1rem 0 1rem;
  max-width: 100%;
  background-color: #ffffff;
  z-index: 100;
}
.stepper {
  position: relative;
  .pages-headers {
    position: sticky;
    width: 100%;
    top: 0;
    background-color: #ffffff;
    z-index: 1000;
  }
}
.zone-page-sticky-header {
  // position: sticky;
  // top: 0;
  // width: 100%;
  // z-index: 1000;
  // background-color: white;
  // left: 0;
  .zone-page-header {
    position: sticky;
    top: 0;
    width: 100%;
    z-index: 1000;
    background-color: white;
    left: 0;
  }
}
.zone-page-header-parent {
  position: relative;
}

.visibility-hidden {
  visibility: hidden;
}
::-webkit-scrollbar-track {
  margin-top: var(--scrollbar-margin-top);
}
</style>
