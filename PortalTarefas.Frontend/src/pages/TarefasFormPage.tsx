import { useState, type FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { apiService, ApiError } from '../services/api';

export function TarefasFormPage() {
  const navigate = useNavigate();

  // Campos do formulário
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Media');
  const [dueDate, setDueDate] = useState('');

  // Estados para controle de envio e erros de validação da API
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setErrorMessage('');
    setFieldErrors({});

    try {
      await apiService.createTarefa({
        title,
        description,
        priority,
        dueDate: dueDate ? new Date(dueDate).toISOString() : new Date().toISOString(),
      });
      
      // Atualização de interface via roteamento sem recarregamento de página
      navigate('/tarefas');
    } catch (err) {
      if (err instanceof ApiError) {
        setErrorMessage(err.message);
        
        // Mapeamento dos erros de validação devolvidos no padrão Problem Details (RFC 9457)
        if (err.problemDetails?.errors) {
          setFieldErrors(err.problemDetails.errors);
        }
      } else {
        setErrorMessage('Erro inesperado ao cadastrar tarefa.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div style={{ padding: '20px', fontFamily: 'sans-serif', maxWidth: '500px' }}>
      <Link to="/tarefas">← Voltar para a lista</Link>

      <h2>Cadastrar Nova Tarefa</h2>

      {errorMessage && (
        <div style={{ padding: '10px', background: '#f8d7da', color: '#721c24', borderRadius: '4px', marginBottom: '15px' }}>
          <strong>Erro no cadastro:</strong> {errorMessage}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Título *</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
            required
          />
          {fieldErrors.Title && (
            <span style={{ color: 'red', fontSize: '12px' }}>{fieldErrors.Title.join(', ')}</span>
          )}
        </div>

        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Descrição</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box', height: '80px' }}
          />
          {fieldErrors.Description && (
            <span style={{ color: 'red', fontSize: '12px' }}>{fieldErrors.Description.join(', ')}</span>
          )}
        </div>

        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Prioridade</label>
          <select
            value={priority}
            onChange={(e) => setPriority(e.target.value)}
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
          >
            <option value="Baixa">Baixa</option>
            <option value="Media">Média</option>
            <option value="Alta">Alta</option>
          </select>
        </div>

        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>Data de Vencimento</label>
          <input
            type="date"
            value={dueDate}
            onChange={(e) => setDueDate(e.target.value)}
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
          />
        </div>

        <button
          type="submit"
          disabled={isSubmitting}
          style={{ padding: '10px 15px', background: '#007bff', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
        >
          {isSubmitting ? 'Salvando...' : 'Salvar Tarefa'}
        </button>
      </form>
    </div>
  );
}