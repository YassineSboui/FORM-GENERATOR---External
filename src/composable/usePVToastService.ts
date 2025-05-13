import { useToast } from "primevue/usetoast";
import app from '@/main';
import { getCurrentInstance } from "vue"
export const usePVToastService = () => {
    const getToast: typeof useToast = () => app.config.globalProperties.$toast
    const toastService = getToast();
    return toastService
}