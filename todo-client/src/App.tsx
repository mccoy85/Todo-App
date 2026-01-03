import { ConfigProvider, App as AntApp } from 'antd';
import { TodoPage } from './components/TodoPage';
import { LandingPage } from './components/LandingPage';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

// App shell with theme + route wiring.
const App = () => {
  return (
    <ConfigProvider
      theme={{
        token: {
          colorPrimary: '#ff7a1a',
          borderRadius: 8,
        },
      }}
    >
      <AntApp>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<LandingPage />} />
            <Route path="/app" element={<TodoPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </AntApp>
    </ConfigProvider>
  );
};

export default App;
