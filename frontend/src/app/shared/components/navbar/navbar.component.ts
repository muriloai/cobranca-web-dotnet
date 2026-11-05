import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  imports: [RouterLink, RouterLinkActive],
  selector: 'cw-navbar',
  templateUrl: './navbar.component.html',
})
export class NavbarComponent {}
