import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/components/navbar/navbar.component';

@Component({
  imports: [NavbarComponent, RouterOutlet],
  selector: 'cw-root',
  styleUrl: './app.component.scss',
  templateUrl: './app.component.html',
})
export class App {}
