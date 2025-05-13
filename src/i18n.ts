import { createI18n } from "vue-i18n";
import en from "@/locales/en";
import fr from "@/locales/fr";

export default createI18n({
  legacy: false,
  locale: "fr",
  messages: {
    en,
    fr,
  },
});
