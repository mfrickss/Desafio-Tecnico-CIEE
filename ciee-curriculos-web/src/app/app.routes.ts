import { Routes } from '@angular/router';

export const routes: Routes = [
  { 
    path: '', 
    loadComponent: () => import('./pages/listagem/listagem.component').then(m => m.ListagemComponent) 
  },
  { 
    path: 'cadastro', 
    loadComponent: () => import('./pages/cadastro/cadastro.component').then(m => m.CadastroComponent) 
  },
  { 
    path: 'detalhes/:id', 
    loadComponent: () => import('./pages/detalhes/detalhes.component').then(m => m.DetalhesComponent) 
  },
  { 
    path: '**', 
    redirectTo: '' 
  }
];
