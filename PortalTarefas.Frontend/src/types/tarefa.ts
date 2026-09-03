/**
 * DTO para exibição resumida na listagem de tarefas.
 */
export interface TarefaSummaryDto {
  id: number;
  title: string;
  priority: string;
  isCompleted: boolean;
  dueDate: string;
}

/**
 * DTO completo com detalhes da tarefa.
 */
export interface TarefaDetailDto extends TarefaSummaryDto {
  description: string;
  createdAt: string;
}

/**
 * DTO para envio de dados na criação de nova tarefa.
 */
export interface TarefaCreateDto {
  title: string;
  description: string;
  priority: string;
  dueDate: string;
}

/**
 * Estrutura genérica de resposta paginada retornada pela API.
 */
export interface PagedResult<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Estrutura para tratamento de erros padronizados (Problem Details - RFC 9457).
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

/**
 * Parâmetros de filtro e paginação sincronizados com a URL.
 */
export interface TarefaFilterParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  direction?: string;
  search?: string;
  isCompleted?: boolean;
}