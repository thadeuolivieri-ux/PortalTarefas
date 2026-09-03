import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { TarefasListPage } from './pages/TarefasListPage';
import { TarefasDetailPage } from './pages/TarefasDetailPage';
import { TarefasFormPage } from './pages/TarefasFormPage';

/**
 * Componente Principal definindo a estrutura de roteamento da SPA React.
 */
export function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* Redireciona a rota raiz para a listagem */}
        <Route path="/" element={<Navigate to="/tarefas" replace />} />
        
        {/* Rotas da aplicação */}
        <Route path="/tarefas" element={<TarefasListPage />} />
        <Route path="/tarefas/nova" element={<TarefasFormPage />} />
        <Route path="/tarefas/:id" element={<TarefasDetailPage />} />
        
        {/* Rota de fallback para caminhos inválidos */}
        <Route path="*" element={<div style={{ padding: '20px' }}>Página não encontrada (404)</div>} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;