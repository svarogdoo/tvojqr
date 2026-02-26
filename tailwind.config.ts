import type { Config } from "tailwindcss";

export default {
  theme: {
    extend: {
      colors: {
        brand: {
          canvas: "#fffdf6",
          blush: "#e2d4e0",
          cloud: "#949ab1",
          dusty: "#7c7e9d",
          ink: "#4c5372",
        },
        // Compatibility palette alias for existing classes already used in the app.
        olive: {
          50: "#fffdf6",
          100: "#f4edf3",
          200: "#e2d4e0",
          300: "#c9bfd1",
          400: "#949ab1",
          500: "#858aa6",
          600: "#7c7e9d",
          700: "#6e7190",
          800: "#4c5372",
          900: "#3c425b",
        },
      },
    },
  },
} satisfies Config;
