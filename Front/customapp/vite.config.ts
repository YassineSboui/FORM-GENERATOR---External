import { fileURLToPath, URL } from "node:url";
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import svgLoader from "vite-svg-loader";
import Components from "unplugin-vue-components/vite";
import { PrimeVueResolver } from "unplugin-vue-components/resolvers";
// I18n

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  plugins: [
    vue(),
    Components({ resolvers: [PrimeVueResolver()] }),
    svgLoader({ svgo: false, defaultImport: "component" }),
  ],
  resolve: {
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  build: {
    target: "es2022", // ✅ add this
    rollupOptions: {
      output: {
        entryFileNames: `assets/[name].js`,
        chunkFileNames: `assets/[name].js`,
        assetFileNames: `assets/[name].[ext]`,
      },
    },
  },
  optimizeDeps: {
    esbuildOptions: { target: "es2022" }, // ✅ add this
  },
  base: "/neoformext/front/",
  esbuild: {
    pure: mode === "client" ? ["logger.error"] : [],
  },
  css: {
    preprocessorOptions: {
      scss: { silenceDeprecations: ["legacy-js-api"] },
    },
  },
}));
