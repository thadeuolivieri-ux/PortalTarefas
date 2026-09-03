import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { apiService, ApiError } from '../services/api';
import type { TarefaDetailDto } from '../types/tarefa';

/**
 * Tipagem para o estado da requisição de detalhes.
 */
type RequestState = 'idle' | 'loading' | 'success' | 'error';

export function TarefasDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [tarefa, setTarefa] = useState<TarefaDetailDto | null>(null);
  const [state, setState] = useState<RequestState>('idle');
  const [errorMessage, setErrorMessage] = useState<string>('');

  useEffect(() => {
    let isSubscribed = true;

    const fetchTarefa = async () => {
      if (!id || isNaN(Number(id))) {
        setErrorMessage('Identificador de tarefa inválido.');
        setState('error');
        return;
      }

      setState('loading');
      setErrorMessage('');

      try {
        const data = await apiService.getTarefaById(Number(id));
        if (isSubscribed) {
          setTarefa(data);
          setState('success');
        }
      } catch (err) {
        if (isSubscribed) {
          if (err instanceof ApiError) {
            if (err.status === 404) {
              setErrorMessage('Tarefa não encontrada no servidor (Erro 404).');
            } else {
              setErrorMessage(err.message);
            }
          } else {
            setErrorMessage('Erro ao carregar detalhes da tarefa.');
          }
          setState('error');
        }
      }
    };

    fetchTarefa();

    return () => {
      isSubscribed = false; // Cancela efeito se o usuário navegar antes de concluir a requisição
    };
  }, [id]);

  return (
    <div style={{ padding: '20px', fontFamily: 'sans-serif' }}>
      <Link to="/tarefas">← Voltar para a lista</Link>

      <h2>Detalhes da Tarefa</h2>

      {state === 'loading' && <p>Carregando detalhes da tarefa...</p>}

      {state === 'error' && (
        <div style={{ padding: '10px', background: '#f8d7da', color: '#721c24', borderRadius: '4px', marginTop: '10px' }}>
          <strong>Atenção:</strong> {errorMessage}
        </div>
      )}

      {state === 'success' && tarefa && (
        <div style={{ border: '1px solid #ccc', padding: '15px', borderRadius: '4px', marginTop: '10px', maxWidth: '500px' }}>
          <p><strong>ID:</strong> {tarefa.id}</p>
          <p><strong>Título:</strong> {tarefa.title}</p>
          <p><strong>Descrição:</strong> {tarefa.description || 'Sem descrição.'}</p>
          <p><strong>Prioridade:</strong> {tarefa.priority}</p>
          <p><strong>Status:</strong> {tarefa.isCompleted ? 'Concluída' : 'Pendente'}</p>
          <p><strong>Data de Vencimento:</strong> {new Date(tarefa.dueDate).toLocaleDateString()}</p>
          <p><strong>Data de Criação:</strong> {new Date(tarefa.createdAt).toLocaleDateString()}</p>
        </div>
      )}
    </div>
  );
}