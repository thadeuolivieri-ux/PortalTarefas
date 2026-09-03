import type {
  TarefaSummaryDto,
  TarefaDetailDto,
  TarefaCreateDto,
  PagedResult,
  ProblemDetails,
  TarefaFilterParams,
} from '../types/tarefa';

// Obtém a URL base da API configurada no ambiente
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

/**
 * Classe customizada para tratamento padronizado de erros de API.
 */
export class ApiError extends Error {
  public status: number;
  public problemDetails?: ProblemDetails;

  constructor(status: number, message: string, problemDetails?: ProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problemDetails = problemDetails;
  }
}

/**
 * Função utilitária para desserializar respostas HTTP e capturar respostas de erro no padrão Problem Details (RFC 9457).
 */
async function handleResponse<T>(response: Response): Promise<T> {
  if (response.ok) {
    // Trata respostas com corpo vazio (ex: HTTP 204 No Content no DELETE)
    if (response.status === 204) {
      return {} as T;
    }
    return response.json() as Promise<T>;
  }

  let problemDetails: ProblemDetails | undefined;
  try {
    problemDetails = await response.json();
  } catch {
    // Caso o corpo da resposta não seja JSON válido
  }

  const errorMessage =
    problemDetails?.detail || problemDetails?.title || `Erro HTTP ${response.status}`;

  throw new ApiError(response.status, errorMessage, problemDetails);
}

/**
 * Camada de acesso isolada à API REST de Tarefas.
 */
export const apiService = {
  /**
   * Consulta tarefas com suporte a filtros, ordenação e paginação.
   */
  async getTarefas(params: TarefaFilterParams = {}): Promise<PagedResult<TarefaSummaryDto>> {
    const query = new URLSearchParams();

    if (params.page) query.append('page', params.page.toString());
    if (params.pageSize) query.append('pageSize', params.pageSize.toString());
    if (params.sortBy) query.append('sortBy', params.sortBy);
    if (params.direction) query.append('direction', params.direction);
    if (params.search) query.append('search', params.search);
    if (params.isCompleted !== undefined) query.append('isCompleted', params.isCompleted.toString());

    try {
      const response = await fetch(`${API_BASE_URL}/api/v1/tarefas?${query.toString()}`);
      return await handleResponse<PagedResult<TarefaSummaryDto>>(response);
    } catch (error) {
      if (error instanceof ApiError) throw error;
      throw new ApiError(0, 'Falha de conexão com a rede ou servidor offline.');
    }
  },

  /**
   * Obtém detalhes de uma tarefa por ID.
   */
  async getTarefaById(id: number): Promise<TarefaDetailDto> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/v1/tarefas/${id}`);
      return await handleResponse<TarefaDetailDto>(response);
    } catch (error) {
      if (error instanceof ApiError) throw error;
      throw new ApiError(0, 'Falha de conexão com a rede ou servidor offline.');
    }
  },

  /**
   * Realiza a criação de uma nova tarefa.
   */
  async createTarefa(data: TarefaCreateDto): Promise<TarefaDetailDto> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/v1/tarefas`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });
      return await handleResponse<TarefaDetailDto>(response);
    } catch (error) {
      if (error instanceof ApiError) throw error;
      throw new ApiError(0, 'Falha de conexão com a rede ou servidor offline.');
    }
  },

  /**
   * Realiza a exclusão de uma tarefa por ID no backend.
   */
  async deleteTarefa(id: number): Promise<void> {
    try {
      const response = await fetch(`${API_BASE_URL}/api/v1/tarefas/${id}`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        await handleResponse<void>(response);
      }
    } catch (error) {
      if (error instanceof ApiError) throw error;
      throw new ApiError(0, 'Falha de conexão com a rede ou servidor offline.');
    }
  },
};