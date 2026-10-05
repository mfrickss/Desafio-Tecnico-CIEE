export interface ErroValidacaoCampo {
  campo: string;
  erro: string;
}

export interface ApiErroValidacaoResponse {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
