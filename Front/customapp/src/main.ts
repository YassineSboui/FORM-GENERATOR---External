import { createApp, reactive, watch } from "vue";
import App from "./App.vue";
import router from "./router";
import { createI18n } from "vue-i18n";
import arabic from "./i18n/ar";
import french from "./i18n/fr";
import PrimeVue from "primevue/config";
import Aura from "@primeuix/themes/aura";
import ToastService from "primevue/toastservice";
import ConfirmationService from "primevue/confirmationservice";
import BadgeDirective from "primevue/badgedirective";
import Tooltip from "primevue/tooltip";
import { createPinia } from "pinia";
import * as NeoComponents from "@/components/NeoComponents";
import * as DraggableZones from "@/components/Zones/Draggable";
import * as PreviewZones from "@/components/Zones/Preview";
import * as DraggableZonesT from "@/components/ZonesTable/Draggable";
import * as ChildComponents from "@/components/ChildComponents";
import { Field, ErrorMessage, defineRule, configure } from "vee-validate";
import { localize } from "@vee-validate/i18n";
import * as rules from "@vee-validate/rules";
import ar from "@vee-validate/i18n/dist/locale/ar.json";
import en from "@vee-validate/i18n/dist/locale/en.json";
import fr from "@vee-validate/i18n/dist/locale/fr.json";
import QrcodeVue, { QrcodeCanvas, QrcodeSvg } from "qrcode.vue";
import Camera from "simple-vue-camera";
import Vue3Signature from "vue3-signature";
import Quill from "quill";

// PrimeVue Components
import Editor from "primevue/editor";
import ConfirmDialog from "primevue/confirmdialog";
import Toast from "primevue/toast";
import Tabs from "primevue/tabs";
import TabList from "primevue/tablist";
import Tab from "primevue/tab";
import TabPanels from "primevue/tabpanels";
import TabPanel from "primevue/tabpanel";
import ToggleSwitch from "primevue/toggleswitch";
import PrimeVueDatePicker from "primevue/datepicker";
import Datepicker from "vuejs3-datepicker";
import Select from "primevue/select";
import Drawer from "primevue/drawer";
import ProgressSpinner from "primevue/progressspinner";
import Stepper from "primevue/stepper";
import StepList from "primevue/steplist";
import StepPanels from "primevue/steppanels";
import StepItem from "primevue/stepitem";
import Step from "primevue/step";
import StepPanel from "primevue/steppanel";
import Accordion from "primevue/accordion";
import AccordionPanel from "primevue/accordionpanel";
import AccordionHeader from "primevue/accordionheader";
import AccordionContent from "primevue/accordioncontent";

// Styles
import "@mdi/font/css/materialdesignicons.css";
import "primeicons/primeicons.css";
import "material-icons/iconfont/material-icons.css";
import "./assets/css/style.css";
import "@/scss/layout.scss";

// VeeValidate rules registration
for (const rule in rules) {
  if (typeof (rules as { [key: string]: any })[rule] === "function") {
    defineRule(rule, (rules as { [key: string]: any })[rule]);
  } else {
    console.warn(`Skipping rule "${rule}" as it's not a function.`);
  }
}
defineRule("alpha_underscore", (value: any) => /^[a-zA-Z0-9_]+$/.test(value));

// i18n setup
const i18n = createI18n({
  legacy: false,
  locale: "fr",
  messages: { ar: arabic, fr: french },
});

// Vee-Validate config
configure({
  generateMessage: localize({ en, ar, fr }),
  validateOnInput: true,
});

// PrimeVue locale setup
const getPrimeVueLocale = () => {
  const currentLocale = i18n.global.locale.value;
  if (currentLocale === "fr") return { ...french.LocaleOptions };
  if (currentLocale === "ar") return { ...arabic.LocaleOptions };
  return {};
};
const primevueLocale = reactive(getPrimeVueLocale());

// Watch for language changes to update PrimeVue locale
watch(
  () => i18n.global.locale.value,
  () => Object.assign(primevueLocale, getPrimeVueLocale())
);

// Fix for Quill v2 with PrimeVue Editor
(Editor as any).methods.renderValue = function renderValue(
  this: { quill?: Quill },
  value: string
) {
  if (this.quill) {
    if (value) {
      const delta = this.quill.clipboard.convert({ html: value });
      this.quill.setContents(delta, "silent");
    } else {
      this.quill.setText("");
    }
  }
};

// App initialization
const app = createApp(App);
const pinia = createPinia();

// Register global components
const globalComponents = {
  Field,
  ErrorMessage,
  ConfirmDialog,
  vue3Datepicker: Datepicker,
  ComponentForm: ChildComponents.ComponentForm,
  ComponentFormTable: ChildComponents.ComponentFormTable,
  ZoneComponent: ChildComponents.ZoneComponent,
  ZoneComponentTable: ChildComponents.ZoneComponentTable,
  QrcodeVue,
  Camera,
  Vue3Signature,
  QrcodeCanvas,
  QrcodeSvg,
  Tabs,
  TabList,
  Tab,
  TabPanels,
  TabPanel,
  ToggleSwitch,
  DatePicker: PrimeVueDatePicker,
  Select,
  Drawer,
  Editor,
  ProgressSpinner,
  Stepper,
  StepList,
  StepPanels,
  StepItem,
  Step,
  StepPanel,
  Accordion,
  AccordionPanel,
  AccordionHeader,
  AccordionContent,
  Toast,
  NeoTable: NeoComponents.NeoTable,
  NeoTextField: NeoComponents.NeoTextField,
  NeoTextArea: NeoComponents.NeoTextArea,
  NeoSelect: NeoComponents.NeoSelect,
  NeoAutoComplete: NeoComponents.NeoAutoComplete,
  NeoDatepicker: NeoComponents.NeoDatepicker,
  NeoCheckbox: NeoComponents.NeoCheckbox,
  NeoTimePicker: NeoComponents.NeoTimePicker,
  NeoRadioImages: NeoComponents.NeoRadioImages,
  NeoEditor: NeoComponents.NeoEditor,
  NeoNumberField: NeoComponents.NeoNumberField,
  NeoCheckboxGroup: NeoComponents.NeoCheckboxGroup,
  NeoSwitch: NeoComponents.NeoSwitch,
  NeoOption: NeoComponents.NeoOption,
  NeoOptionsGroup: NeoComponents.NeoOptionsGroup,
  NeoUploadFile: NeoComponents.NeoUploadFile,
  NeoRating: NeoComponents.NeoRating,
  NeoButton: NeoComponents.NeoButton,
  NeoTableComponent: NeoComponents.NeoTableComponent,
  NeoVHtml: NeoComponents.NeoVHtml,
  NeoChips: NeoComponents.NeoChips,
  NeoMultiSelect: NeoComponents.NeoMultiSelect,
  NeoFlowchart: NeoComponents.NeoFlowchart,
  NeoFlowchart_V2: NeoComponents.NeoFlowchart_V2,
  NeoListDocument: NeoComponents.NeoListDocument,
  NeoRecap: NeoComponents.NeoRecap,
  NeoThesaurus: NeoComponents.NeoThesaurus,
  NeoContact: NeoComponents.NeoContact,
  NeoTreeView: NeoComponents.NeoTreeView,
  NeoCustomTreeView: NeoComponents.NeoCustomTreeView,
  NeoQrCode: NeoComponents.NeoQrCode,
  NeoPhoto: NeoComponents.NeoPhoto,
  NeoSign: NeoComponents.NeoSign,
  NeoMap: NeoComponents.NeoMap,
  NeoIcon: NeoComponents.NeoIcon,
};

Object.entries(globalComponents).forEach(([name, comp]) => {
  app.component(name, comp);
});

// Register draggable zones
app.component("D-NeoBasicZones", DraggableZones.NeoBasicZones);
app.component("D-NeoZones", DraggableZones.NeoZones);
app.component("D-Z0111", DraggableZones.Z0111);
app.component("D-Z1000", DraggableZones.Z1000);
app.component("D-Z1011", DraggableZones.Z1011);
app.component("D-Z1100", DraggableZones.Z1100);
app.component("D-Z1110", DraggableZones.Z1110);
app.component("D-Z1111", DraggableZones.Z1111);
app.component("D-ZR", DraggableZones.ZR);
app.component("D-ZS", DraggableZones.ZS);

// Register preview zones
app.component("P-ZR", PreviewZones.ZR);

// Register table zones
app.component("D-NeoBasicZonesT", DraggableZonesT.NeoBasicZonesT);
app.component("D-Z0111T", DraggableZonesT.Z0111T);
app.component("D-Z1000T", DraggableZonesT.Z1000T);
app.component("D-Z1011T", DraggableZonesT.Z1011T);
app.component("D-Z1100T", DraggableZonesT.Z1100T);
app.component("D-Z1110T", DraggableZonesT.Z1110T);
app.component("D-Z1111T", DraggableZonesT.Z1111T);

// Register directives
app.directive("tooltip", Tooltip);
app.directive("badge", BadgeDirective);

// Mount function
function mountApp() {
  app.use(pinia);
  app.use(router);
  console.log("primevueLocale", primevueLocale);
  app.use(PrimeVue, {
    locale: primevueLocale,
    theme: {
      engine: true,
      preset: Aura,
      options: { darkModeSelector: false },
    },
  });
  app.use(i18n);
  app.use(ToastService);
  app.use(ConfirmationService);
  if (!import.meta.env.DEV) {
    // Development environment: use console.error for warnings
    console.error = (...args) => {
      console.warn("[PROD WARNING]:", ...args);
    };
  }
  app.mount("#app");
}

// Mount the app directly without authentication
mountApp();

export { i18n };
export default app;
