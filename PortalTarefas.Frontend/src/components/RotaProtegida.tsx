import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export const RotaProtegida: React.FC = () => {
  const { estaAutenticado, carregando } = useAuth();
  const location = useLocation();

  if (carregando) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', marginTop: '50px' }}>
        <p>Carregando sessão...</p>
      </div>
    );
  }

  if (!estaAutenticado) {
    // Redireciona para o login e salva a URL que o usuário tentou acessar (requisito do roteiro)
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  // Se estiver autenticado, renderiza as rotas filhas
  return <Outlet />;
};