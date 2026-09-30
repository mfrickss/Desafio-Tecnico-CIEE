export interface Candidato {
  id: string;
  nomeCompleto: string;
  email: string;
  telefone?: string;
  cargoInteresse?: string;
  resumoProfissional?: string;
  dataCadastro: string;
  teveOrigemPdf: boolean;
}

export interface CriarCandidatoDto {
  nomeCompleto: string;
  email: string;
  telefone?: string;
  cargoInteresse?: string;
  resumoProfissional?: string;
  teveOrigemPdf?: boolean;
}

export interface ExtracaoPdfResponse {
  nomeCompleto?: string;
  email?: string;
  telefone?: string;
  cargoInteresse?: string;
  resumoProfissional?: string;
  textoBruto: string;
  sucesso: boolean;
  mensagem: string;
}
