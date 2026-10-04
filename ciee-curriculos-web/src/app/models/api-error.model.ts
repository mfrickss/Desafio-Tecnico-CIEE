export interface ErroValidacaoCampo {
  campo: string;
  erro: string;
}

export interface ApiErroValidacaoResponse {
  title?: string;
  detail?: string;
  status?: number;
  mensagem?: string;
  errors?: Record<string, string[]>;
  erros?: ErroValidacaoCampo[];
}