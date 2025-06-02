<template>
  <div class="form-viewer-container" :dir="isRTL ? 'rtl' : 'ltr'">
    <div class="form-viewer-container-header" v-if="showFormHeader">
      <div class="flex justify-content-start">
        <div v-if="formName">{{ formName }}</div>
      </div>
    </div>
    <div class="form-viewer-container-content">
      <div v-if="form.length === 0">
        <div></div>
      </div>
      <div v-else>
        <div class="form-viewer">
          <component-form
            @done="handleDone"
            v-model="form"
            @update:isSubmit="handleIsSubmit"
            :isSubmit="isSubmit"
            :isFormDisplay="{
              value: true,
              objectId: object?.id,
              objectGuid: object?.guid,
            }"
            :configForm="configForm"
            :stepper="{
              isStepper: isStepper,
              steps: steps,
              showPageNames: showPageNames,
              names: names,
            }"
            :isRTL="isRTL"
            :showPageNum="showPageNumF"
            @update:showPageNum="showPageNumF = $event"
            :executeNavigateNext="executeNavigateNext"
            @update:executeNavigateNext="executeNavigateNext = $event"
            :language="language"
          ></component-form>
        </div>
      </div>
    </div>
    <div
      class="form-viewer-container-footer flex justify-content-end"
      :class="{ 'custom-padding-rtl': isRTL }"
      v-if="isFormDisplay.value && form.length !== 0"
    >
      <div class="flex justify-content-end gap-1">
        <div class="col flex justify-content-start gap-1">
          <!-- <neo-select
            v-if="isMultilingual"
            class="ml-2 pr-4"
            v-model="language"
            :items="languagesList"
            :isParentNeoTable="true"
          ></neo-select> -->
          <div>
            <Button
              v-if="!newDoc"
              v-show="showPageNumF === 1 || showPageNames"
              @click="cancel"
              class="mr-2"
            >
              {{ cancelButtonText }}
            </Button>
          </div>
          <div>
            <Button
              v-show="showPageNumF > 1 && !showPageNames"
              @click="showPageNumF > 1 ? showPageNumF-- : showPageNumF"
              class="mr-2"
            >
              {{ previousButtonText }}
            </Button>
          </div>
        </div>
        <div class="col flex justify-content-end gap-1">
          <div>
            <Button
              v-show="showPageNumF < steps && !showPageNames"
              @click="navigateToPage(showPageNumF + 1)"
            >
              {{ nextButtonText }}
            </Button>
          </div>
          <div>
            <Button
              v-if="showPageNumF === steps || steps === 0"
              @click="submit()"
              class="ml-2"
            >
              {{ submitButtonText }}
            </Button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import {
  computed,
  defineComponent,
  onBeforeMount,
  onMounted,
  ref,
  watch,
  type Ref,
} from "vue";
import { useRoute, useRouter } from "vue-router";
import { fetchOneObject } from "@/api/api";
import { useI18n } from "vue-i18n";
import { i18n } from "@/main"; // Import i18n from main.ts
import { useAppStore } from "@/store/app.store";
import { useHttpRequest } from "@/store/httpRequest.store";

export default defineComponent({
  setup() {
    const { t } = useI18n();
    const appStore = useAppStore();
    const httpRequest = useHttpRequest();
    const object: Ref<ObjectModel | null> = ref(null);
    const route = useRoute();
    const router = useRouter();
    const formID = ref(route.params.guid as string);
    const done = ref(0);
    const isStepper = ref(false);
    const isRTL = ref(false);
    const isMultilingual = ref(false);
    const steps = ref(0);
    const showPageNames = ref(false);
    const names = ref({} as any);
    const formName = ref();
    const language = ref("FR");
    const paramValue = ref({ ...route.query });
    const showFormHeader = ref(paramValue.value.showFormHeader === "true");
    const newDoc = ref(paramValue.value.newDoc === "true");
    const showPageNumF = ref(1);

    const arabicChoice = computed(() => (isRTL.value ? "عربي" : "Arabe"));
    const frenchChoice = computed(() => (isRTL.value ? "فرنسي" : "Français"));
    const englishChoice = computed(() => (isRTL.value ? "إنجليزي" : "Anglais"));

    const languages: Ref<any[]> = ref([]);
    const languagesList: Ref<any[]> = ref([]);

    onBeforeMount(async () => {
      appStore.setExternalAuth(
        route.query.code as string,
        route.params.guid as string
      );
      console.log("Auth Updated");
      object.value = await fetchOneObject(formID.value);
      const localFormConfig = ref(
        JSON.parse(object.value?.objectJson).objectConfig.formConfig
      );
      formName.value = localFormConfig.value.formName;
      isStepper.value = localFormConfig.value.isStepper;
      isRTL.value = localFormConfig.value.isRTL;
      isMultilingual.value = localFormConfig.value.isMultilingual;
      languages.value = localFormConfig.value.languages;
      // languagesList.value = convertLanguages(languages.value);
      console.log("languages.value", languages.value);
      steps.value = localFormConfig.value.stepNumber;
      showPageNames.value = localFormConfig.value.showPageNames;
      if (isStepper.value) {
        names.value = JSON.parse(
          object.value?.objectJson
        ).objectConfig.formTemplate[0].config.names;
      }
      i18n.global.locale.value = isRTL.value ? "arabic" : "french";
    });

    onMounted(() => {
      // removeCodeFromUrl();
    });

    const removeCodeFromUrl = () => {
      // Get the current URL
      const url = window.location.href;

      // Find the position of the query parameter
      const queryStartIndex = url.indexOf("?");

      if (queryStartIndex !== -1) {
        // Remove the query parameter part
        const cleanUrl = url.substring(0, queryStartIndex);

        // Update the URL without reloading the page
        window.history.replaceState({}, document.title, cleanUrl);
      }
    };
    const form: Ref<any[]> = computed(() => {
      return object.value
        ? JSON.parse(object.value.objectJson).objectConfig.formTemplate
        : [];
    });
    const configForm: Ref<any[]> = computed(() => {
      return object.value
        ? JSON.parse(object.value.objectJson).objectConfig.formConfig
        : [];
    });
    const isFormDisplay = ref({
      value: true,
      objectId: object.value?.id ?? "65",
    });
    const isSubmit = ref(false);
    const submit = () => {
      isSubmit.value = true;
    };
    const cancel = () => {
      if (window.self === window.top) {
        router.go(-1);
      } else {
        window.parent.postMessage("EliseCustomActionDone", "*");
      }
    };
    const handleIsSubmit = (value: boolean) => {
      isSubmit.value = value;
    };
    const handleDone = (event: any) => {
      if (event == 1) {
        done.value = 1;
      } else {
        done.value = 2;
      }
      document.body.classList.remove("grayOutBody");
    };
    const showForm = computed(() => {
      return done.value;
    });
    const dynamicHeaderHeight = computed(() => {
      return showFormHeader.value ? "50px" : "0px";
    });
    document.documentElement.style.setProperty(
      "--dynamic-header-height",
      dynamicHeaderHeight.value
    );
    const navigateNext = (page: number) => {
      showPageNumF.value = page + 1;
    };
    const submitStepper = () => {
      done.value = 1;
    };
    const executeNavigateNext = ref(false);
    const navigateToPage = (page: number) => {
      if (page > steps.value || page < 1) {
        return;
      }
      executeNavigateNext.value = true;
    };

    // Computed properties for button texts
    const cancelButtonText = computed(() => t("buttons.cancel"));
    const previousButtonText = computed(() => t("buttons.previous"));
    const nextButtonText = computed(() => t("buttons.next"));
    const submitButtonText = computed(() => t("buttons.validate"));

    // Function to convert language codes to objects with code and name
    // const convertLanguages = (codes: string[]) => {
    //   const result = codes
    //     .map((code) => {
    //       if (code === "AR") {
    //         return { code: "AR", name: arabicChoice.value };
    //       } else if (code === "ENG") {
    //         return { code: "ENG", name: englishChoice.value };
    //       }
    //       return null;
    //     })
    //     .filter(Boolean);

    //   // Always add French language
    //   result.push({ code: "FR", name: frenchChoice.value });

    //   return result;
    // };
    // Example usage

    return {
      form,
      formID,
      object,
      isFormDisplay,
      isSubmit,
      done,
      configForm,
      showForm,
      isStepper,
      isRTL,
      steps,
      showPageNames,
      names,
      formName,
      showFormHeader,
      dynamicHeaderHeight,
      showPageNumF,
      executeNavigateNext,
      newDoc,
      language,
      languages,
      isMultilingual,
      languagesList,
      t,
      submit,
      handleIsSubmit,
      handleDone,
      cancel,
      navigateNext,
      submitStepper,
      navigateToPage,
      cancelButtonText,
      previousButtonText,
      nextButtonText,
      submitButtonText,
    };
  },
});
</script>

<style lang="scss">
.form-viewer-container {
  position: relative;
  &-header {
    padding: 15px;
    box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
    color: #266c87;
    position: fixed;
    width: 100%;
    top: 0;
    z-index: 1000;
    background-color: white;
  }
  &-content {
    padding: 15px;
    margin-top: var(--dynamic-header-height);
    margin-bottom: 50px;
    overflow-y: auto;
    height: calc(100% - 100px);
    .form-viewer {
      width: 100%;
      overflow-y: auto;
      overflow-x: hidden;
    }
  }
  &-footer {
    padding: 15px;
    box-shadow: 0 -2px 4px 0 rgba(0, 0, 0, 0.1);
    color: #266c87;
    z-index: 1000 !important;
    position: fixed;
    width: 100%;
    bottom: 0;
    background-color: white;
  }
}
.form-container {
  max-height: 100vh !important;
  padding: unset !important;
}
.main-container {
  background-color: white !important;
}
.form-viewer-container-content {
  padding: 15px;
  margin-top: var(--dynamic-header-height);
  margin-bottom: 60px; /* Footer height */
  height: calc(
    100vh - var(--dynamic-header-height) - 100px
  ); /* 50px is the footer height */
  overflow-y: auto; /* Enable vertical scrolling */
}
.form-viewer-container-footer {
  padding: 15px;
  box-shadow: 0 -2px 4px 0 rgba(0, 0, 0, 0.1);
  color: #266c87;
  position: fixed;
  width: 100%;
  bottom: 0;
  background-color: white;
  height: 60px; /* Fixed footer height */
  z-index: 1000 !important;
}
.form-viewer-container-content {
  scroll-behavior: smooth; /* Smooth scrolling */
}
.custom-padding-rtl {
  padding-left: 60px;
}
.stepper {
  position: relative;
  .pages-headers {
    position: fixed;
    width: 100%;
    top: 0;
    background-color: #ffffff;
    z-index: 1000;
  }
}
.zone-page-sticky-header {
  .zone-page-header {
    position: fixed;
    top: 0;
    width: 100%;
    z-index: 1000;
    background-color: white;
    left: 0;
  }
}
.main-container {
  display: flex;
  overflow-y: auto;
  padding: 10px 5px 0px 5px;
  background-color: #efefef;
  height: 90vh !important;
  .form-container {
    padding: 15px;
    background-color: rgb(255, 255, 255);
    width: 100%;
    border-radius: 10px;
    overflow-x: hidden;
    overflow-y: hidden;
    height: 100%;
  }
}
</style>
