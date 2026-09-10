export interface UsuarioLogado {
  email: string;
  roles: string[];
  claims: { type: string; value: string }[];
}

export interface LoginResponse {
  mensagem: string;
  usuario: UsuarioLogado;
}

export const authService = {
  async login(email: string, password: string): Promise<UsuarioLogado> {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', // Obrigatório para envio de cookies HTTP-Only
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Credenciais inválidas.');
      }
      if (response.status === 423) {
        throw new Error('Conta bloqueada por muitas tentativas incorretas.');
      }
      throw new Error('Erro ao realizar login.');
    }

    const data: LoginResponse = await response.json();
    return data.usuario;
  },

  async logout(): Promise<void> {
    await fetch('/api/auth/logout', {
      method: 'POST',
      credentials: 'include',
    });
  },

  async getMe(): Promise<UsuarioLogado | null> {
    try {
      const response = await fetch('/api/auth/me', {
        method: 'GET',
        credentials: 'include',
      });

      if (!response.ok) return null;

      const data: UsuarioLogado = await response.json();
      return data;
    } catch {
      return null;
    }
  },
};