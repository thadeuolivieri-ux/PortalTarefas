import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { apiService, ApiError } from '../services/api';
import type { TarefaSummaryDto, PagedResult } from '../types/tarefa';

/**
 * Tipagem para representação explícita dos 5 estados de requisição.
 */
type RequestState = 'idle' | 'loading' | 'success' | 'empty' | 'error';

export function TarefasListPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  // Leitura dos filtros a partir da URL
  const page = parseInt(searchParams.get('page') || '1', 10);
  const search = searchParams.get('search') || '';
  const sortBy = searchParams.get('sortBy') || 'id';

  // Estados locais da página
  const [data, setData] = useState<PagedResult<TarefaSummaryDto> | null>(null);
  const [state, setState] = useState<RequestState>('idle');
  const [errorMessage, setErrorMessage] = useState<string>('');
  const [refreshKey, setRefreshKey] = useState<number>(0);

  // Busca de dados sincronizada com a URL e o gatilho de atualização
  useEffect(() => {
    let isSubscribed = true; // Previne atualização de estado em componente desmontado

    const fetchTarefas = async () => {
      setState('loading');
      setErrorMessage('');

      try {
        const result = await apiService.getTarefas({
          page,
          pageSize: 5,
          search,
          sortBy,
          direction: 'asc',
        });

        if (isSubscribed) {
          setData(result);
          setState(result.items.length === 0 ? 'empty' : 'success');
        }
      } catch (err) {
        if (isSubscribed) {
          if (err instanceof ApiError) {
            setErrorMessage(err.message);
          } else {
            setErrorMessage('Erro inesperado ao carregar dados.');
          }
          setState('error');
        }
      }
    };

    fetchTarefas();

    return () => {
      isSubscribed = false; // Cancela efeito obsoleto ao desmontar ou reexecutar
    };
  }, [page, search, sortBy, refreshKey]);

  // Atualização de filtros na URL
  const handleSearchChange = (value: string) => {
    setSearchParams({ search: value, page: '1', sortBy });
  };

  // Função para exclusão de tarefa
  const handleDelete = async (id: number) => {
    if (!confirm(`Tem certeza de que deseja excluir a tarefa #${id}?`)) {
      return;
    }

    try {
      await apiService.deleteTarefa(id);
      setRefreshKey((prev) => prev + 1); // Dispara a recarga dos dados na tabela
    } catch (err) {
      if (err instanceof ApiError) {
        alert(`Erro ao excluir: ${err.message}`);
      } else {
        alert('Falha ao excluir a tarefa.');
      }
    }
  };

  return (
    <div style={{ padding: '20px', fontFamily: 'sans-serif' }}>
      <h2>Listagem de Tarefas (SPA React)</h2>

      {/* Ações e Filtros */}
      <div style={{ marginBottom: '15px', display: 'flex', gap: '10px' }}>
        <Link
          to="/tarefas/nova"
          style={{
            padding: '8px 12px',
            background: '#28a745',
            color: '#fff',
            textDecoration: 'none',
            borderRadius: '4px',
          }}
        >
          + Nova Tarefa
        </Link>
        <input
          type="text"
          placeholder="Filtrar por título..."
          value={search}
          onChange={(e) => handleSearchChange(e.target.value)}
          style={{ padding: '8px', width: '250px' }}
        />
      </div>

      {/* Renderização condicional conforme os 5 estados da requisição */}
      {state === 'idle' && <p>Aguardando inicialização...</p>}

      {state === 'loading' && <p>Carregando tarefas, aguarde...</p>}

      {state === 'error' && (
        <div style={{ padding: '10px', background: '#f8d7da', color: '#721c24', borderRadius: '4px' }}>
          <strong>Erro:</strong> {errorMessage}
        </div>
      )}

      {state === 'empty' && (
        <p>Nenhuma tarefa encontrada para os filtros aplicados.</p>
      )}

      {state === 'success' && data && (
        <>
          <table border={1} cellPadding={8} cellSpacing={0} style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f2f2f2' }}>
                <th>ID</th>
                <th>Título</th>
                <th>Prioridade</th>
                <th>Status</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((t) => (
                <tr key={t.id}>
                  <td>{t.id}</td>
                  <td>{t.title}</td>
                  <td>{t.priority}</td>
                  <td>{t.isCompleted ? 'Concluída' : 'Pendente'}</td>
                  <td>
                    <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
                      <Link to={`/tarefas/${t.id}`}>Ver Detalhes</Link>
                      <button
                        onClick={() => handleDelete(t.id)}
                        style={{
                          background: '#dc3545',
                          color: '#fff',
                          border: 'none',
                          padding: '4px 8px',
                          borderRadius: '4px',
                          cursor: 'pointer',
                        }}
                      >
                        Excluir
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Paginação */}
          <div style={{ marginTop: '15px', display: 'flex', gap: '10px', alignItems: 'center' }}>
            <button
              disabled={!data.hasPreviousPage}
              onClick={() => setSearchParams({ search, page: (page - 1).toString(), sortBy })}
            >
              Anterior
            </button>
            <span>Página {data.pageIndex} de {data.totalPages}</span>
            <button
              disabled={!data.hasNextPage}
              onClick={() => setSearchParams({ search, page: (page + 1).toString(), sortBy })}
            >
              Próxima
            </button>
          </div>
        </>
      )}
    </div>
  );
}