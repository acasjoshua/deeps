/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.razor", "./**/*.html", "!./node_modules/**", "!./bin/**", "!./obj/**"],
  theme: {
    extend: {
      colors: {
        deeps: { 400: "#34d399", 500: "#10b981", 600: "#059669" }
      }
    }
  },
  plugins: []
};
