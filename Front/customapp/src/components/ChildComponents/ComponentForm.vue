<template>
  <div
    class="componentForm"
    :dir="isRTL ? 'rtl' : 'ltr'"
    :style="{ '--scrollbar-margin-top': marginTop }"
  >
    <!-- Custom Toast with Copy Button -->
    <Toast position="center" group="custom" class="custom-toast-overlay">
      <template #message="slotProps">
        <div class="custom-toast-content">
          <div class="toast-header">
            <i class="pi pi-check-circle success-icon"></i>
            <span class="toast-title">{{ slotProps.message.summary }}</span>
            <button
              @click="closeToast(slotProps)"
              class="close-button"
              title="Close"
            >
              <i class="pi pi-times"></i>
            </button>
          </div>
          <div class="toast-body">
            <div class="chrono-container">
              <span class="chrono-label">Référence:</span>
              <span class="chrono-value">{{ slotProps.message.detail }}</span>
            </div>
            <div class="toast-actions">
              <button
                @click="copyAndClose(slotProps.message.detail, slotProps)"
                class="action-button copy-btn"
                title="Copy chrono and close"
              >
                <i class="pi pi-copy"></i>
                Copy
              </button>
            </div>
          </div>
        </div>
      </template>
    </Toast>

    <!-- Backdrop blur overlay -->
    <div
      v-if="showToastBackdrop"
      class="toast-backdrop"
      @click="closeAllToasts"
    ></div>
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
              {{ $t("FormButtons.previous") }}
            </Button>
          </div>
          <div class="col flex justify-content-end" v-if="isEdit">
            <Button
              v-show="showPageNum < stepper.steps"
              @click="navigateNext(showPageNum)"
            >
              {{ $t("FormButtons.next") }}
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
        >
          {{ $t("FormButtons.validate") }}
        </Button> -->
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
                padding: '0 0 ' + (HeaderHeight - 50) + 'px ' + ' 0 ',
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
  logBlockly,
  callEliseWebService,
  executeWorkflow,
  executeStandalone,
  publishFiles,
  executeAsyncWorkflow,
  executeAsyncStandalone,
  generateModelWithoutNotice,
} from "@/api/api";
import { useConfirm } from "primevue/useconfirm";
import { useToast } from "primevue/usetoast";
import Toast from "primevue/toast";
import { useAppStore } from "@/store/app.store";
import ZoneComponent from "./ZoneComponent.vue";
import { useRoute, useRouter } from "vue-router";
import { useHttpRequest } from "@/store/httpRequest.store";
import { AifileUpload } from "@/api/api";
// import { useComponentStore } from "@/store/component.store";
// import storeMap from "@/main";
import { storeToRefs } from "pinia";
import { localize } from "@vee-validate/i18n";
import { useI18n } from "vue-i18n";
import { cloneDeep } from "lodash";

// Utility class for query parameter decryption
class QueryParameterDecryptor {
  private static readonly SECRET_KEY = "neoform-query-secret-2025";

  static async decryptQueryParams(encryptedData: string): Promise<string> {
    try {
      const encoder = new TextEncoder();
      const decoder = new TextDecoder();

      // Convert from base64
      const combined = new Uint8Array(
        atob(encryptedData)
          .split("")
          .map((c) => c.charCodeAt(0))
      );

      // Extract IV and encrypted data
      const iv = combined.slice(0, 12);
      const encrypted = combined.slice(12);

      // Import the key
      const key = await crypto.subtle.importKey(
        "raw",
        encoder.encode(this.SECRET_KEY.padEnd(32, "0").substring(0, 32)),
        { name: "AES-GCM" },
        false,
        ["decrypt"]
      );

      // Decrypt the data
      const decrypted = await crypto.subtle.decrypt(
        { name: "AES-GCM", iv },
        key,
        encrypted
      );

      return decoder.decode(decrypted);
    } catch (error) {
      console.error("Decryption failed:", error);
      throw error;
    }
  }

  static async decryptQueryParameters(queryParams: any): Promise<any> {
    const decryptedParams: any = {};

    for (const [key, value] of Object.entries(queryParams)) {
      if (key === "code") {
        // Don't decrypt the 'code' parameter
        decryptedParams[key] = value;
      } else if (typeof value === "string" && value) {
        try {
          // Try to decrypt the parameter
          decryptedParams[key] = await this.decryptQueryParams(value);
        } catch (error) {
          console.warn(
            `Failed to decrypt parameter ${key}, using original value:`,
            error
          );
          // If decryption fails, use the original value
          decryptedParams[key] = value;
        }
      } else {
        // Handle non-string values or empty values
        decryptedParams[key] = value;
      }
    }

    return decryptedParams;
  }
}

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
      objectGuid: "",
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
  systemVariables: {
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
  "update:showPageNum",
  "update:executeNavigateNext",
]);
// setup(props, { emit }) {
const store = useAppStore();
const { t } = useI18n();
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
const showToastBackdrop = ref(false);
const internalFormConfig = computed(() => {
  return props.configForm;
});
const systemVariables = computed(() => {
  return props.systemVariables;
});
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
  mappingName: "",
  rackCode: "",
};

// Watch for changes in systemVariables MAPPING_NAME and RACK_CODE
watch(
  () => systemVariables.value.MAPPING_NAME,
  (newMappingName) => {
    if (newMappingName !== undefined) {
      console.log("newMappingName", newMappingName);
      newNotice.mappingName = newMappingName;
    }
  },
  { immediate: true }
);

watch(
  () => systemVariables.value.RACK_CODE,
  (newRackCode) => {
    if (newRackCode !== undefined) {
      console.log("newRackCode", newRackCode);
      newNotice.rackCode = newRackCode;
    }
  },
  { immediate: true }
);

// Remove the duplicate watch statements that appear later
watch(
  () => systemVariables.value.MAPPING_NAME,
  (newMappingName) => {
    if (newMappingName !== undefined) {
      console.log("newMappingName", newMappingName);
      newNotice.mappingName = newMappingName;
    }
  },
  { immediate: true }
);

watch(
  () => systemVariables.value.RACK_CODE,
  (newRackCode) => {
    if (newRackCode !== undefined) {
      console.log("newRackCode", newRackCode);
      newNotice.rackCode = newRackCode;
    }
  },
  { immediate: true }
);

const router = useRouter();

const cancel = () => {
  if (window.self === window.top) {
    router.go(-1);
  } else {
    window.parent.postMessage("EliseCustomActionDone", "*");
    parent.location.reload();
  }
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

watch(itemsForm, (newVal) => {
  itemsFormCopy.value = newVal;
});

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

function uuidv4() {
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0,
      v = c == "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
}
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
// Helper function to process field data based on type
const processFieldData = (element: string, options: any) => {
  const fieldValue = Fields.value[element];
  const related = options?.relatedToElise;
  const fieldType = options?.type;

  switch (fieldType) {
    case "Upload":
      console.log("Processing Upload field");
      if (!options.useAILise && !options.returnBase64) {
        const filesArr = Array.isArray(fieldValue) ? fieldValue : [];
        filesArr.forEach((file) => {
          NoticeAttachements.value.push({
            guid: file.guid,
            fileName: file.fileName,
          });
        });
      }

      if (options.returnBase64) {
        NoticeData.value[element] = mapFileField(fieldValue, true);
      }
      break;

    case "PHOTO":
      console.log("Processing PHOTO field");
      if (!options.returnBase64) {
        const filesArr = Array.isArray(fieldValue) ? fieldValue : [];
        filesArr.forEach((file) => {
          NoticeAttachements.value.push({
            guid: file.guid,
            fileName: file.fileName,
          });
        });
      } else {
        NoticeData.value[element] = mapFileField(fieldValue, true);
      }
      break;

    case "Editor":
      NoticeHtml.value[element] = fieldValue ?? "";
      break;

    case "Table":
      if (options?.isUploadTable) {
        NoticeUploadTable.value[element] = fieldValue ?? "";
        NoticeData.value[element] = fieldValue ?? "";
      } else {
        NoticeData.value[element] = fieldValue ?? "";
      }
      break;

    case "TREEVIEW":
      const keys = Object.keys(fieldValue || {});
      const targetValue =
        options.selectedType === "Organigramme" ? keys[0] ?? "" : keys;
      NoticeData.value[element] = targetValue;
      if (related) {
        NoticeMapping.value[element] = targetValue;
      }
      break;

    case "CUSTOM_TREEVIEW":
      const treeKeys = Object.keys(fieldValue || {});
      const treeValue =
        options.selectionMode === "single" ? treeKeys[0] ?? "" : treeKeys;
      NoticeData.value[element] = treeValue;
      if (related) {
        NoticeMapping.value[element] = treeValue;
      }
      break;

    case "DATE":
      const processedDateValue =
        typeof fieldValue === "string" && fieldValue.includes("T")
          ? fieldValue.split("T")[0]
          : fieldValue;
      NoticeData.value[element] = processedDateValue;
      if (related) {
        NoticeMapping.value[element] = processedDateValue;
      }
      break;

    case "Time":
      let timeOnly = "";
      if (fieldValue) {
        const fullDate = new Date(fieldValue);
        const hours = fullDate.getHours().toString().padStart(2, "0");
        const minutes = fullDate.getMinutes().toString().padStart(2, "0");
        timeOnly = `${hours}:${minutes}`;
      }
      NoticeData.value[element] = timeOnly;
      if (related) {
        NoticeMapping.value[element] = timeOnly;
      }
      break;

    case "FLOWCHART":
      NoticeData.value[element] = fieldValue ?? "";
      if (related) {
        NoticeMapping.value[element] = fieldValue?.id ?? "";
      }
      break;

    default:
      if (fieldType !== "HTML" && options !== undefined) {
        NoticeData.value[element] = fieldValue ?? "";
        if (related && fieldType !== "FLOWCHART") {
          NoticeMapping.value[element] = fieldValue ?? "";
        }
      }
      break;
  }
};

// Helper function to execute before save code
const executeBeforeSaveCode = async () => {
  const beforeSaveCode = internalFormConfig.value?.events.find(
    (evnt: any) => evnt.rule.code == "beforeSave"
  )?.code;

  if (beforeSaveCode) {
    try {
      await eval(
        "(async () => { const store = useAppStore(); " + beforeSaveCode + "})()"
      );
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
};

// Helper function to execute after save code
const executeAfterSaveCode = async (obj: any) => {
  const afterSaveCode = internalFormConfig.value?.events.find(
    (evnt: any) => evnt.rule.code == "afterSave"
  )?.code;

  if (afterSaveCode) {
    store.setNotice(obj);
    try {
      await eval(
        "(async () => { const store = useAppStore(); " + afterSaveCode + "})()"
      );
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
  }
};

// Helper function to handle navigation after save
const handlePostSaveNavigation = (obj: any) => {
  emit("done", true);
  showToastBackdrop.value = true;
  toast.add({
    severity: "success",
    summary: t("ComponentForm.successMessage"),
    detail: obj.chrono,
    group: "custom",
    life: 0, // Make it sticky until user interacts
  });
};

// Function to copy chrono and close toast
const copyAndClose = async (text: string, slotProps: any) => {
  try {
    await navigator.clipboard.writeText(text);
    closeToast(slotProps);
    // Reload after successful copy
    setTimeout(() => {
      parent.location.reload();
    }, 500);
  } catch (err) {
    console.error("Failed to copy text: ", err);
    closeToast(slotProps);
  }
};

// Function to close specific toast
const closeToast = (slotProps: any) => {
  showToastBackdrop.value = false;
  toast.removeGroup("custom");
  // Reload after close
  setTimeout(() => {
    parent.location.reload();
  }, 500);
};

// Function to close all toasts
const closeAllToasts = () => {
  showToastBackdrop.value = false;
  toast.removeAllGroups();
  setTimeout(() => {
    parent.location.reload();
  }, 500);
};

// Helper function to prepare notice data
const prepareNoticeData = async () => {
  // Reset arrays
  Notice.value = [];
  NoticeAttachements.value = [];
  NoticeFiles.value = [];
  NoticeUploadTable.value = [];

  // Process item refs
  itemRefs.value.forEach((elem) => {
    const key = elem.dataset.key;
    NoticeData.value[key] = Fields.value[key] ?? "";
  });

  // Process app refs
  Object.keys(app.refs).forEach((element) => {
    const options = app.refs[element][0]?.options;
    if (options) {
      processFieldData(element, options);
    }
  });

  // Prepare final notice object
  Object.assign(newNotice, {
    Attachements: NoticeAttachements.value,
    data: { ...store.currentNotice?.noticeJson?.data, ...NoticeData.value },
    html: NoticeHtml.value,
    mapping: {
      ...store.currentNotice?.noticeJson?.mapping,
      ...NoticeMapping.value,
    },
    uploadTable: {
      ...store.currentNotice?.noticeJson?.uploadTable,
      ...NoticeUploadTable.value,
    },
    files: NoticeFiles.value,
    mappingName: systemVariables.value.MAPPING_NAME || "",
    rackCode: systemVariables.value.RACK_CODE || "",
  });
};

// Helper function to handle model generation
const handleModelGeneration = async () => {
  const modelData = {
    Data: newNotice.data,
    Mapping: newNotice.mapping,
    Html: newNotice.html,
    Attachements: newNotice.Attachements,
  };

  const ob = await generateXMLModel(modelData);
  await generateModel(modelData);

  if (ob) {
    emit("emitXml", ob);
    emit("done", true);
  }
};

// Helper function to handle notice save/update
const handleNoticeSaveUpdate = async () => {
  emit("done", false);

  const isNewDocument = !store.currentNotice || route.query.newDoc;
  const noticePayload = {
    objectId: props.isFormDisplay.objectId,
    objectGuid: props.isFormDisplay.objectGuid,
    noticeJson: newNotice,
    newDoc: !!route.query.newDoc,
    disableNotice: Boolean(route.query.disableNotice),
  };

  console.log("newNotice", newNotice);

  let obj;
  if (isNewDocument) {
    // put in in try catch and put taost on error
    try {
      obj = await saveNotice(noticePayload as any);
    } catch (error: any) {
      toast.add({
        severity: "error",
        summary: t("ComponentForm.errorMessage"),
        detail:
          error?.response?.data ??
          error?.message ??
          t("ComponentForm.unknownError"),
        life: 3000,
      });
    }
  } else {
    obj = await updateNotice({
      ...noticePayload,
      noticeId: store.currentNotice.id,
    });
  }

  if (obj) {
    await executeAfterSaveCode(obj);
    handlePostSaveNavigation(obj);
  }
};

// Main submit notice function
const submitNotice = async () => {
  try {
    await prepareNoticeData();

    const skipValidation =
      systemVariables.value.DISPLAY_FORM_VALIDATION === false;

    if (skipValidation) {
      httpRequest.setLoading(true);
      await executeBeforeSaveCode();

      if (props.isGenerateModel) {
        await handleModelGeneration();
      } else {
        await handleNoticeSaveUpdate();
      }

      emit("done", false);
    } else {
      confirm.require({
        message: t("ComponentForm.confirmMessage"),
        header: t("ComponentForm.confirmHeader"),
        rejectLabel: t("ComponentForm.confirmNo"),
        rejectClass: "p-button-danger",
        acceptLabel: t("ComponentForm.confirmYes"),
        accept: async () => {
          httpRequest.setLoading(true);
          await executeBeforeSaveCode();
          if (props.isGenerateModel) {
            await handleModelGeneration();
          } else {
            await handleNoticeSaveUpdate();
          }
        },
        reject: () => emit("done", false),
        onHide: () => emit("done", false),
      });
    }
  } catch (error) {
    console.error("Error in submitNotice:", error);
    logger.error(error);
    emit("done", false);
  }
};
const httpRequest = useHttpRequest();
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
const mapFileField = (field: any, base64Only?: boolean) => {
  if (!field) {
    return [];
  }
  const mappedFiles = [];
  console.log("field", field);
  if (base64Only) {
    const files = [];
    for (let i = 0; i < field.length; i++) {
      const file = field[i];
      files.push(file.base64);
    }
    return { File: files };
  }
  for (let i = 0; i < field.length; i++) {
    const file = field[i];
    mappedFiles.push({
      FileB64: file.base64,
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
  logBlockly.info("onMounted");
  GlobalVariables.value = {};
  setLocale();
  if (props.showLoader) {
    useHttpRequest().setLoading(true);
  }
  setFields(itemsFormCopy.value, 0);

  // Decrypt query parameters except 'code'
  try {
    QueryParameters.value =
      await QueryParameterDecryptor.decryptQueryParameters(route.query);
  } catch (error) {
    console.error("Failed to decrypt query parameters:", error);
    // Fallback to original query parameters if decryption fails
    QueryParameters.value = { ...route.query };
  }

  if (!props.isEdit) {
    isLoadingComponent.value = true;
    try {
      // i want to simulate a delay of an await function
      await new Promise((resolve) => setTimeout(resolve, 100));
    } catch (error) {
      console.error("error", error);
      logger.error(error);
    }
    console.log("QueryParameters.value", QueryParameters.value);
    if (QueryParameters.value.fromDoc) {
      try {
        const n = await fetchNotice(QueryParameters.value.noticeType);
        const {
          id,
          noticeJson: { data, mapping, Html, files },
        } = n;
        store.setNotice(n);
        Object.keys(data)?.forEach((v) => {
          Fields.value[v] = data[v];
          if (Fields.value[v].length > 0) {
            repeatableZoneChildrens.value[v] = Fields.value[v].length;
          }
        });
        Object.keys(mapping)?.forEach((v) => {
          Fields.value[v] = mapping[v];
        });
        Object.keys(Html)?.forEach((v) => {
          Fields.value[v] = Html[v];
        });
        // Object.keys(files)?.forEach((v) => {
        //   Fields.value[v] = files[v];
        // });
        // get the fiels with type file from app.refs
        for (let element in app.refs) {
          const options = app.refs[element][0]?.options;
          if (options?.type == "PHOTO") {
            Fields.value[element] = files;
          }
        }
        try {
          const { eliseDocument, metadatas } = await fetchMetadata(
            route.params.guid + ""
          );
          store.setEliseDocument(eliseDocument);
          if (metadatas && metadatas !== undefined) {
            metadatas?.forEach((v: any) => {
              Fields.value[v.key] = v.value;
            });
            isLoadingComponent.value = false;
          }
        } catch (error) {
          console.error("error", error);
          logger.error(error);
        }
        isLoadingComponent.value = false;
      } catch (e) {
        try {
          const { eliseDocument, metadatas } = await fetchMetadata(
            route.params.guid + ""
          );
          store.setEliseDocument(eliseDocument);
          if (metadatas && metadatas !== undefined) {
            metadatas?.forEach((v: any) => {
              Fields.value[v.key] = v.value;
            });
            isLoadingComponent.value = false;
          }
        } catch (error) {
          console.error("error", error);
          logger.error(error);
          isLoadingComponent.value = false;
        }
        isLoadingComponent.value = false;
      } finally {
        isLoadingComponent.value = false;
      }
    }
  }
  internalFormConfig.value?.variables?.forEach((element: any) => {
    Variables.value[element.key] = element.value;
  });
  const afterLoad = ref("" as any);
  internalFormConfig.value?.events?.forEach(async (evnt: any) => {
    if (evnt.code != "" && evnt.rule.code == "beforeLoad") {
      try {
        await new Promise((resolve) => setTimeout(resolve, 100));
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
  console.log("exeeeecccc");
  if (
    !app.refs[event.id] ||
    app.refs[event.id].length === 0 ||
    event.id === undefined
  ) {
    console.log("No reference found for event id:");
    console.warn(`No reference found for event id:`);
    return;
  }
  console.log("event.id", event.id);
  console.log("app.refs", app.refs);
  console.log(" app.refs[event.id]", app.refs[event.id]);
  console.log(" event.code", event.code);
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
      summary: t("ComponentForm.missingOrInvalidInformationsHeader"),
      detail: t("ComponentForm.missingOrInvalidInformationsMessage"),
      life: 3000,
    });
  }
  return valid;
}

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
  let fieldValid = valid;
  if (pageItem.zone === "ZS") {
    return validateFieldSplitterZone(pageItem, columnName, fieldValid);
  } else if (pageItem.zone === "ZR") {
    Fields.value[pageItem.code] ??= [];
    return validateFieldRepeatableZone(pageItem, columnName, fieldValid);
  } else {
    for (let z = 0; z < pageItem.rows[columnName].length; z++) {
      const options = pageItem.rows[columnName][z]?.options;
      if (options && options.type === "HTML") {
        if (options && options.required && options.hidden !== true) {
          fieldValid = false;
        }
      } else if (options) {
        // Required check
        if (
          options.required &&
          options.hidden !== true &&
          isEmpty(Fields.value[options.name])
        ) {
          app.refs[options.name]?.[0]?.setFieldError("Ce champ est requis.");
          fieldValid = false;
        }
        // Rules check: use validateByRule for each rule
        if (
          Array.isArray(options.rules) &&
          !isEmpty(Fields.value[options.name])
        ) {
          console.log("options.rules", options.rules);

          const val = Fields.value[options.name];
          let messages: string[] = [];
          for (const rule of options.rules) {
            const result = validateByRule(
              val ?? "",
              rule,
              options.label || options.name
            );
            console.log("result", result);
            if (!result.valid && result.msg) {
              messages.push(result.msg);
              fieldValid = false;
            }
          }
          if (messages.length > 0) {
            app.refs[options.name]?.[0]?.setFieldError(messages.join("\n"));
          }
        }
      }
    }
  }
  return fieldValid;
};

const validateFieldRepeatableZone = (
  pageItem: any,
  columnName: string,
  valid: boolean
) => {
  const itemCol = pageItem.rows.column1;
  let fieldValid = valid;
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
              if (options && options.hidden !== true) {
                // Required check
                if (options.required && isEmpty(Fields.value[options.name])) {
                  app.refs[options.name]?.[0]?.setFieldError(
                    "Ce champ est requis."
                  );
                  fieldValid = false;
                }
                // Rules check: use validateByRule for each rule
                if (
                  Array.isArray(options.rules) &&
                  !isEmpty(Fields.value[options.name])
                ) {
                  const val = Fields.value[options.name];
                  let messages: string[] = [];
                  for (const rule of options.rules) {
                    const result = validateByRule(
                      val,
                      rule,
                      options.label || options.name
                    );
                    if (!result.valid && result.msg) {
                      messages.push(result.msg);
                      fieldValid = false;
                    }
                  }
                  if (messages.length > 0) {
                    app.refs[options.name]?.[0]?.setFieldError(
                      messages.join("\n")
                    );
                  }
                }
              }
            }
          );
        });
        if (!fieldValid) {
          break;
        }
      }
      if (!fieldValid) {
        break;
      }
    }
    return fieldValid;
  }
};

const validateFieldSplitterZone = (
  pageItem: any,
  columnName: string,
  valid: boolean
) => {
  let fieldValid = valid;
  for (let z = 0; z < pageItem.rows[columnName].length; z++) {
    const row = pageItem.rows[columnName][z];
    if (row.zone === "ZR") {
      Fields.value[row.code] ??= [];
      fieldValid = validateFieldRepeatableZone(row, columnName, fieldValid);
    }
    Object.values(row.rows).forEach((col: any) => {
      Object.values(col).forEach((field: any) => {
        const options = field.options;
        if (options && options.hidden !== true) {
          // Required check
          if (options.required && isEmpty(Fields.value[options.name])) {
            app.refs[options.name]?.[0]?.setFieldError("Ce champ est requis.");
            fieldValid = false;
          }
          // Rules check: use validateByRule for each rule
          if (
            Array.isArray(options.rules) &&
            !isEmpty(Fields.value[options.name])
          ) {
            const val = Fields.value[options.name];
            let messages: string[] = [];
            for (const rule of options.rules) {
              const result = validateByRule(
                val,
                rule,
                options.label || options.name
              );
              if (!result.valid && result.msg) {
                messages.push(result.msg);
                fieldValid = false;
              }
            }
            if (messages.length > 0) {
              app.refs[options.name]?.[0]?.setFieldError(messages.join("\n"));
            }
          }
        }
      });
    });
  }
  return fieldValid;
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

// Universal validation function for rules from ValidationRules.vue
// Returns { valid: boolean, msg: string } for a single rule
function validateByRule(
  value: any,
  rule: { code: string; expression: string },
  fieldLabel?: string
): { valid: boolean; msg: string } {
  if (!rule || typeof rule.code !== "string") return { valid: true, msg: "" };
  console.log("rule", rule);
  console.log("value", value);
  console.log("fieldLabel", fieldLabel);
  // Helper for empty
  const isEmpty = (val: any) => {
    if (val === null || val === undefined) return true;
    if (typeof val === "string" && val.trim() === "") return true;
    if (Array.isArray(val) && val.length === 0) return true;
    if (
      typeof val === "object" &&
      !Array.isArray(val) &&
      Object.keys(val).length === 0
    )
      return true;
    return false;
  };
  const label = fieldLabel || "Ce champ";
  if (isEmpty(value)) {
    if (rule.code === "required") {
      return { valid: false, msg: `${label} est requis.` };
    }
  }
  console.log("rule.code", rule.code);
  switch (rule.code) {
    case "required":
      return {
        valid: !isEmpty(value),
        msg: !isEmpty(value) ? "" : `${label} est requis.`,
      };
    case "min": {
      const min = parseInt(
        rule.expression.split(":")[1] || rule.expression,
        10
      );
      if (typeof value === "string" || Array.isArray(value)) {
        return {
          valid: value.length >= min,
          msg:
            value.length >= min
              ? ""
              : `${label} doit contenir au moins ${min} caractères.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "max": {
      const max = parseInt(
        rule.expression.split(":")[1] || rule.expression,
        10
      );
      if (typeof value === "string" || Array.isArray(value)) {
        return {
          valid: value.length <= max,
          msg:
            value.length <= max
              ? ""
              : `${label} doit contenir au maximum ${max} caractères.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "between": {
      const match = rule.expression.match(/between:(.+) and (.+)/);
      if (match) {
        const min = parseFloat(match[1]);
        const max = parseFloat(match[2]);
        if (typeof value === "number") {
          return {
            valid: value >= min && value <= max,
            msg:
              value >= min && value <= max
                ? ""
                : `${label} doit être entre ${min} et ${max}.`,
          };
        }
        if (typeof value === "string" || Array.isArray(value)) {
          return {
            valid: value.length >= min && value.length <= max,
            msg:
              value.length >= min && value.length <= max
                ? ""
                : `${label} doit contenir entre ${min} et ${max} caractères.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "timeBetween":
    case "dateBetween": {
      const match = rule.expression.match(/between:([\d/: ]+) and ([\d/: ]+)/);
      if (match) {
        const from = match[1].trim();
        const to = match[2].trim();
        let valStr = value;
        if (value instanceof Date) {
          valStr = value.getHours() + ":" + value.getMinutes();
        }
        if (typeof valStr === "string") {
          return {
            valid: valStr >= from && valStr <= to,
            msg:
              valStr >= from && valStr <= to
                ? ""
                : `${label} doit être entre ${from} et ${to}.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "confirmed": {
      return { valid: true, msg: "" };
    }
    case "digits": {
      const match = rule.expression.match(/digits:(\d+)/);
      if (match) {
        const len = parseInt(match[1], 10);
        return {
          valid:
            typeof value === "string" &&
            value.length === len &&
            /^\d+$/.test(value),
          msg:
            typeof value === "string" &&
            value.length === len &&
            /^\d+$/.test(value)
              ? ""
              : `${label} doit contenir exactement ${len} chiffres.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "dimensions": {
      return { valid: true, msg: "" };
    }
    case "ext": {
      const match = rule.expression.match(/ext:([\w,]+)/);
      if (match) {
        const allowed = match[1].split(",");
        if (typeof value === "string") {
          const ext = value.split(".").pop();
          return {
            valid: allowed.includes(ext as any),
            msg: allowed.includes(ext as any)
              ? ""
              : `${label} doit avoir l'une des extensions suivantes : ${allowed.join(
                  ", "
                )}.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "image": {
      return { valid: true, msg: "" };
    }
    case "integer": {
      return {
        valid: /^-?\d+$/.test(String(value)),
        msg: /^-?\d+$/.test(String(value))
          ? ""
          : `${label} doit être un entier.`,
      };
    }
    case "is": {
      const match = rule.expression.match(/is:(.+)/);
      if (match) {
        return {
          valid: String(value) === match[1],
          msg:
            String(value) === match[1] ? "" : `${label} doit être ${match[1]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "is_not": {
      const match = rule.expression.match(/is_not:(.+)/);
      if (match) {
        return {
          valid: String(value) !== match[1],
          msg:
            String(value) !== match[1]
              ? ""
              : `${label} ne doit pas être ${match[1]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "length": {
      const match = rule.expression.match(/length:(\d+)/);
      if (match) {
        const len = parseInt(match[1], 10);
        return {
          valid:
            (typeof value === "string" || Array.isArray(value)) &&
            value.length === len,
          msg:
            (typeof value === "string" || Array.isArray(value)) &&
            value.length === len
              ? ""
              : `${label} doit contenir exactement ${len} caractères.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "max_value": {
      const match = rule.expression.match(/max_value:(\d+)/);
      if (match) {
        const max = parseInt(match[1], 10);
        return {
          valid: typeof value === "number" && value <= max,
          msg:
            typeof value === "number" && value <= max
              ? ""
              : `${label} doit être inférieur ou égal à ${max}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "min_value": {
      const match = rule.expression.match(/min_value:(\d+)/);
      if (match) {
        const min = parseInt(match[1], 10);
        return {
          valid: typeof value === "number" && value >= min,
          msg:
            typeof value === "number" && value >= min
              ? ""
              : `${label} doit être supérieur ou égal à ${min}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "mimes": {
      const match = rule.expression.match(/mimes:([\w,]+)/);
      if (match) {
        const allowed = match[1].split(",");
        if (typeof value === "string") {
          const ext = value.split(".").pop();
          return {
            valid: allowed.includes(ext as any),
            msg: allowed.includes(ext as any)
              ? ""
              : `${label} doit être de l'un des types suivants : ${allowed.join(
                  ", "
                )}.`,
          };
        }
      }
      return { valid: true, msg: "" };
    }
    case "not_one_of": {
      const match = rule.expression.match(/not_one_of:([^,]+),([^,]+)/);
      if (match) {
        return {
          valid: value !== match[1] && value !== match[2],
          msg:
            value !== match[1] && value !== match[2]
              ? ""
              : `${label} ne doit pas être ${match[1]} ou ${match[2]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "one_of": {
      const match = rule.expression.match(/one_of:([^,]+),([^,]+)/);
      if (match) {
        return {
          valid: value === match[1] || value === match[2],
          msg:
            value === match[1] || value === match[2]
              ? ""
              : `${label} doit être ${match[1]} ou ${match[2]}.`,
        };
      }
      return { valid: true, msg: "" };
    }
    case "regex": {
      let pattern = rule.expression;
      let flags = "";
      const regexParts = pattern.match(/^\/([^/]+)\/(\w*)$/);
      if (regexParts) {
        pattern = regexParts[1];
        flags = regexParts[2];
      } else if (pattern.startsWith("/") && pattern.endsWith("/")) {
        pattern = pattern.slice(1, -1);
      }
      try {
        const regex = new RegExp(pattern, flags);
        return {
          valid: regex.test(String(value)),
          msg: regex.test(String(value))
            ? ""
            : `${label} a un format invalide.`,
        };
      } catch (e) {
        return { valid: false, msg: `${label} a un format invalide.` };
      }
    }
    case "size": {
      return { valid: true, msg: "" };
    }
    case "url": {
      const valid =
        /^(https?:\/\/)?([\w\-]+\.)+[\w\-]+(\/[\w\-._~:/?#[\]@!$&'()*+,;=]*)?$/.test(
          String(value)
        );
      return {
        valid,
        msg: valid ? "" : `${label} doit être une URL valide.`,
      };
    }
    case "timeAfter":
    case "timeBefore": {
      const match = rule.expression.match(/(after|before):(\d{1,2}:\d{1,2})/);
      if (match) {
        const ref = match[2];
        let valStr = value;
        if (value instanceof Date) {
          valStr =
            value.getHours().toString().padStart(2, "0") +
            ":" +
            value.getMinutes().toString().padStart(2, "0");
        }
        if (typeof valStr === "string") {
          if (rule.code.includes("After")) {
            return {
              valid: valStr > ref,
              msg: valStr > ref ? "" : `${label} doit être après ${ref}.`,
            };
          }
          if (rule.code.includes("Before")) {
            return {
              valid: valStr < ref,
              msg: valStr < ref ? "" : `${label} doit être avant ${ref}.`,
            };
          }
        }
      }
      return { valid: true, msg: "" };
    }
    case "dateAfter":
    case "dateAfterToday":
    case "dateBefore":
    case "dateBeforeToday": {
      const match = rule.expression.match(
        /(after|before):(\d{1,2})\/(\d{1,2})\/(\d{4})/
      );
      if (match) {
        const refDay = parseInt(match[2], 10);
        const refMonth = parseInt(match[3], 10) - 1;
        const refYear = parseInt(match[4], 10);
        const refDate = new Date(refYear, refMonth, refDay);
        let valDate;
        if (value instanceof Date) {
          valDate = value;
        } else if (
          typeof value === "string" &&
          /^\d{4}-\d{2}-\d{2}/.test(value)
        ) {
          valDate = new Date(value);
        } else if (
          typeof value === "string" &&
          /\d{1,2}\/\d{1,2}\/\d{4}/.test(value)
        ) {
          const [d, m, y] = value.split("/").map(Number);
          valDate = new Date(y, m - 1, d);
        }
        if (valDate instanceof Date && !isNaN(valDate.getTime())) {
          if (rule.code.includes("After")) {
            return {
              valid: valDate > refDate,
              msg:
                valDate > refDate
                  ? ""
                  : `${label} doit être après ${match[0].split(":")[1]}.`,
            };
          }
          if (rule.code.includes("Before")) {
            return {
              valid: valDate < refDate,
              msg:
                valDate < refDate
                  ? ""
                  : `${label} doit être avant ${match[0].split(":")[1]}.`,
            };
          }
        }
      }
      return { valid: true, msg: "" };
    }
    case "dateIsNot": {
      return { valid: true, msg: "" };
    }
    case "disabledDateRange":
    case "disabledMonthDays":
    case "disabledWeekDays": {
      return { valid: true, msg: "" };
    }
    case "email": {
      const valid = /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(String(value));
      return {
        valid,
        msg: valid ? "" : `${label} doit être une adresse e-mail valide.`,
      };
    }
    case "numeric": {
      const valid = /^-?\d*(\.\d+)?$/.test(String(value));
      return {
        valid,
        msg: valid ? "" : `${label} doit être un nombre.`,
      };
    }
    case "alpha": {
      const valid = /^[A-Za-z]+$/.test(String(value));
      return {
        valid,
        msg: valid ? "" : `${label} doit contenir uniquement des lettres.`,
      };
    }
    case "alpha_num": {
      const valid = /^[A-Za-z0-9]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : `${label} doit contenir uniquement des lettres et des chiffres.`,
      };
    }
    case "alpha_dash": {
      const valid = /^[A-Za-z0-9_-]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : `${label} doit contenir uniquement des lettres, des chiffres, des tirets ou des underscores.`,
      };
    }
    case "alpha_spaces": {
      const valid = /^[A-Za-z\s]+$/.test(String(value));
      return {
        valid,
        msg: valid
          ? ""
          : `${label} doit contenir uniquement des lettres et des espaces.`,
      };
    }
    default:
      return { valid: true, msg: "" };
  }
}

// Validate all rules for a field, return { valid, messages[] }
function validateFieldAllRules(
  value: any,
  rules: { code: string; expression: string }[],
  fieldLabel?: string
) {
  let valid = true;
  let messages: string[] = [];
  for (const rule of rules) {
    const result = validateByRule(value, rule, fieldLabel);
    if (!result.valid && result.msg) {
      valid = false;
      messages.push(result.msg);
    }
  }
  return { valid, messages };
}
async function internalExecuteWorkflow(parameters: any) {
  return await executeWorkflow(parameters);
}
async function internalExecuteStandalone(parameters: any) {
  return await executeStandalone(parameters);
}
function internalExecuteAsyncWorkflow(parameters: any) {
  executeAsyncWorkflow(parameters);
}
function internalExecuteAsyncStandalone(parameters: any) {
  executeAsyncStandalone(parameters);
}
async function internalGenerateModel(parameters: any) {
  return await generateModelWithoutNotice(parameters);
}
async function showConfirmationDialog(
  message: string,
  header: string,
  acceptLabel: string,
  rejectLabel: string,
  acceptFn: Function,
  rejectFn: Function
) {
  confirm.require({
    message: message,
    header: header,
    rejectLabel: rejectLabel,
    acceptLabel: acceptLabel,
    accept: () => acceptFn(),
    reject: () => rejectFn(),
  });
}
async function internalPublishFiles(parameters: any) {
  return await publishFiles(parameters);
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
  internalExecuteWorkflow,
  internalExecuteStandalone,
  internalExecuteAsyncWorkflow,
  internalExecuteAsyncStandalone,
  internalGenerateModel,
  showConfirmationDialog,
  internalPublishFiles,
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
    width: 200vw;
    top: 0;
    background-color: #f8f9fa;
    z-index: 1000;
    margin-left: -50rem;
    padding-left: 50rem;
  }
}

/* RTL adjustment for stepper headers */
[dir="rtl"] .stepper .pages-headers {
  margin-left: 0;
  padding-left: 0;
  padding-right: 50rem;
  margin-right: -50rem;
}

.dark .stepper .pages-headers {
  background-color: #121212;
}

.zone-page-sticky-header {
  // position: sticky;
  // top: 0;
  // width: 100%;
  // z-index: 1000;
  // background-color: white;
  // left: 0;
  .zone-page-header {
    position: fixed;
    width: calc(100% - 100px);
    max-width: 1200px;
    left: 50%;
    transform: translateX(-50%);
    justify-content: space-between;
    box-shadow: 0 -2px 4px 0 rgba(0, 0, 0, 0.1);
    padding: 15px;
    border-radius: 20px;
    background-color: white;
    z-index: 1000;
    margin: 0;
  }
}
.zone-page-header-parent {
  position: relative;
}

.visibility-hidden {
  visibility: hidden;
}

/* Toast backdrop blur */
.toast-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(5px);
  z-index: 9998;
}

/* Custom toast overlay */
.custom-toast-overlay {
  z-index: 9999 !important;
}

/* Custom toast content */
.custom-toast-content {
  background: white;
  border-radius: 12px;
  padding: 0;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
  border: 1px solid #e5e5e5;
  min-width: 300px;
  max-width: 500px;
  overflow: hidden;
}

/* Toast header */
.toast-header {
  display: flex;
  align-items: center;
  padding: 16px 20px;
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.success-icon {
  font-size: 20px;
  margin-right: 12px;
}

.toast-title {
  flex: 1;
  font-weight: 600;
  font-size: 16px;
}

.close-button {
  background: transparent;
  border: none;
  color: white;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
}

.close-button:hover {
  background: rgba(255, 255, 255, 0.2);
}

/* Toast body */
.toast-body {
  padding: 20px;
}

.chrono-container {
  display: flex;
  align-items: center;
  margin-bottom: 20px;
  padding: 12px;
  background: #f8f9fa;
  border-radius: 8px;
  border-left: 4px solid #22c55e;
}

.chrono-label {
  font-weight: 600;
  color: #374151;
  margin-right: 8px;
}

.chrono-value {
  font-family: "Courier New", monospace;
  font-weight: bold;
  color: #22c55e;
  font-size: 16px;
  background: white;
  padding: 4px 8px;
  border-radius: 4px;
  border: 1px solid #e5e5e5;
}

/* Toast actions */
.toast-actions {
  display: flex;
  justify-content: center;
}

.action-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  font-size: 14px;
  transition: all 0.2s;
  min-width: 120px;
  justify-content: center;
}

.copy-btn {
  background: #22c55e;
  color: white;
}

.copy-btn:hover {
  background: #16a34a;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
}

.copy-btn i {
  font-size: 14px;
}
.p-toast-center {
  min-width: 20vw;
  transform: translate(-50%, -50%);
  width: fit-content !important;
}
.p-toast-close-button {
  display: none !important;
}
::-webkit-scrollbar-track {
  margin-top: var(--scrollbar-margin-top);
}
</style>
