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
import * as ChildComponents from "@/components/ChildComponents";
import keycloak from "./keycloak";
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
  NeoQrCode: NeoComponents.NeoQrCode,
  NeoPhoto: NeoComponents.NeoPhoto,
  NeoSign: NeoComponents.NeoSign,
};

Object.entries(globalComponents).forEach(([name, comp]) => {
  app.component(name, comp);
});

// Register directives
app.directive("tooltip", Tooltip);
app.directive("badge", BadgeDirective);

// Provide keycloak globally
app.provide("keycloak", keycloak);

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
  app.mount("#app");
}

// Auth logic for /neoformext/front/
const currentPath = window.location.pathname;
if (
  currentPath === "/neoformext/front/" ||
  currentPath === "/neoformext/front"
) {
  keycloak
    .init({ onLoad: "login-required", checkLoginIframe: false })
    .then((authenticated) => {
      if (!authenticated) {
        window.location.reload();
      } else {
        console.log("✅ Authenticated");
        mountApp();
        // Token refresh
        setInterval(() => {
          keycloak.updateToken(60).catch(() => keycloak.login());
        }, 30000);
      }
    })
    .catch((error) => {
      console.error("❌ Keycloak init failed", error);
    });
} else {
  // No auth for other routes
  mountApp();
}
export { i18n };
export default app;
