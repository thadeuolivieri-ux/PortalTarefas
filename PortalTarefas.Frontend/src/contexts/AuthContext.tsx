import React, { createContext, useContext, useEffect, useState } from 'react';
import { authService, type UsuarioLogado } from '../services/authService';

interface AuthContextType {
  usuario: UsuarioLogado | null;
  estaAutenticado: boolean;
  carregando: boolean;
  login: (email: string, pass: string) => Promise<void>;
  logout: () => Promise<void>;
  temRole: (role: string) => boolean;
  temClaim: (tipo: string, valor: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [usuario, setUsuario] = useState<UsuarioLogado | null>(null);
  const [carregando, setCarregando] = useState<boolean>(true);

  // Restauração de sessão ao carregar a SPA
  useEffect(() => {
    authService.getMe()
      .then((dados) => setUsuario(dados))
      .finally(() => setCarregando(false));
  }, []);

  const login = async (email: string, pass: string) => {
    const user = await authService.login(email, pass);
    setUsuario(user);
  };

  const logout = async () => {
    await authService.logout();
    setUsuario(null);
  };

  const temRole = (role: string) => {
    return usuario?.roles?.includes(role) ?? false;
  };

  const temClaim = (tipo: string, valor: string) => {
    return usuario?.claims?.some((c) => c.type === tipo && c.value === valor) ?? false;
  };

  return (
    <AuthContext.Provider
      value={{
        usuario,
        estaAutenticado: !!usuario,
        carregando,
        login,
        logout,
        temRole,
        temClaim,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth deve ser usado dentro de um AuthProvider');
  }
  return context;
};