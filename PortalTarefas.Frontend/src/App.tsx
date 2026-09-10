import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { TarefasListPage } from './pages/TarefasListPage';
import { TarefasDetailPage } from './pages/TarefasDetailPage';
import { TarefasFormPage } from './pages/TarefasFormPage';
import { LoginPage } from './pages/LoginPage'; // Tela de Login
import { RotaProtegida } from './components/RotaProtegida'; // Componente de Rota Privada
import { AuthProvider } from './contexts/AuthContext'; // Contexto de Autenticação

/**
 * Componente Principal definindo a estrutura de roteamento da SPA React
 * com Autenticação e Rotas Protegidas (Unidade 6).
 */
export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Rotas Públicas */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/" element={<Navigate to="/tarefas" replace />} />
          
          {/* Rotas Protegidas (Exigem autenticação) */}
          <Route element={<RotaProtegida />}>
            <Route path="/tarefas" element={<TarefasListPage />} />
            <Route path="/tarefas/nova" element={<TarefasFormPage />} />
            <Route path="/tarefas/:id" element={<TarefasDetailPage />} />
          </Route>
          
          {/* Rota de fallback para caminhos inválidos */}
          <Route path="*" element={<div style={{ padding: '20px' }}>Página não encontrada (404)</div>} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;