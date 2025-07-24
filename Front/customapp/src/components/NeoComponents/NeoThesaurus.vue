<template>
  <div class="neoThesaurusExternal" v-show="!isHidden">
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
      class="neoThesaurus-container input-container"
      :class="{ 'disabled-wrapper': isDisabled, 'readOnly-wrapper': readOnly }"
      ref="neoThesaurusRef"
    >
      <AutoComplete
        multiple
        :typeahead="false"
        ref="chip"
        v-model="terms"
        :label="options.label"
        :required="options.required"
        :hidden="options.hidden"
        :max="termLimitConfig"
        :placeholder="terms.length !== 0 ? '' : 'Ajouter un terme...'"
        @click="openThesaurusFrame"
        @focus="$emit('focus', $event)"
        @blur="$emit('blur', $event)"
        @mouseenter="$emit('mouseenter', $event)"
        @mouseleave="$emit('mouseleave', $event)"
        :pt="{ input: { readonly: 'readonly' } }"
      >
        <template #chip="slotProps">
          <!-- <div> -->
          <span
            >{{
              slotProps.value.isSyno
                ? slotProps.value.syno.label + " / " + slotProps.value.label
                : slotProps.value.label
            }}
            <Button
              style="
                height: 20px !important;
                width: 20px !important ;
                color: white !important;
              "
              text
              icon="pi pi-times-circle"
              @click="removeItem(slotProps.value)"
            >
            </Button>
          </span>
          <!-- </div> -->
        </template>
      </AutoComplete>
    </div>
    <OverlayPanel
      ref="op"
      class="thesaurus-frame"
      :class="{ 'thesaurus-frame-top': showOnTop }"
      :append-to="neoThesaurusRef"
      style="width: 80%"
    >
      <!-- :style="{ width: maxWidth + 'px' }" -->
      <div v-if="isSelectedTermLimit" class="thesaurus-limit truncated">
        <span class="limit-label truncated"> Limite des termes atteinte</span>
      </div>
      <div class="thesaurus-header">
        <span class="thesaurus-label">{{ options.label }}</span>
        <Button
          icon="pi pi-times"
          class="button-cancel"
          @click="closeThesaurusFrame()"
          text
        />
      </div>
      <IconField iconPosition="right" class="search-terms">
        <InputIcon
          class="pi pi-delete-left"
          v-if="searchTermValue.length > 0"
          @click="resetSearchTerm()"
        />
        <InputText
          v-model="searchTermValue"
          placeholder="Search"
          @update:modelValue="searchThesaurusTerm"
        />
      </IconField>
      <div v-if="thesaurusStateIsLoading" class="thesaurus-terms">
        <div class="spinner-section">
          <i class="pi pi-spin pi-sync"></i>
        </div>
      </div>
      <div v-else class="search-results">
        <div
          v-if="showCurrentPath"
          ref="currentPathContainer"
          class="current-path-container"
        >
          <Breadcrumb
            :home="home"
            :model="itemsPath"
            style="border: 0px; padding: 0px"
          />
        </div>
        <span v-else class="results-label truncated">
          résultat ({{ thesaurusTerms.length }})
        </span>
        <div v-if="thesaurusTerms.length === 0" class="no-result">
          <span class="no-result-label truncated">
            Aucun résultat de recherche de terme
          </span>
        </div>
        <div v-else :class="getResultContainerClasses">
          <div class="nui-scroll-bar-container nui-scroll-bar">
            <ScrollPanel style="width: 100%; height: 160px">
              <div class="result-terms">
                <div
                  v-for="term in thesaurusTerms"
                  :key="term.termId"
                  :class="getResultTermClasses(term)"
                >
                  <div
                    :class="getResultTermLeftPartClasses(term)"
                    :style="normalizeAndCompare(term.label, searchTermValue)"
                    :title="term.label"
                    @click="addTerm(term)"
                  >
                    <span
                      v-if="term.isSyno"
                      v-tooltip="`${term.syno?.label}`"
                      class="truncated"
                      >{{ term.label }}</span
                    >
                    <span v-else class="truncated">{{ term.label }}</span>
                  </div>
                  <div
                    v-if="!isLeaf(term)"
                    class="result-term-part result-term-right"
                  >
                    <Button
                      style="min-width: 2rem; padding: 0 !important"
                      class="navigate-term-btn"
                      label="↩"
                      @click="loadTermChildren(term)"
                    >
                    </Button>
                  </div>
                </div>
              </div>
            </ScrollPanel>
          </div>
        </div>
      </div>
    </OverlayPanel>
  </div>
  <!-- Error message display -->
  <small class="p-error" id="text-error" v-if="errorState.errorMessage">
    {{ errorState.errorMessage || "&nbsp;" }}
  </small>
</template>

<script lang="ts">
import { eliseLevelThesaurus, eliseSearchThesaurus } from "@/api/api";
import { computed, ref, watch, Teleport, type Ref, reactive } from "vue";
import _ from "lodash";

export default {
  props: {
    label: String,
    label_AR: String,
    label_ENG: String,
    modelValue: String,
    options: {
      type: Object,
      default: () => ({
        label_AR: "",
        label_ENG: "",
        name: "",
        label: "Field",
        required: false,
        readonly: false,
        disabled: false,
        hidden: false,
        thesaurusId: "",
        termLimit: 1,
        rules: [],
        events: [],
      }),
    },
    prefix: {
      type: String,
      default: "",
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
    "update:options",
    "update:modelValue",
    "errorMessage",
    "focus",
    "blur",
    "mouseleave",
    "mouseenter",
  ],
  component: {
    Teleport,
  },
  setup(props, { emit }) {
    const terms: Ref<Term[]> = ref([]);
    const showOnTop = ref(false);
    watch(
      terms,
      async (newValue) => {
        emit("update:modelValue", JSON.stringify(newValue));
      },
      { deep: true }
    );
    watch(
      () => props.modelValue,
      async (newValue, oldValue) => {
        terms.value = JSON.parse(newValue || "[]");
        // emit("update:modelValue", JSON.stringify(newValue));
      }
    );
    const maxWidth = ref(100);
    const home: Ref<any> = ref({
      icon: "pi pi-home",
      command: ({
        item,
        originalEvent,
      }: {
        item: Term;
        originalEvent: Event;
      }) => {
        originalEvent.preventDefault();
        loadRootTermLevel();
      },
    });
    const neoThesaurusRef = ref();
    const computedStyle = computed(() => {
      return {
        width: neoThesaurusRef.value?.getBoundingClientRect().width
          ? `${neoThesaurusRef.value?.getBoundingClientRect().width}px`
          : "unset",
      };
    });
    const itemsPath: Ref<any[]> = ref([]);
    const op = ref();
    const thesaurusStateIsLoading = ref(false);
    const thesaurusStateIsSuccess = ref(false);
    const searchTermValue = ref("");

    const thesaurusTerms: Ref<Term[]> = ref([]);
    const selectedTerms: Ref<Term[]> = ref([]);
    const showCurrentPath = ref(false);
    const currentPath = ref([]);
    const truncatedCurrentPath: Ref<Term[]> = ref([]);

    const isDisabled = computed({
      get(): boolean {
        return props.options.disabled;
      },
      set(value: boolean) {
        props.options.disabled = value;
        emit("update:options", props.options);
      },
    });

    const isHidden = computed({
      get(): boolean {
        return props.options.hidden;
      },
      set(value: boolean) {
        props.options.hidden = value;
        emit("update:options", props.options);
      },
    });
    const readOnly = computed({
      get(): boolean {
        return props.options.readonly;
      },
      set(value: boolean) {
        props.options.readonly = value;
        emit("update:options", props.options);
      },
    });
    const termLimitConfig = computed(() => {
      return +props.options.termLimit;
    });
    const thesaurusIdConfig = computed(() => {
      return props.options.thesaurusId;
    });
    const showFullPathConfig = computed(() => {
      return props.options.showFullPath;
    });
    const isSelectedTermLimit = computed(() => {
      let length = 0;

      if (terms.value) {
        length = terms.value.length;
      }

      const limitNotNull =
        termLimitConfig.value !== null && termLimitConfig.value !== undefined;
      const isUnlimitedConfig = 0;

      if (limitNotNull) {
        if (termLimitConfig.value === isUnlimitedConfig) {
          return false;
        }
        return termLimitConfig.value <= length;
      }
      return false;
    });
    const isCurrentPathTruncated = computed(() => {
      return truncatedCurrentPath.value.length < currentPath.value.length;
    });
    const getResultContainerClasses = computed(() => {
      let classes = "result-container";

      if (isSelectedTermLimit.value) {
        classes += " result-container-with-limit";
      }

      return classes;
    });
    const getValueTermClasses = (term: Term) => {
      let classes = "thesaurus-value";

      if (isTermSelected(term)) {
        classes += " thesaurus-value-selected";
      }

      if (!showFullPathConfig) {
        classes += " truncated";
      }

      return classes;
    };

    const getResultTermClasses = (term: Term) => {
      let classes = "result-term";

      if (isTermInactive(term) || isTermSuspendu(term)) {
        classes += " result-term-inactive";
      }
      if (isSelectedTermLimit.value) {
        classes += " result-term-limit";
      }
      if (term.isSyno) {
        classes += " result-term-syno";
      }
      return classes;
    };

    const getResultTermLeftPartClasses = (term: Term) => {
      let classes = "result-term-part truncated";

      if (isLeaf(term)) {
        classes += " result-term-left-full";
      } else {
        classes += " result-term-left";
      }

      return classes;
    };

    const selectThesaurus = (term: Term) => {
      if (isDisabled.value) return;
      selectedTerms.value = [term];
    };
    const selectAllThesaurus = () => {
      if (isDisabled.value) return;
      selectedTerms.value = terms.value;
    };
    const deselectAllThesaurus = () => {
      selectedTerms.value = [];
    };
    const closeThesaurusFrame = () => {
      op.value.hide();
      deselectAllThesaurus();
      resetSearchTerm();
    };

    const openThesaurusFrame = (event: any) => {
      if (!op.value.visible) {
        // Check available space
        const inputRect = neoThesaurusRef.value?.getBoundingClientRect();
        const panelHeight = 280; // Should match $thesaurus-frame-height
        const spaceBelow = window.innerHeight - (inputRect?.bottom ?? 0);
        const spaceAbove = inputRect?.top ?? 0;

        showOnTop.value = spaceBelow < panelHeight && spaceAbove > panelHeight;

        op.value.toggle(event);
        loadRootTermLevel();
        selectAllThesaurus();
      }
    };
    /*
    const setFocusOnSearchInput = () => {
      if (
        this.$refs &&
        this.$refs.searchTermInput &&
        this.$refs.searchTermInput.$refs &&
        this.$refs.searchTermInput.$refs.inputComponent
      ) {
        this.$refs.searchTermInput.$refs.inputComponent.focus();
      }
    }



    const closeThesaurusFrameShortcut = (evt) => {
      const escapeIsPress = evt.which === KeyboardKeyEnums.ESCAPE;

      if (escapeIsPress) {
        this.closeThesaurusFrame();
        evt.preventDefault();
        return false;
      }

      return true;
    }
*/
    const searchThesaurusTerm = (event: any) => {
      thesaurusStateIsLoading.value = true;
      showCurrentPath.value = false;
      itemsPath.value = [];
      thesaurusTerms.value = [];
      launchThesaurusSearch();
    };
    /*
const trySearchThesaurusTerm = (event: any) => {
  if (event.which !== KeyboardKeyEnums.ENTER) return;

  launchThesaurusSearch();
};
*/

    const launchThesaurusSearch = _.debounce(async () => {
      const searchTermValueTrimmed = searchTermValue.value.trim().toLowerCase();
      if (searchTermValueTrimmed.length === 0) {
        loadRootTermLevel();
        return;
      }
      const limitCharSearch = 1;
      if (searchTermValueTrimmed.length < limitCharSearch) return;

      //! missing filter

      thesaurusTerms.value = await eliseSearchThesaurus(
        thesaurusIdConfig.value,
        searchTermValueTrimmed
      );
      // sort by alphabetical order thesaurusTerms
      thesaurusTerms.value.sort((a, b) => a.label.localeCompare(b.label));
      // if the term is isLeaf and isSuspended remove it from the list
      thesaurusTerms.value = thesaurusTerms.value.filter((term) => {
        return !(isTermSuspendu(term) && isLeaf(term));
      });
      thesaurusStateIsLoading.value = false;
    }, 2000);

    const resetSearchTerm = () => {
      searchTermValue.value = "";
      loadRootTermLevel();
      // setFocusOnSearchInput();
    };

    const loadTermChildren = async (parentTerm: Term | null) => {
      let parentTermId = null;
      showCurrentPath.value = true;
      if (parentTerm) {
        parentTermId = parentTerm.termId;
        itemsPath.value.push({
          ...parentTerm,
          command: ({
            item,
            originalEvent,
          }: {
            item: Term;
            originalEvent: Event;
          }) => {
            originalEvent.preventDefault();
            updateTruncatedCurrentPath(item.termId);
            loadTermChildren(item);
          },
        });
      }
      thesaurusStateIsLoading.value = true;
      thesaurusTerms.value = await eliseLevelThesaurus(
        thesaurusIdConfig.value,
        parentTermId
      );
      thesaurusTerms.value.sort((a, b) => a.label.localeCompare(b.label));
      thesaurusTerms.value = thesaurusTerms.value.filter((term) => {
        return !(isTermSuspendu(term) && isLeaf(term));
      });

      updateTruncatedCurrentPath();
      thesaurusStateIsLoading.value = false;
    };

    const loadRootTermLevel = () => {
      itemsPath.value = [];
      loadTermChildren(null);
    };

    const addTerm = (term: Term) => {
      if (
        isTermInactive(term) ||
        isSelectedTermLimit.value ||
        isTermSuspendu(term)
      ) {
        return;
      }
      terms.value.push(term);
    };

    const updateTruncatedCurrentPath = (termId?: string | null) => {
      if (termId) {
        const i = itemsPath.value.findIndex((t) => t.termId === termId);
        if (i > -1) itemsPath.value = itemsPath.value.slice(0, i);
      }
    };
    const isTermForbidden = (term: Term) => {
      const forbiddenStatus = 2;
      if (term && term.isSyno) return term.syno?.status === forbiddenStatus;
      return term && term.status === forbiddenStatus;
    };
    const isTermSuspendu = (term: Term) => {
      const suspenduStatus = 3;
      if (term && term.isSyno) return term.syno?.status === suspenduStatus;
      return term && term.status === suspenduStatus;
    };
    const isTermInArray = (term: Term, array: Term[]) => {
      if (!term || !array) {
        return false;
      }

      return array.some((value) => value.termId === term.termId);
    };
    const isTermInValue = (term: Term) => {
      return isTermInArray(term, terms.value);
    };
    const isTermInactive = (term: Term) => {
      return isTermForbidden(term) || isTermInValue(term);
    };
    const isTermSelected = (term: Term) => {
      return isTermInArray(term, selectedTerms.value);
    };

    const isLeaf = (term: Term) => {
      if (!term) {
        return false;
      }

      return term.isLeaf;
    };
    function disableField() {
      isDisabled.value = true;
    }
    function enableField() {
      isDisabled.value = false;
    }
    function hideField() {
      isHidden.value = true;
    }
    function showField() {
      isHidden.value = false;
    }
    const isRequired = computed({
      get(): boolean {
        return props.options.required;
      },
      set(value: boolean) {
        props.options.required = value;
        emit("update:options", props.options);
      },
    });
    function manageProperties(opt: any) {
      if (opt) {
        isDisabled.value = opt.disabled;
        isHidden.value = opt.hidden;
        isRequired.value = opt.required;
      }
    }
    function normalizeAndCompare(str1: string, str2: string) {
      const normalizeString = (str: string) =>
        str
          .normalize("NFD")
          .replace(/[\u0300-\u036f]/g, "")
          .toLowerCase();

      if (normalizeString(str1) === normalizeString(str2)) {
        return "background-color: #00a819 !important; color:white!important";
      } else {
        return "";
      }
    }
    const removeItem = (item: Term) => {
      const index = terms.value.findIndex(
        (term) => term.termId === item.termId
      );
      if (index !== -1) {
        terms.value.splice(index, 1);
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

    // Watch for field validity and clear error if valid
    watch(
      () => terms.value,
      (newValue) => {
        // If there is an error and the value is now valid, clear the error
        if (errorState.errorMessage) {
          let isNotEmpty = false;
          if (Array.isArray(newValue)) {
            isNotEmpty = newValue.length > 0;
          } else if (typeof newValue === "string") {
            isNotEmpty = (newValue as string).trim() !== "";
          }
          if (isNotEmpty) {
            clearFieldError();
          }
        }
      }
    );

    // Add this watcher for required validation
    watch(
      [terms, () => props.options.required],
      ([newTerms, required]) => {
        if (required && (!newTerms || newTerms.length === 0)) {
          setFieldError("Ce champ est obligatoire");
        } else {
          clearFieldError();
        }
      },
      { immediate: true, deep: true }
    );
    return {
      itemsPath,
      home,
      errorState,
      showCurrentPath,
      thesaurusStateIsLoading,
      isSelectedTermLimit,
      searchTermValue,
      readOnly,
      isHidden,
      isDisabled,
      thesaurusTerms,
      getResultContainerClasses,
      terms,
      termLimitConfig,
      op,
      maxWidth,
      neoThesaurusRef,
      computedStyle,
      resetSearchTerm,
      closeThesaurusFrame,
      openThesaurusFrame,
      loadTermChildren,
      isLeaf,
      addTerm,
      getResultTermLeftPartClasses,
      getResultTermClasses,
      searchThesaurusTerm,
      disableField,
      enableField,
      hideField,
      showField,
      manageProperties,
      normalizeAndCompare,
      removeItem,
      setFieldError,
      clearFieldError,
      showOnTop,
    };
  },
};
</script>

<style lang="scss">
@import "@/scss/variables";
.p-overlaypanel-content {
  padding: 0px;
}
.neoThesaurusExternal {
  width: 100%;
  .neoThesaurus-container {
    position: relative;
  }
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
      font-family: Trebuchet MS, sans-serif;
      font-size: 12px;
      .label .label-container-modified {
        text-align: right;
      }
    }
  }
  .input-container {
    width: 100%;
    height: 60px;
    // max-height: 50px;
    position: relative;
    .p-autocomplete {
      width: 100%;
      height: 36px !important;
      padding: 0px !important;
      overflow: overlay !important;
      background-color: #f3f8f9;
      border-radius: 4px;
      .p-autocomplete-chip-item {
        height: 24px !important;
        font-size: 0.9rem !important;
        // padding: 0.5rem 0.5rem !important;
        color: white !important;
        background-color: #0a6e89 !important;
        margin: 0.1rem 0 !important;
        border-radius: 15px !important;
        display: flex !important;
        align-items: center !important;
        justify-content: center !important;
        padding: 0 0.5rem !important;
      }
      .p-inputchips-input-token {
        padding: 0px !important;
      }
    }
  }
  .p-inputchips .p-inputchips-input .p-autocomplete-chip-item {
    padding: 0rem 0.5rem !important;
  }
}

$label-container-min-width: 150px;
$text-input-height: 30px;
$write-mode-input-height: 28px;

$thesaurus-frame-height: 280px;
$thesaurus-header-height: 30px;
$thesaurus-search-bar-height: 40px;
$home-min-width: 26px;
$thesaurus-limit-height: 25px;
$thesaurus-element-height: 30px;
$icon-font-size: 18px;
$thesaurus-container-min-height: $thesaurus-element-height + $new-spacing + 2px; // 2px = 2 * border
$placeholder-min-height: $thesaurus-container-min-height - $new-spacing;
$current-path-container-height: 20px;
$results-label-height: 20px;
$navigate-font-size: 18px;
$thesaurus-element-radius: 15px;
$color-blue-jungle-mist: #b8cad3;
$color-gray: #6a6a6a;
$result-container-height: $thesaurus-frame-height - $thesaurus-header-height -
  $thesaurus-search-bar-height - (2 * $new-spacing) -
  (2 * $results-label-height) - $new-spacing-sm;

.thesaurus-input-container {
  margin-bottom: $new-spacing-input;

  .error-message {
    color: $color-red;
    margin-top: $new-spacing-sm;
  }

  .label {
    display: flex;
    flex: 1;
    flex-direction: row;

    .label-container {
      color: $color-blue-blumine;
      min-width: $label-container-min-width;
      align-items: center;
      display: flex;
      padding-bottom: $new-spacing-sm;
    }

    .label-container-modified {
      width: 100%;
      color: $color-grey;
      font-style: italic;
      padding-bottom: $new-spacing-sm;
    }
  }

  &.context-parameter-value {
    .label-container-modified {
      display: none;
    }
  }

  &.top {
    .label {
      .label-container-modified {
        @at-root {
          [dir="rtl"]#{&} {
            text-align: left;
          }

          [dir="ltr"]#{&} {
            text-align: right;
          }
        }
      }
    }
  }

  &.left {
    .form-input-container {
      display: flex;
    }

    .label {
      display: flex;
      flex-direction: column;
    }

    .label-container-modified {
      @at-root {
        [dir="rtl"]#{&} {
          padding-left: $new-spacing;
        }

        [dir="ltr"]#{&} {
          padding-right: $new-spacing;
        }
      }
    }
  }

  .input-container {
    overflow: visible !important;
  }
}

.read-only.thesaurus-input-container {
  .thesaurus-container {
    border: 1px solid $color-grey-gallery;
    background-color: $color-white;

    &:hover {
      border-color: $color-grey-gallery;
    }

    .thesaurus-value {
      border: 1px solid $color-grey-gallery;
      background-color: $color-grey-gallery;

      .thesaurus-label {
        color: $color-grey;
        padding: 0 $new-spacing-sm;
        margin: $new-spacing;
      }

      .delete-term-btn {
        color: $color-grey;
      }
    }

    &:not(.thesaurus-disabled-mode) {
      .thesaurus-value {
        &:hover {
          border-color: $color-grey-gallery;
          background-color: $color-grey-gallery;
          cursor: default;
        }
      }

      .thesaurus-value.thesaurus-value-selected {
        &:hover {
          border-color: $color-grey-gallery;
          background-color: $color-grey-gallery;
          cursor: default;
        }
      }
    }

    .thesaurus-value-selected {
      .thesaurus-label {
        @at-root {
          [dir="rtl"]#{&} {
            padding: 0 $new-spacing-sm 0 $new-spacing-lg;
          }

          [dir="ltr"]#{&} {
            padding: 0 $new-spacing-lg 0 $new-spacing-sm;
          }
        }
      }

      .delete-term-btn {
        cursor: default;
      }
    }
  }
}

.is-invalid.thesaurus-input-container {
  .label-container {
    color: $color-red;
  }

  .thesaurus-container {
    border: 1px solid $color-red;

    &:hover {
      border-color: $color-red;
    }

    &.thesaurus-focus-mode {
      border: 1px solid $color-red;
    }

    .thesaurus-value {
      border: 1px solid $color-red;

      .thesaurus-label {
        color: $color-red;
        padding: 0 $new-spacing-sm;
      }
    }

    &:not(.thesaurus-disabled-mode) {
      .thesaurus-value {
        &:hover {
          border-color: $color-red;
          background-color: $color-blue-catskill-white;
        }
      }

      .thesaurus-value.thesaurus-value-selected {
        &:hover {
          border-color: $color-red;
          background-color: $color-red;
        }
      }
    }

    .thesaurus-value-selected {
      border: 1px solid $color-red;
      background-color: $color-red;

      .thesaurus-label {
        color: $color-white;

        @at-root {
          [dir="rtl"]#{&} {
            padding: 0 $new-spacing-sm 0 $new-spacing-lg;
          }

          [dir="ltr"]#{&} {
            padding: 0 $new-spacing-lg 0 $new-spacing-sm;
          }
        }
      }
    }
  }
}
.thesaurus-input {
  height: auto !important;
  display: inline-block;
  position: relative;
  overflow: visible !important;
  width: 100%;

  .thesaurus-container {
    display: flex;
    flex-wrap: wrap;
    border: 1px solid transparent;
    min-height: $text-input-height;
    align-items: center;
    width: 100%;
    background-color: $color-blue-catskill-white;
    border-radius: $radius-sm;
    box-sizing: border-box;
    @at-root {
      [dir="rtl"]#{&} {
        padding-right: $new-spacing-sm;
      }

      [dir="ltr"]#{&} {
        padding-left: $new-spacing-sm;
      }
    }

    &:hover {
      border-color: $color-blue-jungle-mist;
    }

    &.thesaurus-edit-mode,
    &.thesaurus-focus-mode {
      outline: none;
      box-sizing: border-box;
    }

    &.thesaurus-focus-mode {
      border: 1px solid $color-blue-dark;
    }

    .thesaurus-value {
      display: flex;
      align-items: center;
      border: 1px solid $color-blue-glacier;
      border-radius: $thesaurus-element-radius;
      position: relative;
      margin-top: $new-spacing-sm;
      margin-bottom: $new-spacing-sm;

      @at-root {
        [dir="rtl"]#{&} {
          margin-left: $new-spacing-sm;
        }

        [dir="ltr"]#{&} {
          margin-right: $new-spacing-sm;
        }
      }

      .thesaurus-label {
        color: $color-blue-blumine;
        font-size: $title-font-size;
        padding: 0 $new-spacing-sm;
        margin: $new-spacing-sm;
      }

      .thesaurus-multiline-label {
        overflow: hidden;
      }

      .delete-term-btn {
        display: flex;
        align-items: center;
        justify-content: center;
        background-color: transparent !important;
        padding: 0;
        position: absolute;
        color: $color-white;
        height: 100%;

        @at-root {
          [dir="ltr"]#{&} {
            right: 4px;
            padding-left: $new-spacing-sm;
          }

          [dir="rtl"]#{&} {
            left: 4px;
            padding-right: $new-spacing-sm;
          }
        }

        @media all and (-ms-high-contrast: none) {
          bottom: 1px;
        }
      }
    }

    &:not(.thesaurus-disabled-mode) {
      .thesaurus-value {
        &:hover {
          border-color: $color-blue-glacier;
          background-color: $color-blue-catskill;
          cursor: pointer;
        }
      }

      .thesaurus-value.thesaurus-value-selected {
        &:hover {
          border-color: $color-blue-dark;
          background-color: $color-blue-dark;
        }
      }
    }

    .thesaurus-value-selected {
      border: 1px solid $color-blue-dark;
      background-color: $color-blue-dark;

      .thesaurus-label {
        color: $color-white;

        @at-root {
          [dir="rtl"]#{&} {
            padding: 0 $new-spacing-sm 0 $new-spacing-lg;
          }

          [dir="ltr"]#{&} {
            padding: 0 $new-spacing-lg 0 $new-spacing-sm;
          }
        }
      }
    }

    .thesaurus-placeholder {
      color: $color-grey-dusty;
      font-style: italic;
      font-size: $title-font-size;
      padding-top: 7px;
      padding-bottom: 7px;
    }

    .thesaurus-scroll-container {
      max-height: 250px;
    }

    .thesaurus-scroll-wrap-container {
      display: flex !important;
      flex-wrap: wrap;
    }
  }

  &.is-required {
    .thesaurus-container {
      border-color: $color-required;
    }
  }
}
.thesaurus-frame {
  position: absolute !important;
  top: 40px !important;
  left: 0 !important;
  width: 100% !important;
  display: flex !important;
  flex-direction: column !important;
  // width: 50% !important;
  height: $thesaurus-frame-height !important;
  background-color: $color-white !important;
  box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.1) !important;
  padding: 0 !important;
  margin: 0 !important;
  // position: absolute;
  box-sizing: border-box !important;
  border: 1px solid $color-blue-dark !important;
  border-radius: $radius-sm !important;
  &.thesaurus-frame-top {
    //label input height + thesaurus frame height
    top: -275px !important;
  }

  .thesaurus-header {
    height: $thesaurus-header-height !important;
    display: flex !important;
    align-items: center !important;
    color: $color-grey-tundora !important;
    padding: 5px 12px !important;

    .thesaurus-label {
      flex: 1 !important;
      font-size: $title-font-size !important;
      font-weight: 400 !important;
    }
  }

  .search-terms {
    display: flex !important;
    align-items: center !important;
    flex-shrink: 0 !important;
    height: $thesaurus-search-bar-height !important;
    margin: 0 $new-spacing !important;
    box-sizing: border-box !important;
    border-bottom: 1px solid $color-grey-mercury !important;

    .form-input {
      height: 100% !important;

      &.text-input-container {
        margin-bottom: 0 !important;
      }

      .input-container {
        input {
          height: 100% !important;
          border: none !important;

          @at-root {
            [dir="rtl"]#{&} {
              margin-right: $new-spacing !important;
            }

            [dir="ltr"]#{&} {
              margin-left: $new-spacing !important;
            }
          }

          &::-ms-clear {
            display: none !important;
          }
        }
      }
    }

    .search-terms-btn {
      display: flex !important;
      align-items: center !important;
      justify-content: center !important;
      background-color: transparent !important ;
      color: $color-dimgrey !important;

      &:hover {
        color: $color-black !important;
      }

      .elise-icon-backspace {
        @at-root {
          [dir="rtl"]#{&} {
            transform: scaleX(-1) !important;
          }
        }
      }
    }

    .search-icon {
      color: $color-blue-dark !important;
      font-size: $elise-icon-font-size !important;

      @at-root {
        [dir="rtl"]#{&} {
          padding-left: $new-spacing-sm !important;
        }

        [dir="ltr"]#{&} {
          padding-right: $new-spacing-sm !important;
        }
      }
    }
  }

  .search-results {
    display: flex !important;
    flex: 1 !important;
    flex-direction: column !important;
    margin: $new-spacing !important;

    .current-path-container {
      display: flex !important;
      align-items: center !important;
      height: $current-path-container-height !important;
      width: 100% !important;
      color: $color-blue-dark !important;
      padding-bottom: $new-spacing-sm !important;

      .root-level {
        width: $home-min-width !important;
        min-width: $home-min-width !important;
      }

      .current-path-element-level {
        display: flex !important;
        justify-content: center !important;
        align-items: center !important;
        overflow: hidden !important;
        font-size: $elise-icon-font-size-sm !important;

        .current-path-element {
          padding: $new-spacing-sm !important;

          &:hover {
            cursor: pointer !important;
            border-radius: 15px !important;
            background-color: $color-white-sprint-wood !important;
          }
        }
      }

      .level-separator {
        padding: 0 $new-spacing-sm !important;
      }
    }

    .results-label {
      color: $color-grey !important;
      margin-bottom: $new-spacing-sm !important;
      height: $results-label-height !important;
    }

    .no-result {
      display: flex !important;
      flex: 1 !important;
      justify-content: center !important;
      align-items: center !important;

      .no-result-label {
        color: $color-grey !important;
        font-weight: bold !important;
      }
    }

    .result-container {
      max-height: $result-container-height !important;

      .result-terms {
        display: flex !important;
        flex-wrap: wrap !important;

        .result-term {
          display: flex !important;
          align-items: center !important;
          justify-content: center !important;
          height: $thesaurus-element-height !important;
          margin-bottom: $new-spacing !important;
          background-color: $color-blue-squeeze !important;
          border-radius: $thesaurus-element-radius !important;
          color: $color-blue-dark !important;
          overflow: hidden !important;
          cursor: pointer !important;
          border: 2px solid $color-white !important;

          @at-root {
            [dir="rtl"]#{&} {
              margin-left: $new-spacing-sm !important;
            }

            [dir="ltr"]#{&} {
              margin-right: $new-spacing-sm !important;
            }
          }

          .result-term-part {
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
            height: 100% !important;
            padding-right: 5px !important;
            padding-left: 5px !important;
          }

          .result-term-left {
            @at-root {
              [dir="ltr"]#{&} {
                padding-left: $new-spacing !important;
                padding-right: $new-spacing-sm !important;
                border-top-left-radius: $thesaurus-element-radius !important;
                border-bottom-left-radius: $thesaurus-element-radius !important;
              }

              [dir="rtl"]#{&} {
                padding-right: $new-spacing !important;
                padding-left: $new-spacing-sm !important;
                border-top-right-radius: $thesaurus-element-radius !important;
                border-bottom-right-radius: $thesaurus-element-radius !important;
              }
            }
          }

          .result-term-left-full {
            border-radius: $thesaurus-element-radius !important;
            padding: 0 $new-spacing !important;
          }

          .result-term-right {
            min-width: $navigate-font-size + (3 * $new-spacing-sm) !important;

            @at-root {
              [dir="ltr"]#{&} {
                padding-right: $new-spacing-sm !important;
                border-top-right-radius: $thesaurus-element-radius !important;
                border-bottom-right-radius: $thesaurus-element-radius !important;
              }

              [dir="rtl"]#{&} {
                padding-left: $new-spacing-sm !important;
                border-top-left-radius: $thesaurus-element-radius !important;
                border-bottom-left-radius: $thesaurus-element-radius !important;
              }
            }

            &:hover {
              background-color: $color-blue-jungle-mist !important;
            }

            .navigate-term-btn {
              color: $color-blue-dark !important;
              font-size: $elise-icon-font-size-sm !important;
              display: flex !important;
              align-items: center !important;
              justify-content: center !important;
              background-color: transparent !important;
              padding: 0 $new-spacing-sm !important;

              .icon-go-back {
                color: $color-blue-dark !important;

                @at-root {
                  [dir="rtl"]#{&} {
                    transform: scaleX(-1) !important;
                  }
                }
              }
            }
          }
        }

        .result-term:not(.result-term-inactive) {
          .result-term-part {
            &:hover {
              color: $color-white !important;
              background-color: $color-blue-jungle-mist !important;
            }
          }
        }

        .result-term-inactive {
          border-color: $color-blue-jungle-mist !important;
          background-color: #fdd2d6 !important;
        }
        .result-term-syno {
          border-color: $color-gray !important;
        }

        .result-term-limit,
        .result-term-inactive {
          cursor: default !important;
        }
      }
    }
    .result-container-with-limit {
      max-height: $result-container-height - $thesaurus-limit-height !important;
    }
    .nui-scroll-bar-container {
      width: 100% !important;
      height: 100% !important;
      overflow: auto !important;
      max-height: $result-container-height !important;
    }
    .nui-scroll-bar::-webkit-scrollbar-thumb {
      background: none !important;
    }
  }

  .thesaurus-terms,
  .thesaurus-limit {
    display: flex !important;
    align-items: center !important;
  }

  .thesaurus-limit {
    height: $thesaurus-limit-height !important;
    padding: 0 $new-spacing !important;
    background-color: $warn-color-bg !important;
    flex-shrink: 0 !important;
    position: absolute !important;
    width: 80% !important;

    .limit-label {
      color: $warn-color-text !important;
      font-weight: bold !important;
    }
  }

  .thesaurus-terms {
    height: 100% !important;

    .spinner-section {
      display: flex !important;
      justify-content: center !important;
      flex: 1 !important;
      font-size: 50px !important;
    }
  }
}
.disabled-wrapper {
  pointer-events: none; /* Disable all interactions */
  opacity: 0.5; /* Optional: Make it look visually disabled */
}
.readOnly-wrapper {
  pointer-events: none; /* Disable all interactions */
}
</style>
