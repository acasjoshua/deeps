/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.razor", "./**/*.html", "!./node_modules/**", "!./bin/**", "!./obj/**"],
  theme: {
    extend: {
      colors: {
        deeps: {
          50: "rgb(var(--deeps-50) / <alpha-value>)",
          100: "rgb(var(--deeps-100) / <alpha-value>)",
          200: "rgb(var(--deeps-200) / <alpha-value>)",
          300: "rgb(var(--deeps-300) / <alpha-value>)",
          400: "rgb(var(--deeps-400) / <alpha-value>)",
          500: "rgb(var(--deeps-500) / <alpha-value>)",
          600: "rgb(var(--deeps-600) / <alpha-value>)",
          700: "rgb(var(--deeps-700) / <alpha-value>)",
          800: "rgb(var(--deeps-800) / <alpha-value>)",
          900: "rgb(var(--deeps-900) / <alpha-value>)",
          950: "rgb(var(--deeps-950) / <alpha-value>)",
          page: "rgb(var(--deeps-page) / <alpha-value>)",
          surface: "rgb(var(--deeps-surface) / <alpha-value>)",
          raised: "rgb(var(--deeps-raised) / <alpha-value>)",
          hover: "rgb(var(--deeps-hover) / <alpha-value>)",
          input: "rgb(var(--deeps-input) / <alpha-value>)",
          overlay: "rgb(var(--deeps-overlay) / <alpha-value>)",
          border: "rgb(var(--deeps-border) / <alpha-value>)",
          "border-strong": "rgb(var(--deeps-border-strong) / <alpha-value>)",
          text: "rgb(var(--deeps-text) / <alpha-value>)",
          bright: "rgb(var(--deeps-bright) / <alpha-value>)",
          "bright-soft": "rgb(var(--deeps-bright-soft) / <alpha-value>)",
          soft: "rgb(var(--deeps-soft) / <alpha-value>)",
          muted: "rgb(var(--deeps-muted) / <alpha-value>)",
          subtle: "rgb(var(--deeps-subtle) / <alpha-value>)",
          faint: "rgb(var(--deeps-faint) / <alpha-value>)",
          "on-accent": "rgb(var(--deeps-on-accent) / <alpha-value>)",
          accent: "rgb(var(--deeps-accent) / <alpha-value>)",
          "accent-soft": "rgb(var(--deeps-accent-soft) / <alpha-value>)",
          "accent-surface": "rgb(var(--deeps-accent-surface) / <alpha-value>)",
          success: "rgb(var(--deeps-success) / <alpha-value>)",
          warning: "rgb(var(--deeps-warning) / <alpha-value>)",
          "warning-surface": "rgb(var(--deeps-warning-surface) / <alpha-value>)",
          danger: "rgb(var(--deeps-danger) / <alpha-value>)",
          "danger-soft": "rgb(var(--deeps-danger-soft) / <alpha-value>)",
          "danger-hover": "rgb(var(--deeps-danger-hover) / <alpha-value>)",
          "danger-strong": "rgb(var(--deeps-danger-strong) / <alpha-value>)",
          "danger-solid": "rgb(var(--deeps-danger-solid) / <alpha-value>)",
          "danger-surface": "rgb(var(--deeps-danger-surface) / <alpha-value>)"
        }
      }
    }
  },
  plugins: []
};
