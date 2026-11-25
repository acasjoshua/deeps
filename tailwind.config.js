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
          950: "rgb(var(--deeps-950) / <alpha-value>)"
        }
      }
    }
  },
  plugins: []
};
