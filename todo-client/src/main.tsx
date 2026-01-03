import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import './css/styles.css'
import App from './App.tsx'

dayjs.extend(utc);

// Bootstrap the React app into the DOM.
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
