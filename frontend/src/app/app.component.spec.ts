import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { App } from './app.component';
import { routes } from './app.routes';
import { HomeComponent } from './features/home/home.component';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App, HomeComponent],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  it('should create the app shell', () => {
    const fixture = TestBed.createComponent(App);

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the home page', () => {
    const fixture = TestBed.createComponent(HomeComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Cobranças em um só lugar');
  });
});