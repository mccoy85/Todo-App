import { ConfigProvider } from 'antd';
import { TodoPage } from './components/TodoPage';
import { LandingPage } from './components/LandingPage';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

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
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<LandingPage />} />
          <Route path="/app" element={<TodoPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </ConfigProvider>
  );
};

export default App;
