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
    Components({
      resolvers: [PrimeVueResolver()],
      directoryAsNamespace: true,
      include: [/\.vue$/, /\.vue\?vue/],
      exclude: [
        /[\\/]node_modules[\\/]/,
        /[\\/]\.git[\\/]/,
        /[\\/]\.nuxt[\\/]/,
      ],
      dts: true,
    }),
    svgLoader({
      svgo: false,
      defaultImport: "component",
    }),
  ],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  build: {
    rollupOptions: {
      output: {
        entryFileNames: `assets/[name].js`,
        chunkFileNames: `assets/[name].js`,
        assetFileNames: `assets/[name].[ext]`,
      },
    },
  },
  base: "/neoformext/front/",
  esbuild: {
    pure: [
      "logger.error",
      "logger.warn",
      "logger.info",
      "logger.debug",
      "logger.log",
      "console.log",
      "console.warn",
      "console.info",
      "console.debug",
      "console.error",
    ],
  },
  css: {
    preprocessorOptions: {
      scss: {
        silenceDeprecations: [
          "legacy-js-api",
          "import",
          "global-builtin",
          "mixed-decls",
          "color-functions",
          "slash-div",
        ],
        quietDeps: true,
      },
    },
  },
}));
