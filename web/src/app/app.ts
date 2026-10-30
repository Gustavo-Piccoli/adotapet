import { Component } from '@angular/core';
import { Shell } from './layout/shell';
import { ToastContainer } from './core/toast/toast-container';

@Component({
  selector: 'app-root',
  imports: [Shell, ToastContainer],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
