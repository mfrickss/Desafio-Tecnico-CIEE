/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        ciee: {
          blue: '#024089',
          navy: '#003087',
          dark: '#001f5c',
          deep: '#0a1c3d',
          orange: '#ed6b06',
          orangeFocus: '#C64F01',
          tint: '#e8f1fb',
          soft: '#f0f6ff',
          bg: '#f8fafc',
          text: '#1e293b'
        }
      },
      fontFamily: {
        sans: ['Lato', 'ui-sans-serif', 'system-ui', '-apple-system', 'sans-serif'],
      }
    },
  },
  plugins: [],
}
