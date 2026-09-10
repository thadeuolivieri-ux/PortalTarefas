import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // Recupera a rota que o usuário tentou acessar antes do redirecionamento
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErro(null);
    setCarregando(true);

    try {
      await login(email, password);
      // Redireciona de volta para a URL pretendida
      navigate(from, { replace: true });
    } catch (err: unknown) {
      if (err instanceof Error) {
        setErro(err.message);
      } else {
        setErro('Ocorreu um erro ao tentar realizar o login.');
      }
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div style={{ maxWidth: '400px', margin: '50px auto', padding: '20px', border: '1px solid #ccc', borderRadius: '8px' }}>
      <h2>PortalTarefas - Autenticação</h2>
      
      {erro && (
        <div style={{ padding: '10px', marginBottom: '15px', backgroundColor: '#f8d7da', color: '#721c24', borderRadius: '4px' }}>
          {erro}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '15px' }}>
          <label htmlFor="email" style={{ display: 'block', marginBottom: '5px' }}>E-mail:</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
          />
        </div>

        <div style={{ marginBottom: '15px' }}>
          <label htmlFor="password" style={{ display: 'block', marginBottom: '5px' }}>Senha:</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
          />
        </div>

        <button
          type="submit"
          disabled={carregando}
          style={{ width: '100%', padding: '10px', backgroundColor: '#007bff', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
        >
          {carregando ? 'Entrando...' : 'Entrar'}
        </button>
      </form>

      <div style={{ marginTop: '20px', fontSize: '0.85em', color: '#555' }}>
        <p><strong>Contas de teste (Seed):</strong></p>
        <ul>
          <li>Admin: <code>admin@portal.com</code> / <code>Senha@123</code></li>
          <li>Usuário: <code>usuario@portal.com</code> / <code>Senha@123</code></li>
        </ul>
      </div>
    </div>
  );
};