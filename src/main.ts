import { createApp } from "vue";
import Quill from "quill";
import { createI18n } from "vue-i18n";
import App from "./App.vue";
import router from "./router";
import arabic from "./i18n/ar";
import french from "./i18n/fr";
import "@mdi/font/css/materialdesignicons.css";
import PrimeVue from "primevue/config";
import "primeicons/primeicons.css";
import Aura from "@primeuix/themes/aura";
import ToastService from "primevue/toastservice";
import ConfirmationService from "primevue/confirmationservice";
import DialogService from "primevue/dialogservice";
import BadgeDirective from "primevue/badgedirective";
import Tooltip from "primevue/tooltip";
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
import "material-icons/iconfont/material-icons.css";
import "./assets/css/style.css";
import "@/scss/layout.scss";
// import "primevue/resources/themes/aura-light-green/theme.css";
import { createPinia } from "pinia";

import QrcodeVue, { QrcodeCanvas, QrcodeSvg } from "qrcode.vue";
import Camera from "simple-vue-camera";
import Vue3Signature from "vue3-signature";

import { Field, ErrorMessage, defineRule, configure } from "vee-validate";
import { localize } from "@vee-validate/i18n";
import * as rules from "@vee-validate/rules";
import ar from "@vee-validate/i18n/dist/locale/ar.json";
import en from "@vee-validate/i18n/dist/locale/en.json";
import fr from "@vee-validate/i18n/dist/locale/fr.json";

import * as NeoComponents from "@/components/NeoComponents";
import * as ChildComponents from "@/components/ChildComponents";

// Iterate through the rules and define them if they are functions
for (const rule in rules) {
  if (typeof (rules as { [key: string]: any })[rule] === "function") {
    defineRule(rule, (rules as { [key: string]: any })[rule]);
  } else {
    console.warn(`Skipping rule "${rule}" as it's not a function.`);
  }
}
defineRule("alpha_underscore", (value: any) => {
  const regex = /^[a-zA-Z0-9_]+$/;
  return regex.test(value);
});

const app = createApp(App);
const pinia = createPinia();

// const store = useAppStore();
const i18n = createI18n({
  legacy: false,
  locale: "french",
  messages: {
    arabic,
    french,
  },
});
app.use(i18n);
export { i18n };

configure({
  generateMessage: localize({
    en: en,
    ar: ar,
    fr: fr,
  }),
  validateOnInput: true, // Optional, validates on input events
});

// Fix needed for Quill v2: https://github.com/primefaces/primevue/issues/5606#issuecomment-2203975395
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

// Set the initial locale (optional)

localize("fr");
app.use(pinia);
app.use(router);
app.use(PrimeVue, {
  theme: {
    preset: Aura,
    options: {
      darkModeSelector: false,
    },
  },
});
app.use(i18n);
app.use(ToastService);
app.use(ConfirmationService);

app.component("Field", Field);
app.component("ErrorMessage", ErrorMessage);
app.component("ConfirmDialog", ConfirmDialog);
//app.component("defineRule", defineRule);
app.component("vue3Datepicker", Datepicker);
app.component("ComponentForm", ChildComponents.ComponentForm);
app.component("ComponentFormTable", ChildComponents.ComponentFormTable);
app.component("ZoneComponent", ChildComponents.ZoneComponent);
app.component("ZoneComponentTable", ChildComponents.ZoneComponentTable);

app.component("NeoTextField", NeoComponents.NeoTextField);
app.component("NeoTextArea", NeoComponents.NeoTextArea);
app.component("NeoSelect", NeoComponents.NeoSelect);
app.component("NeoAutoComplete", NeoComponents.NeoAutoComplete);
app.component("NeoDatepicker", NeoComponents.NeoDatepicker);
app.component("NeoCheckbox", NeoComponents.NeoCheckbox);
app.component("NeoTimePicker", NeoComponents.NeoTimePicker);
app.component("NeoRadioImages", NeoComponents.NeoRadioImages);
app.component("NeoEditor", NeoComponents.NeoEditor);
app.component("NeoNumberField", NeoComponents.NeoNumberField);
app.component("NeoCheckboxGroup", NeoComponents.NeoCheckboxGroup);
app.component("NeoSwitch", NeoComponents.NeoSwitch);
app.component("NeoOption", NeoComponents.NeoOption);
app.component("NeoOptionsGroup", NeoComponents.NeoOptionsGroup);
app.component("NeoUploadFile", NeoComponents.NeoUploadFile);
app.component("NeoRating", NeoComponents.NeoRating);
app.component("NeoButton", NeoComponents.NeoButton);
app.component("NeoTableComponent", NeoComponents.NeoTableComponent);
app.component("NeoVHtml", NeoComponents.NeoVHtml);
app.component("NeoChips", NeoComponents.NeoChips);
app.component("NeoMultiSelect", NeoComponents.NeoMultiSelect);
app.component("NeoFlowchart", NeoComponents.NeoFlowchart);
app.component("NeoFlowchart_V2", NeoComponents.NeoFlowchart_V2);

app.component("NeoListDocument", NeoComponents.NeoListDocument);
app.component("NeoRecap", NeoComponents.NeoRecap);
app.component("NeoThesaurus", NeoComponents.NeoThesaurus);
app.component("NeoContact", NeoComponents.NeoContact);
app.component("NeoTreeView", NeoComponents.NeoTreeView);
app.component("NeoQrCode", NeoComponents.NeoQrCode);
app.component("NeoPhoto", NeoComponents.NeoPhoto);
app.component("NeoSign", NeoComponents.NeoSign);

app.component("DataView", DataView);
app.component("Image", Image);
app.component("Toast", Toast);

app.component("QrcodeVue", QrcodeVue);
app.component("Camera", Camera);
app.component("Vue3Signature", Vue3Signature);
app.component("QrcodeCanvas", QrcodeCanvas);
app.component("QrcodeSvg", QrcodeSvg);

app.component("Tabs", Tabs);
app.component("TabList", TabList);
app.component("Tab", Tab);
app.component("TabPanels", TabPanels);
app.component("TabPanel", TabPanel);
app.component("ToggleSwitch", ToggleSwitch);
app.component("DatePicker", PrimeVueDatePicker);
app.component("Select", Select);
app.component("Drawer", Drawer);

app.component("Stepper", Stepper);
app.component("StepList", StepList);
app.component("StepPanels", StepPanels);
app.component("StepItem", StepItem);
app.component("Step", Step);
app.component("StepPanel", StepPanel);

app.component("Accordion", Accordion);
app.component("AccordionPanel", AccordionPanel);
app.component("AccordionHeader", AccordionHeader);
app.component("AccordionContent", AccordionContent);

app.component("NeoTable", NeoComponents.NeoTable);
app.directive("tooltip", Tooltip);
app.directive("badge", BadgeDirective);

app.mount("#app");

export default app;
